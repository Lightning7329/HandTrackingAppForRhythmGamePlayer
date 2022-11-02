
//==============================================================================
//
// Video Camera Image Capture and Make Texture
//
//------------------------------------------------------------------------------
// (C) All rights reserved by Shigekazu SAKAI from 2021 -
//..............................................................................
using UnityEngine;
//..............................................................................
public class	SS_AVTEX
{
	private int		n_cam_v , n_ext_v ;
	private string[]	Nam_v ;
	private WebCamTexture[]	Vtex = new WebCamTexture[2];
//..............................................................................
	public string[]	ListV(string m, string s)
	{
		int	n = WebCamTexture.devices.Length;
		n_cam_v = -1;
		n_ext_v = -1;
		Nam_v = new string[n];
		for (int i = 0 ; i < n ; i++)
		{
			string	str = WebCamTexture.devices[i].name;
			if (str.Contains(m))		n_cam_v = i;
			else if (str.Contains(s))	n_ext_v = i;
			Nam_v[i] = str;
		}
//
		return(Nam_v);
	}
//..............................................................................
	public void	Assign(int n, Material Mt, GameObject Gc)
	{
		int	nv = (n == 0) ? n_cam_v : n_ext_v;
		if (nv >= 0)
		{
			Vtex[n] = new WebCamTexture(Nam_v[nv], 1920, 1080, 30);
			Mt.mainTexture = Vtex[n];
			Vtex[n].Play();
			Vtex[n].Stop();
			Vector3	sc = Gc.transform.localScale;
			float	v_asp = 1920f / 1080f;
			float	w_asp = sc.x / sc.y;
			Vector3	scc = Vector3.one;
			Vector3	ofc = Vector3.zero;
			if (w_asp >= v_asp)
			{
				scc.y = v_asp / w_asp;
				ofc.y = 0.5f * (1f - scc.y);
			}
			else if (w_asp < v_asp)
			{
				scc.x = w_asp / v_asp;
				ofc.x = 0.5f * (1f - scc.x);
			}
			Mt.SetTextureScale(Mt.GetTexturePropertyNames()[0], scc);
			Mt.SetTextureOffset(Mt.GetTexturePropertyNames()[0], ofc);
		}
	}
//..............................................................................
	public void	Play(int n, bool flg)
	{
		int	nv = (n == 0) ? n_cam_v : n_ext_v;
		if (flg)
		{
			if (nv >= 0)	Vtex[nv].Play();
		}
		else
		{
			if (nv >= 0)	Vtex[nv].Stop();
		}
	}
};
//------------------------------------------------------------------------------
// EOF
//------------------------------------------------------------------------------
