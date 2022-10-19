// OBJECT(= HUMAN's HAND) ASSIGN to BORN STRUCTURE

using System.Linq;
using UnityEngine;

public class HandSetting : MonoBehaviour
{
	[SerializeField] private string[] fingerNames = {"Index", "Middle", "Ring"};
	[SerializeField] private Material	mat_B , mat_J ;
	[HideInInspector] public int		now_go ;
    private const int sensorNum = 9;
    public GameObject[] Go;

	private void	Start()
	{
        Go = new GameObject[sensorNum];
        GameObject[] children = GetComponentsInChildren<Transform>().Where<Transform>(t => t.name != "J" && t.name != "F" && t.name != "Top").Select(t => t.gameObject).ToArray();
		now_go = 0;
		foreach (string fingerName in fingerNames)
		{
            foreach(GameObject Gc in children)
			{
				if (Gc.name == fingerName + (now_go % 3).ToString()) Go[now_go++] = Gc;
			}
		}

		SetMaterial(this.gameObject);
	}

	private void	SetMaterial(GameObject Go)
	{
		if (Go.name == "B")		Go.GetComponent<MeshRenderer>().material = mat_B;
		else if (Go.name == "F")	Go.GetComponent<MeshRenderer>().material = mat_B;
		else if (Go.name == "J")	Go.GetComponent<MeshRenderer>().material = mat_J;
		else if (Go.name == "Top")	Go.GetComponent<MeshRenderer>().material = mat_J;
		
		int	n = Go.transform.childCount;
		if (n < 1)	return;

		for (int i = 0 ; i < n ; i++)
		{
			GameObject	Gc = Go.transform.GetChild(i).gameObject;
			SetMaterial(Gc);
		}
	}
};
//------------------------------------------------------------------------------
// EOF
//------------------------------------------------------------------------------
