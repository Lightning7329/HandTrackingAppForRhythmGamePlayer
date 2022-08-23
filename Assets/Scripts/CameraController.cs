using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform cameraTransform;
    //public GameObject cameraPivot;
    [SerializeField] private GameObject display;
    [SerializeField] private float cameraPos_yz;
    [SerializeField] private float center;

    void Start()
    {
        cameraTransform = GetComponent<Transform>();
    }

    void Update()
    {
        Camera.main.transform.LookAt(display.GetComponent<Transform>().transform);
        cameraTransform.position = position();
    }

    Vector3 position()
    {
        float r = center - cameraTransform.position.z;
        float x = r * Mathf.Sin(cameraPos_yz);
        float z = center - r * Mathf.Cos(cameraPos_yz);
        return new Vector3(x, cameraTransform.position.y, z);
    }
}
