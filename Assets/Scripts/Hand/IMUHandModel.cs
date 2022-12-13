using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KW_Mocap;
using SS_KinetrackIII;

public class IMUHandModel
{
    private const int n_fing = 5;
    private const int n_bone = 4;

    /// <summary>
    /// sensor to hand(LeapMotionが定義した関節番号)
    /// 関節番号0から3までの関節に何番のセンサーを割り当てるかをスペース区切りで記述する。
    /// センサーが存在しない場合は 'x' と入れる。 
    /// </summary>
    private string[] sensorTable =
        {
            "x 2 1 0",
            "x 5 4 3",
            "9 8 7 6",
            "x 12 11 10",
            "x 15 14 13"
        };

    /// <summary>
    /// finger to referenceの変換マップ
    /// sensorTableから作成される。
    /// </summary>
    private readonly int[,] f2r;

    private readonly Transform[,] Tr;

    private readonly SS_CALIB imuCalibration = new SS_CALIB();

    public IMUHandModel(string name, int sensorCount, ushort sensorStatus, Transform[,] joints)
    {
        int[] r2s = CreateReference2SensorMap(sensorCount, sensorStatus);
        imuCalibration.Init(name, r2s);
        f2r = CreateFinger2ReferenceMap(sensorTable);
        this.Tr = joints;
    }

    public void Calibrate(long sf, long df, IMUPAR[,] P)
    {
        imuCalibration.Record(sf, df, P);
        imuCalibration.Calib(FixedPose.cal_joints);
    }

    public void Draw(long nf, IMUPAR[,] Q)
    {
        Quaternion[] Qs = new Quaternion[16];   //キャリブレーション済みの絶対回転?
        Quaternion[] Qd;                        //キャリブレーション済みの絶対回転から計算される相対回転?
        imuCalibration.GetQd(nf, Q, Qs);
        Qd = Layer(Qs);                         //親のオブジェクトからの相対回転に変換

        for (int i = 0; i < n_fing; i++)
        {
            for (int j = 0; j < n_bone; j++)
            {
                int n = f2r[i, j];
                if (n != -1 && n != 9)
                    Tr[i, j].localRotation = Qd[n];
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
        int[,] f2r = new int[n_bone, n_fing];
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
}
