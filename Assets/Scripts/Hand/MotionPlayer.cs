using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KW_Mocap
{
    public class MotionPlayer : MonoBehaviour
    {
        MotionData[] motionData = null;
        int playDataCount = 0;
        bool isLoaded = false;
        bool isPlaying = false;
        [SerializeField] GameObject left, right;
        GameObject[] leftJoints, rightJoints;

        public int PlayDataCount
        {
            get => playDataCount;
            set => playDataCount = value > 0 ? value : 0;
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
            /*
            if (motionData[playDataCount] == null)
            {
                Debug.Log($"motionData[{playDataCount}] == null");
                return;
            }
            else if (motionData[playDataCount].left == null)
            {
                Debug.Log($"motionData[{playDataCount}].left == null");
                return;
            }
            else if (motionData[playDataCount].right == null)
            {
                Debug.Log($"motionData[{playDataCount}].right == null");
                return;
            }*/
            // TODO: leftJoint[0]~leftKJoint[8]のモーションデータも再生する。rightも然り。

            // TODO: NullReferenceException: Object reference not set to an instance of an object
            // 上のif文をどれも通らない当たり参照は入ってるけど、その参照先が怪しい
            // TODO: 多分直ったと思うから研究室で要検証
            leftJoints[9].transform.position = motionData[playDataCount].left.palmPos;
            rightJoints[9].transform.position = motionData[playDataCount].right.palmPos;
        }

        public void StartPlaying()
        {
            if (!isLoaded) throw new MotionDataNotLoadedException();
            if (isPlaying) return;

            isPlaying = true;
            WorldTimer.CountUp += PlayDataCountUp;
            Debug.Log("Start Playing");
        }

        public void StopPlaying()
        {
            if (!isPlaying) return;

            isPlaying = false;
            WorldTimer.CountUp -= PlayDataCountUp;
            Debug.Log("Stop Playing");
        }


        /// <summary>
        /// StartPlaying()でWorldTimerクラスのCountUpにデリゲートとして渡される。
        /// motionデータ点数を超えると自動的に再生が止まる。
        /// </summary>
        void PlayDataCountUp()
        {
            if (playDataCount >= motionData.Length) StopPlaying();

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
