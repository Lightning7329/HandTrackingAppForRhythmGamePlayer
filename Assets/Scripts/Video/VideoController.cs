using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace KW_Mocap
{
    public class VideoController : MonoBehaviour
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

        public void Skip(int seconds)
        {
            video.frame += seconds * (long)video.frameRate;
        }

        public void ChangeSpeed(float speed)
        {
            video.playbackSpeed = speed;
        }
    }
}
