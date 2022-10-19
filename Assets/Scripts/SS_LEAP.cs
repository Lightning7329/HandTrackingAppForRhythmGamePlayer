
using UnityEngine;
using Leap ;
using Leap.Unity;
public class	SS_LEAP : HandModelBase
{
	[SerializeField] private Chirality	LR;
	[SerializeField][RangeAttribute(0.1f,100.0f)] private float	scl ;
	private Hand	lmHand;
//..............................................................
	public override ModelType	HandModelType
	{
		get {	return(ModelType.Graphics);	}
	}
//..............................................................
	public override Chirality Handedness
	{
		get {	return(LR);	}
		set { }
	}
//..............................................................
	public override Hand	GetLeapHand()	{	return(lmHand);	}
//..............................................................
	public override void	SetLeapHand(Hand hand)	{	lmHand = hand;	}
//..............................................................
	public override void	UpdateHand()
	{
		Vector3	Pc = lmHand.PalmPosition;
		gameObject.transform.localPosition = scl * Pc;
	}
};
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
