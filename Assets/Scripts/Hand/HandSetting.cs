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
		private const int SENSOR_NUM = 10;

		private void Start()
		{
			gameObjects = new GameObject[SENSOR_NUM];
			GameObject[] children = GetComponentsInChildren<Transform>().Where<Transform>(t => t.name != "J" && t.name != "F" && t.name != "Top").Select(t => t.gameObject).ToArray();
			now_go = 0;
			foreach (string fingerName in fingerNames)
			{
				foreach (GameObject Gc in children)
				{
					if (Gc.name == fingerName + (2 - now_go % 3).ToString())
						gameObjects[now_go++] = Gc;
				}
			}
			gameObjects[now_go] = this.gameObject;
		}

		/// <summary>
		/// 再帰的に手を構成するオブジェクトのMaterialを変更する。
		/// </summary>
		/// <param name="Go">変更したい一番上の階層のGame Object</param>
		/// <param name="flg">trueのとき通常のMaterial、falseのときエラー用のMaterial</param>
		public void SetMaterial(GameObject Go, bool flg)
		{
			switch (Go.name)
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
			}

			int n = Go.transform.childCount;
			if (n < 1) return;

			for (int i = 0; i < n; i++)
			{
				GameObject Gc = Go.transform.GetChild(i).gameObject;
				SetMaterial(Gc, flg);
			}
		}
	}
}
//------------------------------------------------------------------------------
// EOF
//------------------------------------------------------------------------------
