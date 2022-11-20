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
        /// モーションデータファイルが保存されているディレクトリパス
        /// </summary>
        static readonly string savedMotionDataDirectory = "./SavedMotionData";

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
        /// 選択中のファイルのインデックス。
        /// Selectボタンを押したときにfileNameToLoadがこのインデックスのボタンの名前で上書きされる。
        /// </summary>
        private int selectingFileIndex = -1;

        private int fileCount = 0;

        /// <summary>
        /// savedMotionDataDirectory配下の全binファイルのインスタンス
        /// </summary>
        FileInfo[] files;

        Text noFiles;
        Button cancelButton, selectButton, deleteButton;
        Transform Content;
        GameObject originalFileButton;
        (string name, GameObject gameObject)[] fileButtons;

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
            selectState = SelectState.NotSelected;
            noFiles = transform.Find("NoFiles").GetComponent<Text>();
            noFiles.gameObject.SetActive(false);
            UISetting.SetButton(ref cancelButton, "CancelButton", OnBtn_Cancel);
            UISetting.SetButton(ref selectButton, "SelectButton", OnBtn_Select);
            UISetting.SetButton(ref deleteButton, "DeleteButton", OnBtn_Delete);
            selectButton.interactable = false;
            deleteButton.interactable = false;
            Content = transform.Find("Scroll View/Viewport/Content");
            originalFileButton = Content.transform.Find(originalFileButtonName).gameObject;
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// ファイルボタンを列挙してファイル選択画面を表示する。
        /// Loadボタンを押したとき、PlayModeManagerのOnBtn_FileSelect()で呼ばれる。
        /// </summary>
        public void List()
        {
            if (selectingFileIndex != -1) selectState = SelectState.Selecting;

            fileCount = GetFileInfos();
            if (fileCount == 0)
            {
                noFiles.gameObject.SetActive(true);
                Debug.LogWarning("no files");
                return;
            }

            fileButtons = new (string, GameObject)[fileCount];
            for (int i = 0; i < files.Length; i++)
            {
                fileButtons[i] = CreateContent(i);
            }

            if (selectState == SelectState.Selecting)
            {
                selectButton.interactable = true;
                deleteButton.interactable = true;
            }
            originalFileButton.SetActive(false);
            this.gameObject.SetActive(true);
        }

        void OnBtn_File(int fileIndex)
        {
            for (int i = 0; i < fileButtons.Length; i++)
            {
                //選択されたファイルボタンはグレーにする。それ以外は元の色に戻す。
                fileButtons[i].gameObject.GetComponent<Image>().color = i == fileIndex ? highlighted : normal;
            }
            selectingFileIndex = fileIndex;
            selectButton.interactable = true;
            deleteButton.interactable = true;
            selectState = SelectState.Selecting;
        }

        void OnBtn_Select()
        {
            //いずれかのファイルを選択中でない場合は押せない
            if (selectState != SelectState.Selecting) return;

            fileNameToLoad = fileButtons[selectingFileIndex].name;
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

        void OnBtn_Delete()
        {
            Debug.Log(fileButtons[selectingFileIndex].name + " was deleted.");
            fileButtons[selectingFileIndex].gameObject.SetActive(false);
            files[selectingFileIndex].Delete();
            selectingFileIndex = -1;
            if (--fileCount == 0)
            {
                noFiles.gameObject.SetActive(true);
            }
            selectButton.interactable = false;
            deleteButton.interactable = false;
            selectState = SelectState.NotSelected;
        }

        /// <summary>
        /// Scroll ViewのContentの子オブジェクトとしてoriginalFileButtonを元にファイルボタンを作成・追加する。
        /// </summary>
        /// <param name="index">ファイルのインデックス</param>
        /// <returns>作成したボタンの名前とGameObject</returns>
        (string name, GameObject gameObject) CreateContent(int index)
        {
            /* ボタンのGameObjectの作成と配置 */
            GameObject fileButtonClone = GameObject.Instantiate(originalFileButton);
            fileButtonClone.transform.SetParent(Content.transform);
            fileButtonClone.transform.localScale = Vector3.one;

            /* ボタンの見た目の設定 */
            string fileName = Path.GetFileNameWithoutExtension(files[index].Name);
            fileButtonClone.GetComponent<Image>().color = fileName == fileNameToLoad ? highlighted : normal;
            UISetting.SetButton(fileButtonClone, () => { OnBtn_File(index); }, fileName);
            fileButtonClone.SetActive(true);
            return (fileName, fileButtonClone);
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
            if (!Directory.Exists(savedMotionDataDirectory))
            {
                Debug.LogError(savedMotionDataDirectory + "が存在しません。");
                return 0;
            }

            // 保存データディレクトリ内のモーションデータファイルを取得
            var Dir = new DirectoryInfo(savedMotionDataDirectory + "/");
            files = Dir.GetFiles("*.bin");
            return files.Length;
        }
    }
}
