using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProMovieCapture;

namespace KW_Mocap
{
	public class VideoCapture : MonoBehaviour
	{
		public Material[] Vmat;
		WebCamTexture[] Vtex;
		CaptureFromWebCamTexture capture;

		void Start()
		{
			if (WebCamTexture.devices.Length < 1) return;

			Vtex = new WebCamTexture[WebCamTexture.devices.Length];
			for (int i = 0; i < Vtex.Length; i++)
				SelVid(i, i);   // ビデオソースの0番をテクスチャー0番に割り当てる
			GetComponent<MeshRenderer>().material = Vmat[0];
			check();

			PrepareCapture();
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

		private void PrepareCapture()
        {

			GameObject go = new GameObject();
			go.name = "AVProMovieCapture";
			capture = go.AddComponent<CaptureFromWebCamTexture>();
			capture.FrameRate = 30f;
			capture.StopMode = StopMode.None;
			capture.OutputFolder = CaptureBase.OutputPath.RelativeToProject;
			capture.OutputFolderPath = "Assets/Resources/Videos";
#if AVPRO_MOVIECAPTURE_WEBCAMTEXTURE_SUPPORT
			capture.WebCamTexture = Vtex[0];
#endif
			capture.AudioCaptureSource = AudioCaptureSource.Microphone;
			capture.ForceAudioInputDeviceIndex = 0;
			//capture.StopAfterFramesElapsed = capture.FrameRate * 10f;
			//capture.NativeForceVideoCodecIndex = -1;
			//capture.VideoCodecPriorityWindows = new string[] { "H264", "HEVC" };
		}

		public void StartRecording()
        {
			capture.StartCapture();
		}

		public void StopRecording()
        {
			capture.StopCapture();
        }

        private void OnDestroy()
        {
            foreach (var texture in Vtex)
            {
				if (texture != null)
				{
					texture.Stop();
					Destroy(texture);
				}
            }
        }
    }
}
//------------------------------------------------------------------------------
// EOF
//------------------------------------------------------------------------------
