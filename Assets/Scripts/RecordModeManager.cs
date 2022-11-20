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
        MotionRecoder motionRecoder = null;
        CameraController cameraController = null;
        Button recordButton, openInputPanelButton, saveButton, cancelButton, sceneChangeButton;
        GameObject FileNameInputPanel;
        InputField fileNameInputField;
        Text caution;

        void Start()
        {
            WorldTimer.Run();
            motionRecoder = GameObject.Find("Hands").GetComponent<MotionRecoder>();
            cameraController = Camera.main.GetComponent<CameraController>();
            UISetting.SetButton(ref recordButton, "RecordButton", OnBtn_Record);
            UISetting.SetButton(ref openInputPanelButton, "OpenInputPanelButton", OnBtn_OpenInputPanel);
            UISetting.SetButton(ref sceneChangeButton, "SceneChangeButton", OnBtn_SceneChange);

            /* FileName Input Panelの設定 */
            FileNameInputPanel = GameObject.Find("FileName Input Panel");
            UISetting.SetButton(ref saveButton, "Save", OnBtn_Save);
            UISetting.SetButton(ref cancelButton, "Cancel", OnBtn_Cancel);
            fileNameInputField = FileNameInputPanel.GetComponentInChildren<InputField>();
            caution = FileNameInputPanel.transform.Find("Caution").GetComponent<Text>();
            caution.gameObject.SetActive(false);
            FileNameInputPanel.SetActive(false);
        }

        void Update()
        {

        }

        void OnBtn_Record()
        {
            Text t = recordButton.GetComponentInChildren<Text>();
            if (isRecording)
            {
                t.text = "Rec";
                t.color = Color.black;
                recordButton.GetComponent<Image>().color = Color.white;
                motionRecoder.StopRecording();
                isRecording = false;
            }
            else
            {
                t.text = "Stop";
                t.color = Color.white;
                recordButton.GetComponent<Image>().color = Color.red;
                motionRecoder.StartRecording();
                isRecording = true;
            }
        }

        void OnBtn_OpenInputPanel()
        {
            FileNameInputPanel.SetActive(true);
            cameraController.SetActive(false);
            var now = DateTime.Now;
            fileNameInputField.text = now.ToString("yyyyMMdd-hhmmss");
            fileNameInputField.ActivateInputField();
        }

        void OnBtn_Save()
        {
            string fileName = fileNameInputField.text;
            try
            {
                motionRecoder.Save(fileName);
                CloseInputPanel();
            }
            catch(DuplicateFileNameException e)
            {
                caution.text = e.ToString();
                caution.gameObject.SetActive(true);
            }
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
            UnityEngine.SceneManagement.SceneManager.LoadScene("VideoPlay");
        }
    }
}
