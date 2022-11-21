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
        [SerializeField] GameObject left, right;
        GameObject[] leftJoints, rightJoints;
        private bool isPlaying = false;
        public bool isLoaded { get; private set; } = false;
        public int frameCount { get; private set; } = 0;

        private int _frame = 0;
        public int frame
        {
            get => _frame;
            set
            {
                if (value < 0)
                    _frame = 0;
                else if (value >= frameCount)
                    _frame = frameCount - 1;
                else
                    _frame = value;
            }
        }

        void Start()
        {
            left.GetComponent<HandSetting>().SetMaterial(left, true);
            leftJoints = left.GetComponent<HandSetting>().joints;
            right.GetComponent<HandSetting>().SetMaterial(right, true);
            rightJoints = right.GetComponent<HandSetting>().joints;
        }

        void Update()
        {
            if (isPlaying) Play();
        }

        /// <summary>
        /// 各フレームの動き（位置と回転）を記述
        /// </summary>
        void Play()
        {
            // TODO: leftJoint[0]~leftKJoint[8]のモーションデータも再生する。rightも然り。
            left.transform.SetPositionAndRotation(motionData[_frame].left.palmPos, motionData[_frame].left.palmRot);
            right.transform.SetPositionAndRotation(motionData[_frame].right.palmPos, motionData[_frame].right.palmRot);
        }

        public void StartPlaying()
        {
            if (!isLoaded) throw new MotionDataNotLoadedException();
            if (frameCount == 0) return;
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
            this.frame += (int)(WorldTimer.frameRate * seconds);
            if(isPlaying) StartPlaying();
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
            if (this._frame >= frameCount - 1)
                PausePlaying();

            this.frame++;
        }

        public void ResetFrameCount()
        {
            frame = 0;
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
                    frameCount = BitConverter.ToInt32(buf, 0);
                    motionData = new MotionData[frameCount];

                    for (int i = 0; i < motionData.Length; i++)
                    {
                        fs.Read(buf, 0, bufSize);
                        motionData[i] = new MotionData(buf);
                    }
                }
                isLoaded = true;
                Debug.Log($"Motion Loaded " + fileName);
            }
            catch (IOException e)
            {
                Debug.Log(e);
            }            
        }
    }
}
