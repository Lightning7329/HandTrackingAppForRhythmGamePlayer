// OBJECT(= HUMAN's HAND) ASSIGN to BORN STRUCTURE

using System;
using System.Linq;
using UnityEngine;
using SS_KinetrackIII;

namespace KW_Mocap {
	public class HandSetting : MonoBehaviour
	{
		/// <summary>
		/// 指を表すGameObjectの名前からindexを省いたもの。
		/// </summary>
		[SerializeField] private string[] fingerNames = { "Thumb" ,"Index", "Middle", "Ring", "Pinky" };
        /// <summary>
        /// 指の関節を表すGameObjectのインデックスに使用する文字。
        /// </summary>
        [SerializeField] private char[] fingerBoneIndex = { '1', '2', '3' };
		/// <summary>
		/// sensor to hand(LeapMotionが定義した関節番号)
		/// 関節番号0から3までの関節に何番のセンサーを割り当てるかをスペース区切りで記述する。
		/// センサーが存在しない場合は 'x' と入れる。 
		/// </summary>
		[SerializeField]
		private string[] sensorTable =
		{
			"x 15 14 13",
			"x 12 11 10",
			"9 8 7 6",
			"x 5 4 3",
			"x 2 1 0"
		};
		/// <summary>
		/// 正常時とエラー時のマテリアルを集めたクラス。
		/// </summary>
		[SerializeField] private HandMaterial materials;
		/// <summary>
		/// ハンドモデルの関節を表す2次元配列。1次元目が指、2次元目が関節。
		/// </summary>
		public GameObject[,] joints = new GameObject[5,3];  //指の本数 * 関節の数

		/// <summary>
		/// finger to referenceの変換マップ
		/// sensorTableから作成される。
		/// </summary>
		private int[,] f2r = new int[5, 4];

		private SS_CALIB imuCalibration = new SS_CALIB();

		private void Start()
		{
			GrabJoints();
        }

		/// <summary>
		/// 生きてるセンサー情報をもとにセンサーとの関連付けをしたりなど変換マップを作成する。
		/// </summary>
		/// <param name="sensorCount">センサーの個数</param>
		/// <param name="sensorStatus">生きてるセンサー情報</param>
		public void Init(int sensorCount, ushort sensorStatus)
		{
			int[] r2s = CreateReference2SensorMap(sensorCount, sensorStatus);
			imuCalibration.Init(this.gameObject.name, r2s);
			CreateFinger2ReferenceMap();
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
		private void CreateFinger2ReferenceMap()
        {
			for (int i = 0; i < f2r.GetLength(0); i++)
			{
				string[] str = sensorTable[i].Split(' ');
				for (int j = 0; j < f2r.GetLength(1); j++)
				{
					f2r[i, j] = ((j < str.Length) && !str[j].Equals("x")) ? int.Parse(str[j]) : -1;
				}
			}
		}

		/// <summary>
		/// 自分の孫を含めた子オブジェクトの中からボーンになるオブジェクトを二次元配列に格納する。
		/// </summary>
		private void GrabJoints()
		{
			GameObject[] children = GetComponentsInChildren<Transform>()
									.Where<Transform>(t => t.name != "J" && t.name != "F" && t.name != "Top")
									.Select(t => t.gameObject)
									.ToArray();
            foreach (GameObject Gc in children)
            {
                int i = Array.IndexOf(fingerNames, Gc.name.TrimEnd(fingerBoneIndex));
				if (i == -1) continue;
                int j = int.Parse(Gc.name[Gc.name.Length - 1].ToString()) - 1;
                this.joints[i, j] = Gc;
				//Debug.Log($"joint[{i}, {j}] assigned");
			}
        }

		/// <summary>
		/// 再帰的に手を構成するオブジェクトのMaterialを変更する。
		/// </summary>
		/// <param name="Go">変更したい一番上の階層のGame Object</param>
		/// <param name="flg">trueのとき通常のMaterial、falseのときエラー用のMaterial</param>
		public void SetMaterial(GameObject Go, bool flg)
		{
			switch (Go.name)
			{
				case "P":
					Go.GetComponent<MeshRenderer>().material = flg ? materials.Palm : materials.Error;
					break;
				case "F":
					Go.GetComponent<MeshRenderer>().material = flg ? materials.Finger : materials.Error;
					break;
				case "J":
					Go.GetComponent<MeshRenderer>().material = flg ? materials.Joint : materials.Error;
					break;
				case "Top":
					Go.GetComponent<MeshRenderer>().material = flg ? materials.Top : materials.Error;
					break;
				default:
					break;
			}

			int n = Go.transform.childCount;
			if (n < 1) return;

			for (int i = 0; i < n; i++)
			{
				GameObject Gc = Go.transform.GetChild(i).gameObject;
				SetMaterial(Gc, flg);
			}
		}

		/// <summary>
		/// ハンドモデルの表示/非表示
		/// </summary>
		/// <param name="flg"></param>
		public void Active(bool flg) => _SetVisible(this.gameObject, flg);
		private void _SetVisible(GameObject Go, bool flg)
		{
			if (Go.GetComponent<MeshRenderer>()) Go.GetComponent<MeshRenderer>().enabled = flg;
			int n = Go.transform.childCount;
			if (n < 1) return;

			for (int i = 0; i < n; i++)
			{
				GameObject Gc = Go.transform.GetChild(i).gameObject;
				_SetVisible(Gc, flg);
			}
		}

		public void Calibrate(long sf, long df, IMUPAR[,] P)
		{
			Quaternion[] offset = new Quaternion[]
			{
				//TODO: 各関節の回転オフセットをここに書こう
			};
			imuCalibration.Record(sf, df, P);
			imuCalibration.Calib(offset);
		}

		[ContextMenu("SetCalibrationPose")]
		public void SetCalibrationPose()
		{
			for (int j = 0; j < joints.GetLength(1); j++)
			{
				joints[0, j].transform.localRotation = FixedPose.cal_Thumb[j];
			}
			for (int i = 1; i < joints.GetLength(0); i++)
            {
				for (int j = 0; j < joints.GetLength(1); j++)
                {
					joints[i, j].transform.localRotation = Quaternion.identity;
                }
            }
		}

		[ContextMenu("SetNormalPose")]
		public void SetNormalPose()
        {
			for (int i = 0; i < joints.GetLength(0); i++)
			{
				for (int j = 0; j < joints.GetLength(1); j++)
				{
					joints[i, j].transform.localRotation = FixedPose.nor_Joints[i, j];
				}
			}
		}
	}

    [Serializable]
    public class HandMaterial
    {
        public Material Palm;
        public Material Finger;
        public Material Joint;
        public Material Top;
        public Material Error;
    }

	public static class FixedPose
	{
		public static Quaternion[] cal_Thumb = new Quaternion[]
		{
			new Quaternion(0.0132610817f, 0.113525867f, -0.604481161f, 0.788377225f),
			new Quaternion(0.432321817f, 0.0f, 0.0f, 0.901719451f),
			new Quaternion(0.436801821f, 0.0f, 0.0f, 0.899557769f)
		};

		public static Quaternion[,] nor_Joints = new Quaternion[5, 3]
        {
			{	// Thumb
				new Quaternion(-0.109803915f, 0.31287995f, -0.542804599f, 0.771629691f),
				new Quaternion(0.133986175f, 0f, 0f, 0.990983188f),
				new Quaternion(0.233445361f, 0f, 0f, 0.972369969f)
			},
			{	// Index
				new Quaternion(0.216725737f, 0.0969353765f, -0.0216321182f, 0.971167147f),
				new Quaternion(0.201932624f, 0f, 0f, 0.979399443f),
				new Quaternion(0.0453629717f, 0f, 0f, 0.998970628f)
			},
			{	// Middle
				new Quaternion(0.169693574f, 7.4505806e-09f, 1.58324838e-08f, 0.985496938f),
				new Quaternion(0.207911685f, 0f, 0f, 0.978147626f),
				new Quaternion(0.114070177f, 0f, 0f, 0.993472695f)
			},
			{	// Ring
				new Quaternion(0.0558102429f, -0.0200386923f, 0.00112032972f, 0.998239696f),
				new Quaternion(0.0392597802f, 0f, 0f, 0.999229074f),
				new Quaternion(0.0360332578f, 0f, 0f, 0.999350607f)
			},
			{	// Pinky
				new Quaternion(0.0326956734f, -0.206515551f, 0.00690491032f, 0.977872491f),
				new Quaternion(0.0250428189f, 0f, 0f, 0.99968636f),
				new Quaternion(0.0184994023f, 0f, 0f, 0.999828875f)
			}
		};
	}
}
