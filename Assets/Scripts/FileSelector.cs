//==============================================================================
//
// This script should be attached on "Canvas/File Selection Panel".
//
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class FileSelector : MonoBehaviour
    {
        /// <summary>
        /// Hierarchy ViewでのContent配下のファイルボタンのオブジェクト名。
        /// 探したりCloneを削除するときに削除するときに使用する。
        /// </summary>
        static readonly string originalFileButtonName = "file";

        /// <summary>
        /// ファイル選択中のボタンの色
        /// </summary>
        static readonly Color32 highlighted = new Color32(89, 89, 89, 255);    //グレー

        /// <summary>
        /// ファイル選択中でない時のボタンの色
        /// </summary>
        static readonly Color32 normal = new Color32(0, 0, 0, 255);            //黒

        [HideInInspector] public SelectState selectState { get; private set; } = SelectState.NotSelected;

        /// <summary>
        /// ロードするファイル名。最初は未選択のためnull。
        /// </summary>
        public string fileNameToLoad { get; private set; } = null;

        /// <summary>
        /// 選択中のファイル。Selectボタンを押したときにfileNameToLoadがこれで上書きされる。
        /// </summary>
        private string SelectingfileName = null;

        /// <summary>
        /// ./SavedMotionDataディレクトリ配下の全binファイルのインスタンス
        /// </summary>
        FileInfo[] files;

        Button cancelButton, selectButton;
        Transform Content;
        GameObject[] fileButtons;

        /// <summary>
        /// ファイル選択状態
        /// </summary>
        public enum SelectState
        {
            /// <summary>
            /// 未選択
            /// </summary>
            NotSelected,
            /// <summary>
            /// 選択画面で選択中
            /// </summary>
            Selecting,
            /// <summary>
            /// 選択して決定済み
            /// </summary>
            Selected,
            /// <summary>
            /// 選択画面でキャンセルが押された
            /// </summary>
            Canceled
        }


        void Start()
        {
            UISetting.SetButton(ref cancelButton, "CancelButton", OnBtn_Cancel, "Cancel");
            UISetting.SetButton(ref selectButton, "SelectButton", OnBtn_Select, "Select");
            Content = transform.Find("Scroll View/Viewport/Content");
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// ファイルボタンを列挙してファイル選択画面を表示する。
        /// Loadボタンを押したとき、PlayModeManagerのOnBtn_FileSelect()で呼ばれる。
        /// </summary>
        public void List()
        {
            selectState = SelectState.NotSelected;

            int fileCount = GetFileInfos();
            if (fileCount == 0)
            {
                // TODO: セーブファイルが存在しないことを表示
                Debug.LogWarning("no files");
                return;
            }

            fileButtons = new GameObject[fileCount];
            fileButtons[0] = Content.transform.Find(originalFileButtonName).gameObject;
            // originalFileButtonを1つ目のファイル名に書き換えて表示する
            SetContent(fileButtons[0], 0);          

            // 2つ目以降のファイル
            for (int i = 1; i < files.Length; i++)
            {
                fileButtons[i] = CreateContent(i);
            }

            this.gameObject.SetActive(true);
        }

        void OnBtn_File(int fileIndex)
        {
            for (int i = 0; i < fileButtons.Length; i++)
            {
                //選択されたファイルボタンはグレーにする。それ以外は元の色に戻す。
                fileButtons[i].GetComponent<Image>().color = i == fileIndex ? highlighted : normal;
            }
            SelectingfileName = Path.GetFileNameWithoutExtension(files[fileIndex].Name);
            selectState = SelectState.Selecting;
        }

        void OnBtn_Select()
        {
            //いずれかのファイルを選択中でない場合は押せない
            if (selectState != SelectState.Selecting) return;

            fileNameToLoad = SelectingfileName;
            DeleteContentsAll();
            this.gameObject.SetActive(false);
            selectState = SelectState.Selected;
        }

        void OnBtn_Cancel()
        {
            DeleteContentsAll();
            this.gameObject.SetActive(false);
            selectState = SelectState.Canceled;
        }

        /// <summary>
        /// Scroll ViewのContentの子オブジェクトとして2つ目以降のファイルボタンを作成・追加する。
        /// </summary>
        /// <param name="index">ファイルのインデックス</param>
        /// <returns>作成したGameObject</returns>
        GameObject CreateContent(int index)
        {
            GameObject fileButtonClone = GameObject.Instantiate(fileButtons[0]);
            fileButtonClone.transform.SetParent(Content.transform);
            fileButtonClone.transform.localScale = Vector3.one;
            SetContent(fileButtonClone, index);
            return fileButtonClone;
        }

        /// <summary>
        /// 選択状況に応じてボタンの色を決定し、ボタンの名前とハンドラを割り当てる。
        /// </summary>
        /// <param name="fileButton">ファイルボタン</param>
        /// <param name="index">ファイルのインデックス</param>
        private void SetContent(GameObject fileButton, int index)
        {
            string fileName = Path.GetFileNameWithoutExtension(files[index].Name);
            fileButton.GetComponent<Image>().color = fileName == fileNameToLoad ? highlighted : normal;
            UISetting.SetButton(fileButton, () => { OnBtn_File(index); }, fileName);
            fileButton.SetActive(true);
        }

        /// <summary>
        /// Scroll ViewのContentの子オブジェクトをオリジナル以外削除する。
        /// ファイル選択画面を閉じるとき(CancelボタンまたはSelectボタンが押されたとき)に呼ばれる。
        /// </summary>
        private void DeleteContentsAll()
        {
            if (Content.childCount == 0) return;

            foreach (Transform fileButton in Content.transform)
            {
                if (fileButton.gameObject.name != originalFileButtonName)
                    GameObject.Destroy(fileButton.gameObject);
            }
        }

        /// <summary>
        /// ./SavedMotionDataディレクトリを調べ、インスタンスフィールドfilesの初期化を行う。
        /// </summary>
        /// <returns>モーションデータファイルの個数</returns>
        private int GetFileInfos()
        {
            // 保存データディレクトリがあるか確認
            if (!Directory.Exists("./SavedMotionData"))
            {
                Debug.LogError("./SavedMotionDataが存在しません。");
                return 0;
            }

            // 保存データディレクトリ内のモーションデータファイルを取得
            var Dir = new DirectoryInfo("./SavedMotionData/");
            files = Dir.GetFiles("*.bin");
            return files.Length;
        }
    }
}
