using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class RecordModeManager : MonoBehaviour
    {
        bool isRecording = false;
        MotionRecorder motionRecorder = null;
        VideoCapture videoCapture = null;
        CameraController cameraController = null;
        Button recordButton, openInputPanelButton, saveButton, cancelButton, sceneChangeButton;
        GameObject FileNameInputPanel;
        InputField fileNameInputField;
        Text caution;
        Text dataCount;

        void Start()
        {
            WorldTimer.Run();
            motionRecorder = GameObject.Find("Hands").GetComponent<MotionRecorder>();
            videoCapture = GameObject.FindWithTag("Display").GetComponent<VideoCapture>();
            cameraController = Camera.main.GetComponent<CameraController>();
            UISetting.SetButton(ref recordButton, "RecordButton", OnBtn_Record);
            UISetting.SetButton(ref openInputPanelButton, "OpenInputPanelButton", OnBtn_OpenInputPanel);
            UISetting.SetButton(ref sceneChangeButton, "SceneChangeButton", OnBtn_SceneChange);
            dataCount = GameObject.Find("DataCount").GetComponent<Text>();

            /* FileName Input Panelの設定 */
            FileNameInputPanel = GameObject.Find("FileName Input Panel");
            UISetting.SetButton(ref saveButton, "Save", () => StartCoroutine(OnBtn_Save()));
            UISetting.SetButton(ref cancelButton, "Cancel", OnBtn_Cancel);
            fileNameInputField = FileNameInputPanel.GetComponentInChildren<InputField>();
            caution = FileNameInputPanel.transform.Find("Caution").GetComponent<Text>();
            caution.gameObject.SetActive(false);
            FileNameInputPanel.SetActive(false);
        }

        void Update()
        {
            dataCount.text = "Data Count: " + motionRecorder.recordDataCount.ToString();
        }

        void OnBtn_Record()
        {
            Text t = recordButton.GetComponentInChildren<Text>();
            if (isRecording)
            {
                t.text = "Rec";
                t.color = Color.black;
                recordButton.GetComponent<Image>().color = Color.white;
                motionRecorder.StopRecording();
                videoCapture.StopRecording();
                isRecording = false;
            }
            else
            {
                t.text = "Stop";
                t.color = Color.white;
                recordButton.GetComponent<Image>().color = Color.red;
                motionRecorder.StartRecording();
                videoCapture.StartRecording();
                isRecording = true;
            }
        }

        void OnBtn_OpenInputPanel()
        {
            FileNameInputPanel.SetActive(true);
            cameraController.SetActive(false);
            var now = DateTime.Now;
            fileNameInputField.text = now.ToString("yyyyMMdd-HHmmss");
            fileNameInputField.ActivateInputField();
        }

        IEnumerator OnBtn_Save()
        {
            string fileName = fileNameInputField.text;
            if(fileName == "")
            {
                caution.text = "Input File Name.";
                caution.gameObject.SetActive(true);
                yield break;
            }

            try
            {
                motionRecorder.Save(fileName);   //ファイル名に問題があればこの行で例外スロー
            }
            catch(DuplicateFileNameException e)
            {
                caution.text = e.ToString();
                caution.gameObject.SetActive(true);
            }

            /* 動画の保存処理の終了を待ってからFileName Input Panelを閉じる */
            yield return videoCapture.Save(fileName);
            CloseInputPanel();
        }

        void OnBtn_Cancel()
        {
            CloseInputPanel();
        }

        /// <summary>
        /// ファイル名入力画面を閉じるとき（SaveまたはCancelが押されたとき）の共通処理
        /// </summary>
        private void CloseInputPanel()
        {
            fileNameInputField.text = "";
            caution.gameObject.SetActive(false);
            FileNameInputPanel.SetActive(false);
            cameraController.SetActive(true);
        }

        void OnBtn_SceneChange()
        {
            WorldTimer.Stop();
            cameraController.HoldCurrentSceneTransform();
            UnityEngine.SceneManagement.SceneManager.LoadScene("VideoPlay");
        }
    }
}
