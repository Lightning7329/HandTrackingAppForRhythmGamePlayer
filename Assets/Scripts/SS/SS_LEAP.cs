
//==============================================================
//
// Hand Motion Tracking System ver.3
//	Leap Motion Data Management Class
//
//--------------------------------------------------------------
// (C)Copyright Allrights reserved by Sakai Shigekazu 2022 -
//..............................................................
using UnityEngine ;
using Leap ;
using Leap.Unity ;
//--------------------------------------------------------------
public class	SS_LEAP : HandModelBase
{
	[SerializeField] private Chirality	LR;
	private Hand	hand ;
	public  Vector3	Root ;
	public	Bone[,]	B = new Bone[5,4] ;
	public	bool	ready = false;
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
	public override Hand	GetLeapHand()	{	return(hand);	}
//..............................................................
	public override void	SetLeapHand(Hand hc)	{	hand = hc;	}
//..............................................................
	public override void	UpdateHand()
	{
		for (int i = 0 ; i < 5 ; i++)
		{
			for (int j = 0 ; j < 4 ; j++)
			{
				if (hand.Fingers[i].bones[j] != null)
					B[i,j] = hand.Fingers[i].bones[j];
			}
		}
		Root = hand.Fingers[2].bones[0].PrevJoint;
		ready = true;
	}
};
//--------------------------------------------------------------
// EOF
//--------------------------------------------------------------
