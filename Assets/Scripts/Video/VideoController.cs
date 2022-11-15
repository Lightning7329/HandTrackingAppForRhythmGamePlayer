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
        public bool isPlaying { get; private set; } = false;

        void Start()
        {
            video = GetComponent<VideoPlayer>();
            video.frame = 500;
            video.Play();
            video.Pause();
            //Debug.Log("lengh: " + video.length + "s");
            //Debug.Log("frame rate: " + video.frameRate);
            //Debug.Log("frame count: " + video.frameCount);
        }

        /// <summary>
        /// 再生ボタンを押したときに実行。再生中の場合は一時停止、一時停止中の場合は再生する。
        /// </summary>
        public void TogglePlayAndPause()
        {
            if (isPlaying)
            {
                video.Pause();
                isPlaying = false;
            }
            else
            {
                video.Play();
                isPlaying = true;
            }
        }

        public void StartPlaying()
        {
            if (isPlaying) return;

            video.Play();
            isPlaying = true;
        }

        public void PausePlaying()
        {
            if (!isPlaying) return;

            video.Pause();
            isPlaying = false;
        }

        public void Skip(float seconds)
        {
            video.frame += (long)(seconds * video.frameRate);
        }

        public void ChangeSpeed(float speedRatio)
        {
            video.playbackSpeed = speedRatio;
        }
    }
}
