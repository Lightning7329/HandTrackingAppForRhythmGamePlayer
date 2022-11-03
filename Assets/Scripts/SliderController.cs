using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

namespace KW_Mocap
{
    public class SliderController : MonoBehaviour
    {
        EventTrigger eventTrigger;
        private VideoPlayer video;
        private Slider timeSlider;
        private Text currentTime, maxTime;
        private bool isTouching = false;
        private bool wasPlaying = false;
        [SerializeField] private GameObject display;
        [SerializeField] private float wait = 0.2f;

        void Start()
        {
            //準備ができるまでUpdate()が呼ばれないようにするため無効化する
            enabled = false;

            //Event Triggerコンポーネントの追加
            eventTrigger = this.gameObject.AddComponent<EventTrigger>();

            //Sliderの取得、設定
            timeSlider = GetComponent<Slider>();
            timeSlider.wholeNumbers = true;

            //VideoPlayerの取得、設定
            video = display.GetComponent<VideoPlayer>();
            video.prepareCompleted += OnCompletePrepare;
        }

        private void OnCompletePrepare(VideoPlayer source)
        {
            Debug.Log("再生準備完了");
            timeSlider.maxValue = video.frameCount;

            //[イベントトリガー] PointerDownの追加
            EventTrigger.Entry entryDown = new EventTrigger.Entry();
            entryDown.eventID = EventTriggerType.PointerDown;
            entryDown.callback.AddListener(PointerDown);
            eventTrigger.triggers.Add(entryDown);

            //[イベントトリガー] PointerUpの追加
            EventTrigger.Entry entryUp = new EventTrigger.Entry();
            entryUp.eventID = EventTriggerType.PointerUp;
            entryUp.callback.AddListener(PointerUp);
            eventTrigger.triggers.Add(entryUp);

            //時刻の表示を初期化
            maxTime = GameObject.Find("MaxTime").GetComponent<Text>();
            maxTime.text = secondsToMMSS(video.length);
            currentTime = GameObject.Find("CurrentTime").GetComponent<Text>();
            currentTime.text = secondsToMMSS(video.clockTime);

            //GameObjectを有効することでUpdate()が呼ばれるようになる
            enabled = true;
        }

        void Update()
        {
            //現時刻の表示を更新
            currentTime.text = secondsToMMSS(video.clockTime);

            //スライダーを触っていないときのみ毎フレームスライダーを進める
            if (!isTouching)
                timeSlider.value = video.frame;
        }

        /// <summary>
        /// スライダーを操作しているときの動作。
        /// 再生中であれば、それのことを記録して一時停止する。
        /// ShowPreview関数をコルーチンで呼び出す。
        /// </summary>
        private void PointerDown(BaseEventData data)
        {
            isTouching = true;
            if (video.isPlaying)
            {
                wasPlaying = true;
                video.Pause();
            }
            StartCoroutine("ShowPreview");
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
        private IEnumerator ShowPreview()
        {
            Debug.Log("コルーチン開始");
            while (isTouching)
            {
                video.frame = (long)timeSlider.value;
                video.Play();
                video.Pause();
                yield return new WaitForSeconds(wait);
            }

            // スライダー操作前に再生中だった場合は再生する。
            if (wasPlaying)
            {
                video.Play();
                wasPlaying = false;
            }
            Debug.Log("コルーチン終了");
        }

        /// <summary>
        /// 秒から "分分:秒秒" 形式の文字列を得る。
        /// </summary>
        /// <param name="second">秒</param>
        /// <returns>"分分:秒秒" 形式文字列</returns>
        private string secondsToMMSS(double second)
        {
            var t = System.TimeSpan.FromSeconds(second);
            return string.Format("{0:0}:{1:00}", (int)t.TotalMinutes, t.Seconds);
        }
    }
}
