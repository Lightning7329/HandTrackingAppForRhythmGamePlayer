using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace KW_Mocap
{
    [RequireComponent(typeof(Slider))]
    public class SliderController : MonoBehaviour
    {
        private bool isTouching = false;
        EventTrigger eventTrigger;
        MotionPlayer motion;
        VideoController video;
        Slider timeSlider;
        Text currentTime, maxTime;
        [SerializeField] private float coroutineWaitTime = 0.2f;

        void Start()
        {
            /* 準備ができるまでUpdate()が呼ばれないようにするため無効化する */
            enabled = false;

            /* Event TriggerコンポーネントをアタッチしてPointerDown, PointerUpを追加 */
            eventTrigger = this.gameObject.AddComponent<EventTrigger>();
            eventTrigger.AddEventTrigger(EventTriggerType.PointerDown, PointerDown);
            eventTrigger.AddEventTrigger(EventTriggerType.PointerUp, PointerUp);

            /* Sliderの取得 */
            timeSlider = GetComponent<Slider>();

            /* VideoPlayerの取得、設定 */
            video = GameObject.FindWithTag("Display").GetComponent<VideoController>();
            video.SetPrepareCompleted(OnCompletePrepare);

            motion = GameObject.Find("Hands").GetComponent<MotionPlayer>();
        }

        /// <summary>
        /// VideoPlayerの準備が終わってからUpdateメソッド開始前までの間にやってもらう処理
        /// </summary>
        /// <param name="source"></param>
        private void OnCompletePrepare(VideoPlayer source)
        {
            Debug.Log("再生準備完了");

            /* 時刻の表示を初期化 */
            maxTime = transform.Find("MaxTime").GetComponent<Text>();
            maxTime.text = SecondsToMMSS(video.Length);
            currentTime = transform.Find("CurrentTime").GetComponent<Text>();
            currentTime.text = SecondsToMMSS(video.ClockTime);

            /* GameObjectを有効することでUpdate()が呼ばれるようになる */
            enabled = true;
        }

        void Update()
        {
            /* 現時刻の表示を更新 */
            currentTime.text = SecondsToMMSS(video.ClockTime);

            /* スライダーを触っていないときのみ毎フレームスライダーを進める
             * video.Frame / video.FrameCountでtimeSlider.valueを計算しようとするとSkip時に上手くいかない
             * 一瞬倍くらいの値の位置までスライダーのヘッドが移動する
             */
            if (!isTouching)
                timeSlider.value = (float)(video.ClockTime / video.Length);

            /* モーションのフレームは動画時間分のモーションフレーム数にスライダーのvalueを掛けたもの */
            motion.frame = (int)(timeSlider.value * motion.frameRate * video.Length);
        }

        /// <summary>
        /// 指定秒数動画を進める。
        /// スライダーはUpdateメソッド内でvideoを参照して更新される。
        /// モーションはスライダーに依存しているのでここで変える必要なし。
        /// </summary>
        /// <param name="second">進める秒数。負だと戻る。</param>
        public void Skip(double second)
        {
            video.Time += second;
            if (!video.isPlaying)
            {
                video.PlayAndPause();
                motion.Play();
            }
        }

        /// <summary>
        /// スライダーを操作しているときの動作。
        /// 再生中であれば、それのことを記録して一時停止する。
        /// ShowPreview関数をコルーチンで呼び出す。
        /// </summary>
        private void PointerDown(BaseEventData data)
        {
            isTouching = true;
            bool wasPlaying = false;
            if (video.isPlaying)
            {
                wasPlaying = true;
                video.PausePlaying();
            }
            StartCoroutine(ShowPreview(wasPlaying));
        }

        /// <summary>
        /// スライダーの操作をやめたときの動作。
        /// </summary>
        private void PointerUp(BaseEventData data)
        {
            isTouching = false;
        }

        /// <summary>
        /// スライダーを操作している間のコルーチン。
        /// wait秒おきにプレビューを更新。
        /// スライダー操作前に再生中だった場合は再生する。
        /// </summary>
        private IEnumerator ShowPreview(bool wasPlaying)
        {
            var wait = new WaitForSeconds(coroutineWaitTime);
            while (isTouching)
            {
                video.Time = timeSlider.value * video.Length;
                video.PlayAndPause();
                motion.Play();
                yield return wait;
            }

            /* スライダー操作前に再生中だった場合は再生する */
            if (wasPlaying)
            {
                video.StartPlaying();
            }
        }

        /// <summary>
        /// 秒から "分分:秒秒" 形式の文字列を得る。
        /// </summary>
        /// <param name="second">秒</param>
        /// <returns>"分分:秒秒" 形式文字列</returns>
        private static string SecondsToMMSS(double second)
        {
            var t = System.TimeSpan.FromSeconds(second);
            return string.Format("{0:0}:{1:00}", (int)t.TotalMinutes, t.Seconds);
        }
    }
}
