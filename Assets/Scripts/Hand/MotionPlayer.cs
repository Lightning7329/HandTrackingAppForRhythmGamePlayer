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
        [SerializeField] GameObject left, right;
        HandSetting leftHandSetting, rightHandSetting;
        private bool isPlaying = false;
        public bool isLoaded { get; private set; } = false;
        public int frameCount { get; private set; } = 0;
        public float frameRate { get; private set; } = 30.0f;
        [HideInInspector] public int playbackOffset = 0;
        private string currentMotionDataFilePath = null;

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
            leftHandSetting = left.GetComponent<HandSetting>();
            leftHandSetting.SetMaterial(true);
            rightHandSetting = right.GetComponent<HandSetting>();
            rightHandSetting.SetMaterial(true);
        }

        void Update()
        {
            if (isPlaying) Play();
        }

        /// <summary>
        /// 現在のフレームにオフセットを加えたモーションを再生
        /// </summary>
        public void Play() => this.Play(frame + playbackOffset);
        /// <summary>
        /// 各フレームの動き（位置と回転）を記述
        /// </summary>
        public void Play(int n)
        {
            if (n < 0 || frameCount <= n) return;
            
            left.transform.localPosition = motionData[n].left.palmPos;
            left.transform.localRotation = motionData[n].left.palmRot;
            //leftHandSetting.SetJointsRotation(motionData[n].left.jointRot);
            right.transform.localPosition = motionData[n].right.palmPos;
            right.transform.localRotation = motionData[n].right.palmRot;
            //rightHandSetting.SetJointsRotation(motionData[n].right.jointRot);
        }

        public void StartPlaying()
        {
            if (!isLoaded) throw new MotionDataNotLoadedException();
            if (frameCount == 0) return;
            if (isPlaying) return;

            isPlaying = true;
            Debug.Log("Start Playing");
        }

        public void PausePlaying()
        {
            if (!isPlaying) return;

            isPlaying = false;
            Debug.Log("Stop Playing");
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
                    /* Virtual Deskに対する相対位置 */
                    fs.Read(buf, 0, 12);
                    if (fileName == "Echo over you_Hard40FPS")
                        this.transform.localPosition = new Vector3(100f, 2.8599999f, 102.510002f);
                    else this.transform.localPosition = ExtendedBitConverter.GetVector3FromBytes(buf, 0).vector3;

                    /* Displayのサイズ */
                    fs.Read(buf, 0, 12);
                    GameObject.FindWithTag("Display").transform.localScale = ExtendedBitConverter.GetVector3FromBytes(buf, 0).vector3;

                    /* モーションデータのデータオフセット */
                    fs.Read(buf, 0, 4);
                    this.playbackOffset = BitConverter.ToInt32(buf, 0);

                    /* フレームレート */
                    fs.Read(buf, 0, 4);
                    this.frameRate = BitConverter.ToSingle(buf, 0);

                    /* フレーム数 */
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
                this.currentMotionDataFilePath = pass;
                Debug.Log($"Motion Loaded " + fileName);
            }
            catch (IOException e)
            {
                Debug.Log(e);
            }
        }

        public void SavePlaybackOffset()
        {
            try
            {
                using (FileStream fileStream = new FileStream(this.currentMotionDataFilePath, FileMode.Open, FileAccess.Write))
                {
                    fileStream.Seek(12, SeekOrigin.Begin);
                    byte[] buf = BitConverter.GetBytes(this.playbackOffset);
                    fileStream.Write(buf, 0, 4);
                }
                Debug.Log($"MotionOffset saved: " + this.playbackOffset);
            }
            catch (IOException e)
            {
                Debug.Log(e);
            }
        }
    }
}
