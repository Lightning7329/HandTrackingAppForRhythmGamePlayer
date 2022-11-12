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
        const int MaxDataCount = 10000;   //fps30で5分ちょっと

        public int recordDataCount { get; private set; } = 0;
        MotionData[] motionData = new MotionData[MaxDataCount];
        bool isRecording = false;
        LeapHandModel leftHand, rightHand;

        void Start()
        {
            leftHand = transform.GetChild(0).GetComponent<LeapHandModel>();
            rightHand = transform.GetChild(1).GetComponent<LeapHandModel>();
        }

        void Update()
        {
            if (isRecording) Record();
        }

        void Record()
        {
            // TODO
            /*
             * 例外処理せよ
             * LeapMotionからの認識が外れた瞬間
             * NullReferenceException: Object reference not set to an instance of an object
             * KW_Mocap.MotionRecoder.Record () (at Assets/Scripts/MotionRecoder.cs:33)
             * KW_Mocap.MotionRecoder.Update () (at Assets/Scripts/MotionRecoder.cs:27)
             * が発生する。
             */
            var leftPose = leftHand.GetLeapHand().GetPalmPose();
            var rightPose = rightHand.GetLeapHand().GetPalmPose();
            HandData left = new HandData(leftPose.position, leftPose.rotation);
            HandData right = new HandData(rightPose.position, rightPose.rotation);

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
                using (FileStream fs = new FileStream($"SavedMotionData/{fileName}.bin", FileMode.Create, FileAccess.Write))
                {
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
                Debug.Log(e);
                throw new DuplicateFileNameException("This file name is already exists.", e);
            }
        }
    }
}
