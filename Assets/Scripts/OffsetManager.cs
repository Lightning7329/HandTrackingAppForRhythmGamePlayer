using System.Collections;
using System.Collections.Generic;
using KW_Mocap;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class OffsetManager
    {
        MotionPlayer motionPlayer;
        InputField offsetInputField;
        Button subButton, addButton, saveButton;

        private int motionOffset = 0;
        public int MotionOffset
        {
            get => motionOffset;
            set
            {
                motionPlayer.playbackOffset = motionOffset = value;
                offsetInputField.text = value.ToString();
            }
        }

        public OffsetManager(GameObject motionOffsetPanel, MotionPlayer motionPlayer)
        {
            this.motionPlayer = motionPlayer;
            offsetInputField = motionOffsetPanel.GetComponentInChildren<InputField>();
            offsetInputField.onEndEdit.AddListener(OnEndEdit);
            UISetting.SetButton(ref subButton, motionOffsetPanel.transform, "SubOffsetButton", OnBtn_SubOffset);
            UISetting.SetButton(ref addButton, motionOffsetPanel.transform, "AddOffsetButton", OnBtn_AddOffset);
            UISetting.SetButton(ref saveButton, motionOffsetPanel.transform, "SaveOffsetButton", OnBtn_SaveMotionOffset);
        }

        void OnEndEdit(string input)
        {
            motionOffset = int.Parse(input);
            PlayAfterEdit();
        }

        void OnBtn_SubOffset()
        {
            offsetInputField.text = (--motionOffset).ToString();
            PlayAfterEdit();
        }

        void OnBtn_AddOffset()
        {
            offsetInputField.text = (++motionOffset).ToString();
            PlayAfterEdit();
        }

        /// <summary>
        /// motionOffsetが変更されたときの共通処理。
        /// フィールドのmotionOffsetをmotionPlayerに渡し、
        /// 実際にそのオフセットを加えたフレームのモーションデータを表示する。
        /// </summary>
        void PlayAfterEdit()
        {
            motionPlayer.playbackOffset = motionOffset;
            motionPlayer.Play();
        }

        void OnBtn_SaveMotionOffset()
        {
            motionPlayer.SavePlaybackOffset();
        }
    }
}
