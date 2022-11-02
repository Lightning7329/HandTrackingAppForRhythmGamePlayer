using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
	public class VideoCapture : MonoBehaviour
	{
		public Material[] Vmat;
		WebCamTexture[] Vtex;
		void Start()
		{
			if (WebCamTexture.devices.Length < 1) return;

			Vtex = new WebCamTexture[WebCamTexture.devices.Length];
			SelVid(0, 0);   // ビデオソースの0番をテクスチャー0番に割り当てる
			GetComponent<MeshRenderer>().material = Vmat[0];
			check();
		}
		void SelVid(int nc, int nd)
		{
			WebCamDevice cam = WebCamTexture.devices[nd];
			Vtex[nc] = new WebCamTexture(cam.name, 1920, 1440);
			Vmat[nc].mainTexture = Vtex[nc];
			Vtex[nc].Play();
		}

		public void check()
        {
			foreach (var cam in WebCamTexture.devices)
            {
				Debug.Log("name:" + cam.name);
            }
        }
	}
}
//------------------------------------------------------------------------------
// EOF
//------------------------------------------------------------------------------
