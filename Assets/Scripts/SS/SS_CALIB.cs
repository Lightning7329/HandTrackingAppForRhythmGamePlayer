
//==============================================================
//
// Hand Motion Tracking System ver.3
// IMU Data Calibration Class
//
//--------------------------------------------------------------
// (C)Copyright Allrights reserved by Sakai Shigekazu 2022 -
//...............................................................
using UnityEngine ;
//--------------------------------------------------------------
namespace SS_KinetrackIII
{
	public class SS_CALIB
	{
		public Quaternion[] Qi = new Quaternion[16];
		public Quaternion[] Qo = new Quaternion[16];
		public Quaternion[] Qp = new Quaternion[16];
		public Quaternion[,] Qs;    // recording data for calibration
		private const float _Scl = 1f / 16384f;
		private int cal_size;   // frames for calibration
		private Vector3 ang_d;
		private Vector3 ang_s, ang_e;
		private string fnam;
		private int[] r2s = new int[16];
		//...............................................................
		public void Init(string name, int[] tbl)
		{
			fnam = name;
			//
			for (int i = 0; i < 16; i++)
			{
				Qi[i] = Quaternion.identity;
				Qo[i] = Quaternion.identity;
				Qp[i] = Quaternion.identity;
			}
			//
			for (int i = 0; i < 16; i++)
			{
				if (i < tbl.Length) r2s[i] = tbl[i];
			}
		}
		//...............................................................
		/// <summary>
		/// sfからncフレーム分の16個のIMUPARを使ってQsを作成する
		/// </summary>
		/// <param name="sf">start frame</param>
		/// <param name="nc">number of calibration</param>
		/// <param name="P"></param>
		public void Record(long sf, long nc, IMUPAR[,] P)
		{
			cal_size = (int)nc;
			if (Qs != null) Qs = null;
			Qs = new Quaternion[cal_size, 16];
			//
			for (int i = 0; i < cal_size; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					int n = r2s[j];
					if (n >= 0)
					{
						IMUPAR Pc = P[sf, n];
						Qs[i, j] = new Quaternion(_Scl * (float)Pc.x, _Scl * (float)Pc.y, _Scl * (float)Pc.z, _Scl * (float)Pc.w);
					}
					else
					{
						Qs[i, j] = Quaternion.identity;
					}
				}
				sf++;
			}
		}
		//...............................................................
		public void MemPose(IMUPAR[] P)
		{
			for (int i = 0; i < 16; i++)
			{
				int n = r2s[i];
				if (n >= 0)
				{
					IMUPAR Pc = P[n];
					Quaternion Qc = new Quaternion(_Scl * (float)Pc.x, _Scl * (float)Pc.y, _Scl * (float)Pc.z, _Scl * (float)Pc.w);
					Qp[i] = Qi[i] * Qc * Qo[i];
				}
				else
				{
					Qp[i] = Quaternion.identity;
				}
			}
			_SaveQiop(fnam);
		}
		//...............................................................
		public void AdjPose(IMUPAR[] P)
		{
			for (int i = 0; i < 16; i++)
			{
				int n = r2s[i];
				if (n >= 0)
				{
					IMUPAR Pc = P[n];
					Quaternion Qc = new Quaternion(_Scl * (float)Pc.x, _Scl * (float)Pc.y, _Scl * (float)Pc.z, _Scl * (float)Pc.w);
					Quaternion Qcc = Qi[i] * Qc * Qo[i];
					Quaternion dQ = Qp[i] * Quaternion.Inverse(Qcc);
					Qi[i] = dQ * Qi[i];
				}
				else
				{
					Qi[i] = Quaternion.identity;
				}
			}
		}
		//...............................................................
		public void AdjLeap(IMUPAR[] P, Quaternion[] Ql)
		{
			for (int i = 0; i < 16; i++)
			{
				int n = r2s[i];
				if (n >= 0)
				{
					IMUPAR Pc = P[n];
					Quaternion Qc = new Quaternion(_Scl * (float)Pc.x, _Scl * (float)Pc.y, _Scl * (float)Pc.z, _Scl * (float)Pc.w);
					Quaternion Qcc = Qi[i] * Qc * Qo[i];
					Quaternion dQ = Ql[i] * Quaternion.Inverse(Qcc);
					Qi[i] = dQ * Qi[i];
				}
				else
				{
					Qi[i] = Quaternion.identity;
				}
			}
		}
		//...............................................................
		public void TrimQi(IMUPAR[] P, Quaternion[] Qoff)
		{
			for (int i = 0; i < 16; i++)
			{
				int n = r2s[i];
				if (n >= 0)
				{
					IMUPAR Pc = P[n];
					Quaternion Qc = new Quaternion(_Scl * (float)Pc.x, _Scl * (float)Pc.y, _Scl * (float)Pc.z, _Scl * (float)Pc.w);
					Qi[i] = Quaternion.Inverse(Qc * Qoff[i] * Qo[i]);
				}
				else
				{
					Qi[i] = Quaternion.identity;
				}
			}
		}
		//...............................................................
		/// <summary>
		/// 与えられた各関節の回転に合わせてキャリブレーションをする。
		/// 事前にRecordメソッドを呼んでおく必要あり。
		/// </summary>
		/// <param name="Qoff"></param>
		public void Calib(Quaternion[] Qoff)
		{
			_InitQio(Qoff);
			for (int i = 0; i < 16; i++)
			{
				if (i != 9)
				{
					ang_d = new Vector3(30f, 30f, 30f);
					ang_e = new Vector3(180, 180, 180);
					ang_s = -ang_e;
					for (int j = 0; j < 7; j++)
					{
						_SetQi(i);
						ang_d *= 0.5f;
						ang_s = -ang_d;
						ang_e = ang_d;
					}
					Qo[i] = _SetQo(i);
				}
			}
			_SaveQiop(fnam);
		}
		//...............................................................
		public void GetQd(long nf, IMUPAR[,] P, Quaternion[] Qd)
		{
			for (int i = 0; i < 16; i++)
			{
				int n = r2s[i];
				if (n >= 0)
				{
					IMUPAR Pc = P[nf, n];
					Quaternion Qc = new Quaternion(_Scl * (float)Pc.x, _Scl * (float)Pc.y, _Scl * (float)Pc.z, _Scl * (float)Pc.w);
					Qd[i] = Qi[i] * Qc * Qo[i];
				}
			}
		}
		//...............................................................
		private void _InitQio(Quaternion[] Qoff)
		{
			for (int i = 0; i < 16; i++)
			{
				Qi[i] = Quaternion.Inverse(Qs[0, i] * Qoff[i]);
				Qo[i] = Qoff[i];
			}
		}
		//...............................................................
		private void _SetQi(int ns)
		{
			Quaternion Qic;
			float u_max = 0f;
			Vector3 ang = new Vector3();
			Vector3 a_max = Vector3.zero;
			//
			Qic = Qi[ns];
			for (ang.z = ang_s.z; ang.z <= ang_e.z; ang.z += ang_d.z)
			{
				for (ang.x = ang_s[0]; ang.x <= ang_e[0]; ang.x += ang_d.x)
				{
					for (ang.y = ang_s[1]; ang.y <= ang_e[1]; ang.y += ang_d.y)
					{
						Qi[ns] = Quaternion.Euler(ang) * Qic;
						float u = _FinDir(ns);
						if (u > u_max)
						{
							u_max = u;
							a_max = ang;
						}
					}
				}
			}
			Qi[ns] = Quaternion.Euler(a_max) * Qic;
		}
		//...............................................................
		private float _FinDir(int ns)
		{
			Vector3[] Vs = new Vector3[cal_size];
			Vector3 Vc = Vector3.zero;
			for (long i = 0; i < cal_size; i++)
			{
				Quaternion Qr = Qi[9] * Qs[i, 9] * Qo[9];
				Quaternion Qc = Qi[ns] * Qs[i, ns];
				Vs[i] = (Quaternion.Inverse(Qr) * Qc) * Vector3.forward;
				Vc += Vs[i];
			}
			Vector3 Vr = Vector3.Normalize(Vc);
			//
			float u = 0f;
			for (long i = 0; i < cal_size; i++)
			{
				float a = Vector3.Dot(Vs[i], Vr);
				if (a >= 0f) u += a;
			}
			//
			return (u);
		}
		//...............................................................
		private Quaternion _SetQo(int ns)
		{
			Quaternion Qc, Qr;
			Vector3 Vu = Vector3.zero;
			Vector3 Vf = Vector3.zero;
			for (long i = 0; i < cal_size; i++)
			{
				Qr = Quaternion.Inverse(Qi[9] * Qs[i, 9] * Qo[9]);
				Qc = Qr * Qi[ns] * Qs[i, ns] * Qo[ns];
				Vu += (Qc * Vector3.up);
				Vf += (Qc * Vector3.forward);
			}
			//
			return (Quaternion.Inverse(Quaternion.LookRotation(Vf.normalized, Vu.normalized)));
		}
		//...............................................................
		public bool LoadQiop(string nam)
		{
			string str = PlayerPrefs.GetString((nam + "_Qiop"), "NG");
			bool flg = !str.Equals("NG");
			if (flg)
			{
				string[] dat = str.Split();
				int n = dat.Length;
				float[] p = new float[n];
				for (int i = 0; i < n; i++)
				{
					p[i] = float.Parse(dat[i]);
				}
				int j = 0;
				Quaternion Qc = new Quaternion();
				for (int i = 0; i < 16; i++)
				{
					Qc.w = p[j]; j++;
					Qc.x = p[j]; j++;
					Qc.y = p[j]; j++;
					Qc.z = p[j]; j++;
					Qi[i] = Qc;

					Qc.w = p[j]; j++;
					Qc.x = p[j]; j++;
					Qc.y = p[j]; j++;
					Qc.z = p[j]; j++;
					Qo[i] = Qc;

					Qc.w = p[j]; j++;
					Qc.x = p[j]; j++;
					Qc.y = p[j]; j++;
					Qc.z = p[j]; j++;
					Qp[i] = Qc;
				}
			}

			return (flg);
		}
		//...............................................................
		private void _SaveQiop(string nam)
		{
			string str = "";
			for (int i = 0; i < 16; i++)
			{
				str += (Qi[i].w.ToString() + " " +
					Qi[i].x.ToString() + " " +
					Qi[i].y.ToString() + " " +
					Qi[i].z.ToString() + " " +

					Qo[i].w.ToString() + " " +
					Qo[i].x.ToString() + " " +
					Qo[i].y.ToString() + " " +
					Qo[i].z.ToString() + " " +

					Qp[i].w.ToString() + " " +
					Qp[i].x.ToString() + " " +
					Qp[i].y.ToString() + " " +
					Qp[i].z.ToString());

				if (i < 15) str += " ";
			}
			PlayerPrefs.SetString((nam + "_Qiop"), str);
			str = null;
		}
	};
}
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
