
//==============================================================================
//
// 
//
//------------------------------------------------------------------------------
// (C) All rights reserved by Shigekazu SAKAI from 2021 -
//..............................................................................
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
//..............................................................................
public class REC_DATA : MonoBehaviour
{
	GameObject	Link ;
	private Button	B_tim , B_rec , B_ply , B_save , B_load ;
	private bool	flg_tmr ;
	private bool	flg_rec ;
	private bool	flg_ply ;
	private int	rec_data_count , ply_data_count ;
//..............................................................................
	private void Start()
	{
		Link = GameObject.Find("Link");
		B_tim  = GameObject.Find("B_tmr").GetComponent<Button>();	B_tim.onClick.AddListener(OnBtnTmr);
		B_rec  = GameObject.Find("B_rec").GetComponent<Button>();	B_rec.onClick.AddListener(OnBtnRec);
		B_ply  = GameObject.Find("B_ply").GetComponent<Button>();	B_ply.onClick.AddListener(OnBtnPly);
		B_save = GameObject.Find("B_save").GetComponent<Button>();	B_save.onClick.AddListener(OnBtnSave);
		B_load = GameObject.Find("B_load").GetComponent<Button>();	B_load.onClick.AddListener(OnBtnLoad);
//
		flg_tmr = false;
		flg_rec = false;
		flg_ply = false;
		RecCountReset();
		PlyCountReset();
	}
//------------------------------------------------------------------------------
// TIMER Thread
//..............................................................................
	System.Threading.Timer	Tmr = null;
//..............................................................................
	private void	OnBtnTmr()
	{
		//記録中の場合はボタンをしても無効
		if (flg_rec)	return;

		//ボタンのON/OFF切り替え。色も変える
		flg_tmr = !flg_tmr;
		B_tim.GetComponent<Image>().color = (flg_tmr) ? Color.green : Color.white;
		
		//タイマーがONなら止める。OFFならタイマー作動
		if (flg_tmr)	TmrRun();
		else		TmrStop();
	}
//..............................................................................
	private void	TmrRun()
	{
		int	dT = (int)(1000f / 30);
		if (Tmr == null)	Tmr = new System.Threading.Timer(state => { CountUp(); }, null, 0, dT);
	}
//..............................................................................
	private void	TmrStop()
	{
		if (Tmr != null)
		{
			Tmr.Change(Timeout.Infinite, Timeout.Infinite);
			Tmr.Dispose();
			Tmr = null;
		}
	}
//..............................................................................
	private void	CountUp()
	{
		if (flg_rec)	rec_data_count++;
		if (flg_ply)	ply_data_count++;
	}
//..............................................................................
	private void	RecCountReset()
	{
		rec_data_count = 0;
	}
//..............................................................................
	private void	PlyCountReset()
	{
		ply_data_count = 0;
	}
//------------------------------------------------------------------------------
// DATA(Quaternion) Rec and Play
//..............................................................................
	const int	max_data_count = 900;
	Quaternion[]	Qrec = new Quaternion[max_data_count] ;
//..............................................................................
	private void	OnBtnRec()
	{
		if (!flg_tmr)	return;
		if (flg_ply)	OnBtnPly();
//
		flg_rec = !flg_rec;
		B_rec.GetComponent<Image>().color = (flg_rec) ? Color.red : Color.white;
		B_rec.GetComponentInChildren<Text>().color = (flg_rec) ? Color.white : Color.black;
	}
//..............................................................................
	private void	OnBtnPly()
	{
		if (!flg_tmr)	return;
		if (flg_rec)	return;
//
		flg_ply = !flg_ply;
		B_ply.GetComponent<Image>().color = (flg_ply) ? Color.green : Color.white;
		if (!flg_ply)	PlyCountReset();
	}
//..............................................................................
	private void Update()
	{
		if (flg_tmr)
		{
			Vector3	P_mouse = 0.2f * Input.mousePosition;
			Qrec[(rec_data_count % max_data_count)] = Quaternion.Euler(P_mouse);
			int	n = (!flg_ply) ? (rec_data_count % max_data_count) : (ply_data_count % rec_data_count);
			Link.transform.rotation = Qrec[n];
		}
	}
//------------------------------------------------------------------------------
// DATA(Quaternion) Save and Load
//..............................................................................
	private void	OnBtnSave()
	{
		if (flg_rec)	return;
//
		byte[] buf = new byte[64];
		try
		{
			FileStream	fs = new FileStream("data.qua", FileMode.Create, FileAccess.Write);
			int	ns = rec_data_count - max_data_count;	if (ns < 0)	ns = 0;
			int	ne = rec_data_count;

			SetByteBuf(BitConverter.GetBytes(ne - ns), buf, 0, 4);
			fs.Write(buf, 0, 4);

			for (int i = ns ; i < ne ; i++)
			{
				SetByteBuf(BitConverter.GetBytes(Qrec[i].w), buf,  0, 4);
				SetByteBuf(BitConverter.GetBytes(Qrec[i].x), buf,  4, 4);
				SetByteBuf(BitConverter.GetBytes(Qrec[i].y), buf,  8, 4);
				SetByteBuf(BitConverter.GetBytes(Qrec[i].z), buf, 12, 4);
				fs.Write(buf, 0, 16);
			}

			fs.Close();
		}
		catch(IOException e)
		{
			Debug.Log(e);
		}
	}
//..............................................................................
	private void	OnBtnLoad()
	{
		if (flg_rec)	return;

		byte[] buf = new byte[64];
		try
		{
			FileStream	fs = new FileStream("data.qua", FileMode.Open, FileAccess.Read);
			fs.Read(buf, 0, 4);
			rec_data_count   = BitConverter.ToInt32(buf, 0);

			for (int i = 0 ; i < rec_data_count ; i++)
			{
				fs.Read(buf, 0, 16);
				Qrec[i].w = BitConverter.ToSingle(buf,  0);
				Qrec[i].x = BitConverter.ToSingle(buf,  4);
				Qrec[i].y = BitConverter.ToSingle(buf,  8);
				Qrec[i].z = BitConverter.ToSingle(buf, 12);
			}
		}
		catch(IOException e)
		{
			Debug.Log(e);
		}
	}
//..............................................................................
	private void	SetByteBuf(byte[] bc, byte[] buf, int off, int n)
	{
		for (int i = 0 ; i < n ; i++)
		{
			buf[off + i] = bc[i];
		}
	}
}
//------------------------------------------------------------------------------
// EOF
//------------------------------------------------------------------------------
