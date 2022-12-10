
//==============================================================
//
// WiFi(UDP) Data Communication Class
//
//--------------------------------------------------------------
// (C)Copyright Allrights reserved by Sakai Shigekazu 2022 -
//..............................................................
using System ;
using System.Net ;
using System.Net.Sockets ;
using System.Threading ;
using UnityEngine ;
//--------------------------------------------------------------
// UDP Host Communication
//..............................................................
namespace SS_KinetrackIII
{
	public class SS_UDP
	{
		public ushort err = 99;
		private UdpClient _udp = null;
		private IPEndPoint _end = null;
		private int port = 8888;
		//..............................................................
		public bool Open(string tgt)
		{
			if (_udp != null) _udp.Close();
			//
			try
			{
				IPAddress[] ip_tgt = Dns.GetHostAddresses(tgt);
				_end = new IPEndPoint(ip_tgt[0], port);
				_udp = new UdpClient();
				_udp.Client.ReceiveTimeout = 240;
				_udp.Connect(_end);
				Thread.Sleep(33);
				err = 0;
			}
			catch (Exception e)
			{
				err = 1;
				Debug.Log("UdpOpen() ... " + e.Message);
				Thread.Sleep(1000);
				return (false);
			}
			//
			return (true);
		}
		//..............................................................
		public void Close()
		{
			if (_udp != null) _udp.Close();
			_end = null;
			_udp = null;
		}
		//..............................................................
		public bool Available(int n)
		{
			int i = 0;
			try
			{
				while ((_udp != null) && (_udp.Available < n))
				{
					i++;
					if (i < 33) Thread.Sleep(1);
					else return (false);
				}
				return (true);
			}
			catch (System.Exception e)
			{
				Debug.Log(e);
				err = 5;
				return (false);
			}
		}
		//..............................................................
		public byte[] Read()
		{
			if (_udp == null) return (null);
			//
			try
			{
				return (_udp.Receive(ref _end));
			}
			catch (System.Exception e)
			{
				Debug.Log(e);
				err = 2;
				return (null);
			}
		}
		//..............................................................
		public int Write(byte[] buf, int n)
		{
			if (_udp == null) return (0);
			//
			try
			{
				return (_udp.Send(buf, n));
			}
			catch (System.Exception e)
			{
				Debug.Log(e);
				err = 3;
				return (0);
			}
		}
		//..............................................................
		public int Write(char c)
		{
			if (_udp == null) return (0);
			//
			try
			{
				byte[] buf = new byte[4];
				buf[0] = (byte)c;
				return (_udp.Send(buf, 1));
			}
			catch (System.Exception e)
			{
				Debug.Log(e);
				err = 4;
				return (0);
			}
		}
		//..............................................................
		public void Flush()
		{
			while (_udp.Available > 0)
			{
				_udp.Receive(ref _end);
			}

		}
	};
}
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
