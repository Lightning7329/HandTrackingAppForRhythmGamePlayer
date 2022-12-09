
//==============================================================
//
// Hand Motion Tracking System ver.3
// Motion Capture Data Management Class
//
//--------------------------------------------------------------
// (C)Copyright Allrights reserved by Sakai Shigekazu 2022 -
//..............................................................
using System;
using System.IO;
using UnityEngine;
//--------------------------------------------------------------
namespace SS_KinetrackIII
{
	/// <summary>
	/// Motion control unit
	/// </summary>
	public struct MCUPAR
	{
		/// <summary>
		/// error, フレーム欠落を判断するためのcounter, deltatime(フレーム間の時間差)
		/// </summary>
		public ushort er, ct, dt;

		/// <summary>
		/// LeapMotionを使っているかどうかのフラグを入れておく。
		/// </summary>
		public short n0, n1;

		/// <summary>
		/// LeapMotionのpositionとかなんかの加速度入れたりできるオプション。
		/// </summary>
		public Vector3 p0, p1;
	};
	//..............................................................

	public struct IMUPAR
	{
		/// <summary>
		/// 時間。記録した時刻
		/// </summary>
		public ushort t;
		/// <summary>
		/// Quaternion用
		/// </summary>
		public short w, x, y, z;
	};
	//..............................................................
	/// <summary>
	/// 任意のタイミングで打つキャリブレーション用のキーフレームの情報
	/// </summary>
	public struct KEYPAR
	{
		/// <summary>
		/// now frame, start frame, end frame
		/// </summary>
		public long nf, sf, ef;

		/// <summary>
		/// 補完のタイプ。今は1だけ。
		/// </summary>
		public int typ;

		/// <summary>
		/// キャリブレーション用のクォータニオン。インバース（電源を入れたときの回転。あとで相殺する）とオフセット。
		/// </summary>
		public Quaternion[] Qi, Qo;
	};
	//--------------------------------------------------------------
	public class SS_DAT
	{
		public long now_rec, now_key;

		/// <summary>
		/// frame * sensor
		/// </summary>
		public IMUPAR[,] Pim;

		/// <summary>
		/// 
		/// </summary>
		public MCUPAR[] Pmc;

		/// <summary>
		/// 手動で作るから適当な個数。
		/// </summary>
		public KEYPAR[] Key;
		private long max_rec, max_key;

		/// <summary>
		/// 生きてるセンサーを各ビットに入れてある
		/// </summary>
		private ushort sta;

		/// <summary>
		/// センサーのフレームレート
		/// </summary>
		private float fps;

		/// <summary>
		/// センサーの個数
		/// </summary>
		private int now_sens;

		/// <summary>
		/// 記録中かどうか
		/// </summary>
		private bool flg_rec;
		//..............................................................
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mf">max frame</param>
		/// <param name="ms">sensor</param>
		/// <param name="mk">max key</param>
		/// <param name="stat">status from imu</param>
		/// <param name="fs">frame rate</param>
		public void Init(long mf, int ms, long mk, ushort stat, float fs)
		{
			Free();
			max_rec = mf; now_rec = 0;
			now_sens = ms;
			max_key = mk; now_key = 0;
			sta = stat;
			fps = fs;
			Pmc = new MCUPAR[max_rec];
			Pim = new IMUPAR[max_rec, now_sens];
			Key = new KEYPAR[max_key];
		}
		//..............................................................
		private void Free()
		{
			if (Key != null) Key = null;
			if (Pim != null) Pim = null;
			if (Pmc != null) Pmc = null;
			max_key = 0; now_key = 0;
			now_sens = 0;
			max_rec = 0; now_rec = 0;
		}
		//..............................................................
		// IMU Data Recording Functions
		//..............................................................
		public void SetRecFlg(bool flg)
		{
			flg_rec = flg;
		}
		//..............................................................
		/// <summary>
		/// 一フレーム当たりのデータ
		/// flg_recがtrueのときだけ記録、それ以外は同じところに上書き。
		/// 常に呼ばれる。
		/// </summary>
		/// <param name="Im">センサーの個数分のデータ</param>
		/// <param name="Mc"></param>
		public void Rec(IMUPAR[] Im, MCUPAR Mc)
		{
			if (flg_rec) now_rec++;
			long nf = now_rec % max_rec;
			for (int i = 0; i < now_sens; i++)
			{
				Pim[nf, i] = Im[i];
			}
			Pmc[nf] = Mc;
		}
		//..............................................................
		public long GetNowRec() { return (now_rec); }
		//..............................................................
		public void SetNowRec(long n) { now_rec = n; }
		//..............................................................
		public void ClearRec() { now_rec = 0; }
		//..............................................................
		// IMU Calibration Data Functions
		//..............................................................
		/// <summary>
		/// キャリブレーション時に呼び出してそのときの回転とかを入れる
		/// </summary>
		/// <param name="nf">そのときのフレーム</param>
		/// <param name="Qi"></param>
		/// <param name="Qo"></param>
		public void AddKey(long nf, Quaternion[] Qi, Quaternion[] Qo)
		{
			if ((now_key > 1) && (Key[now_key - 2].nf == nf))
			{
				Key[now_key - 1].Qi = Qi;
				Key[now_key - 1].Qo = Qo;
			}
			else
			{
				Key[now_key].typ = 1;
				Key[now_key].nf = nf;
				Key[now_key].sf = 0;
				Key[now_key].ef = 0;
				Key[now_key].Qi = Qi;
				Key[now_key].Qo = Qo;
				now_key++;
			}
		}
		//..............................................................
		public void ClearKey()
		{
			for (long i = (now_key - 1); i >= 0; i--)
			{
				Key[i].Qo = null;
				Key[i].Qi = null;
			}
		}
		//..............................................................
		// Data Save Functions
		//..............................................................
		public bool Save(string fnam)
		{
			byte[] buf = new byte[256];
			try
			{
				FileStream fs = new FileStream((fnam + ".imq"), FileMode.Create, FileAccess.Write);
				// save Header
				int n = _SetHdr(buf, 0);
				fs.Write(buf, 0, n);
				// save IMU data
				for (long i = 0; i < now_rec; i++)
				{
					n = _SetPmc(Pmc[i], buf, 0);
					for (int j = 0; j < now_sens; j++)
					{
						n = _SetPim(Pim[i, j], buf, n);
					}
					n = _SetOpt(Pmc[i], buf, n);
					fs.Write(buf, 0, n);
				}
				// save Qio data
				for (int i = 0; i < now_key; i++)
				{
					n = _SetKeyP(Key[i], buf, 0);
					fs.Write(buf, 0, n);
					for (int j = 0; j < now_sens; j++)
					{
						n = _SetKeyQ(Key[i], j, buf, 0);
						fs.Write(buf, 0, n);
					}
				}
				//
				fs.Close();
				buf = null;
			}
			catch (IOException e)
			{
				Debug.Log(e);
				return (false);
			}
			//
			return (true);
		}
		//..............................................................
		/// <summary>
		/// ヘッダー情報の記録
		/// </summary>
		/// <param name="bc">バッファ？</param>
		/// <param name="n">バッファへの書き込み位置</param>
		/// <returns></returns>
		private int _SetHdr(byte[] bc, int n)
		{
			_SetByte(BitConverter.GetBytes((long)now_rec), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((short)now_sens), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((ushort)sta), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((int)now_key), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)fps), bc, n, 4); n += 4;
			//
			return (n);
		}
		//..............................................................
		private int _SetPmc(MCUPAR Mc, byte[] bc, int n)
		{
			_SetByte(BitConverter.GetBytes((ushort)Mc.er), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((ushort)Mc.ct), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((ushort)Mc.dt), bc, n, 2); n += 2;
			//
			return (n);
		}
		//..............................................................
		private int _SetPim(IMUPAR Mc, byte[] bc, int n)
		{
			_SetByte(BitConverter.GetBytes((short)Mc.t), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((short)Mc.w), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((short)Mc.x), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((short)Mc.y), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((short)Mc.z), bc, n, 2); n += 2;
			//
			return (n);
		}
		//..............................................................
		private int _SetOpt(MCUPAR Mc, byte[] bc, int n)
		{
			_SetByte(BitConverter.GetBytes((short)Mc.n0), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((float)Mc.p0.x), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)Mc.p0.y), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)Mc.p0.z), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((short)Mc.n1), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((float)Mc.p1.x), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)Mc.p1.y), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)Mc.p1.z), bc, n, 4); n += 4;
			//
			return (n);
		}
		//..............................................................
		private int _SetKeyP(KEYPAR Kc, byte[] bc, int n)
		{
			_SetByte(BitConverter.GetBytes((int)Kc.typ), bc, n, 2); n += 2;
			_SetByte(BitConverter.GetBytes((long)Kc.nf), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((long)Kc.sf), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((long)Kc.ef), bc, n, 4); n += 4;
			//
			return (n);
		}
		//..............................................................
		private int _SetKeyQ(KEYPAR Kc, int j, byte[] bc, int n)
		{
			Quaternion Qc = Kc.Qi[j];
			_SetByte(BitConverter.GetBytes((float)Qc.w), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)Qc.x), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)Qc.y), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)Qc.z), bc, n, 4); n += 4;
			Qc = Kc.Qo[j];
			_SetByte(BitConverter.GetBytes((float)Qc.w), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)Qc.x), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)Qc.y), bc, n, 4); n += 4;
			_SetByte(BitConverter.GetBytes((float)Qc.z), bc, n, 4); n += 4;
			//
			return (n);
		}
		//..............................................................
		private void _SetByte(byte[] bc, byte[] buf, int off, int n)
		{
			for (int i = 0; i < n; i++)
			{
				buf[off] = bc[i];
				off++;
			}
		}
		//..............................................................
		// Data Load Functions
		//..............................................................
		public bool Load(string fnam)
		{
			byte[] buf = new byte[256];
			try
			{
				FileStream fs = new FileStream((fnam + ".imq"), FileMode.Open, FileAccess.Read);
				// load header
				fs.Read(buf, 0, 16);
				int n = 0;
				Init((now_rec + 16), now_sens, now_key, sta, fps);
				// load IMU data
				for (int i = 0; i < now_rec; i++)
				{
					fs.Read(buf, 0, 6);
					n = _GetPmc(i, buf, 0);
					for (int j = 0; j < now_sens; j++)
					{
						fs.Read(buf, 0, 10);
						n = _GetPim(i, j, buf, 0);
					}
					fs.Read(buf, 0, 28);
					n = _GetOpt(i, buf, 0);
				}
				// load Qio data
				for (int i = 0; i < now_key; i++)
				{
					fs.Read(buf, 0, 12);
					_GetKeyP(i, buf, 0);
					Key[i].Qi = new Quaternion[now_sens];
					Key[i].Qo = new Quaternion[now_sens];
					for (int j = 0; j < now_sens; j++)
					{
						fs.Read(buf, 0, 16);
						_GetKeyQ(i, j, buf, 0);
					}
				}
				// close
				fs.Close();
				buf = null;
			}
			catch (IOException e)
			{
				Debug.Log(e);
				return (false);
			}
			//
			return (true);
		}
		//..............................................................
		private int _GetHdr(byte[] buf, int n)
		{
			long nr = BitConverter.ToInt32(buf, n); n += 4;
			short ns = BitConverter.ToInt16(buf, n); n += 2;
			ushort st = BitConverter.ToUInt16(buf, n); n += 2;
			int nk = BitConverter.ToInt16(buf, n); n += 4;
			float fp = BitConverter.ToSingle(buf, n); n += 4;
			//
			now_rec = nr;
			now_sens = ns;
			sta = st;
			now_key = nk;
			fps = fp;
			//
			return (n);
		}
		//..............................................................
		private int _GetPmc(int i, byte[] buf, int n)
		{
			MCUPAR Mc = new MCUPAR();
			Mc.er = BitConverter.ToUInt16(buf, n); n += 2;
			Mc.ct = BitConverter.ToUInt16(buf, n); n += 2;
			Mc.dt = BitConverter.ToUInt16(buf, n); n += 2;
			//		Mc.n0 = Pmc[i].n0;
			//		Mc.p0 = Pmc[i].p0;
			//		Mc.n1 = Pmc[i].n1;
			//		Mc.p1 = Pmc[i].p1;

			Pmc[i] = Mc;
			//
			return (n);
		}
		//..............................................................
		private int _GetPim(long i, int j, byte[] buf, int n)
		{
			IMUPAR Ic = new IMUPAR();
			Ic.t = BitConverter.ToUInt16(buf, n); n += 2;
			Ic.w = BitConverter.ToInt16(buf, n); n += 2;
			Ic.x = BitConverter.ToInt16(buf, n); n += 2;
			Ic.y = BitConverter.ToInt16(buf, n); n += 2;
			Ic.z = BitConverter.ToInt16(buf, n); n += 2;

			if (Ic.t != Pim[i, j].t) Debug.Log("Err.. Ic.t");
			if (Ic.w != Pim[i, j].w) Debug.Log("Err.. Ic.w");
			if (Ic.x != Pim[i, j].x) Debug.Log("Err.. Ic.x");
			if (Ic.y != Pim[i, j].y) Debug.Log("Err.. Ic.y");
			if (Ic.z != Pim[i, j].z) Debug.Log("Err.. Ic.z");

			Pim[i, j] = Ic;
			//
			return (n);
		}
		//..............................................................
		private int _GetOpt(int i, byte[] buf, int n)
		{
			MCUPAR Mc = new MCUPAR();
			Mc.n0 = BitConverter.ToInt16(buf, n); n += 2;
			Mc.p0.x = BitConverter.ToSingle(buf, n); n += 4;
			Mc.p0.y = BitConverter.ToSingle(buf, n); n += 4;
			Mc.p0.z = BitConverter.ToSingle(buf, n); n += 4;

			Mc.n1 = BitConverter.ToInt16(buf, n); n += 2;
			Mc.p1.x = BitConverter.ToSingle(buf, n); n += 4;
			Mc.p1.y = BitConverter.ToSingle(buf, n); n += 4;
			Mc.p1.z = BitConverter.ToSingle(buf, n); n += 4;

			//		if (Mc.n0 != Pmc[i].n0)	Debug.Log(String.Format("Err.. Mc.n0 = {0} .. ({1})", Mc.n0, Pmc[i].n0));
			//		if (Mc.p0 != Pmc[i].p0)	Debug.Log(String.Format("Err.. Mc.p0 = {0} .. ({1})", Mc.p0, Pmc[i].p0));
			//		if (Mc.n1 != Pmc[i].n1)	Debug.Log(String.Format("Err.. Mc.n1 = {0} .. ({1})", Mc.n1, Pmc[i].n1));
			//		if (Mc.p1 != Pmc[i].p1)	Debug.Log(String.Format("Err.. Mc.p1 = {0} .. ({1})", Mc.p1, Pmc[i].p1));

			Pmc[i].n0 = Mc.n0;
			Pmc[i].p0 = Mc.p0;
			Pmc[i].n1 = Mc.n1;
			Pmc[i].p1 = Mc.p1;
			//
			return (n);
		}
		//..............................................................
		private int _GetKeyP(long i, byte[] buf, int n)
		{
			KEYPAR Kc = new KEYPAR();
			Kc.typ = BitConverter.ToInt16(buf, n); n += 2;
			Kc.nf = BitConverter.ToInt32(buf, n); n += 4;
			Kc.sf = BitConverter.ToInt32(buf, n); n += 4;
			Kc.ef = BitConverter.ToInt32(buf, n); n += 4;

			//		if (Kc.typ != Key[i].typ)	Debug.Log("Err.. Kc.typ");
			//		if (Kc.nf != Key[i].nf)		Debug.Log("Err.. Kc.nf");
			//		if (Kc.sf != Key[i].sf)		Debug.Log("Err.. Kc.sf");
			//		if (Kc.ef != Key[i].ef)		Debug.Log("Err.. Kc.ef");
			//
			return (n);
		}
		//..............................................................
		private int _GetKeyQ(long i, int j, byte[] buf, int n)
		{
			Quaternion Qc = new Quaternion();
			Qc.w = BitConverter.ToSingle(buf, n); n += 4;
			Qc.x = BitConverter.ToSingle(buf, n); n += 4;
			Qc.y = BitConverter.ToSingle(buf, n); n += 4;
			Qc.z = BitConverter.ToSingle(buf, n); n += 4;
			//		if (Qc != Key[i].Qi[j])	Debug.Log(String.Format("Err.. Kc.Qi ({0}) .. ({1})", Qc, Key[i].Qi[j]));
			Key[i].Qi[j] = Qc;
			//
			Qc.w = BitConverter.ToSingle(buf, n); n += 4;
			Qc.x = BitConverter.ToSingle(buf, n); n += 4;
			Qc.y = BitConverter.ToSingle(buf, n); n += 4;
			Qc.z = BitConverter.ToSingle(buf, n); n += 4;
			//		if (Qc != Key[i].Qo[j])	Debug.Log(String.Format("Err.. Kc.Qi ({0}) .. ({1})", Qc, Key[i].Qo[j]));
			Key[i].Qo[j] = Qc;
			//
			return (n);
		}
		//..............................................................
		// Test File format Functions
		//..............................................................
		public void DatGen()
		{
			long nr = 100;
			int ns = 16;
			ushort st = 12345;
			int nk = 30;
			float fp = 50.0f;
			//
			Init(nr, ns, nk, st, fp);
			//
			SetRecFlg(true);
			short ccc = 0;
			for (long i = 0; i < nr; i++)
			{
				MCUPAR Mc = new MCUPAR();
				Mc.er = 10101;
				Mc.ct = (ushort)i;
				Mc.dt = 15;

				Mc.n0 = (short)i;
				Mc.p0 = new Vector3(1f, 2f, -(float)i);
				Mc.n1 = (short)(i + 1);
				Mc.p1 = new Vector3(3f, 4f, -(float)i);

				IMUPAR[] Ic = new IMUPAR[ns];
				for (int j = 0; j < ns; j++)
				{
					Ic[j].t = (ushort)ccc; ccc++;
					Ic[j].w = ccc; ccc++;
					Ic[j].x = ccc; ccc++;
					Ic[j].y = ccc; ccc++;
					Ic[j].z = ccc; ccc++;
				}
				Rec(Ic, Mc);
			}
			//
			for (int i = 0; i < nk; i++)
			{
				Quaternion[] K_Qi = new Quaternion[ns];
				Quaternion[] K_Qo = new Quaternion[ns];
				for (int j = 0; j < ns; j++)
				{
					K_Qi[j] = Quaternion.Euler((float)i, (float)j, 0f);
					K_Qo[j] = Quaternion.Euler(-(float)i, -(float)j, 0f);
				}
				AddKey(i, K_Qi, K_Qo);
			}
		}
		//..............................................................
		public void SaveTest()
		{
			Save("./mcdata/test");
		}
		//..............................................................
		public bool LoadTest()
		{
			string fnam = "./mcdata/test";
			byte[] buf = new byte[256];
			try
			{
				FileStream fs = new FileStream((fnam + ".imq"), FileMode.Open, FileAccess.Read);
				fs.Read(buf, 0, 16);

				int n = 0;
				long nr = BitConverter.ToInt32(buf, n); n += 4;
				short ns = BitConverter.ToInt16(buf, n); n += 2;
				ushort st = BitConverter.ToUInt16(buf, n); n += 2;
				int nk = BitConverter.ToInt32(buf, n); n += 4;
				float fp = BitConverter.ToSingle(buf, n); n += 4;

				if (nr != now_rec) Debug.Log(String.Format("Err nr({0}) != now_rec({1})", nr, now_rec));
				if (ns != now_sens) Debug.Log(String.Format("Err ns({0}) != now_sens({1})", ns, now_sens));
				if (st != sta) Debug.Log(String.Format("Err st({0}) != stat({1})", st, sta));
				if (nk != now_key) Debug.Log(String.Format("Err nk({0}) != now_key({1})", nk, now_key));
				if (fp != fps) Debug.Log(String.Format("Err fp({0}) != fps({1})", fp, fps));
				//
				for (int i = 0; i < nr; i++)
				{
					fs.Read(buf, 0, 6);
					_GetPmc(i, buf, 0);
					for (int j = 0; j < ns; j++)
					{
						fs.Read(buf, 0, 10);
						_GetPim(i, j, buf, 0);
					}
					fs.Read(buf, 0, 28);
					_GetOpt(i, buf, 0);
				}
				//
				for (int i = 0; i < nk; i++)
				{
					fs.Read(buf, 0, 14);
					_GetKeyP(i, buf, 0);
					for (int j = 0; j < ns; j++)
					{
						fs.Read(buf, 0, 32);
						_GetKeyQ(i, j, buf, 0);
					}
				}
				fs.Close();
				buf = null;
			}
			catch (IOException e)
			{
				Debug.Log(e);
				return (false);
			}
			//
			return (true);
		}
	};
}
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
