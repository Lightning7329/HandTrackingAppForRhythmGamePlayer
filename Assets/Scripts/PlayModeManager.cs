using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class PlayModeManager : MonoBehaviour
    {
        [SerializeField] private float neutralSkipSeconds = 5.0f;
        [SerializeField] private float speedChange = 0.05f;
        [SerializeField] private float minSpeed = 0.25f;
        [SerializeField] private float maxSpeed = 2.00f;
        private bool isPlaying = false;
        private float skipSeconds;
        private float currentSpeed = 1.0f;

        // control側
        SliderController sliderController = null;
        MotionPlayer motionPlayer = null;
        VideoController videoController = null;
        CameraController cameraController = null;
        OffsetManager offsetManager = null;

        // uGUI側
        private Text txt_speed, dataCount;
        private Button fileSelectButton, playButton, forwardButton, backwardButton, addSpeedButton, subSpeedButton, sceneChangeButton, rotateClockwiseButton, rotateAnticlockwiseButton;
        private FileSelector fileSelector = null;
        [SerializeField] GameObject obj_fileSelector;

        void Start()
        {
            // control側
            sliderController = GameObject.Find("TimeSlider").GetComponent<SliderController>();
            motionPlayer = GameObject.Find("Hands").GetComponent<MotionPlayer>();
            videoController = GameObject.FindWithTag("Display").GetComponent<VideoController>();
            cameraController = Camera.main.GetComponent<CameraController>();
            offsetManager = new OffsetManager(GameObject.Find("Canvas/Motion Offset Panel"), motionPlayer);
            skipSeconds = neutralSkipSeconds;

            // uGUI側
            UISetting.SetButton(ref fileSelectButton, "FileSelectButton", OnBtn_FileSelect, "Load");
            UISetting.SetButton(ref playButton, "PlayButton", OnBtn_Play, "Play");
            UISetting.SetButton(ref forwardButton, "ForwardButton", OnBtn_Forward, $"{neutralSkipSeconds}s");
            UISetting.SetButton(ref backwardButton, "BackwardButton", OnBtn_Backward, $"{neutralSkipSeconds}s");
            UISetting.SetButton(ref sceneChangeButton, "SceneChangeButton", OnBtn_SceneChange, "RecordMode");
            fileSelector = obj_fileSelector.GetComponent<FileSelector>();

            Transform speedPanel = GameObject.Find("Speed Panel").transform;
            txt_speed = speedPanel.Find("Speed").GetComponent<Text>();
            txt_speed.text = "x1.00";
            UISetting.SetButton(ref addSpeedButton, speedPanel, "AddSpeedButton", OnBtn_AddSpeed, $"+{speedChange:F2}");
            UISetting.SetButton(ref subSpeedButton, speedPanel, "SubSpeedButton", OnBtn_SubSpeed, $"-{speedChange:F2}");

            dataCount = GameObject.Find("Play Data Count").GetComponent<Text>();
        }

        void Update()
        {
            int frame = motionPlayer.frame + motionPlayer.playbackOffset;
            frame = frame > 0 ? frame : 0;
            dataCount.text = "Data Count: " + frame.ToString();
        }

        void OnBtn_FileSelect()
        {
            // 再生中だったら再生を止める
            if (isPlaying) OnBtn_Play();

            cameraController.SetActive(false);
            fileSelector.List();
            StartCoroutine(LoadFile());
        }

        IEnumerator LoadFile()
        {
            while (fileSelector.selectState == FileSelector.SelectState.NotSelected || fileSelector.selectState == FileSelector.SelectState.Selecting)
            {
                yield return null;
            }

            if (fileSelector.selectState == FileSelector.SelectState.Selected)
            {
                string fileName = fileSelector.fileNameToLoad;
                if (motionPlayer != null)
                {
                    motionPlayer.Load(fileName);
                }
                motionPlayer.ResetFrameCount();
                offsetManager.MotionOffset = motionPlayer.playbackOffset;

                var videoFilePath = Application.streamingAssetsPath + "/../Resources/Videos/" + fileName;
                if (System.IO.File.Exists(videoFilePath + ".MP4") || System.IO.File.Exists(videoFilePath + ".MOV"))
                {
                    sliderController.enabled = false;   // VideoPlayer側のPrepareが終わったらtrueに戻る
                    videoController.SetVideoClip(fileName);
                }
                else
                {
                    Debug.LogError($"VideoClip {fileName} could not be found.");
                }
            }
            cameraController.SetActive(true);
        }

        void OnBtn_Play()
        {
            if (!motionPlayer.isLoaded)
            {
                Debug.LogError("Motion Data has not been loaded.");
                return;
            }

            if (isPlaying)
            {
                playButton.SetButtonText("Play");
                videoController.PausePlaying();
                motionPlayer.PausePlaying();
                isPlaying = false;
            }
            else
            {
                playButton.SetButtonText("Pause");
                videoController.StartPlaying();
                motionPlayer.StartPlaying();
                isPlaying = true;
            }
        }

        void OnBtn_Forward()
        {
            sliderController.Skip(skipSeconds);
        }

        void OnBtn_Backward()
        {
            sliderController.Skip(-skipSeconds);
        }

        void OnBtn_AddSpeed()
        {
            if (currentSpeed > maxSpeed - speedChange / 2) return;

            ChangeSpeed(currentSpeed += speedChange);
            if (currentSpeed > maxSpeed - speedChange / 2)
            {
                addSpeedButton.interactable = false;
            }
            subSpeedButton.interactable = true;
        }

        void OnBtn_SubSpeed()
        {
            if (currentSpeed < minSpeed + speedChange / 2) return;

            ChangeSpeed(currentSpeed -= speedChange);
            if (currentSpeed < minSpeed + speedChange / 2)
            {
                subSpeedButton.interactable = false;
            }
            addSpeedButton.interactable = true;
        }

        void ChangeSpeed(float newSpeed)
        {
            // control側
            videoController.ChangeSpeed(newSpeed);

            // uGUI側
            skipSeconds = neutralSkipSeconds * currentSpeed;
            string newSkipSeconds = skipSeconds.ToString("F2").TrimEnd('0').TrimEnd('.') + "s";
            forwardButton.SetButtonText(newSkipSeconds);
            backwardButton.SetButtonText(newSkipSeconds);
            txt_speed.text = $"x{newSpeed:F2}";
        }

        void OnBtn_SceneChange()
        {
            cameraController.HoldCurrentSceneTransform();
            UnityEngine.SceneManagement.SceneManager.LoadScene("VideoRecord");
        }
    }
}
