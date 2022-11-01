// OBJECT(= HUMAN's HAND) ASSIGN to BORN STRUCTUREusing System.Linq;using UnityEngine;namespace KW_Mocap {	public class HandSetting : MonoBehaviour	{		[SerializeField] private string[] fingerNames = { "Index", "Middle", "Ring" };		[SerializeField] private HandMaterial materials;		[HideInInspector] public int now_go;		public GameObject[] gameObjects;		private const int SENSOR_NUM = 9;		private void Start()		{			gameObjects = new GameObject[SENSOR_NUM];			GameObject[] children = GetComponentsInChildren<Transform>().Where<Transform>(t => t.name != "J" && t.name != "F" && t.name != "Top").Select(t => t.gameObject).ToArray();			now_go = 0;			foreach (string fingerName in fingerNames)			{				foreach (GameObject Gc in children)				{					if (Gc.name == fingerName + (now_go % 3).ToString())						gameObjects[now_go++] = Gc;				}			}		}		/// <summary>
		/// �ċA�I�Ɏ���\������I�u�W�F�N�g��Material��ύX����B
		/// </summary>
		/// <param name="Go">�ύX��������ԏ�̊K�w��Game Object</param>
		/// <param name="flg">true�̂Ƃ��ʏ��Material�Afalse�̂Ƃ��G���[�p��Material</param>		public void SetMaterial(GameObject Go, bool flg)		{			switch (Go.name)
			{
				case "P":
					Go.GetComponent<MeshRenderer>().material = flg ? materials.Palm : materials.Error;
					break;
				case "F":
					Go.GetComponent<MeshRenderer>().material = flg ? materials.Finger : materials.Error;
					break;
				case "J":
					Go.GetComponent<MeshRenderer>().material = flg ? materials.Joint : materials.Error;
					break;
				case "Top":
					Go.GetComponent<MeshRenderer>().material = flg ? materials.Top : materials.Error;
					break;
				default:
					break;
			}			int n = Go.transform.childCount;			if (n < 1) return;			for (int i = 0; i < n; i++)			{				GameObject Gc = Go.transform.GetChild(i).gameObject;				SetMaterial(Gc, flg);			}		}	}}//------------------------------------------------------------------------------// EOF//------------------------------------------------------------------------------