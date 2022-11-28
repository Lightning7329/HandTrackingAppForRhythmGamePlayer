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
        [SerializeField] private float skipSeconds = 5.0f;

        MotionPlayer motionPlayer = null;

        VideoController videoController = null;

        CameraController cameraController = null;

        // uGUI側
        private Text txt_speed, txt_playButton;
        private Button playButton, forwardButton, backwardButton, addSpeedButton, subSpeedButton, sceneChangeButton, fileSelectButton;
        private FileSelector fileSelector = null;
        public GameObject obj_fileSelector;

        void Start()
        {
            //WorldTimer.DisplayFrameCount();
            WorldTimer.Run();
            motionPlayer = GameObject.Find("Hands").GetComponent<MotionPlayer>();

            // video側
            videoController = GameObject.Find("Display for Play").GetComponent<VideoController>();
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
        }

        void Update()
        {

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
                videoController.SetVideoClip(fileName);
            }
            cameraController.SetActive(true);
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
            // motion側
            motionPlayer.Skip(skipSeconds);

            // video側
            videoController.Skip(skipSeconds);
        }

        void OnBtn_Backward()
        {
            // motion側
            motionPlayer.Skip(-skipSeconds);

            // video側
            videoController.Skip(-skipSeconds);
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
            // motion側
            motionPlayer.ChangeSpeed(newSpeed);

            // video側
            videoController.ChangeSpeed(newSpeed);

            // uGUI側
            txt_speed.text = $"x{newSpeed:F2}";
        }

        void OnBtn_SceneChange()
        {
            WorldTimer.Stop();
            UnityEngine.SceneManagement.SceneManager.LoadScene("VideoRecord");
        }
    }
}
