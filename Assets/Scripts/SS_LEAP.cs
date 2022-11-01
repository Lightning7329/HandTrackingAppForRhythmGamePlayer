
using UnityEngine;
using Leap ;
using Leap.Unity;

namespace KW_Mocap
{
	public class SS_LEAP : HandModelBase
	{
		[SerializeField] private Chirality LR;
		[SerializeField] [RangeAttribute(0.1f, 100.0f)] private float scl;
		public Hand lmHand;

		public override Chirality Handedness
		{
			get { return (LR); }
			set { }
		}

		public override ModelType HandModelType
		{
			get { return (ModelType.Graphics); }
		}

        public void Start()
        {
            OnBegin += changeMaterial_OnBegin;
			OnFinish += changeMaterial_OnFinish;
		}

		/// <summary>
		/// LeapMotion��������m���n�߂���Material��ʏ탂�[�h�ɕύX����B
		/// </summary>
		private void changeMaterial_OnBegin()
        {
			Debug.Log(LR.ToString() + " Begin");
			GetComponent<HandSetting>().SetMaterial(this.gameObject, true);
		}

		/// <summary>
		/// LeapMotion��������m�ł��Ȃ��Ȃ�����Material���G���[���[�h�ɕύX����B
		/// </summary>
        private void changeMaterial_OnFinish()
        {
			Debug.Log(LR.ToString() + " Finish");
			GetComponent<HandSetting>().SetMaterial(this.gameObject, false);
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
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
