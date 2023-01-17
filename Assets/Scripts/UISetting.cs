using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KW_Mocap
{
    public static class UISetting
    {
        public static void SetButton(ref Button button, string name, UnityAction call)
        {
            button = GameObject.Find(name).GetComponent<Button>();
            button.onClick.AddListener(call);
        }

        public static void SetButton(ref Button button, string name, UnityAction call, string text)
        {
            button = GameObject.Find(name).GetComponent<Button>();
            button.onClick.AddListener(call);
            button.GetComponentInChildren<Text>().text = text;
        }

        public static void SetButton(ref Button button, Transform parent, string name, UnityAction call)
        {
            button = parent.Find(name).GetComponent<Button>();
            if (button == null)
            {
                Debug.Log($"child object {name} could not be found.");
            }
            button.onClick.AddListener(call);
        }

        public static void SetButton(ref Button button, Transform parent, string name, UnityAction call, string text)
        {
            button = parent.Find(name).GetComponent<Button>();
            if (button == null)
            {
                Debug.Log($"child object {name} could not be found.");
            }
            button.onClick.AddListener(call);
            button.GetComponentInChildren<Text>().text = text;
        }

        public static void SetButton(GameObject buttonObject, UnityAction call, string text)
        {
            buttonObject.GetComponent<Button>().onClick.AddListener(call);
            buttonObject.GetComponentInChildren<Text>().text = text;
        }

        public static void SetButtonText(GameObject buttonObject, string text)
        {
            buttonObject.GetComponentInChildren<Text>().text = text;
        }

        public static void SetButtonText(this Button button, string text)
        {
            SetButtonText(button.gameObject, text);
        }

        public static void SetButtonColor(Button button, Color color)
        {
            button.GetComponent<Image>().color = color;
        }

        public static void AddEventTrigger(this EventTrigger eventTrigger, EventTriggerType triggerType, UnityAction<BaseEventData> call)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = triggerType;
            entry.callback.AddListener(call);
            eventTrigger.triggers.Add(entry);
        }
    }
}
