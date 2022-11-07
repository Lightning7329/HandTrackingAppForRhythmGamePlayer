using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using KW_Mocap;

public class Logging : MonoBehaviour
{
    LeapHandModel leap = null;

    void Start()
    {
        leap = GameObject.Find("Hand-L").GetComponent<LeapHandModel>();
    }

    void Update()
    {

    }
}
