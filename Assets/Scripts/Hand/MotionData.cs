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
        /// byte配列を作成するために渡す配列bufの大きさの最小値
        /// localPosition -> 4*3=12byte
        /// localRotation -> 4*4=16byte
        /// joints[5,4].localRotation -> 4*4*20=320byte
        /// 合計 12+16+320=348byte
        /// </summary>
        public const int MinimumBufferSize = 348;

        /// <summary>
        /// 手のモーションデータのうち左か右かでバッファ読み取り時のオフセットが変わる
        /// </summary>
        public enum Offset { Left = 0, Right = MinimumBufferSize };

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
        public Quaternion[,] jointRot = new Quaternion[5, 4];

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
            for (int i = 0; i < jointRot.GetLength(0); i++)
                for (int j = 0; j < jointRot.GetLength(1); j++)
                    this.jointRot[i, j] = Quaternion.identity;
        }

        /// <summary>
        /// 手のひらの位置と回転、各関節の回転から片手分のモーションデータを作成するコンストラクタ。
        /// </summary>
        /// <param name="palmPos"></param>
        /// <param name="palmRot"></param>
        /// <param name="jointsRot"></param>
        public HandData(Vector3 palmPos, Quaternion palmRot, Quaternion[,] jointsRot)
        {
            this.palmPos = palmPos;
            this.palmRot = palmRot;
            for (int i = 0; i < jointRot.GetLength(0); i++)
                for (int j = 0; j < jointRot.GetLength(1); j++)
                    this.jointRot[i, j] = jointsRot[i, j];
        }

        /// <summary>
        /// 手のひらの位置と回転、各関節のtransformから片手分のモーションデータを作成するコンストラクタ。
        /// </summary>
        /// <param name="palmPos"></param>
        /// <param name="palmRot"></param>
        /// <param name="joints"></param>
        public HandData(Vector3 palmPos, Quaternion palmRot, Transform[,] joints)
        {
            this.palmPos = palmPos;
            this.palmRot = palmRot;
            for (int i = 0; i < jointRot.GetLength(0); i++)
                for (int j = 0; j < jointRot.GetLength(1); j++)
                    this.jointRot[i, j] =
                        joints[i, j] == null ?
                        Quaternion.identity :
                        joints[i, j].localRotation;
        }

        /// <summary>
        /// byte配列から片手分のモーションデータを復元するコンストラクタ。
        /// MotionPlayerから呼ばれる。
        /// </summary>
        /// <param name="buf">1フレーム分のモーションデータを格納したbyte配列</param>
        /// <param name="offset">左手か右手かでbyte配列の読み取り位置が異なる</param>
        public HandData(byte[] buf, Offset offset)
        {
            int next = (int)offset;
            // 手のひらの位置と回転
            (palmPos, next) = ExtendedBitConverter.GetVector3FromBytes(buf, next);
            (palmRot, next) = ExtendedBitConverter.GetQuaternionFromBytes(buf, next);
            // 関節の回転
            for (int i = 0; i < jointRot.GetLength(0); i++)
                for (int j = 0; j < jointRot.GetLength(1); j++)
                    (jointRot[i, j], next) = ExtendedBitConverter.GetQuaternionFromBytes(buf, next);
        }

        /// <summary>
        /// 受け取ったbyte配列に片手分のモーションデータをシリアライズして格納する
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        public void SetBytes(byte[] buf, Offset offset)
        {
            int next = (int)offset;
            /* 手のひらの位置と回転
             * ここで配列bufのbuf[0 + offset]からbuf[27 + offset]まで使用 */
            next = palmPos.SetBytesFromVector3(buf, next);
            next = palmRot.SetBytesFromQuaternion(buf, next);

            /* 関節の回転
             * ここで配列bufのbuf[28 + offset]からbuf[347 + offset]まで使用 */
            for (int i = 0; i < jointRot.GetLength(0); i++)
                for (int j = 0; j < jointRot.GetLength(1); j++)
                    next = jointRot[i, j].SetBytesFromQuaternion(buf, next);
        }
    }

    public static class ExtendedBitConverter
    {
        /// <summary>
        /// byteDataからdataCount個の数値ををbufのoffset番目から代入する
        /// </summary>
        /// <param name="byteData">bufに入れたいbyte配列</param>
        /// <param name="buf">バッファ用byte配列</param>
        /// <param name="offset">bufに入れ始めるindex</param>
        /// <param name="dataCount">bufに記録する数値の数</param>
        public static void SetByteBuf(byte[] byteData, byte[] buf, int offset, int dataCount)
        {
            for (int i = 0; i < dataCount; i++)
                buf[offset + i] = byteData[i];
        }

        /// <summary>
        /// Vector2型の構造体変数をシリアライズして引数で受け取ったbyte配列に格納する。
        /// </summary>
        /// <param name="vector2"></param>
        /// <param name="buf">バッファ用byte配列</param>
        /// <param name="offset">bufに入れ始めるindex</param>
        /// <returns></returns>
        public static int SetBytesFromVector2(this Vector2 vector2, byte[] buf, int offset)
        {
            for (int i = 0; i < 2; i++)
                SetByteBuf(BitConverter.GetBytes(vector2[i]), buf, 4 * i + offset, 4);
            return 8 + offset;
        }

        /// <summary>
        /// Vector3型の構造体変数をシリアライズして引数で受け取ったbyte配列に格納する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="buf">バッファ用byte配列</param>
        /// <param name="offset">bufに入れ始めるindex</param>
        /// <returns>次の要素をbufに入れる際のoffset</returns>
        public static int SetBytesFromVector3(this Vector3 position, byte[] buf, int offset)
        {
            for (int i = 0; i < 3; i++)
                SetByteBuf(BitConverter.GetBytes(position[i]), buf, 4 * i + offset, 4);
            return 12 + offset;
        }

        /// <summary>
        /// Quaternion型の構造体変数をシリアライズして引数で受け取ったbyte配列に格納する。
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="buf">バッファ用byte配列</param>
        /// <param name="offset">bufに入れ始めるindex</param>
        /// <returns>次の要素をbufに入れる際のoffset</returns>
        public static int SetBytesFromQuaternion(this Quaternion rotation, byte[] buf, int offset)
        {
            for (int i = 0; i < 4; i++)
                SetByteBuf(BitConverter.GetBytes(rotation[i]), buf, 4 * i + offset, 4);
            return 16 + offset;
        }

        /// <summary>
        /// byte配列のoffsetの位置から8バイト読み取ってVector2型構造体を作成して返す。
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset">読み取り開始位置</param>
        /// <returns></returns>
        public static (Vector2 vector2, int next) GetVector2FromBytes(byte[] buf, int offset)
        {
            Vector2 vector2 = Vector2.zero;
            for (int i = 0; i < 2; i++)
                vector2[i] = BitConverter.ToSingle(buf, 4 * i + offset);
            return (vector2, 8 + offset);
        }

        /// <summary>
        /// byte配列のoffsetの位置から12バイト読み取ってVector3型構造体を作成して返す。
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset">読み取り開始位置</param>
        /// <returns>読み取ったVector3と次の読み取り位置のタプル</returns>
        public static (Vector3 vector3, int next) GetVector3FromBytes(byte[] buf, int offset)
        {
            Vector3 position = Vector3.zero;
            for (int i = 0; i < 3; i++)
                position[i] = BitConverter.ToSingle(buf, 4 * i + offset);
            return (position, 12 + offset);
        }

        /// <summary>
        /// byte配列のoffsetの位置から16バイト読み取ってQuaternion型構造体を作成して返す。
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset">読み取り開始位置</param>
        /// <returns>読み取ったQuaternionと次の読み取り位置のタプル</returns>
        public static (Quaternion rotation, int next) GetQuaternionFromBytes(byte[] buf, int offset)
        {
            Quaternion rotation = Quaternion.identity;
            for (int i = 0; i < 4; i++)
                rotation[i] = BitConverter.ToSingle(buf, 4 * i + offset);
            return (rotation, 16 + offset);
        }
    }
}
