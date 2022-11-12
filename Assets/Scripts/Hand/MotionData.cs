using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KW_Mocap
{
    public class MotionData
    {
        public HandData left, right;

        public MotionData(HandData left, HandData right)
        {
            this.left = left;
            this.right = right;
        }

        /// <summary>
        /// byte配列からモーションデータを復元するコンストラクタ
        /// </summary>
        /// <param name="buf">1フレーム分のモーションデータを格納したbyte配列</param>
        /// <param name="offset">左手か右手かでbyte配列の読み取り位置が異なる</param>
        public MotionData(byte[] buf)
        {
            this.left = new HandData(buf, HandData.Offset.Left);
            this.right = new HandData(buf, HandData.Offset.Right);
        }

        /// <summary>
        /// 受け取ったbyte配列にモーションデータをセットする
        /// </summary>
        /// <param name="buf">長さ144以上のbyte配列</param>
        public void SetBytes(byte[] buf)
        {
            left.SetBytes(buf, HandData.Offset.Left);
            right.SetBytes(buf, HandData.Offset.Right);
        }
    }

    public class HandData
    {
        /// <summary>
        /// 手のモーションデータのうち左か右かでバッファ読み取り時のオフセットが変わる
        /// </summary>
        public enum Offset { Left = 0, Right = 192 };

        /// <summary>
        /// 手のひらの位置の座標。Transform.Positionに入れる。
        /// </summary>
        public Vector3 palmPos;

        /// <summary>
        /// 手のひらの回転のクォータニオン。Tranform.Rotationに入れる。
        /// </summary>
        public Quaternion palmRot;

        /// <summary>
        /// 指の関節の回転のクォータニオンの配列。Tranform.Rotationに入れる。
        /// </summary>
        public Quaternion[] jointRot = new Quaternion[10];

        /// <summary>
        /// 手のひらの位置と回転のみから片手分のモーションデータを作成するコンストラクタ。
        /// 関節の回転は何もしない。
        /// </summary>
        /// <param name="palmPos">手のひらの位置</param>
        /// <param name="palmRot">手のひらの回転</param>
        public HandData(Vector3 palmPos, Quaternion palmRot)
        {
            this.palmPos = palmPos;
            this.palmRot = palmRot;
            for (int i = 0; i < jointRot.Length; i++)
            {
                this.jointRot[i] = Quaternion.identity;
            }
        }

        /// <summary>
        /// byte配列から片手分のモーションデータを復元するコンストラクタ
        /// </summary>
        /// <param name="buf">1フレーム分のモーションデータを格納したbyte配列</param>
        /// <param name="offset">左手か右手かでbyte配列の読み取り位置が異なる</param>
        public HandData(byte[] buf, Offset offset)
        {
            palmPos = Vector3.zero;
            palmRot = Quaternion.identity;

            // 手のひらの位置と回転
            palmPos.x = BitConverter.ToSingle(buf, (int)offset);
            palmPos.y = BitConverter.ToSingle(buf, 4 + (int)offset);
            palmPos.z = BitConverter.ToSingle(buf, 8 + (int)offset);
            palmRot.w = BitConverter.ToSingle(buf, 12 + (int)offset);
            palmRot.x = BitConverter.ToSingle(buf, 16 + (int)offset);
            palmRot.y = BitConverter.ToSingle(buf, 20 + (int)offset);
            palmRot.z = BitConverter.ToSingle(buf, 24 + (int)offset);

            // 関節の回転
            for (int i = 0; i < jointRot.Length; i++)
            {
                jointRot[i] = Quaternion.identity;

                int shift = 16 * i + (int)offset;
                jointRot[i].w = BitConverter.ToSingle(buf, 28 + shift);
                jointRot[i].x = BitConverter.ToSingle(buf, 32 + shift);
                jointRot[i].y = BitConverter.ToSingle(buf, 36 + shift);
                jointRot[i].z = BitConverter.ToSingle(buf, 40 + shift);
            }
        }

        /// <summary>
        /// 受け取ったbyte配列に片手分のモーションデータをシリアライズして格納する
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        public void SetBytes(byte[] buf, Offset offset)
        {
            // 手のひらの位置と回転
            SetByteBuf(BitConverter.GetBytes(palmPos.x), buf, (int)offset, 4);
            SetByteBuf(BitConverter.GetBytes(palmPos.y), buf, 4 + (int)offset, 4);
            SetByteBuf(BitConverter.GetBytes(palmPos.z), buf, 8 + (int)offset, 4);
            SetByteBuf(BitConverter.GetBytes(palmRot.w), buf, 12 + (int)offset, 4);
            SetByteBuf(BitConverter.GetBytes(palmRot.x), buf, 16 + (int)offset, 4);
            SetByteBuf(BitConverter.GetBytes(palmRot.y), buf, 20 + (int)offset, 4);
            SetByteBuf(BitConverter.GetBytes(palmRot.z), buf, 24 + (int)offset, 4);
            // ここまでで配列bufのbuf[0 + offset]からbuf[27 + offset]まで使用

            // 関節の回転
            // ここで配列bufのbuf[28 + offset]からbuf[188 + offset]まで使用（40+16*9+4=188）
            for (int i = 0; i < jointRot.Length; i++)
            {
                int shift = 16 * i + (int)offset;
                SetByteBuf(BitConverter.GetBytes(jointRot[i].w), buf, 28 + shift, 4);
                SetByteBuf(BitConverter.GetBytes(jointRot[i].x), buf, 32 + shift, 4);
                SetByteBuf(BitConverter.GetBytes(jointRot[i].y), buf, 36 + shift, 4);
                SetByteBuf(BitConverter.GetBytes(jointRot[i].z), buf, 40 + shift, 4);
            }
        }

        /// <summary>
        /// byteDataからdataCount個の数値ををbufのoffset番目から代入する
        /// </summary>
        /// <param name="byteData">bufに入れたいbyte配列</param>
        /// <param name="buf">バッファ用byte配列</param>
        /// <param name="offset">bufに入れ始めるindex</param>
        /// <param name="dataCount">bufに記録する数値の数</param>
        private void SetByteBuf(byte[] byteData, byte[] buf, int offset, int dataCount)
        {
            for (int i = 0; i < dataCount; i++)
            {
                buf[offset + i] = byteData[i];
            }
        }
    }
}
