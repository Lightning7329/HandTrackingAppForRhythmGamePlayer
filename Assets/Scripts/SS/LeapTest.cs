
//==============================================================
//
// Hand Motion Tracking System ver.3
// Hand GameObject Mapping to Sensor Class
//
//--------------------------------------------------------------
// (C)Copyright Allrights reserved by Sakai Shigekazu 2022 -
//..............................................................
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap ;
using Leap.Unity ;
//--------------------------------------------------------------
public class    LeapTest : MonoBehaviour
{
	[SerializeField] private GameObject	G0;
	[SerializeField] private SS_LEAP	R ;
	private GameObject[,]	G = new GameObject[5,4];
	//private SS_PALLET	Pen ;
//..............................................................
	private void Start()
	{
		SetObj(G0);
		SetParent(G0.transform);
	}
//..............................................................
	private void	SetObj(GameObject Top)
	{
		for (int i = 0 ; i < 5 ; i++)
		{
			for (int j = 0 ; j < 4 ; j++)
			{
				G[i,j] = _Find(string.Format("F{0}{1}",i,j), Top);
			}
		}
	}
//..............................................................
	private void	SetParent(Transform Par)
	{
		for (int i = 0 ; i < 5 ; i++)
		{
			for (int j = 0 ; j < 4 ; j++)
			{
				if (G[i,j] != null)
				{
					G[i,j].transform.parent = Par;
				}
			}
		}
	}
//..............................................................
	private void	SetObjTr()
	{
		for (int i = 0 ; i < 5 ; i++)
		{
			for (int j = 1 ; j < 4 ; j++)
			{
				Bone	B = R.B[i,j];
				if (B != null)
				{
					G[i,j].transform.localPosition = 100f * B.PrevJoint;
					G[i,j].transform.localScale = new Vector3(1f,1f,100f * B.Length);
					G[i,j].transform.localRotation = Quaternion.LookRotation(B.Direction, Vector3.up);
				}
			}
		}
		G[2,0].transform.localPosition = 100f * R.Root;
	}
//..............................................................
	private void Update()
	{
		SetObjTr();
	}
//..............................................................
	private GameObject	_Find(string nam, GameObject Gc)
	{
		if (nam == Gc.name)	return(Gc);
//
		int	n = Gc.transform.childCount;
		if (n > 0)
		{
			GameObject	Gcc ;
			for (int i = 0 ; i < n ; i++)
			{
				Gcc = _Find(nam, Gc.transform.GetChild(i).gameObject);
				if (Gcc != null)	return(Gcc);
			}
		}
//
		return(null);
	}
};
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
