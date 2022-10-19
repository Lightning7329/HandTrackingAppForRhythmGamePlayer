using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    private VideoPlayer video;
    private Text txt_speed;
    private Button playButton, forwardButton, backwardButton, addSpeedButton, subSpeedButton;
    private bool isPlaying = false;
    [SerializeField] private int skipSeconds = 5;
    private float currentSpeed = 1.0f;
    [SerializeField] private float speedChange = 0.05f;

    void Start()
    {
        video = GetComponent<VideoPlayer>();
        video.frame = 500;
        video.Play();
        video.Pause();
        //Debug.Log("lengh: " + video.length + "s");
        //Debug.Log("frame rate: " + video.frameRate);
        //Debug.Log("frame count: " + video.frameCount);
        SetButton(ref playButton, "PlayButton", OnBtn_playButton, "Play");
        SetButton(ref forwardButton, "ForwardButton", OnBtn_Forward, $"{skipSeconds}s");
        SetButton(ref backwardButton, "BackwardButton", OnBtn_Backward, $"{skipSeconds}s");
        SetButton(ref addSpeedButton, "AddSpeedButton", OnBtn_AddSpeed, "+0.05");
        SetButton(ref subSpeedButton, "SubSpeedButton", OnBtn_SubSpeed, "-0.05");
        txt_speed = GameObject.Find("Speed").transform.Find("Text").gameObject.GetComponent<Text>();
        txt_speed.text = "x1.00";
    }

    //void Update()
    //{

    //}

    /// <summary>
    /// 再生ボタンを押したときに実行。再生中の場合は一時停止、一時停止中の場合は再生する。
    /// </summary>
    void OnBtn_playButton()
    {
        Text t = playButton.transform.Find("Text").GetComponent<Text>();
        if (isPlaying)
        {
            video.Pause();
            t.text = "Play";
            isPlaying = false;
        }
        else
        {
            video.Play();
            t.text = "Pause";
            isPlaying = true;
        }
    }

    /// <summary>
    /// FBSeconds 秒進む
    /// </summary>
    void OnBtn_Forward()
    {
        video.frame += skipSeconds * (long)video.frameRate;
    }

    /// <summary>
    /// FBSeconds 秒戻る
    /// </summary>
    void OnBtn_Backward()
    {
        video.frame -= skipSeconds * (long)video.frameRate;
    }

    void OnBtn_AddSpeed()
    {
        video.playbackSpeed = currentSpeed += speedChange;
        txt_speed.text = $"x{currentSpeed:F2}";
    }

    void OnBtn_SubSpeed()
    {
        video.playbackSpeed = currentSpeed -= speedChange;
        txt_speed.text = $"x{currentSpeed:F2}";
    }

    private void SetButton(ref Button button, string name, UnityEngine.Events.UnityAction call, string text)
    {
        button = GameObject.Find(name).GetComponent<Button>();
        button.onClick.AddListener(call);
        button.transform.Find("Text").GetComponent<Text>().text = text;
    }
}
