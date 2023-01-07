using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProMovieCapture;

namespace KW_Mocap
{
	[RequireComponent(typeof(MeshRenderer))]
	public class VideoCapture : MonoBehaviour
	{
		[Range(1.0f,30f)]
		public float displayScale = 21.0f;
		private float aspectRatio = 1.33f;
        [SerializeField] Material material;
		WebCamTexture texture;
		CaptureFromWebCamTexture capture;
		bool isFileWritingCompleted = true;

		IEnumerator Start()
		{
			if (WebCamTexture.devices.Length < 1) yield break;

			DisplayAllWebCams();
			yield return SetWebCamTexture(0);
			PrepareCapture();
		}

        void Update()
        {
            this.transform.localScale = new Vector3(displayScale * aspectRatio, displayScale, 1.0f);
        }

        /// <summary>
        /// 指定の番号のデバイスからの映像をmaterialに割り当てる。
        /// WebCamTextureクラスのオブジェクトは作成した直後は正しいTextureの情報をアクセスできないので
        /// Playメソッドを呼んだ後、正しい情報がアクセスできるまで待機する。
        /// https://qiita.com/akiojin/items/a97fe7fea7a123330486
        /// </summary>
        /// <param name="index">デバイス番号</param>
        /// <returns></returns>
        public IEnumerator SetWebCamTexture(int index)
        {
			WebCamDevice camera = WebCamTexture.devices[index];

            /* 解像度を指定すると、デバイスが対応している最も近いものを使う
			 * iMacのWebCameraだと
			 * 16:9になってるのも4:3になってるものもどっちも作れる
			 * iPadとかiPhone、Androidは要検証 */
            texture = new WebCamTexture(camera.name, 960, 720, 30); //4:3
            //texture = new WebCamTexture(camera.name, 1280, 720, 30); //16:9
			//texture = new WebCamTexture(camera.name);   //これだと元々の解像度になるらしい？？
            texture.Play();
			while (texture.width < 100)
			{
				Debug.Log("Waiting for camera. Width is still under 100.");
				yield return null;
			}
			Debug.Log($"width: {texture.width} / height: {texture.height}");
            Debug.Log($"requestedWidth: {texture.requestedWidth} / requestedHeight: {texture.requestedHeight}/ fps: {texture.requestedFPS}");
            material.mainTexture = texture;
			GetComponent<MeshRenderer>().material = material;
			aspectRatio = (float)texture.width / (float)texture.height;
		}

		/// <summary>
		/// アクセスできるWebCamTextureを列挙してコンソールに表示する
		/// </summary>
		public void DisplayAllWebCams()
        {
			for (int i = 0;i < WebCamTexture.devices.Length; i++)
            {
				Debug.Log($"WebCamDevice[{i}]::name = {WebCamTexture.devices[i].name}");
            }
        }

        /// <summary>
        /// AVProMovieCapture.CaptureFromWebCamTextureコンポーネントの設定
        /// </summary>
        private void PrepareCapture()
        {
			/* コンポーネント作成 */
			GameObject go = new GameObject("AVProMovieCapture");
			capture = go.AddComponent<CaptureFromWebCamTexture>();
			capture.FrameRate = 30f;
			capture.StopMode = StopMode.None;

			/* Texture */
#if AVPRO_MOVIECAPTURE_WEBCAMTEXTURE_SUPPORT
			capture.WebCamTexture = texture;
#endif
			/* Audio */
			capture.AudioCaptureSource = AudioCaptureSource.Microphone;
			capture.ForceAudioInputDeviceIndex = 0;

			/* Output */
			capture.OutputTarget = OutputTarget.VideoFile;
			capture.OutputFolder = CaptureBase.OutputPath.RelativeToProject;
#if UNITY_EDITOR_WIN
			capture.OutputFolderPath = @"Assets\Resources\Videos";
#elif UNITY_EDITOR_OSX
			capture.OutputFolderPath = "Assets/Resources/Videos";
#endif
			capture.FilenamePrefix = "pending_file";
			capture.AppendFilenameTimestamp = true;
			capture.AllowManualFileExtension = false;
			capture.CompletedFileWritingAction += OnCompleteFinalFileWriting;
		}

		/// <summary>
		/// 録画を開始する
		/// </summary>
		public void StartRecording()
        {
			Debug.Log("VideoCapture: StartRecording");
			if (capture.IsCapturing())
				capture.CancelCapture();
			capture.StartCapture();
		}

		/// <summary>
		/// 録画を停止する
		/// </summary>
		public void StopRecording()
        {
			if (!capture.IsCapturing()) return;
			Debug.Log("VideoCapture: StopRecording(Paused)");
			capture.PauseCapture();
        }

        /// <summary>
        /// 動画を保存する。
        /// StopCaptureメソッドを呼ぶと自動的に "pending_file" + タイムスタンプ のファイル名で動画が保存される。
		/// その後、保存したいファイル名にリネームする。リネームは動画の書き出しが終了するのを待ってから行われる。
        /// </summary>
        /// <param name="fileName">拡張子なしのファイル名</param>
        /// <returns></returns>
        public IEnumerator Save(string fileName)
        {
			capture.StopCapture();
			isFileWritingCompleted = false;
			yield return Rename(fileName);
		}

        /// <summary>
        /// CaptureFromWebCamTextureによって直前に保存された動画をリネームする
        /// </summary>
        /// <param name="fileName">リネーム後のファイル名</param>
        /// <returns></returns>
        private IEnumerator Rename(string fileName)
        {
			/* 動画の書き出しが完了するまで待機 */
			while (!isFileWritingCompleted)
			{
				Debug.Log("Waiting for FinalFileWriting");
				yield return null;
			}

			string lastFilePath = capture.LastFilePath;
			string dir = System.IO.Path.GetDirectoryName(lastFilePath);
			string ext = System.IO.Path.GetExtension(lastFilePath);
#if UNITY_EDITOR_WIN
			string dest = $@"{dir}\{fileName}{ext}";
#elif UNITY_EDITOR_OSX
			string dest = $"{dir}/{fileName}.{ext}";
#endif
			System.IO.File.Move(lastFilePath, dest);
			Debug.Log("File.Move executed");
		}

        /// <summary>
        /// CaptureFromWebCamTextureの動画書き出しが完了したらフラグをtrueにしてもらうコールバック。
        /// </summary>
        /// <param name="handler"></param>
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
			Debug.Log("VideoCapture has been destroyed.");
		}
    }
}
