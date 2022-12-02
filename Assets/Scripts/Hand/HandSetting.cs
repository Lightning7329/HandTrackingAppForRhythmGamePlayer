// OBJECT(= HUMAN's HAND) ASSIGN to BORN STRUCTURE

using System;
using System.Linq;
using UnityEngine;

namespace KW_Mocap {
	public class HandSetting : MonoBehaviour
	{
		/// <summary>
		/// 指を表すGameObjectの名前からindexを省いたもの。
		/// </summary>
		[SerializeField] private string[] fingerNames = { "Thumb" ,"Index", "Middle", "Ring", "Pinky" };
        /// <summary>
        /// 指の関節を表すGameObjectのインデックスに使用する文字。
        /// </summary>
        [SerializeField] private char[] fingerBoneIndex = { '1', '2', '3' };
		/// <summary>
		/// 正常時とエラー時のマテリアルを集めたクラス。
		/// </summary>
		[SerializeField] private HandMaterial materials;
		/// <summary>
		/// ハンドモデルの関節を表す2次元配列。1次元目が指、2次元目が関節。
		/// </summary>
		public GameObject[,] joints = new GameObject[5,3];	//指の本数 * 関節の数

		private void Start()
		{
			GrabJoints();
        }

		/// <summary>
		/// 自分の孫を含めた子オブジェクトの中からボーンになるオブジェクトを二次元配列に格納する。
		/// </summary>
		private void GrabJoints()
		{
			GameObject[] children = GetComponentsInChildren<Transform>()
									.Where<Transform>(t => t.name != "J" && t.name != "F" && t.name != "Top")
									.Select(t => t.gameObject)
									.ToArray();
            foreach (GameObject Gc in children)
            {
                int i = Array.IndexOf(fingerNames, Gc.name.TrimEnd(fingerBoneIndex));
				if (i == -1) continue;
                int j = int.Parse(Gc.name[Gc.name.Length - 1].ToString()) - 1;
                this.joints[i, j] = Gc;
			}
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

		public void SetCalibrationPose()
		{

		}
	}

    [Serializable]
    public class HandMaterial
    {
        public Material Palm;
        public Material Finger;
        public Material Joint;
        public Material Top;
        public Material Error;
    }
}
