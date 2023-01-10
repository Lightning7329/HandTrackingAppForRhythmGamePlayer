
//==============================================================
//
// Hand Motion Tracking System ver.3
// IMU Data Controll Class
//
//--------------------------------------------------------------
// (C)Copyright Allrights reserved by Sakai Shigekazu 2022 -
//...............................................................
using System ;
using System.Threading ;
using UnityEngine ;
//--------------------------------------------------------------
namespace SS_KinetrackIII
{
	public class SS_IMU
	{
		public bool flg_ready = false;

		/// <summary>
		/// ＰＣの方が早い。IMUからデータが来ていたら読むようにしたいから、もう来てるかどうかを入れる。
		/// </summary>
		public bool flg_setdat = false;

		/// <summary>
		/// 
		/// </summary>
		public bool flg_auto = false;
		public int now_sens;
		public ushort stat;
		public float vbt, fps;
		public MCUPAR Pmc;
		public IMUPAR[] Pim = new IMUPAR[16];

		/// <summary>
		/// Communication
		/// </summary>
		private SS_UDP Com = new SS_UDP();
		System.Threading.Thread rcvThread = null;
		//..............................................................
		/// <summary>
		/// Connectボタンが押されたとき呼ばれる
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		public bool Connect(string ip)
		{
			int n = 0;
			while (!Com.Open(ip))
			{
				Debug.Log("=>");
				n++;
				if (n > 3)
				{
					return (false);
				}
			}
			//
			if (!(flg_ready = Stat()))
			{
				stat = 0x0000;
				vbt = 0.0f;
				fps = 0.0f;
				Com.Close();
			}
			Com.Write('0');
			//
			return (flg_ready);
		}
		//..............................................................
		/// <summary>
		/// Readyボタンが押されたとき呼ばれる
		/// </summary>
		/// <param name="flg"></param>
		public void Ready(bool flg)
		{
			if (flg)
			{
				Stat();
				Com.Write('1');
				if (!flg_auto)
				{
					Com.Write('T'); //timer starts
					Com.Write('q'); //begin sending
					rcvThread = new System.Threading.Thread(new System.Threading.ThreadStart(_RcvAuto));
					rcvThread.Start();
				}
			}
			else
			{
				if (flg_auto)
				{
					flg_auto = false;
					Com.Write('t');
				}
				Com.Write('0');
			}
		}
		//..............................................................
		public bool Stat()
		{
			if (!flg_auto)
			{
				Com.Flush();
				if (Com.Write('P') == 1)
				{
					byte[] bc = Com.Read();
					if (bc != null)
					{
						int np = bc.Length;
						int np_c = 2 * (int)BitConverter.ToUInt16(bc, 0);
						if (np_c <= np)
						{
							stat = BitConverter.ToUInt16(bc, 2);
							now_sens = (int)BitConverter.ToUInt16(bc, 6);
							vbt = 0.01f * (float)BitConverter.ToUInt16(bc, 8);
							fps = (float)BitConverter.ToUInt16(bc, 10);
							//
							return (true);
						}
					}
				}
			}
			//
			return (false);
		}
		//..............................................................
		public void Rec(bool flg)
		{
			if (flg)
			{
				Com.Write('2'); ///録画中を示すランプ
				if (!flg_auto)
				{
					Com.Write('T');
					Com.Write('q');
					rcvThread = new System.Threading.Thread(new System.Threading.ThreadStart(_RcvAuto));
					rcvThread.Start();
				}
			}
			else
			{
				Com.Write('1');
			}
		}
		//..............................................................
		public void Close()
		{
			Ready(false); Thread.Sleep(33);
			Com.Close();
			flg_ready = false;
		}
		//..............................................................
		private void _RcvAuto()
		{
			while (DateTime.Now.Millisecond != 0) ;
			int dat_size = 6 + 5 * now_sens;
			flg_setdat = false;
			flg_auto = true;
			while (flg_auto)
			{
				if (Com.Available(dat_size * 2))
				{
					byte[] bc = Com.Read();
					if (bc != null)
					{
						int nb = (int)BitConverter.ToUInt16(bc, 0);
						if (nb >= dat_size) _SetPar(bc);
					}
				}
				else
				{
					Thread.Sleep(1);
				}
			}
			rcvThread.Abort();
		}
		//..............................................................
		private void _SetPar(byte[] buf)
		{
			int n = 4;
			Pmc.er = BitConverter.ToUInt16(buf, n); n += 2;
			int ns = BitConverter.ToInt16(buf, n); n += 2;
			Pmc.ct = (ushort)DateTime.Now.Millisecond; n += 2;
			Pmc.dt = BitConverter.ToUInt16(buf, n); n += 2;
			for (int i = 0; i < ns; i++)
			{
				Pim[i].t = BitConverter.ToUInt16(buf, n); n += 2;
				Pim[i].w = BitConverter.ToInt16(buf, n); n += 2;
				Pim[i].x = BitConverter.ToInt16(buf, n); n += 2;
				Pim[i].y = BitConverter.ToInt16(buf, n); n += 2;
				Pim[i].z = BitConverter.ToInt16(buf, n); n += 2;
			}
			flg_setdat = true;
			if (buf.Length <= n) return;
			//
			float sc = 0.01f;
			short[] s = new short[8];
			for (int i = 0; i < 8; i++)
			{
				s[i] = BitConverter.ToInt16(buf, n); n += 2;
			}
			Pmc.n0 = s[0]; Pmc.p0 = new Vector3(sc * (float)s[1], sc * (float)s[2], sc * (float)s[3]);
			Pmc.n1 = s[4]; Pmc.p1 = new Vector3(sc * (float)s[5], sc * (float)s[6], sc * (float)s[7]);
		}
	};
}
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
