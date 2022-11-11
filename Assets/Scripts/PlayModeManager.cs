using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class PlayModeManager : MonoBehaviour
    {
        bool isPlaying = false;
        private float currentSpeed = 1.0f;
        [SerializeField] private float speedChange = 0.05f;
        [SerializeField] private int skipSeconds = 5;

        MotionPlayer motionPlayer = null;

        VideoController videoController = null;

        // uGUI側
        private Text txt_speed;
        private Button playButton, forwardButton, backwardButton, addSpeedButton, subSpeedButton;


        void Start()
        {
            //WorldTimer.DisplayFrameCount();
            WorldTimer.Run();
            motionPlayer = GameObject.Find("Hands").GetComponent<MotionPlayer>();

            // video側
            videoController = GameObject.Find("Display for Play").GetComponent<VideoController>();

            // uGUI側
            SetButton(ref playButton, "PlayButton", OnBtn_Play, "Play");
            SetButton(ref forwardButton, "ForwardButton", OnBtn_Forward, $"{skipSeconds}s");
            SetButton(ref backwardButton, "BackwardButton", OnBtn_Backward, $"{skipSeconds}s");
            SetButton(ref addSpeedButton, "AddSpeedButton", OnBtn_AddSpeed, "+0.05");
            SetButton(ref subSpeedButton, "SubSpeedButton", OnBtn_SubSpeed, "-0.05");
            txt_speed = GameObject.Find("Speed").transform.Find("Text").gameObject.GetComponent<Text>();
            txt_speed.text = "x1.00";
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (motionPlayer != null)
                {
                    string fileName = "TestMotion";
                    // TODO: 便宜的にstaticにしたLoadメソッドを修正
                    //MotionRecoder motionRecorder = new MotionRecoder();
                    MotionData[] motionData = MotionRecoder.Load(fileName);

                    motionPlayer.SetMotionData(motionData);
                }
            }

        }

        void OnBtn_Play()
        {
            // motion側
            motionPlayer.StartPlaying();

            // video側
            videoController.TogglePlayAndPause();

            // uGUI側
            Text t = playButton.transform.Find("Text").GetComponent<Text>();
            if (isPlaying)
            {
                t.text = "Play";
                isPlaying = false;
            }
            else
            {
                t.text = "Pause";
                isPlaying = false;
            }
        }

        void OnBtn_Forward()
        {
            // motion側
            // TODO

            // video側
            videoController.Skip(skipSeconds);
        }

        void OnBtn_Backward()
        {
            // motion側
            // TODO

            // video側
            videoController.Skip(-skipSeconds);
        }

        void OnBtn_AddSpeed()
        {
            // motion側
            // TODO

            // video側
            videoController.ChangeSpeed(currentSpeed += speedChange);

            // uGUI側
            txt_speed.text = $"x{currentSpeed:F2}";
        }

        void OnBtn_SubSpeed()
        {
            // motion側
            // TODO

            // video側
            videoController.ChangeSpeed(currentSpeed -= speedChange);

            // uGUI側
            txt_speed.text = $"x{currentSpeed:F2}";
        }

        private void SetButton(ref Button button, string name, UnityEngine.Events.UnityAction call, string text)
        {
            button = GameObject.Find(name).GetComponent<Button>();
            button.onClick.AddListener(call);
            button.transform.Find("Text").GetComponent<Text>().text = text;
        }
    }
}
