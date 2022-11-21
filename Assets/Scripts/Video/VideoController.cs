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
        [SerializeField] private int startFrame = 500;
        public bool isPlaying { get; private set; } = false;

        void Start()
        {
            video = GetComponent<VideoPlayer>();
            SetVideoClip("drop pop candy(EXPERT)");
            video.frame = startFrame;
            video.Play();
            video.Pause();
            //Debug.Log("lengh: " + video.length + "s");
            //Debug.Log("frame rate: " + video.frameRate);
            //Debug.Log("frame count: " + video.frameCount);
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
        /// Resources/Videosディレクトリ配下の指定されたファイル名のVideoClipをVideoPlayerに割り当てる。
        /// </summary>
        /// <param name="name">動画ファイル名</param>
        public void SetVideoClip(string name)
        {
            VideoClip videoClip = Resources.Load("Videos/" + name) as VideoClip;
            if (videoClip == null)
            {
                Debug.LogError($"VideoClip {name} could not be found.");
                return;
            }
            video.clip = videoClip;
        }
    }
}
