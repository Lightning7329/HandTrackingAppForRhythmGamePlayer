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

        // Start is called before the first frame update
        void Start()
        {
            WorldTimer.Run();
            motionRecoder = GameObject.Find("Hands").GetComponent<MotionRecoder>();
            UISetting.SetButton(ref recordButton, "RecordButton", OnBtn_Record, "Rec");
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                motionRecoder.Save("TestMotion2");
            }
        }

        void OnBtn_Record()
        {
            if (isRecording)
            {
                motionRecoder.StopRecording();
            }
            else
            {
                motionRecoder.StartRecording();
            }
        }
    }
}
