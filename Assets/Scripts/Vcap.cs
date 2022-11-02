//==============================================================================
//
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//..............................................................................
public class Vcap : MonoBehaviour
{
	public Material[]	Vmat ;
	WebCamTexture[]		Vtex ;
//..............................................................................
	void	Start()
	{
		if (WebCamTexture.devices.Length < 1)	return;

		Vtex = new WebCamTexture[WebCamTexture.devices.Length];
		SelVid(0, 0);   // ビデオソースの0番をテクスチャー0番に割り当てる
	}
//..............................................................................
	void	SelVid(int nc, int nd)
	{
		WebCamDevice	cam = WebCamTexture.devices[nd];
		Vtex[nc] = new WebCamTexture (cam.name);
		Vmat[nc].mainTexture = Vtex[nc];
		Vtex[nc].Play();
	}
}
//------------------------------------------------------------------------------
// EOF
//------------------------------------------------------------------------------
