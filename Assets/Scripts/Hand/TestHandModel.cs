using UnityEngine;
using Leap;
using Leap.Unity;

namespace KW_Mocap
{
	public class TestHandModel : HandModelBase
	{
		[SerializeField] private Chirality LR;
		[SerializeField] [Range(0.1f, 100.0f)] private float scl;
		private Hand lmHand;

		public GameObject wrist, palm, thumb0, thumb1, index0, index1, middle0, middle1, ring0, ring1, pinky0, pinky1;

		public override Chirality Handedness
		{
			get => LR;
			set { }
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
			Debug.Log(LR.ToString() + " Begin");
			GetComponent<HandSetting>().SetMaterial(true);
		}

		/// <summary>
		/// LeapMotionが手を検知できなくなったらMaterialをエラーモードに変更する。
		/// </summary>
		private void changeMaterial_OnFinish()
		{
			Debug.Log(LR.ToString() + " Finish");
			GetComponent<HandSetting>().SetMaterial(false);
		}

		public override Hand GetLeapHand() { return (lmHand); }
		public override void SetLeapHand(Hand hand) { lmHand = hand; }
		public override void UpdateHand()
		{
			var palmPose = lmHand.GetPalmPose();
			//lmHand.WristPosition	手首の位置。おそらくこっちも使った方がいい。
			//lmHand.PalmWidth		手首の幅
			wrist.transform.localPosition = scl * lmHand.WristPosition;
			palm.transform.localPosition = scl * lmHand.PalmPosition;
			thumb0.transform.localPosition = scl * lmHand.GetThumb().bones[1].PrevJoint;
			thumb1.transform.localPosition = scl * lmHand.GetThumb().bones[2].PrevJoint;

			index0.transform.localPosition = scl * lmHand.GetIndex().bones[0].PrevJoint;
			index1.transform.localPosition = scl * lmHand.GetIndex().bones[1].PrevJoint;

			middle0.transform.localPosition = scl * lmHand.GetMiddle().bones[0].PrevJoint;
			middle1.transform.localPosition = scl * lmHand.GetMiddle().bones[1].PrevJoint;

			ring0.transform.localPosition = scl * lmHand.GetRing().bones[0].PrevJoint;
			ring1.transform.localPosition = scl * lmHand.GetRing().bones[1].PrevJoint;

			pinky0.transform.localPosition = scl * lmHand.GetPinky().bones[0].PrevJoint;
			pinky1.transform.localPosition = scl * lmHand.GetPinky().bones[1].PrevJoint;

			gameObject.transform.localPosition = scl * palmPose.position;
			gameObject.transform.localRotation = palmPose.rotation;
		}
	}
}
