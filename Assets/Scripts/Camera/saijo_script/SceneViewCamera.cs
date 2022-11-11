using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SceneViewCamera : MonoBehaviour
{
	private string label = "";
	
	Vector3 rot_center = new Vector3(0, 0, 0);
	
	/* */
	[SerializeField, Range(1f, 30f)]
	private float wheelSpeed = 10f;
	
	[SerializeField, Range(0.01f, 0.1f)]
	private float moveSpeed = 0.01f;
	
	[SerializeField, Range(0.1f, 10f)]
	private float rotateSpeed = 0.3f;
	
	/* */
	private Vector3 preMousePos;
	
	/* */
	private Quaternion rot_org;
	private Vector3 pos_org;
	
	/* */
	bool b_option = false;
	bool b_command = false;
	
	void Awake() { 
		rot_org = transform.rotation;
		pos_org = transform.position;
	}
	
	private void Update()
	{
		/********************
		■KeyCode
			https://docs.unity3d.com/ja/2019.4/ScriptReference/KeyCode.html
		********************/
		if(Input.GetKey(KeyCode.LeftAlt))		b_option = true;
		else									b_option = false;
		
		if(Input.GetKey(KeyCode.LeftCommand))	{ b_command = true; }
		else									{ b_command = false; }
		
		if(Input.GetKeyDown(KeyCode.R)) ResetTransform();
		if(Input.GetKeyDown(KeyCode.F))	transform.LookAt(rot_center, Vector3.up);
		
		// for test
		if(Input.GetKeyDown(KeyCode.T)){
			float angle = Vector3.SignedAngle(new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
			Debug.Log(angle);
			
			transform.Rotate(new Vector3(0, 0, 30), Space.Self); 
		}
		
		if(Input.GetMouseButtonDown(0)){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit, Mathf.Infinity)){
				if (hit.collider.tag == "Player"){
					rot_center = hit.transform.position;
				}
			}
		}
		
		label = string.Format("( {0:0.00}, {1:0.00}, {2:0.00} )", rot_center.x, rot_center.y, rot_center.z);
		
		MouseUpdate();
		
		return;
	}
	
	private void ResetTransform(){
		transform.position = pos_org;
		transform.rotation = rot_org;
	}
	
	/******************************
	■How to get MouseScroll Input?
		https://stackoverflow.com/questions/5675472/how-to-get-mousescroll-input
			-	Hey Friend, Instead of Input.GetAxis you may use Input.GetAxisRaw. The value for GetAxis is smoothed and is in range -1 .. 1 , however GetAxisRaw is -1 or 0 or 1.
	
	■Input.GetAxis
		https://docs.unity3d.com/ScriptReference/Input.GetAxis.html
			-	The value will be in the range -1...1 for keyboard and joystick input devices.
	******************************/
	private void MouseUpdate()
	{
		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
		if (scrollWheel != 0.0f)	MouseWheel(scrollWheel);

		if( Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2) ){
			preMousePos = Input.mousePosition;
		}

		MouseDrag(Input.mousePosition);
	}

	/******************************
	******************************/
	private void MouseWheel(float delta)
	{
		transform.position += transform.forward * delta * wheelSpeed;
		return;
	}

	/******************************
	What is the kEpsilon field on Vectors?
		https://answers.unity.com/questions/580108/what-is-the-kepsilon-field-on-vectors.html
	******************************/
	private void MouseDrag(Vector3 mousePos)
	{
		Vector3 diff = mousePos - preMousePos;

		if(diff.magnitude < Vector3.kEpsilon) return;
		
		if (Input.GetMouseButton(0)){
			if(b_option)		CameraRotate(new Vector2(-diff.y, diff.x) * rotateSpeed);
			else if(b_command)	CameraRotate_z();
			else				transform.Translate(-diff * moveSpeed);
		}
		
		preMousePos = mousePos;
	}
	
	/******************************
	******************************/
	public void CameraRotate(Vector2 angle)
	{
		// transform.Rotate(new Vector3(angle.x, angle.y, 0), Space.Self); // transform.Rotate(new Vector3(angle.x, angle.y, 0), Space.World);
		
		/********************
		********************/
		transform.RotateAround(rot_center, transform.right, angle.x);
		transform.RotateAround(rot_center, transform.up, angle.y); // transform.RotateAround(transform.position, Vector3.up, angle.y);
		
		/********************
		********************/
		float _angle = Vector3.Angle(transform.forward, Vector3.up);
		const float _thresh = 25.0f;
		if( (_thresh < _angle)/* 上向 でない */ && (_angle < 180.0f - _thresh)/* 下向 でない */ ){	// camが真下 or 真上 を向いている時は、水平補正しない
			Plane plane = new Plane(transform.forward, transform.position);
			Vector3 pos_up = transform.position + new Vector3(0, 1, 0);
			Vector3 pos_up_on_plane = plane.ClosestPointOnPlane(pos_up);
			
			if(pos_up_on_plane != transform.position){
				Vector3 v_up = (pos_up_on_plane - transform.position).normalized;
				
				float diff_angle = Vector3.SignedAngle(transform.up, v_up, transform.forward); // public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis);
				// transform.Rotate(new Vector3(0, 0, diff_angle), Space.Self);	// direct
				transform.Rotate(new Vector3(0, 0, diff_angle * 0.35f), Space.Self);	// lerp
				/********************
				↑	Lerpさせると、自然な動きになるが、
					本関数に入ってきた時のみ、水平補正が入るので、
						cf.
							if(diff.magnitude < Vector3.kEpsilon) return;
							if(b_option)		CameraRotate(new Vector2(-diff.y, diff.x) * rotateSpeed);
					いつも少し、足りない程度しか補正が働かないことになる。
					しかし、完全に水平でなくても見た目的に問題ないのであれば、ここに示したlerpでもいいかもしれない。
				********************/
			}
		}
	}
	
	/******************************
	■【Unity入門】スクリプトで画面サイズを取得・設定しよう
		https://www.sejuku.net/blog/83691
	
	■【Unity】2つのベクトル間の角度を求める
		https://nekojara.city/unity-vector-angle
	******************************/
	public void CameraRotate_z()
	{
		Vector3 center = new Vector3(Screen.width/2, Screen.height/2, 0);
		Vector3 v_mouse		= Input.mousePosition - center;
		Vector3 v_PreMouse	= preMousePos - center;
		
		if( (v_mouse.magnitude == 0) || (v_PreMouse.magnitude == 0) ) return;
		
		float angle = Vector3.SignedAngle(v_PreMouse, v_mouse, new Vector3(0, 0, 1)); // public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis);
		
		transform.Rotate(new Vector3(0, 0, -angle), Space.Self); // transform.RotateAround(transform.position, transform.forward, angle);
	}
	
	/******************************
	******************************/
	void OnGUI()
	{
		GUI.color = Color.white;
		GUI.skin.label.fontSize = 30;
		
		GUI.Label(new Rect(15, 20, 500, 50), label);
	}
	
}