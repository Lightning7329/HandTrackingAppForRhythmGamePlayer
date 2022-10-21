using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class Logging : MonoBehaviour
{
    SS_LEAP leap = null;

    void Start()
    {
        leap = GameObject.Find("Hand-L").GetComponent<SS_LEAP>();
    }

    void Update()
    {

    }
}
