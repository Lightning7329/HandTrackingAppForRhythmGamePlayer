using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KW_Mocap
{
    public class RecordModeManager : MonoBehaviour
    {
        MotionRecoder motionRecoder = null;

        // Start is called before the first frame update
        void Start()
        {
            WorldTimer.Run();
            motionRecoder = GameObject.Find("Hands").GetComponent<MotionRecoder>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                motionRecoder.StartRecording();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                motionRecoder.StopRecording();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                motionRecoder.Save("TestMotion2");
            }
        }
    }
}
