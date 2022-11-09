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

        int recordDataCount;
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
            var leftPose = leftHand.GetLeapHand().GetPalmPose();
            var rightPose = leftHand.GetLeapHand().GetPalmPose();
            HandData left = new HandData(leftPose.position, leftPose.rotation);
            HandData right = new HandData(rightPose.position, rightPose.rotation);

            motionData[recordDataCount] = new MotionData(left, right);

            recordDataCount++;
        }

        public void StartRecording()
        {
            if (isRecording) return;
            isRecording = true;
            recordDataCount = 0;
            WorldTimer.CountUp += RecordDataCountUp;
        }

        public void StopRecording()
        {
            if (!isRecording) return;
            isRecording = false;
            WorldTimer.CountUp -= RecordDataCountUp;
        }

        void RecordDataCountUp()
        {
            recordDataCount++;
        }

        public void Save(String fileName)
        {
            if (isRecording) return;

            byte[] buf = new byte[144];
            try
            {
                using (FileStream fs = new FileStream($"SavedMotionData/{fileName}.bin", FileMode.CreateNew, FileAccess.Write))
                {
                    // 記録するデータ点数
                    int savingDataCount = recordDataCount < MaxDataCount ? recordDataCount : MaxDataCount;
                    byte[] byte_DataCount = BitConverter.GetBytes(savingDataCount);
                    fs.Write(byte_DataCount, 0, byte_DataCount.Length);

                    for (int i = 0; i < savingDataCount; i++)
                    {
                        //データをシリアル化してFileStreamに書き込み
                        motionData[i].SetBytes(buf);
                        fs.Write(buf, 0, 144);
                    }
                }
            }
            catch (IOException e)
            {
                Debug.Log(e);
                throw new DuplicateFileNameException("This file name is already exists.", e);
            }
        }

        public MotionData[] Load(String fileName)
        {
            if (isRecording) return null;

            byte[] buf = new byte[144];
            try
            {
                using (FileStream fs = new FileStream($"SavedMotionData/{fileName}.bin", FileMode.Open, FileAccess.Read))
                {
                    // 読み込むデータ点数
                    fs.Read(buf, 0, 4);
                    recordDataCount = BitConverter.ToInt32(buf, 0);

                    for (int i = 0; i < recordDataCount; i++)
                    {
                        fs.Read(buf, 0, 144);
                        motionData[i] = new MotionData(buf);
                    }
                }
            }
            catch (IOException e)
            {
                Debug.Log(e);
            }

            return motionData;
        }
    }
}
