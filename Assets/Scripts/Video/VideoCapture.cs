using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProMovieCapture;

namespace KW_Mocap
{
	public class VideoCapture : MonoBehaviour
	{
		public Material material;
		WebCamTexture texture;
		CaptureFromWebCamTexture capture;
		bool isFileWritingCompleted = true;

		void Start()
		{
			if (WebCamTexture.devices.Length < 1) return;

			check();
			SetWebCamTexture(0);

			PrepareCapture();
		}

		public void SetWebCamTexture(int index)
        {
			WebCamDevice camera = WebCamTexture.devices[index];
			texture = new WebCamTexture(camera.name, 1920, 1440);
			material.mainTexture = texture;
			texture.Play();
			GetComponent<MeshRenderer>().material = material;
		}

		public void check()
        {
			for (int i = 0;i < WebCamTexture.devices.Length; i++)
            {
				Debug.Log($"WebCamDevice[{i}]::name = {WebCamTexture.devices[i].name}");
            }
        }

		private void PrepareCapture()
        {
			GameObject go = new GameObject();
			go.name = "AVProMovieCapture";
			capture = go.AddComponent<CaptureFromWebCamTexture>();
			capture.FrameRate = 30f;
			capture.StopMode = StopMode.None;


			/* 映像入力 */
#if AVPRO_MOVIECAPTURE_WEBCAMTEXTURE_SUPPORT
			capture.WebCamTexture = texture;
#endif
			/* 音声入力 */
			capture.AudioCaptureSource = AudioCaptureSource.Microphone;
			capture.ForceAudioInputDeviceIndex = 0;

			/* 出力先 */
			capture.OutputFolder = CaptureBase.OutputPath.RelativeToProject;
#if UNITY_EDITOR_WIN
			capture.OutputFolderPath = @"Assets\Resources\Videos";
#elif UNITY_EDITOR_OSX
			capture.OutputFolderPath = @"Assets/Resources/Videos";
#endif
			capture.FilenamePrefix = "pending_file";
			capture.AppendFilenameTimestamp = true;
			capture.CompletedFileWritingAction += OnCompleteFinalFileWriting;
		}

		public void StartRecording()
        {
			Debug.Log("VideoCapture: StartRecording");
			capture.CancelCapture();
			capture.StartCapture();
		}

		public void StopRecording()
        {
			Debug.Log("VideoCapture: StopRecording(Paused)");
			capture.PauseCapture();
        }

		public void Save(string fileName)
        {
			capture.StopCapture();
			isFileWritingCompleted = false;
			StartCoroutine(Rename(fileName));
		}

		private IEnumerator Rename(string fileName)
        {
			/* 動画の書き出しが完了するまで待機 */
			while (!isFileWritingCompleted) yield return null;

			string lastFilePath = capture.LastFilePath;
			string dir = System.IO.Path.GetDirectoryName(lastFilePath);
			string ext = System.IO.Path.GetExtension(lastFilePath);
#if UNITY_EDITOR_WIN
			string dest = $@"{dir}\{fileName}{ext}";
#elif UNITY_EDITOR_OSX
			string dist = $"{dir}/{fileName}.{ext}";
#endif
			System.IO.File.Move(lastFilePath, dest);
			Debug.Log("File.Move executed");
		}

		private void OnCompleteFinalFileWriting(FileWritingHandler handler)
		{
			isFileWritingCompleted = true;
		}

		private void OnDestroy()
        {
			if (texture != null)
			{
				texture.Stop();
				Destroy(texture);
			}
			capture.CompletedFileWritingAction -= OnCompleteFinalFileWriting;

		}
    }
}
//------------------------------------------------------------------------------
// EOF
//------------------------------------------------------------------------------
