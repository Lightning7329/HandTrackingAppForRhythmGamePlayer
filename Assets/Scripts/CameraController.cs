using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Transform cameraTransform;
    //public GameObject cameraPivot;
    public GameObject display;
    // Start is called before the first frame update
    void Start()
    {
        //cameraTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.transform.LookAt(display.GetComponent<Transform>().transform);
    }
}
