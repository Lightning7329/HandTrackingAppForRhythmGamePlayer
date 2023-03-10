using UnityEngine;
using Leap;
using Leap.Unity;

namespace KW_Mocap
{
    public class LeapHandModel : HandModelBase
    {
        [SerializeField] private Chirality LR;

        [SerializeField, Range(0.1f, 100.0f)]
        public float scl = 70.0f;

        public bool isDetected = false;

        private Hand lmHand;

        public override Chirality Handedness
        {
            get => LR;
            set {}
        }

        public override ModelType HandModelType
        {
            get => ModelType.Graphics;
        }

        public void Start()
        {
            OnBegin += changeMaterial_OnBegin;
            OnFinish += changeMaterial_OnFinish;
        }

        /// <summary>
        /// LeapMotionが手を検知し始めたらMaterialを通常モードに変更する。
        /// </summary>
        private void changeMaterial_OnBegin()
        {
            isDetected = true;
            Debug.Log(LR.ToString() + " Begin");
            GetComponent<HandSetting>().SetMaterial(true);
        }

        /// <summary>
        /// LeapMotionが手を検知できなくなったらMaterialをエラーモードに変更する。
        /// </summary>
        private void changeMaterial_OnFinish()
        {
            isDetected = false;
            Debug.Log(LR.ToString() + " Finish");
            GetComponent<HandSetting>().SetMaterial(false);
        }

        public override Hand GetLeapHand() { return (lmHand); }
        public override void SetLeapHand(Hand hand) { lmHand = hand; }
        public override void UpdateHand()
        {
            var palmPose = lmHand.GetPalmPose();
            gameObject.transform.localPosition = scl * palmPose.position;
            gameObject.transform.localRotation = palmPose.rotation;
        }
    }
}
