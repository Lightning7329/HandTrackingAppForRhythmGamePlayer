//==============================================================================
//
// This script should be attached on "Canvas/File Selection Panel".
//
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class FileSelector : MonoBehaviour
    {
        const string originalFileButtonName = "file";
        bool isSelecting = false;
        [HideInInspector] public SelectState selectState { get; private set; } = SelectState.NotSelected;
        public string fileNameToLoad { get; private set; } = null;
        FileInfo[] files;
        Button cancelButton, selectButton;
        Transform Content;
        IList<GameObject> fileButtons;

        public enum SelectState
        {
            NotSelected,
            Selecting,
            Selected,
            Canceled
        }

        private void Start()
        {
            UISetting.SetButton(ref cancelButton, "CancelButton", OnBtn_Cancel, "Cancel");
            UISetting.SetButton(ref selectButton, "SelectButton", OnBtn_Select, "Select");
            Content = transform.Find("Scroll View/Viewport/Content");
            this.gameObject.SetActive(false);
            isSelecting = false;
        }

        public void List()
        {
            // 2回以上押せないように（いらないかも。その場合はisSelectingフラグごと不要。）
            if (isSelecting) return;

            selectState = SelectState.NotSelected;

            // 保存データディレクトリがあるか確認
            if (!Directory.Exists("./SavedMotionData"))
            {
                Debug.LogError("./SavedMotionDataが存在しません。");
                return;
            }

            // 保存データディレクトリ内のモーションデータファイルを取得
            var Dir = new DirectoryInfo("./SavedMotionData/");
            files = Dir.GetFiles("*.bin");

            if (files.Length == 0)
            {
                // TODO: セーブファイルが存在しないことを表示
                Debug.Log("no files");
                return;
            }

            fileButtons = new List<GameObject>();

            // originalFileButtonを1つ目のファイル名に書き換えて表示する
            //GameObject originalFileButton = Content.transform.Find(originalFileButtonName).gameObject;
            fileButtons.Add(Content.transform.Find(originalFileButtonName).gameObject);
            UISetting.SetButton(fileButtons[0], () => { OnBtn_File(0); }, Path.GetFileNameWithoutExtension(files[0].Name));
            fileButtons[0].SetActive(true);

            
            
            float v_spc = -30f;
            float vw = v_spc * (float)files.Length;

            // 2つ目以降のファイル
            for (int i = 1; i < files.Length; i++)
            {
                CreateContents(i);
            }
            Content.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, -vw);
            this.gameObject.SetActive(true);
            isSelecting = true;
        }

        private void OnBtn_File(int fileIndex)
        {
            fileButtons[fileIndex].GetComponent<Image>().color = new Color32(89, 89, 89, 255);  //グレー
            for (int i = 0; i < fileButtons.Count(); i++)
            {
                if (i == fileIndex) continue;
                fileButtons[i].GetComponent<Image>().color = new Color32(0, 0, 0, 255); //黒
            }
            fileNameToLoad = Path.GetFileNameWithoutExtension(files[fileIndex].Name);
            selectState = SelectState.Selecting;
        }

        void OnBtn_Select()
        {
            if (selectState != SelectState.Selecting) return;

            DeleteContents();
            this.gameObject.SetActive(false);
            selectState = SelectState.Selected;
        }

        private void OnBtn_Cancel()
        {
            DeleteContents();
            this.gameObject.SetActive(false);
            selectState = SelectState.Canceled;
        }

        private void CreateContents(int index)
        {
            GameObject fileButtonClone = GameObject.Instantiate(fileButtons[0]);
            fileButtonClone.transform.SetParent(Content.transform);
            fileButtonClone.transform.localScale = Vector3.one;

            UISetting.SetButton(fileButtonClone, () => { OnBtn_File(index); }, Path.GetFileNameWithoutExtension(files[index].Name));
            fileButtonClone.SetActive(true);
            fileButtons.Add(fileButtonClone);
        }

        private void DeleteContents()
        {
            if (Content.childCount > 0)
            {
                foreach (Transform nb in Content.transform)
                {
                    if (nb.gameObject.name != originalFileButtonName) GameObject.Destroy(nb.gameObject);
                }
            }
            fileButtons.Clear();
            isSelecting = false;
        }
    }
}
