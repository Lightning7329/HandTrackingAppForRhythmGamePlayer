using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class CameraController : MonoBehaviour
    {
        private bool isActive = true;
        Transform cameraTransform;
        Button camera1, camera2;
        [SerializeField] private float moveSpead = 0.02f;
        [SerializeField] private float rotateSpead = 0.07f;
        [SerializeField] private float zoomSpead = 0.03f;

        void Start()
        {
            cameraTransform = GetComponent<Transform>();
            UISetting.SetButton(ref camera1, "Camera1", OnBtn_Camera1);
            UISetting.SetButton(ref camera2, "Camera2", OnBtn_Camera2);
        }

        void Update()
        {
            if (!isActive) return;
            TransformPos();
            TransformRot();
            Zoom();
        }

        public void SetActive(bool flg) => isActive = flg;

        /// <summary>
        /// カメラの平行移動
        /// </summary>
        private void TransformPos()
        {
            if (Input.GetKey(KeyCode.W)) cameraTransform.position += new Vector3(0.0f, 0.0f, moveSpead);
            if (Input.GetKey(KeyCode.A)) cameraTransform.position += new Vector3(-moveSpead, 0.0f, 0.0f);
            if (Input.GetKey(KeyCode.S)) cameraTransform.position += new Vector3(0.0f, 0.0f, -moveSpead);
            if (Input.GetKey(KeyCode.D)) cameraTransform.position += new Vector3(moveSpead, 0.0f, 0.0f);
        }

        /// <summary>
        /// カメラのローカル座標周りの回転
        /// </summary>
        private void TransformRot()
        {
            Vector3 ang = Vector3.zero;
            if (Input.GetKey(KeyCode.LeftArrow)) ang.y += rotateSpead;
            else if (Input.GetKey(KeyCode.RightArrow)) ang.y -= rotateSpead;
            if (Input.GetKey(KeyCode.UpArrow)) ang.x -= rotateSpead;
            else if (Input.GetKey(KeyCode.DownArrow)) ang.x += rotateSpead;
            cameraTransform.Rotate(ang, Space.Self);
        }

        /// <summary>
        /// カメラのズームイン/アウト
        /// </summary>
        private void Zoom()
        {
            if (Input.GetKey(KeyCode.F)) cameraTransform.position -= transform.forward * zoomSpead;
            else if (Input.GetKey(KeyCode.N)) cameraTransform.position += transform.forward * zoomSpead;
        }

        /// <summary>
        /// Camera1ボタンが押されたときに実行。カメラ位置をデフォルトに戻す。
        /// </summary>
        private void OnBtn_Camera1()
        {
            cameraTransform.position = new Vector3(0.0f, 19.95238f, -11.01941f);
            cameraTransform.rotation = Quaternion.Euler(new Vector3(60.27f, 0.0f, 0.0f));
        }

        /// <summary>
        /// Camera2ボタンが押されたときに実行。カメラ位置を俯瞰視点にする。
        /// </summary>
        private void OnBtn_Camera2()
        {
            cameraTransform.position = new Vector3(0.0f, 18.2f, 0.0f);
            cameraTransform.rotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));
        }
    }
}
