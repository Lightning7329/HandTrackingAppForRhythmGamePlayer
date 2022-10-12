using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform cameraTransform;
    [SerializeField] private float moveSpead = 0.02f;
    [SerializeField] private float rotateSpead = 0.07f;
    [SerializeField] private float zoomSpead = 0.03f;

    void Start()
    {
        cameraTransform = GetComponent<Transform>();
    }

    void Update()
    {
        TransformPos();
        TransformRot();
        Zoom();
    }

    private void TransformPos()
    {
        if (Input.GetKey(KeyCode.W)) cameraTransform.position += new Vector3(0.0f, 0.0f, moveSpead);
        if (Input.GetKey(KeyCode.A)) cameraTransform.position += new Vector3(-moveSpead, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.S)) cameraTransform.position += new Vector3(0.0f, 0.0f, -moveSpead);
        if (Input.GetKey(KeyCode.D)) cameraTransform.position += new Vector3(moveSpead, 0.0f, 0.0f);
    }

    private void TransformRot()
    {
        Vector3 ang = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftArrow)) ang.y += rotateSpead;
        else if (Input.GetKey(KeyCode.RightArrow)) ang.y -= rotateSpead;
        if (Input.GetKey(KeyCode.UpArrow)) ang.x -= rotateSpead;
        else if (Input.GetKey(KeyCode.DownArrow)) ang.x += rotateSpead;
        cameraTransform.Rotate(ang, Space.Self);
    }

    private void Zoom()
    {
        if (Input.GetKey(KeyCode.F)) cameraTransform.position -= transform.forward * zoomSpead;
        else if (Input.GetKey(KeyCode.N)) cameraTransform.position += transform.forward * zoomSpead;
    }
}
