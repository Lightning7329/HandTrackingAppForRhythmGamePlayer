using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class PlayModeManager : MonoBehaviour
    {
        [SerializeField] bool isPlaying = false;
        [SerializeField] int motionOffset = 0;
        [SerializeField] private float skipSeconds = 5.0f;
        [SerializeField] private float speedChange = 0.05f;
        private float currentSpeed = 1.0f;

        // control側
        SliderController sliderController = null;
        MotionPlayer motionPlayer = null;
        VideoController videoController = null;
        CameraController cameraController = null;

        // uGUI側
        private Text txt_speed, txt_playButton, dataCount;
        private Button playButton, forwardButton, backwardButton, addSpeedButton, subSpeedButton, sceneChangeButton, fileSelectButton;
        private FileSelector fileSelector = null;
        public GameObject obj_fileSelector;

        void Start()
        {
            // control側
            sliderController = GameObject.Find("TimeSlider").GetComponent<SliderController>();
            motionPlayer = GameObject.Find("Hands").GetComponent<MotionPlayer>();
            videoController = GameObject.FindWithTag("Display").GetComponent<VideoController>();
            cameraController = Camera.main.GetComponent<CameraController>();

            // uGUI側
            //fileSelector = GameObject.Find("File Selection Panel").GetComponent<FileSelector>();
            fileSelector = obj_fileSelector.GetComponent<FileSelector>();
            UISetting.SetButton(ref fileSelectButton, "FileSelectButton", OnBtn_FileSelect, "Load");
            UISetting.SetButton(ref playButton, "PlayButton", OnBtn_Play, "Play");
            UISetting.SetButton(ref forwardButton, "ForwardButton", OnBtn_Forward, $"{skipSeconds}s");
            UISetting.SetButton(ref backwardButton, "BackwardButton", OnBtn_Backward, $"{skipSeconds}s");
            UISetting.SetButton(ref addSpeedButton, "AddSpeedButton", OnBtn_AddSpeed, "+0.05");
            UISetting.SetButton(ref subSpeedButton, "SubSpeedButton", OnBtn_SubSpeed, "-0.05");
            UISetting.SetButton(ref sceneChangeButton, "SceneChangeButton", OnBtn_SceneChange, "RecordMode");
            txt_speed = GameObject.Find("Speed").transform.Find("Text").gameObject.GetComponent<Text>();
            txt_speed.text = "x1.00";
            txt_playButton = playButton.GetComponentInChildren<Text>();
            dataCount = GameObject.Find("Play Data Count").GetComponent<Text>();
        }

        void Update()
        {
            int frame = motionPlayer.frame + motionPlayer.playbackOffset;
            frame = frame > 0 ? frame : 0;
            dataCount.text = "Data Count: " + frame.ToString();
            motionPlayer.playbackOffset = motionOffset;
        }

        void OnBtn_FileSelect()
        {
            // 再生中だったら再生を止める
            if (isPlaying) OnBtn_Play();

            cameraController.SetActive(false);
            fileSelector.List();
            StartCoroutine(LoadFile());
            cameraController.SetActive(true);
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

                var videoFilePath = Application.streamingAssetsPath + "/../Resources/Videos/" + fileName + ".MP4";
                if (System.IO.File.Exists(videoFilePath))
                {
                    sliderController.enabled = false;   // VideoPlayer側のPrepareが終わったらtrueに戻る
                    videoController.SetVideoClip(fileName);
                }
                else
                {
                    Debug.LogError($"VideoClip {fileName} could not be found.");
                }
            }
        }

        void OnBtn_Play()
        {
            if (!motionPlayer.isLoaded)
            {
                Debug.LogError("モーションデータがロードされていません");
                return;
            }

            if (isPlaying)
            {
                txt_playButton.text = "Play";
                videoController.PausePlaying();
                motionPlayer.PausePlaying();
                isPlaying = false;
            }
            else
            {
                txt_playButton.text = "Pause";
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
            ChangeSpeed(currentSpeed += speedChange);
        }

        void OnBtn_SubSpeed()
        {
            ChangeSpeed(currentSpeed -= speedChange);
        }

        void ChangeSpeed(float newSpeed)
        {
            // control側
            videoController.ChangeSpeed(newSpeed);

            // uGUI側
            txt_speed.text = $"x{newSpeed:F2}";
        }

        void OnBtn_SceneChange()
        {
            cameraController.HoldCurrentSceneTransform();
            UnityEngine.SceneManagement.SceneManager.LoadScene("VideoRecord");
        }
    }
}
