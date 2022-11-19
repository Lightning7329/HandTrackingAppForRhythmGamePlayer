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

        private Button recordButton, saveButton, sceneChangeButton;

        void Start()
        {
            WorldTimer.Run();
            motionRecoder = GameObject.Find("Hands").GetComponent<MotionRecoder>();
            UISetting.SetButton(ref recordButton, "RecordButton", OnBtn_Record);
            UISetting.SetButton(ref saveButton, "SaveButton", OnBtn_Record);
            UISetting.SetButton(ref sceneChangeButton, "SceneChangeButton", OnBtn_SceneChange);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                motionRecoder.Save("TestMotion");
            }
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

        void OnBtn_Save()
        {
            motionRecoder.Save("TestMotion");
        }

        void OnBtn_SceneChange()
        {
            WorldTimer.Stop();
            UnityEngine.SceneManagement.SceneManager.LoadScene("VideoPlay");
        }
    }
}
