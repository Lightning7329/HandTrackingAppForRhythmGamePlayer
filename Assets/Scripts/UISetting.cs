using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class UISetting
    {
        public static void SetButton(ref Button button, string name, UnityEngine.Events.UnityAction call, string text)
        {
            button = GameObject.Find(name).GetComponent<Button>();
            button.onClick.AddListener(call);
            button.GetComponentInChildren<Text>().text = text;
        }

        public static void SetButton(GameObject buttonObject, UnityEngine.Events.UnityAction call, string text)
        {
            buttonObject.GetComponent<Button>().onClick.AddListener(call);
            buttonObject.GetComponentInChildren<Text>().text = text;
        }
    }
}
