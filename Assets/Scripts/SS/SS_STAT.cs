
//==============================================================
//								
// Hand Motion Tracking System ver.3
// Mcu Status Display Class
//
//--------------------------------------------------------------
// (C)Copyright Allrights reserved by Sakai Shigekazu 2022 -
//..............................................................
using UnityEngine ;
using UnityEngine.UI ;
//--------------------------------------------------------------
namespace SS_KinetrackIII
{
	public class SS_STAT
	{
		private Image[] Led = new Image[16];
		private Text Tx_par0, Tx_par1;
		private Color C_on = new Color(0.3f, 0.75f, 0.3f);
		private Color C_off = new Color(0.04f, 0.04f, 0.04f);
		private Color C_err = new Color(0.8f, 0.05f, 0.05f);
		//..............................................................
		public void Init(GameObject Go)
		{
			GameObject Gc;
			//
			Gc = Go.transform.Find("LED").gameObject;
			for (int i = 0; i < 16; i++)
			{
				Led[i] = Gc.transform.Find("S" + i.ToString()).GetComponent<Image>();
				Led[i].color = C_off;
			}
			Tx_par0 = Go.transform.Find("I_p0").GetComponentInChildren<Text>();
			Tx_par1 = Go.transform.Find("I_p1").GetComponentInChildren<Text>();
		}
		//..............................................................
		public void Name(string nam)
		{
			Tx_par0.text = nam;
		}
		//..............................................................
		/// <summary>
		/// 各センサーの状態をLEDに反映。
		/// </summary>
		/// <param name="stat">SS_IMUクラスのstatフィールド</param>
		public void Stat(ushort stat)
		{
			for (int i = 0; i < 16; i++)
			{
				ushort msk = (ushort)(0x01 << i);
				if ((stat & msk) != 0x00) Led[i].color = C_on;
				else Led[i].color = C_off;
			}
		}
		//..............................................................
		public void Error(ushort err)
		{
			for (int i = 0; i < 16; i++)
			{
				ushort msk = (ushort)(0x01 << i);
				if ((err & msk) != 0x00) Led[i].color = C_err;
			}
		}
		//..............................................................
		public void FpsVolt(float fps, float v)
		{
			Tx_par0.text = string.Format("{0}fs/{1:0.00}v", fps, v);
		}
		//..............................................................
		public void Frame(string nf)
		{
			Tx_par1.text = nf;
		}
	};
}
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
