using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Leap.Unity;

namespace KW_Mocap
{
    public class MotionRecorder : MonoBehaviour
    {
        /// <summary>
        /// 記録できるデータ点数の最大値。
        /// 30FPSだと5分ちょっと相当。
        /// </summary>
        const int MaxDataCount = 100000;
        [HideInInspector] public bool useLeftGrove = false;
        [HideInInspector] public bool useRightGrove = false;
        public int recordDataCount { get; private set; } = 0;
        MotionData[] motionData = new MotionData[MaxDataCount];
        bool isRecording = false;
        Transform leftHand, rightHand;
        Transform[,] leftJoints, rightJoints;

        void Start()
        {
            leftHand = transform.GetChild(0);
            leftJoints = leftHand.GetComponent<HandSetting>().joints;
            rightHand = transform.GetChild(1);
            rightJoints = rightHand.GetComponent<HandSetting>().joints;
        }

        void Update()
        {
            if (isRecording) Record();
        }

        void Record()
        {
            HandData left = useLeftGrove ?
                new HandData(leftHand.localPosition, leftHand.localRotation, leftJoints) :
                new HandData(leftHand.localPosition, leftHand.localRotation);
            HandData right = useRightGrove ?
                new HandData(rightHand.localPosition, rightHand.localRotation, rightJoints) :
                new HandData(rightHand.localPosition, rightHand.localRotation);
            motionData[recordDataCount] = new MotionData(left, right);
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
            if (recordDataCount >= MaxDataCount) StopRecording();

            recordDataCount++;
        }

        /// <summary>
        /// 記録したモーションデータをシリアライズしてバイナリファイルに書き出す。
        /// </summary>
        /// <param name="fileName"></param>
        /// <exception cref="DuplicateFileNameException"></exception>
        public void Save(string fileName)
        {
            if (isRecording) return;

            int bufSize = HandData.MinimumBufferSize * 2;
            byte[] buf = new byte[bufSize];
            string targetFilePath = $"SavedMotionData/{fileName}.bin";
            try
            {
                if (File.Exists(targetFilePath))
                    throw new DuplicateFileNameException("This file name is already exists.");

                using (FileStream fs = new FileStream(targetFilePath, FileMode.CreateNew, FileAccess.Write))
                {
                    /* HandsオブジェクトのVirtual Deskに対する相対位置を書き込み */
                    this.transform.localPosition.SetBytesFromVector3(buf, 0);
                    fs.Write(buf, 0, 12);

                    /* Displayのサイズ */
                    GameObject.FindWithTag("Display").GetComponent<VideoCapture>().DisplaySize.SetBytesFromVector2(buf, 0);
                    fs.Write(buf, 0, 8);

                    /* モーションデータのデータオフセット。とりあえず0に設定。 */
                    fs.Write(BitConverter.GetBytes(0), 0, 4);

                    /* フレームレート */
                    byte[] byte_FrameRate = BitConverter.GetBytes(WorldTimer.frameRate);
                    fs.Write(byte_FrameRate, 0, byte_FrameRate.Length);

                    /* フレーム数 */
                    byte[] byte_DataCount = BitConverter.GetBytes(recordDataCount);
                    fs.Write(byte_DataCount, 0, byte_DataCount.Length);

                    for (int i = 0; i < recordDataCount; i++)
                    {
                        //データをシリアル化してFileStreamに書き込み
                        if (motionData[i] == null) { Debug.Log($"motionData[{i}] == null"); continue; }
                        motionData[i].SetBytes(buf);
                        fs.Write(buf, 0, bufSize);
                    }
                }
                Debug.Log($"Saved as SavedMotionData/{fileName}.bin");
            }
            catch (IOException e)
            {
                Debug.LogError(e);
            }
        }
    }
}
