

//==============================================================
// カメラの設定
//--------------------------------------------------------------
// (C) All rights reserved by Shigekazu SAKAI from 2021 -
//..............................................................
using UnityEngine;
//..............................................................
public class MoveCam : MonoBehaviour
{
	public Vector3	Ang = Vector3.zero;
	public float	ddY = 0.0f;
	public float	Dist = -100.0f;
	private Vector3 At = Vector3.zero;
	private float	rY = 0.0f;
//..............................................................
	private void	Start()
	{
		Camera.main.transform.localPosition = SetPos(Dist, Ang);
		//Camera.main.transform.LookAt(At);
	}
//..............................................................
	private void	Update ()
	{
		if (Input.GetKey (KeyCode.LeftArrow))		Ang.y -= 0.5f;
		else if (Input.GetKey (KeyCode.RightArrow))	Ang.y += 0.5f;
		else if (Input.GetKey (KeyCode.UpArrow))	Ang.x -= 0.5f;
		else if (Input.GetKey (KeyCode.DownArrow))	Ang.x += 0.5f;
		else if (Input.GetKey (KeyCode.N))	Dist += 0.1f;
		else if (Input.GetKey (KeyCode.F))	Dist -= 0.1f;
		Ang.x = (Ang.x > -90.0f) ? Ang.x : -89.5f;
		Ang.x = (Ang.x < 90.0f) ?  Ang.x : 89.5f;

		Camera.main.transform.localPosition = SetPos (Dist,Ang);
		//Camera.main.transform.LookAt(At);
		Camera.main.transform.Rotate(Vector3.zero);
	}
//..............................................................
	private Vector3 SetPos(float dst, Vector3 ang)
	{
		Vector3 rad;
		rad.x = Mathf.PI * ang.x / 180.0f;
		rad.y = -Mathf.PI * (ang.y + rY) / 180.0f;
		rY += ddY;
		Vector3 pc;
		pc.x = dst * Mathf.Cos(rad.x) * Mathf.Sin(rad.y);
		pc.y = dst * Mathf.Sin(rad.x);
		pc.z = dst * Mathf.Cos(rad.x) * Mathf.Cos(rad.y);
//
		return (pc);
	}
}
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------

