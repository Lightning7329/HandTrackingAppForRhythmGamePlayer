using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class PlayModeManager : MonoBehaviour
    {
        [SerializeField] bool isPlaying = false;
        private float currentSpeed = 1.0f;
        [SerializeField] private float speedChange = 0.05f;
        [SerializeField] private int skipSeconds = 5;

        MotionPlayer motionPlayer = null;

        VideoController videoController = null;

        // uGUI側
        private Text txt_speed, txt_playButton;
        private Button playButton, forwardButton, backwardButton, addSpeedButton, subSpeedButton, sceneChangeButton;


        void Start()
        {
            //WorldTimer.DisplayFrameCount();
            WorldTimer.Run();
            //motionPlayer = GameObject.Find("Hands").GetComponent<MotionPlayer>();

            // video側
            videoController = GameObject.Find("Display for Play").GetComponent<VideoController>();

            // uGUI側
            UISetting.SetButton(ref playButton, "PlayButton", OnBtn_Play, "Play");
            UISetting.SetButton(ref forwardButton, "ForwardButton", OnBtn_Forward, $"{skipSeconds}s");
            UISetting.SetButton(ref backwardButton, "BackwardButton", OnBtn_Backward, $"{skipSeconds}s");
            UISetting.SetButton(ref addSpeedButton, "AddSpeedButton", OnBtn_AddSpeed, "+0.05");
            UISetting.SetButton(ref subSpeedButton, "SubSpeedButton", OnBtn_SubSpeed, "-0.05");
            UISetting.SetButton(ref sceneChangeButton, "SceneChangeButton", OnBtn_SceneChange, "RecordMode");
            txt_speed = GameObject.Find("Speed").transform.Find("Text").gameObject.GetComponent<Text>();
            txt_speed.text = "x1.00";
            txt_playButton = playButton.transform.Find("Text").GetComponent<Text>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (motionPlayer != null)
                {
                    string fileName = "TestMotion";
                    motionPlayer.Load(fileName);
                }
            }
        }

        void OnBtn_Play()
        {
            if (isPlaying)
            {
                txt_playButton.text = "Play";
                videoController.PausePlaying();
                motionPlayer?.PausePlaying();
                isPlaying = false;
            }
            else
            {
                txt_playButton.text = "Pause";
                videoController.StartPlaying();
                motionPlayer?.StartPlaying();
                isPlaying = true;
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

        void OnBtn_SceneChange()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("VideoRecord");
        }
    }
}
