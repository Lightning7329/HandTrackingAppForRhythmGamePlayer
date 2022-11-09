using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KW_Mocap
{
    public class MotionPlayer : MonoBehaviour
    {
        MotionData[] motionData = null;
        bool isSet = false;
        bool isPlaying = false;
        int playDataCount = 0;
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
            // TODO: leftJoint[0]~leftKJoint[8]のモーションデータも再生する。rightも然り。

            leftJoints[9].transform.position = motionData[playDataCount].left.palmPos;
            rightJoints[9].transform.position = motionData[playDataCount].right.palmPos;
        }

        public void StartPlaying()
        {
            if (!isSet) return;
            if (isPlaying) return;

            isPlaying = true;
            WorldTimer.CountUp += PlayDataCountUp;
        }

        public void StopPlaying()
        {
            if (!isPlaying) return;

            isPlaying = false;
            WorldTimer.CountUp -= PlayDataCountUp;
        }

        void PlayDataCountUp()
        {
            playDataCount++;
        }

        public void SetMotionData(MotionData[] motionData)
        {
            this.motionData = motionData;
            isSet = true;
        }
    }
}
