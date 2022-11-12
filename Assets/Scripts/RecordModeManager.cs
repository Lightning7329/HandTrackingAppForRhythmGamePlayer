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

        private Button recordButton;

        void Start()
        {
            WorldTimer.Run();
            motionRecoder = GameObject.Find("Hands").GetComponent<MotionRecoder>();
            UISetting.SetButton(ref recordButton, "RecordButton", OnBtn_Record, "Rec");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                motionRecoder.Save("TestMotion2");
            }
        }

        void OnBtn_Record()
        {
            Text t = recordButton.transform.Find("Text").GetComponent<Text>();
            if (isRecording)
            {
                t.text = "Rec";
                motionRecoder.StopRecording();
                isRecording = false;
            }
            else
            {
                t.text = "Stop";
                motionRecoder.StartRecording();
                isRecording = true;
            }
        }
    }
}
