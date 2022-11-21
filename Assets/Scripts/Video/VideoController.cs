using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace KW_Mocap
{
    public class VideoController : MonoBehaviour, Player
    {
        private VideoPlayer video;
        private SliderController sliderController;
        [SerializeField] private int startFrame = 500;
        public bool isPlaying { get; private set; } = false;

        void Start()
        {
            video = GetComponent<VideoPlayer>();
            sliderController = GameObject.Find("TimeSlider").GetComponent<SliderController>();
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
        /// 指定秒数動画を進める。
        /// </summary>
        /// <param name="seconds">進める秒数。負のときは戻す。</param>
        public void Skip(float seconds)
        {
            video.frame += (long)(seconds * video.frameRate);
        }

        /// <summary>
        /// 指定倍率で動画の再生速度を変更する。
        /// </summary>
        /// <param name="speedRatio">再生速度倍率</param>
        public void ChangeSpeed(float speedRatio)
        {
            video.playbackSpeed = speedRatio;
        }

        public void ResetFrameCount()
        {
            video.frame = startFrame;
        }

        /// <summary>
        /// 全体時間の秒、フレームレート、フレーム数をコンソールに表示
        /// </summary>
        public void DisplayState()
        {
            Debug.Log("lengh: " + video.length + "s");
            Debug.Log("frame rate: " + video.frameRate);
            Debug.Log("frame count: " + video.frameCount);
        }

        /// <summary>
        /// Resources/Videosディレクトリ配下の指定されたファイル名のVideoClipをVideoPlayerに割り当てる。
        /// </summary>
        /// <param name="name">拡張子なしの動画ファイル名</param>
        public void SetVideoClip(string name)
        {
            // 前に割り当てていたvideo clipをアンロードする
            if (video.clip != null) {
                Resources.UnloadAsset(video.clip);
                Debug.Log("VideoClip Unloaded " + video.clip.name);
            }

            // 新しくvideo clipをロードする（拡張子はなしでいいらしい）
            VideoClip videoClip = Resources.Load("Videos/" + name) as VideoClip;

            if (videoClip == null)
            {
                Debug.LogError($"VideoClip {name} could not be found.");
                return;
            }

            // VideoPlayerコンポーネントに割り当てて再生準備をする
            sliderController.enabled = false;   // VideoPlayer側のPrepareが終わったらtrueに戻る
            video.clip = videoClip;
            video.frame = startFrame;
            video.Play();
            video.Pause();
        }
    }
}
