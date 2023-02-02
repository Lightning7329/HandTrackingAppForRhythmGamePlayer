using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KW_Mocap;
using SS_KinetrackIII;

public class IMUHandModel
{
    private const int n_fing = 5;
    private const int n_bone = 4;
    private readonly Chirality LR;

    /// <summary>
    /// 左右の関節番号マップ
    /// </summary>
    private readonly Dictionary<Chirality, string[]> LR2sensorTable
        = new Dictionary<Chirality, string[]>
        {
            { Chirality.Left, new string[]{
                    "x 2 1 0",
                    "x 5 4 3",
                    "9 8 7 6",
                    "x 12 11 10",
                    "x 15 14 13"
                }
            },
            { Chirality.Right, new string[]{
                    "x 15 14 13",
                    "x 12 11 10",
                    "9 8 7 6",
                    "x 5 4 3",
                    "x 2 1 0"
                }
            }
        };
    /// <summary>
    /// reference number to sensorの変換マップ
    /// SS_IMUクラスのstatフィールドから作成される
    /// </summary>
    private readonly int[] r2s;
    /// <summary>
    /// finger to referenceの変換マップ
    /// sensorTableから作成される。
    /// </summary>
    private readonly int[,] f2r;

    private readonly Transform[,] Tr;

    private readonly SS_CALIB imuCalibration = new SS_CALIB();

    public enum Chirality { Left, Right }

    public IMUHandModel(string name, int sensorCount, ushort sensorStatus, Chirality LR, Transform[,] joints)
    {
        this.LR = LR;
        r2s = CreateReference2SensorMap(sensorCount, sensorStatus);
        imuCalibration.Init(name, r2s);
        f2r = CreateFinger2ReferenceMap(LR2sensorTable[LR]);
        this.Tr = joints;
    }

    /// <summary>
    /// フレーム数 * センサー個数分の配列imuParsのうちstartFrameからdeltaFrame分を使ってキャリブレーションを実行する
    /// </summary>
    /// <param name="startFrame">開始フレーム</param>
    /// <param name="deltaFrame">フレーム数</param>
    /// <param name="imuPars"></param>
    public void Calibrate(long startFrame, long deltaFrame, IMUPAR[,] imuPars)
    {
        imuCalibration.Record(startFrame, deltaFrame, imuPars);
        Debug.Log("IMUHandModel.Calibrate");
        imuCalibration.Calib(FixedPose.cal_joints);
    }

    public void Draw(IMUPAR[] Q)
    {
        Quaternion[] Qs = new Quaternion[16];
        Quaternion[] Qd;
        imuCalibration.GetQd(Q, Qs);
        Qd = Layer(Qs);
        if (this.LR == Chirality.Left)
        {
            for (int i = 0; i < n_fing; i++)
            {
                for (int j = 0; j < n_bone; j++)
                {
                    int n = f2r[i, j];
                    /* 第一、第二、第三関節のいずれかである かつ センサーが取り付けられている */
                    if ((n != -1 && n != 9) && r2s[n] != -1)
                        Tr[i, j].localRotation = Qd[n];
                }
            }
        }
        else
        {
            for (int i = 0; i < n_fing; i++)
            {
                for (int j = 0; j < n_bone; j++)
                {
                    int n = f2r[i, j];
                    /* 第一、第二、第三関節のいずれかである かつ センサーが取り付けられている */
                    if ((n != -1 && n != 9) && r2s[n] != -1)
                        Tr[i, j].localRotation = InvertRotationAboutX(Qd[n]);  //X軸固定の反転
                }
            }
        }
    }

    private Quaternion[] Layer(Quaternion[] Qs)
    {
        Quaternion[] Qd = new Quaternion[16];
        Quaternion Q9 = Qs[9];
        for (int i = 0; i < n_fing; i++)
        {
            for (int j = 1; j < n_bone; j++)
            {
                int k = n_bone - j; //第一関節 → 第二関節 → 第三関節
                int n0 = f2r[i, k];
                if (n0 >= 0)
                {
                    /* 注目している関節 */
                    Quaternion Q0 = Qs[n0];

                    /* 注目している関節の手前の関節があればそれ
                     * 注目している関節が中指の第三関節だったら手のひらの回転
                     * それ以外は回転なし */
                    int n1 = f2r[i, (k - 1)];
                    Quaternion Q1 = (n1 >= 0) ? Qs[n1] : (k == 1) ? Q9 : Quaternion.identity;
                    Qd[n0] = Quaternion.Inverse(Q1) * Q0;
                }
            }
        }
        Qd[9] = Q9;
        return Qd;
    }

    /// <summary>
    /// 生きてるセンサー情報をもとに関節番号とセンサー番号の関連付けを行う。
    /// </summary>
    /// <param name="sensorCount">センサーの個数</param>
    /// <param name="sensorStatus">生きてるセンサー情報</param>
    /// <returns>Reference To Sensor Map</returns>
    private int[] CreateReference2SensorMap(int sensorCount, ushort sensorStatus)
    {
        int[] r2s = new int[16];    //reference to sensor number
        int n = 0;
        for (int i = 0; i < 16; i++)
        {
            ushort msk = (ushort)(0x01 << i);
            r2s[i] = ((msk & sensorStatus) == msk) ? n++ : -1;
        }

        if (n != sensorCount)
            Debug.LogError($"{sensorCount}個のセンサーのうち{n}個しかキャリブレーションにいってない");

        return r2s;
    }

    /// <summary>
    /// sensorTableをもとにf2r(Finger To Reference Map)を作成する。
    /// </summary>
    private int[,] CreateFinger2ReferenceMap(string[] sensorTable)
    {
        int[,] f2r = new int[n_fing, n_bone];
        for (int i = 0; i < n_fing; i++)
        {
            string[] str = sensorTable[i].Split(' ');
            for (int j = 0; j < n_bone; j++)
            {
                f2r[i, j] = ((j < str.Length) && !str[j].Equals("x")) ? int.Parse(str[j]) : -1;
            }
        }
        return f2r;
    }

    /// <summary>
    /// X軸周りの回転はそのままにしてY軸、Z軸周りの回転を反転したクォータニオンを返す。
    /// transform.localScale.xを-1にしたオブジェクトの回転に対応している。
    /// 次のWeb上の記事を参考にしている。
    /// http://momose-d.cocolog-nifty.com/blog/2014/08/post-0735.html
    /// </summary>
    /// <param name="q">反転したいクォータニオン</param>
    /// <returns>反転後のクォータニオン</returns>
    private Quaternion InvertRotationAboutX(Quaternion q) => new Quaternion(q.x, -q.y, -q.z, q.w);
}
