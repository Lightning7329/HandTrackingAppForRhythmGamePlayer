using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProMovieCapture;

namespace KW_Mocap
{
	[RequireComponent(typeof(MeshRenderer))]
	public class VideoCapture : DisplaySetting
    {
		[Range(10f,50f)]
		public float displayScale = 30.0f;
		public Vector2Int targetResolution = new Vector2Int(1920, 1440);
		private Vector2Int capturedResolution = new Vector2Int(16, 16);
		WebCamTexture texture = null;

		CaptureFromWebCamTexture capture = null;
		FileWritingHandler fileWritingHandler = null;

        private IEnumerator Start()
		{
			displayMaterial = GetComponent<MeshRenderer>().material;

			if (WebCamTexture.devices.Length < 1) yield break;

			DisplayAllWebCams();
			yield return SetWebCamTexture(0);
			ChangeResolution();
			PrepareCapture();
		}

        private void Update()
        {
			/* ディスプレイのアスペクト比をheightを固定して変更 */
			float targetAspectRatio = (float)targetResolution.x / targetResolution.y;
			this.DisplaySize = new Vector2(displayScale, displayScale / targetAspectRatio);
		}

		/// <summary>
		/// アクセスできるWebCamTextureを列挙してコンソールに表示する
		/// </summary>
		public void DisplayAllWebCams()
		{
			for (int i = 0; i < WebCamTexture.devices.Length; i++)
			{
				Debug.Log($"WebCamDevice[{i}]::name = {WebCamTexture.devices[i].name}");
			}
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
			texture = new WebCamTexture(camera.name);   //これだと元々の解像度になる
            texture.Play();
			while (texture.width < 100)
			{
				Debug.Log("Waiting for camera. Width is still under 100.");
				yield return null;
			}
			capturedResolution = new Vector2Int(texture.width, texture.height);
            displayMaterial.mainTexture = texture;
		}

		[ContextMenu("Change Resolution")]
		public void ChangeResolution() => ChangeResolution(targetResolution.x, targetResolution.y);

		public void ChangeResolution(int width, int height)
		{
            float marginedAspectRatio = (float)capturedResolution.x / capturedResolution.y;
			float targetAspectRatio = (float)width / height;
			FitVideoAspect(marginedAspectRatio, targetAspectRatio);
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
            capture.OutputFolderPath = "CapturedPendingVideos";
            capture.FilenamePrefix = "pending_file";
			capture.AppendFilenameTimestamp = true;
			capture.AllowManualFileExtension = false;
			capture.BeginFinalFileWritingAction += OnBeginFinalFileWriting;
		}

		/// <summary>
		/// 録画を開始する
		/// </summary>
		public void StartRecording()
        {
			if (capture.IsCapturing())
			{
				capture.CancelCapture();
				DeleteLastPendingVideo();
			}
			capture.StartCapture();
		}

		/// <summary>
		/// 録画を停止する
		/// </summary>
		public void StopRecording()
        {
			if (!capture.IsCapturing()) return;
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
			while (!fileWritingHandler.IsFileReady())
				yield return null;

			string lastFilePath = fileWritingHandler.Path;
			string ext = System.IO.Path.GetExtension(lastFilePath);
			string dest = VideoPreferences.VideoFileDirectory + fileName + ext;
			System.IO.File.Move(lastFilePath, dest);
			Debug.Log("File.Move executed");
			fileWritingHandler = null;
		}

        /// <summary>
        /// CaptureFromWebCamTextureの動画書き出しを知るためのハンドラをもらうために書き出しが始まったら呼ばれるコールバック。
        /// </summary>
        /// <param name="handler"></param>
		private void OnBeginFinalFileWriting(FileWritingHandler handler)
		{
			fileWritingHandler = handler;
		}

		private void DeleteLastPendingVideo()
        {
			if (System.IO.File.Exists(capture.LastFilePath))
				System.IO.File.Delete(capture.LastFilePath);
		}

		private void OnDestroy()
        {
			if (texture != null)
			{
				texture.Stop();
				Destroy(texture);
			}
			DeleteLastPendingVideo();
			capture.BeginFinalFileWritingAction -= OnBeginFinalFileWriting;
			if (fileWritingHandler != null) fileWritingHandler.Dispose();
		}
    }
}
