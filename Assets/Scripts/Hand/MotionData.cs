using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KW_Mocap
{
    public class MotionData
    {
        public HandData left, right;
    }

    public class HandData
    {
        public Transform palm;
        public Quaternion[] quaternions = new Quaternion[10];
    }
}
