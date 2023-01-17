using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace KW_Mocap
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoController : MonoBehaviour
    {
        private VideoPlayer video;
        private RenderTexture renderTexture;
        [SerializeField] private int startFrame = 0;
        public bool isPlaying { get; private set; } = false;
        /// <summary>
        /// The length of the Video, in seconds.
        /// </summary>
        public double Length { get => video.length; }
        /// <summary>
        /// The clock time that the VideoPlayer follows to schedule its samples, in seconds. 
        /// </summary>
        public double ClockTime { get => video.clockTime; }
        /// <summary>
        /// The presentation time of the currently available frame in VideoPlayer.texture.
        /// </summary>
        public double Time
        {
            get => video.time;
            set => video.time = value;
        }

        void Start()
        {
            video = GetComponent<VideoPlayer>();
        }

        /// <summary>
        /// 停止中、再生ボタンを押したときに実行。
        /// </summary>
        public void StartPlaying()
        {
            if (isPlaying) return;

            video.Play();
            isPlaying = true;
        }

        /// <summary>
        /// 再生中、再生ボタンを押したときに実行。
        /// </summary>
        public void PausePlaying()
        {
            if (!isPlaying) return;

            video.Pause();
            isPlaying = false;
        }

        /// <summary>
        /// 一瞬再生して一時停止する。
        /// </summary>
        public void PlayAndPause()
        {
            if (!isPlaying)
                video.Play();
            video.Pause();
            isPlaying = false;
        }

        /// <summary>
        /// 指定倍率で動画の再生速度を変更する。
        /// </summary>
        /// <param name="speedRatio">再生速度倍率</param>
        public void ChangeSpeed(float speedRatio) => video.playbackSpeed = speedRatio;

        public void SetPrepareCompleted(VideoPlayer.EventHandler method) => video.prepareCompleted += method;

        public void ResetFrameCount() => video.frame = startFrame;

        /// <summary>
        /// 全体時間の秒、フレームレート、フレーム数、ピクセル数をコンソールに表示
        /// </summary>
        public void DisplayState()
        {
            Debug.Log($"lengh: {video.length}s");
            Debug.Log("frame rate: " + video.frameRate);
            Debug.Log("frame count: " + video.frameCount);
            Debug.Log($"width: {video.width} / height: {video.height}");
        }

        /// <summary>
        /// Resources/Videosディレクトリ配下の指定されたファイル名のVideoClipをVideoPlayerに割り当てる。
        /// </summary>
        /// <param name="name">拡張子なしの動画ファイル名</param>
        public void SetVideoClip(string name)
        {
            /* 前に割り当てていたvideo clipをアンロードする */
            if (video.clip != null) {
                Resources.UnloadAsset(video.clip);
                renderTexture.Release();
                Debug.Log("VideoClip Unloaded " + video.clip.name);
            }

            /* 新しくvideo clipをロードする（拡張子はなしでいいらしい）*/
            VideoClip videoClip = Resources.Load("Videos/" + name) as VideoClip;

            if (videoClip == null)
            {
                Debug.LogError($"VideoClip {name} could not be loaded.");
                return;
            }

            /* VideoPlayerコンポーネントに割り当てて再生準備をする */
            CreateAndSetRenderTexture((int)videoClip.width, (int)videoClip.height);
            SetDisplaySize((int)videoClip.width, (int)videoClip.height);
            video.clip = videoClip;
            video.frame = startFrame;
            PlayAndPause();
        }

        /// <summary>
        /// video clipのピクセル数から新しいRender Textureを作成し、
        /// DisplayのGameObjectのMaterialのMain Textureとしてそれを貼り付ける。
        /// さらにVideoPlayerのtargetTextureにも設定する。
        /// </summary>
        /// <param name="width">video clipの横のピクセル数</param>
        /// <param name="height">video clipの縦のピクセル数</param>
        private void CreateAndSetRenderTexture(int width, int height)
        {
            renderTexture = new RenderTexture(width, height, 24);
            renderTexture.Create();
            var mat = this.GetComponent<MeshRenderer>().material;
            mat.mainTexture = renderTexture;
            video.targetTexture = renderTexture;
        }

        /// <summary>
        /// Displayのサイズを引数のピクセル数から変更する
        /// </summary>
        /// <param name="width">video clipの横のピクセル数</param>
        /// <param name="height">video clipの縦のピクセル数</param>
        private void SetDisplaySize(int width, int height)
        {
            float displayScale = 7.0f / 480.0f;
            this.transform.localScale = new Vector3(width * displayScale, height * displayScale, 1);
        }

        /// <summary>
        /// Displayを90度回転する
        /// </summary>
        /// <param name="clockWise">trueなら時計回り、falseなら反時計回り</param>
        public void RotateDisplay(bool clockWise)
        {
            Vector3 localOrigin = this.transform.parent.position;
            float angle = clockWise ? 90.0f : -90.0f;
            this.transform.RotateAround(localOrigin, Vector3.up, angle);
        }

        private void OnApplicationQuit()
        {
            if (renderTexture != null && renderTexture.IsCreated())
                renderTexture.Release();
        }
    }
}
