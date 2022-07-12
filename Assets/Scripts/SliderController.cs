using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

public class SliderController : MonoBehaviour
{
    EventTrigger eventTrigger;
    private VideoPlayer video;
    private Slider timeSlider;
    private Text currentTime, maxTime;
    private float staticFrameRate;
    private bool isTouching = false;
    private bool wasPlaying = false;
    [SerializeField] private float wait = 0.2f;

    public void Start()
    {
        //準備ができるまでUpdate()が呼ばれないようにするため無効化する
        enabled = false;

        //Event Triggerコンポーネントの追加
        eventTrigger = this.gameObject.AddComponent<EventTrigger>();

        //Sliderの取得、設定
        timeSlider = GameObject.Find("TimeSlider").GetComponent<Slider>();
        timeSlider.wholeNumbers = true;

        //VideoPlayerの取得、設定
        video = GameObject.Find("Display").GetComponent<VideoPlayer>();
        video.prepareCompleted += OnCompletePrepare;
    }

    void OnCompletePrepare(VideoPlayer source)
    {
        Debug.Log("準備できた");
        staticFrameRate = video.frameRate;
        Debug.Log("frameRate: " + staticFrameRate);
        timeSlider.maxValue = video.frameCount;

        //PointerDownの追加
        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback.AddListener(PointerDown);
        eventTrigger.triggers.Add(entryDown);

        //PointerUpの追加
        EventTrigger.Entry entryUp = new EventTrigger.Entry();
        entryUp.eventID = EventTriggerType.PointerUp;
        entryUp.callback.AddListener(PointerUp);
        eventTrigger.triggers.Add(entryUp);

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
    public void PointerDown(BaseEventData data)
    {
        isTouching = true;
        if (video.isPlaying)
        {
            Debug.Log("再生中だったので止めました");
            wasPlaying = true;
            video.Pause();
        }
        StartCoroutine("ShowPreview");
    }

    /// <summary>
    /// スライダーの操作をやめたときの動作。
    /// </summary>
    public void PointerUp(BaseEventData data)
    {
        isTouching = false;  
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

    /// <summary>
    /// スライダーを操作している間のコルーチン。
    /// wait秒おきにプレビューを更新。
    /// スライダー操作前に再生中だった場合は再生する。
    /// </summary>
    private IEnumerator ShowPreview()
    {
        while (isTouching)
        {
            video.frame = (long)timeSlider.value;
            Debug.Log("コルーチン");
            video.Play();
            video.Pause();
            yield return new WaitForSeconds(wait);
        }

        Debug.Log("コルーチン終了");
        if (wasPlaying)
        {
            video.Play();
            Debug.Log("restart frame: " + video.frame);
            wasPlaying = false;
        }
    }
}
