// OBJECT(= HUMAN's HAND) ASSIGN to BORN STRUCTURE

using System.Linq;
using UnityEngine;

namespace KW_Mocap {
	public class HandSetting : MonoBehaviour
	{
		[SerializeField] private string[] fingerNames = { "Index", "Middle", "Ring" };
		[SerializeField] private HandMaterial materials;
		[HideInInspector] public int now_go;
		public GameObject[] gameObjects;
		private const int SENSOR_NUM = 9;

		private void Start()
		{
			gameObjects = new GameObject[SENSOR_NUM];
			GameObject[] children = GetComponentsInChildren<Transform>().Where<Transform>(t => t.name != "J" && t.name != "F" && t.name != "Top").Select(t => t.gameObject).ToArray();
			now_go = 0;
			foreach (string fingerName in fingerNames)
			{
				foreach (GameObject Gc in children)
				{
					if (Gc.name == fingerName + (now_go % 3).ToString()) gameObjects[now_go++] = Gc;
				}
			}

			SetMaterial(this.gameObject);
		}

		private void SetMaterial(GameObject Go)
		{
			if (Go.name == "P") Go.GetComponent<MeshRenderer>().material = materials.Palm;
			else if (Go.name == "F") Go.GetComponent<MeshRenderer>().material = materials.Finger;
			else if (Go.name == "J") Go.GetComponent<MeshRenderer>().material = materials.Joint;
			else if (Go.name == "Top") Go.GetComponent<MeshRenderer>().material = materials.Top;

			int n = Go.transform.childCount;
			if (n < 1) return;

			for (int i = 0; i < n; i++)
			{
				GameObject Gc = Go.transform.GetChild(i).gameObject;
				SetMaterial(Gc);
			}
		}
	}
}
//------------------------------------------------------------------------------
// EOF
//------------------------------------------------------------------------------
