using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KW_Mocap
{
    public class MotionPlayer : MonoBehaviour, Player
    {
        MotionData[] motionData = null;
        [SerializeField] int playDataCount = 0;
        bool isLoaded = false;
        bool isPlaying = false;
        [SerializeField] GameObject left, right;
        GameObject[] leftJoints, rightJoints;

        public int PlayDataCount
        {
            get => playDataCount;
            set
            {
                if (value < 0)
                    playDataCount = 0;
                else if (motionData != null && value >= motionData.Length)
                    playDataCount = motionData.Length - 1;
                else
                    playDataCount = value;
            }
        }

        void Start()
        {
            leftJoints = left.GetComponent<HandSetting>().joints;
            rightJoints = right.GetComponent<HandSetting>().joints;
        }

        void Update()
        {
            if (isPlaying) Play();
        }

        void Play()
        {
            // TODO: leftJoint[0]~leftKJoint[8]のモーションデータも再生する。rightも然り。

            left.transform.localPosition = motionData[playDataCount].left.palmPos;
            left.transform.rotation = motionData[playDataCount].left.palmRot;
            right.transform.position = motionData[playDataCount].right.palmPos;
            right.transform.rotation = motionData[playDataCount].right.palmRot;
        }

        public void StartPlaying()
        {
            if (!isLoaded) throw new MotionDataNotLoadedException();
            if (isPlaying) return;

            isPlaying = true;
            WorldTimer.CountUp += PlayDataCountUp;
            Debug.Log("Start Playing");
        }

        public void PausePlaying()
        {
            if (!isPlaying) return;

            isPlaying = false;
            WorldTimer.CountUp -= PlayDataCountUp;
            Debug.Log("Stop Playing");
        }

        public void Skip(float seconds)
        {
            this.PlayDataCount += (int)(WorldTimer.frameRate * seconds);
            StartPlaying();
        }

        public void ChangeSpeed(float speedRatio)
        {
            WorldTimer.ChangeSpeed(speedRatio);
        }

        /// <summary>
        /// StartPlaying()でWorldTimerクラスのCountUpにデリゲートとして渡される。
        /// motionデータ点数を超えると自動的に再生が止まる。
        /// </summary>
        void PlayDataCountUp()
        {
            if (playDataCount >= motionData.Length - 1) PausePlaying();

            playDataCount++;
        }

        public void Load(string fileName)
        {
            string pass = $"SavedMotionData/{fileName}.bin";
            int bufSize = HandData.MinimumBufferSize * 2;
            byte[] buf = new byte[bufSize];
            try
            {
                using (FileStream fs = new FileStream(pass, FileMode.Open, FileAccess.Read))
                {
                    // 読み込むデータ点数
                    fs.Read(buf, 0, 4);
                    int DataCount = BitConverter.ToInt32(buf, 0);
                    motionData = new MotionData[DataCount];

                    for (int i = 0; i < DataCount; i++)
                    {
                        fs.Read(buf, 0, bufSize);
                        motionData[i] = new MotionData(buf);
                    }
                }
                isLoaded = true;
                Debug.Log($"Loaded" + pass);
            }
            catch (IOException e)
            {
                Debug.Log(e);
            }            
        }
    }
}
