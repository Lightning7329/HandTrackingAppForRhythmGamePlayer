using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KW_Mocap
{
    /// <summary>
    /// カメラの移動を扱うクラス。カメラコンポーネントと一緒に使う。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        #region StaticField
        /* シーンを跨ぐときに最後の位置と回転を記憶するための変数 */
        private static Vector3 lastPosition = CameraControllerPreferences.pos1;
        private static Quaternion lastRotation = CameraControllerPreferences.rot1;
        #endregion

        #region Field
        private bool isActive = true;
        private Vector3 preMousePos;
        Button camera1, camera2, saveCamera1, saveCamera2, resetCamera1, resetCamera2;

        [SerializeField]
        private Vector3 rotationCenter = Vector3.zero;

        [SerializeField, Range(0.0f, 100.0f)]
        private float zoomSpeed = 50.0f;
        private float ZoomSpeed
        {
            get => zoomSpeed * 10.0f;
        }

        [SerializeField, Range(0.0f, 100.0f)]
        private float moveSpead = 50.0f;
        private float MoveSpeed
        {
            get => moveSpead * 0.5f;
        }

        [SerializeField]
        private RotateSpeed rotateSpeed = new RotateSpeed(50.0f, 50.0f);
        #endregion

        /// <summary>
        /// ファイル名入力画面などでは有効になってほしくないので、他のクラスに無効にしてもらうためのメソッド。
        /// </summary>
        /// <param name="flg">有効か無効か</param>
        public void SetActive(bool flg) => isActive = flg;

        void Start()
        {
            SetLastSceneTransform();
            UISetting.SetButton(ref camera1, "Camera1", () => SetTransform(1));
            UISetting.SetButton(ref camera2, "Camera2", () => SetTransform(2));
            UISetting.SetButton(ref saveCamera1, "SaveCamera1", () => SaveTransform(1, transform.localPosition, transform.localRotation));
            UISetting.SetButton(ref saveCamera2, "SaveCamera2", () => SaveTransform(2, transform.localPosition, transform.localRotation));
            UISetting.SetButton(ref resetCamera1, "ResetCamera1", () => { ResetSavedTransform(1); SetTransform(1); });
            UISetting.SetButton(ref resetCamera2, "ResetCamera2", () => { ResetSavedTransform(2); SetTransform(2); });
            rotationCenter = GameObject.FindWithTag("Display").GetComponent<Transform>().position;
        }

        #region CameraMotion
        void Update()
        {
            if (!isActive) return;

            MouseControl();
            KeyControl();
        }

        /// <summary>
        /// 水平方向のカメラの平行移動
        /// </summary>
        /// <param name="amout">正だと右。負だと左。</param>
        private void HorizontalMove(float amout)
        {
            transform.Translate(amout * MoveSpeed * Time.deltaTime * transform.right, Space.World);
        }

        /// <summary>
        /// 垂直方向のカメラの平行移動
        /// </summary>
        /// <param name="amout">正だと前進。負だと後退。</param>
        private void VerticalMove(float amout)
        {
            /* transform.forwardベクトルのxz平面への正射影を正規化したベクトル */
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            transform.Translate(amout * MoveSpeed * Time.deltaTime * forward, Space.World);
        }

        /// <summary>
        /// カメラのズームイン/アウト
        /// </summary>
        /// <param name="amount">正だと前進。負だと後退。</param>
        private void Zoom(float amount)
        {
            bool goingUp = !(transform.forward.y > 0 ^ amount > 0);
            bool canGoUp = goingUp && transform.position.y < CameraControllerPreferences.ceiling;
            bool canGoDown = !goingUp && transform.position.y > CameraControllerPreferences.floor;
            if (canGoUp || canGoDown)
                transform.Translate(amount * ZoomSpeed * Time.deltaTime * transform.forward, Space.World);
        }

        /// <summary>
        /// HorizontalRotateAroundの回転角の補正係数
        /// </summary>
        /// <returns>回転中心を通る上向きのベクトルと回転中心からカメラへと向かう方向ベクトルとのなす角のサイン</returns>
        private float GetRotationCorrectionFactor()
        {
            float angle = Vector3.Angle(Vector3.up, transform.position - rotationCenter);
            return Mathf.Sin(Mathf.Deg2Rad * angle);
        }

        /// <summary>
        /// カメラのディスプレイを中心とした水平方向の回転。
        /// 点rotCenterを通る方向ベクトルVector3.upを軸とするyAngle度の回転。
        /// </summary>
        /// <param name="amount">正だと左回り。負だと右回り。</param>
        private void HorizontalRotateAround(float amount)
        {
            float yAngle = GetRotationCorrectionFactor() * amount * rotateSpeed.Horizontal * Time.deltaTime;
            transform.RotateAround(rotationCenter, Vector3.up, yAngle);
        }

        /// <summary>
        /// カメラのディスプレイを中心とした垂直方向の回転。
        /// 点rotCenterを通る方向ベクトルtranform.rightを軸とするxAngle度の回転。
        /// </summary>
        /// <param name="amount">正だと上回り。負だと下回り。</param>
        private void VerticalRotateAround(float amount)
        {
            /* カメラが上や下から回り込まないように俯角を制限 */
            float depression = Vector3.Angle(Vector3.up, transform.up);
            bool canGoDown = amount < 0 && depression > 5.0f;   // 下に回り込もうとしている && まだそんなに横向きではない -> まだ下に行ける
            bool canGoUp = amount > 0 && depression < 85.0f;    // 上に回り込もうとしている && まだそんなに下向きではない -> まだ上に行ける
            if (canGoDown || canGoUp)
            {
                float xAngle = amount * rotateSpeed.Vertical * Time.deltaTime;
                transform.RotateAround(rotationCenter, transform.right, xAngle);
            }
        }

        /// <summary>
        /// キー入力によるカメラ操作。
        /// </summary>
        private void KeyControl()
        {
            /* 移動 */
            if (Input.GetKey(KeyCode.E)) Zoom(1.0f);
            if (Input.GetKey(KeyCode.C)) Zoom(-1.0f);
            if (Input.GetKey(KeyCode.D)) HorizontalMove(1.0f);
            if (Input.GetKey(KeyCode.A)) HorizontalMove(-1.0f);
            if (Input.GetKey(KeyCode.W)) VerticalMove(1.0f);
            if (Input.GetKey(KeyCode.S)) VerticalMove(-1.0f);

            /* 回転 */
            if (Input.GetKey(KeyCode.LeftArrow)) HorizontalRotateAround(1.0f);
            if (Input.GetKey(KeyCode.RightArrow)) HorizontalRotateAround(-1.0f);
            if (Input.GetKey(KeyCode.UpArrow)) VerticalRotateAround(1.0f);
            if (Input.GetKey(KeyCode.DownArrow)) VerticalRotateAround(-1.0f);
        }

        /// <summary>
        /// マウスによるカメラ操作。
        /// </summary>
        private void MouseControl()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                preMousePos = Input.mousePosition;
            if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
                MouseDrag(Input.mousePosition);
            Zoom(Input.GetAxis("Mouse ScrollWheel") * 5.0f);
        }

        /// <summary>
        /// マウスのドラッグ操作。
        /// 右ボタンかホイールボタンが押されているときに呼ばれる。
        /// </summary>
        /// <param name="mousePos">現在のマウス位置</param>
        private void MouseDrag(Vector3 mousePos)
        {
            Vector3 diff = mousePos - preMousePos;
            if (diff.sqrMagnitude < Vector3.kEpsilon * Vector3.kEpsilon) return;

            /* 右クリックで回転 */
            if (Input.GetMouseButton(1))
            {
                HorizontalRotateAround(diff.x);
                VerticalRotateAround(-diff.y);
            }

            /* ホイールクリックで移動 */
            if (Input.GetMouseButton(2))
            {
                HorizontalMove(-diff.x * 0.5f);
                VerticalMove(-diff.y * 0.5f);
            }

            preMousePos = mousePos;
        }
        #endregion

        #region transformPreset
        /// <summary>
        /// PlayerPrefsに保存してある位置と回転にカメラを移動する
        /// </summary>
        /// <param name="number">ボタン番号</param>
        void SetTransform(int number)
        {
            if (    !PlayerPrefs.HasKey($"Camera{number}_localPosition")
                ||  !PlayerPrefs.HasKey($"Camera{number}_localRotation"))
                ResetSavedTransform(number);

            string localPosition = PlayerPrefs.GetString($"Camera{number}_localPosition");
            string localRotation = PlayerPrefs.GetString($"Camera{number}_localRotation");
            transform.localPosition = JsonUtility.FromJson<Vector3>(localPosition);
            transform.localRotation = JsonUtility.FromJson<Quaternion>(localRotation);
        }

        /// <summary>
        /// 指定のカメラボタンに対応するの位置と回転を保存する。
        /// </summary>
        /// <param name="number">ボタン番号</param>
        void SaveTransform(int number, Vector3 localPosition, Quaternion localRotation)
        {
            if (!(number == 1 || number == 2)) return;

            string pos = JsonUtility.ToJson(localPosition);
            string rot = JsonUtility.ToJson(localRotation);
            PlayerPrefs.SetString($"Camera{number}_localPosition", pos);
            PlayerPrefs.SetString($"Camera{number}_localRotation", rot);
        }

        /// <summary>
        /// PlayerPrefsに保存した値を初期値となるCameraControllerPreferencesで定義された値に戻す。
        /// </summary>
        /// <param name="number">ボタン番号</param>
        void ResetSavedTransform(int number)
        {
            switch (number)
            {
                case 1:
                    SaveTransform(1, CameraControllerPreferences.pos1, CameraControllerPreferences.rot1);
                    break;
                case 2:
                    SaveTransform(2, CameraControllerPreferences.pos2, CameraControllerPreferences.rot2);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// シーンをアンロードするときに現在のカメラの位置と回転を記憶する。
        /// </summary>
        public void HoldCurrentSceneTransform()
        {
            lastPosition = transform.localPosition;
            lastRotation = transform.localRotation;
        }

        /// <summary>
        /// シーンをロードしたときに前のシーンのカメラの位置と回転をセットする。
        /// </summary>
        private void SetLastSceneTransform()
        {
            transform.localPosition = lastPosition;
            transform.localRotation = lastRotation;
        }
        #endregion
    }

    public static class CameraControllerPreferences
    {
        /* 標準的な位置のプリセット */
        /// <summary>
        /// カメラ1の位置
        /// </summary>
        public static readonly Vector3 pos1 = new Vector3(0.0f, 19.95238f, -11.01941f);
        /// <summary>
        /// カメラ1の向き
        /// </summary>
        public static readonly Quaternion rot1 = Quaternion.Euler(60.0f, 0.0f, 0.0f);
        /// <summary>
        /// カメラ2の位置
        /// </summary>
        public static readonly Vector3 pos2 = new Vector3(0.0f, 18.1077747f, -2.13255429f);
        /// <summary>
        /// カメラ2の向き
        /// </summary>
        public static readonly Quaternion rot2 = new Quaternion(0.676393151f, 8.08870536e-05f, -8.61902517e-05f, 0.736540854f);

        /* カメラの移動可能範囲 */
        /// <summary>
        /// カメラが動ける位置のy座標の上限
        /// </summary>
        public static readonly float ceiling = 60.0f;
        /// <summary>
        /// カメラが動ける位置のy座標の下限
        /// </summary>
        public static readonly float floor = 3.0f;
    }

    [Serializable]
    public struct RotateSpeed
    {
        [SerializeField, Range(0.0f, 100.0f)]
        private float horizontal;
        public float Horizontal
        {
            get => horizontal * 3.0f;
        }

        [SerializeField, Range(0.0f, 100.0f)]
        private float vertical;
        public float Vertical
        {
            get => vertical * 0.6f;
        }

        public RotateSpeed(float horizontal, float vertical)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
        }
    }
}
