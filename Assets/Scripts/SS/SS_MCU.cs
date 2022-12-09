
//==============================================================
//
// Hand Motion Tracking System ver.3
//	Mcu Controll for Recording Class
//
//--------------------------------------------------------------
// (C)Copyright Allrights reserved by Sakai Shigekazu 2022 -
//..............................................................
using System.Collections ;
using UnityEngine ;
using UnityEngine.UI ;
using SS_KinetrackIII;
//--------------------------------------------------------------
public class	SS_MCU : MonoBehaviour
{
	[SerializeField] private GameObject	Go_hand ;
	public	int		cal_sec = 3;
	public	int		cal_lev = 0;
	public	int		max_sec = 1800;	// = 30 minutes
	private	SS_DAT		Dat = new SS_DAT();
	private SS_IMU		Imu = new SS_IMU();
	private	SS_STAT		Sta = new SS_STAT();
	private SS_HAND		hand ;
	private	InputField	I_net ;
	private	Button		B_net ;
	private	Toggle		Tg_rdy ;
	private	Image		Im_rdy ;
	private	int		run_mode = -1;	// -1 NRY / 0 STBY / 1 RDY / 2 REC,CAL
//..............................................................
	private	void	Start()
	{
		GameObject	Gc  = gameObject;
		Tg_rdy = Gc.transform.Find("Tgl").GetComponent<Toggle>();
		Im_rdy = Gc.transform.Find("Tgl/Background/Checkmark").GetComponent<Image>();
		B_net  = Gc.transform.Find("Btn").GetComponent<Button>();
		I_net  = Gc.transform.Find("I_net").GetComponent<InputField>();
		B_net.onClick.AddListener(_OnBtnConnect);
		Tg_rdy.onValueChanged.AddListener(_OnBtnReady);
//
		Sta.Init(Gc);
		hand = Go_hand.GetComponent<SS_HAND>();
		Sta.Name(Go_hand.name);
//
		string	last_ip = PlayerPrefs.GetString(gameObject.name + "last_ip");
		if (last_ip.Length > 10)	I_net.text = last_ip;
		run_mode = -1;
		Tg_rdy.isOn = false;
	}
//..............................................................
	private	void	_OnBtnConnect()
	{
		if (Imu.flg_ready)
		{
			Imu.Close();
			B_net.GetComponentInChildren<Text>().text = "Connect";
		}
		else if (I_net.text.Length < 7)
		{
			I_net.text = "Wrn...Set IPaddress";
		}
		else if (Imu.Connect(I_net.text))
		{
			if (Imu.Stat())
			{
				max_sec = (max_sec < 60) ? 60 : max_sec;
				long	max_data = max_sec * (long)Imu.fps;
				long	max_key  = max_sec / 30 + 2;
				Dat.Init(max_data, Imu.now_sens, max_key, Imu.stat, Imu.fps);
				hand.Init(Imu.now_sens, Imu.stat);
				Sta.Stat(Imu.stat);
				Sta.FpsVolt(Imu.fps, Imu.vbt);
				Sta.Frame(string.Format("{0:.00}", max_sec));
				run_mode = 0;
				Im_rdy.color = (hand.LeapRdy()) ? Color.blue : Color.white;
				PlayerPrefs.SetString((gameObject.name + "last_ip"), I_net.text);
				B_net.GetComponentInChildren<Text>().text = "Close";
			}
			else
			{
				Imu.Close();
				run_mode = -1;
				Im_rdy.color = Color.magenta;
				Debug.Log("Err... " + gameObject.name + " is not ready !!");
			}
		}
	}
//..............................................................
	private	void	_OnBtnReady(bool flg)
	{
		if (run_mode >= 0)
		{
			Imu.Ready(flg);
			Sta.FpsVolt(Imu.fps, Imu.vbt);
			run_mode = (flg) ? 1 : 0;
			Im_rdy.color = (hand.LeapRdy()) ? Color.cyan : Color.green;
			hand.Active(flg);
		}
	}
//..............................................................
// System Ready
//..............................................................
	public	void	Ready()	{	_OnBtnReady(true);	}
//..............................................................
// Sensor Calibration
//..............................................................
	public	void	Calib()
	{
		if ((run_mode < 0) || (cal_lev > 0))	return;
//
		run_mode = 2;
		Im_rdy.color = Color.yellow;
		cal_lev = 1;
		hand.SetOff();
		StartCoroutine("Calibration");
	}
//..............................................................
	private	IEnumerator	Calibration()
	{
		long	hs = Dat.GetNowRec();
		long	dh = (long)(cal_sec * (int)Imu.fps);
		long	he = hs + dh;
		Dat.SetRecFlg(true);
		Imu.Rec(true);
		while(Dat.GetNowRec() < he)	yield return(null);
		Dat.SetRecFlg(false);
		Imu.Rec(false);
//
		cal_lev = 2;
		hand.Calibrate(hs, dh, Dat.Pim);
		Dat.SetNowRec(he);
		cal_lev = 0;
	}
//..............................................................
	public	void	Adjust(int mode)
	{
		if (run_mode > 0)
		{
			hand.Adjust(mode, Imu.Pim);
		}
	}
//..............................................................
// Recording Data
//..............................................................
	public	float	NowRec(float tn)
	{
		if (run_mode > 0)
		{
			float	tc = (float)Dat.now_rec / Imu.fps;
			if ((tn > tc) && (Dat.now_rec > 150))	tn = tc;
		}
//
		return(tn);
	}
//..............................................................
	public	void	ClearRec()
	{
		Dat.ClearRec();
		Dat.ClearKey();
		Sta.Frame("0");
	}
//..............................................................
	private	bool	flg_rec = false;
//..............................................................
	public	void	Rec()
	{
		if (run_mode >= 0)
		{
			Dat.AddKey(Dat.GetNowRec(), hand.CalQi(), hand.CalQo());
			flg_rec = true;
			Dat.SetRecFlg(true);
			run_mode = 2;
			Im_rdy.color = Color.red;
			hand.Active(true);
		}
	}
//..............................................................
	private	void	Update()
	{
		if (run_mode < 1)	return;
//
		if (Imu.flg_setdat)
		{
			if (hand.LeapRdy())
			{
				Imu.Pmc.n0 = 99;
				Imu.Pmc.p0 = hand.LeapRoot();
			}
			else if (hand.Pt != null)
			{
				Imu.Pmc.n0 = 99;
				Imu.Pmc.p0 = hand.Pt.localPosition;
			}
			Dat.Rec(Imu.Pim, Imu.Pmc);
			flg_newdat = true;
			Imu.flg_setdat = false;
		}
		if (flg_rec)
		{
			Imu.Rec(true);
			flg_rec = false;
		}
	}
//..............................................................
// Data Save
//..............................................................
	public	void	Save(string path)
	{
		if (run_mode >= 0)
		{
			string fnam = path + "_" + Go_hand.name;
			Dat.Save(fnam);
			Debug.Log("Save data to " + fnam);
		}
	}
//..............................................................
// Draw Hand
//..............................................................
	private bool	flg_newdat ;
//..............................................................
	public	void	Draw(float nf)
	{
		if ((run_mode < 1) || (!flg_newdat && (nf < 0f)))	return;
//
		long	n = (nf < 0f) ? Dat.GetNowRec() : (long)(nf * Imu.fps);
		Vector3	Pc = (Dat.Pmc[n].n0 == 99) ? Dat.Pmc[n].p0 : Vector3.zero;
		hand.Draw(n, Dat.Pim, Pc);
		Sta.Frame(string.Format("{0:.00}", ((float)n / Imu.fps)));
	}
//..............................................................
// End Process
//..............................................................
	public	void	End()
	{
		Imu.Ready(false);
		Imu.Close();
	}
};
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
