using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Leap.Unity;

namespace KW_Mocap
{
    public class MotionRecoder : MonoBehaviour
    {
        /// <summary>
        /// 記録できるデータ点数の最大値。
        /// 30FPSだと5分ちょっと相当。
        /// </summary>
        const int MaxDataCount = 10000;
        public int recordDataCount { get; private set; } = 0;
        MotionData[] motionData = new MotionData[MaxDataCount];
        bool isRecording = false;
        Transform leftHand, rightHand;

        void Start()
        {
            leftHand = transform.GetChild(0);
            rightHand = transform.GetChild(1);
        }

        void Update()
        {
            if (isRecording) Record();
        }

        void Record()
        {
            HandData left = new HandData(leftHand.position, leftHand.rotation);
            HandData right = new HandData(rightHand.position, rightHand.rotation);
            motionData[recordDataCount] = new MotionData(left, right);
        }

        public void StartRecording()
        {
            if (isRecording) return;
            isRecording = true;
            recordDataCount = 0;
            WorldTimer.CountUp += RecordDataCountUp;
            Debug.Log("Start Recording");
        }

        public void StopRecording()
        {
            if (!isRecording) return;
            isRecording = false;
            WorldTimer.CountUp -= RecordDataCountUp;
            Debug.Log("Stop Recording");
        }

        void RecordDataCountUp()
        {
            if (recordDataCount >= MaxDataCount) StopRecording();

            recordDataCount++;
        }

        public void Save(string fileName)
        {
            if (isRecording) return;

            int bufSize = HandData.MinimumBufferSize * 2;
            byte[] buf = new byte[bufSize];
            try
            {
                using (FileStream fs = new FileStream($"SavedMotionData/{fileName}.bin", FileMode.CreateNew, FileAccess.Write))
                {
                    // TODO: handsオブジェクトのlocalPositionを記録する部分を書く。
                    //byte[] handsLocalPosition = 
                    byte[] byte_DataCount = BitConverter.GetBytes(recordDataCount);
                    fs.Write(byte_DataCount, 0, byte_DataCount.Length);

                    for (int i = 0; i < recordDataCount; i++)
                    {
                        //データをシリアル化してFileStreamに書き込み
                        motionData[i].SetBytes(buf);
                        fs.Write(buf, 0, bufSize);
                    }
                }
                Debug.Log($"Saved as SavedMotionData/{fileName}.bin");
            }
            catch (IOException e)
            {
                throw new DuplicateFileNameException("This file name is already exists.", e);
            }
        }
    }
}
