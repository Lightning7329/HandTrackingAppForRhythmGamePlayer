
//==============================================================
//
// Hand Motion Tracking System ver.3
// Hand GameObject Mapping to Sensor Class
//
//--------------------------------------------------------------
// (C)Copyright Allrights reserved by Sakai Shigekazu 2022 -
//..............................................................
using UnityEngine ;
using Leap ;
using SS_KinetrackIII;
//--------------------------------------------------------------
public class SS_HAND : MonoBehaviour
{
	private const float _Scl = 1f / 16384f;
	private const int n_fing = 5;
	private const int n_bone = 4;
	[SerializeField] private SS_LEAP Leap;

	/// <summary>
	/// parent transform。LeapMotionを使う場合は使用しない。
	/// </summary>
	[SerializeField] public Transform Pt = null;

	/// <summary>
	/// 各関節のセンサーの回転オフセットのオイラー角
	/// </summary>
	[SerializeField] private Vector3[] offAng = new Vector3[16];

	/// <summary>
	/// sensor to hand(LeapMotionが定義した関節番号)
	/// 関節番号0から3までの関節に何番のセンサーを割り当てるかをスペース区切りで記述する。
	/// センサーが存在しない場合は 'x' と入れる。
	/// </summary>
	[SerializeField] private string[] s2h = new string[n_fing];

	private SS_CALIB Cal = new SS_CALIB();

	/// <summary>
	/// このスクリプトがアタッチされているGameObjectである手の名前
	/// </summary>
	private string h_name;
	private int now_sens;

	/// <summary>
	/// 各関節のTransform
	/// </summary>
	private Transform[,] Tr = new Transform[n_fing, n_bone];

	/// <summary>
	/// finger to referenceの変換マップ
	/// s2hから作成される。
	/// </summary>
	private int[,] f2r = new int[n_fing, n_bone];       // [finger_no, bone_no] = reference_no

	/// <summary>
	/// reference to sensorの変換マップ。
	/// 各関節位置に取り付けてあるセンサー番号。センサーがない場合は-1を入れておく。
	/// </summary>
	private int[] r2s = new int[16];
	private Quaternion[] Qoff = new Quaternion[16];
	//..............................................................
	private void Start()
	{
		GameObject Go = gameObject;
		h_name = Go.name;
		for (int i = 0; i < n_fing; i++)
		{
			for (int j = 0; j < n_bone; j++)
			{
				GameObject Gc = _Find(string.Format("F{0}{1}", i, j), Go);
				Tr[i, j] = (Gc != null) ? Gc.transform : null;
			}
		}
		Active(false);
	}
	//..............................................................
	public void Init(int ns, ushort sta)
	{
		now_sens = ns;
		int n = 0;
		for (int i = 0; i < 16; i++)
		{
			ushort msk = (ushort)(0x01 << i);
			r2s[i] = ((msk & sta) == msk) ? n++ : -1;
		}
		if (n != now_sens) Debug.Log("Err.. ns != now_sens ... " + n + " : " + now_sens);
		//
		Cal.Init(h_name, r2s);
		//
		for (int i = 0; i < n_fing; i++)
		{
			string[] str = s2h[i].Split(' ');
			for (int j = 0; j < n_bone; j++)
			{
				f2r[i, j] = ((j < str.Length) && !str[j].Equals("x")) ? int.Parse(str[j]) : -1;
			}
		}
	}
	//..............................................................
	public bool LeapRdy() { return (Leap.ready); }
	//..............................................................
	public Vector3 LeapRoot() { return (100f * Leap.Root); }
	//..............................................................
	public void SetOff()
	{
		if (false)
		{
			Vector3 Vc = Leap.B[2, 0].Direction;
			Vc.y = 0f;
			Vc.z = 0f;
			Quaternion Qci = Quaternion.Inverse(Quaternion.LookRotation(Vc, Vector3.forward));
			for (int i = 0; i < n_fing; i++)
			{
				for (int j = 0; j < n_bone; j++)
				{
					int n = f2r[i, j];
					if ((n >= 0) && (Leap.B[i, j] != null))
					{
						Vc = Leap.B[i, j].Direction;
						Vc.y = 0f;
						Vc.z = 0f;
						Qoff[n] = Quaternion.Inverse(Qci * Quaternion.LookRotation(Vc, Vector3.forward));
					}
				}
			}
		}
		else
		{
			for (int i = 0; i < 16; i++)
			{
				Qoff[i] = Quaternion.Euler(offAng[i]);
			}
		}
	}
	//..............................................................
	public void Active(bool flg) { _SetVisible(this.gameObject, flg); }
	//..............................................................
	public void Calibrate(long sf, long df, IMUPAR[,] P)
	{
		Cal.Record(sf, df, P);
		Cal.Calib(Qoff);
	}
	//..............................................................
	public void Adjust(int mode, IMUPAR[] P)
	{
		if (mode == 0) Cal.MemPose(P);      // Save Pose Data
		else if (mode == 1) Cal.AdjPose(P);     // Adjust by Pose Data
		else if (mode == 2) Cal.TrimQi(P, Qoff);    // Trim by Leap Finger
		else if (mode == 3) Cal.LoadQiop(h_name);   // Qio Load
	}
	//..............................................................
	public void Draw(long nf, IMUPAR[,] Q, Vector3 Pc)
	{
		Quaternion[] Qs = new Quaternion[16];	//キャリブレーション済みの絶対回転?
		Quaternion[] Qd = new Quaternion[16];	//キャリブレーション済みの絶対回転から計算される相対回転?
		Cal.GetQd(nf, Q, Qs);
		_Layer(Qs, Qd);
		//
		Tr[2, 0].localPosition = Pc;
		for (int i = 0; i < n_fing; i++)
		{
			for (int j = 0; j < n_bone; j++)
			{
				int n = f2r[i, j];
				if (n >= 0) Tr[i, j].localRotation = Qd[n];
			}
		}
	}
	//..............................................................
	public Quaternion[] CalQi() { return (Cal.Qi); }
	//..............................................................
	public Quaternion[] CalQo() { return (Cal.Qo); }
	//..............................................................
	private void _Layer(Quaternion[] Qs, Quaternion[] Qd)
	{
		Quaternion Q9 = Qs[9];
		for (int i = 0; i < n_fing; i++)
		{
			for (int j = 1; j < n_bone; j++)
			{
				int k = n_bone - j;
				int n0 = f2r[i, k];
				if (n0 >= 0)
				{
					int n1 = f2r[i, (k - 1)];
					Quaternion Q0 = Qs[n0];
					Quaternion Q1 = (n1 >= 0) ? Qs[n1] : (k == 1) ? Q9 : Quaternion.identity;
					Qd[n0] = Quaternion.Inverse(Q1) * Q0;
				}
			}
		}
		Qd[9] = Q9;
	}
	//..............................................................
	private void _SetVisible(GameObject Go, bool flg)
	{
		if (Go.GetComponent<MeshRenderer>()) Go.GetComponent<MeshRenderer>().enabled = flg;
		int n = Go.transform.childCount;
		if (n < 1) return;
		//
		for (int i = 0; i < n; i++)
		{
			GameObject Gc = Go.transform.GetChild(i).gameObject;
			_SetVisible(Gc, flg);
		}
	}
	//..............................................................
	private GameObject _Find(string nam, GameObject Gc)
	{
		if (nam == Gc.name) return (Gc);
		//
		int n = Gc.transform.childCount;
		if (n > 0)
		{
			GameObject Gcc;
			for (int i = 0; i < n; i++)
			{
				Gcc = _Find(nam, Gc.transform.GetChild(i).gameObject);
				if (Gcc != null) return (Gcc);
			}
		}
		//
		return (null);
	}
	//..............................................................
	private void _TraceObjectTree(GameObject Go)
	{
		int n = Go.transform.childCount;
		if (n < 1) return;
		//
		for (int i = 0; i < n; i++)
		{
			GameObject Gc = Go.transform.GetChild(i).gameObject;
			_TraceObjectTree(Gc);
		}
	}
};
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
