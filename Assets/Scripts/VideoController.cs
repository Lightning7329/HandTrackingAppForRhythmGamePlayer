using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    private VideoPlayer video;
    private Button playButton, forwardButton, backwardButton, addSpeedButton, subSpeedButton;
    private bool isPlaying = false;
    public int skipSeconds = 5;
    private float currentSpeed = 1.0f;
    public float speedChange = 0.05f;
    public bool useURL = true;

    void Start()
    {
        video = GetComponent<VideoPlayer>();
        if (useURL) setVideoClip();
        else video.source = VideoSource.VideoClip;
        video.frame = 500;
        video.Play();
        video.Pause();
        //Debug.Log("lengh: " + video.length + "s");
        //Debug.Log("frame rate: " + video.frameRate);
        //Debug.Log("frame count: " + video.frameCount);
        SetButton(ref playButton, "PlayButton", OnBtn_playButton, "Play");
        SetButton(ref forwardButton, "ForwardButton", OnBtn_Forward, $"{skipSeconds}s");
        SetButton(ref backwardButton, "BackwardButton", OnBtn_Backward, $"{skipSeconds}s");
        SetButton(ref addSpeedButton, "AddSpeedButton", OnBtn_AddSpeed, $"×{currentSpeed + speedChange:F2}");
        SetButton(ref subSpeedButton, "SubSpeedButton", OnBtn_SubSpeed, $"×{currentSpeed - speedChange:F2}");
    }

    void Update()
    {

    }

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
            //Debug.Log("current frame: " + video.frame);
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
        addSpeedButton.transform.Find("Text").GetComponent<Text>().text = $"×{currentSpeed + speedChange:F2}";
        subSpeedButton.transform.Find("Text").GetComponent<Text>().text = $"×{currentSpeed - speedChange:F2}";
    }

    void OnBtn_SubSpeed()
    {
        video.playbackSpeed = currentSpeed -= speedChange;
        addSpeedButton.transform.Find("Text").GetComponent<Text>().text = $"×{currentSpeed + speedChange:F2}";
        subSpeedButton.transform.Find("Text").GetComponent<Text>().text = $"×{currentSpeed - speedChange:F2}";
    }

    private void SetButton(ref Button button, string name, UnityEngine.Events.UnityAction call, string text)
    {
        button = GameObject.Find(name).GetComponent<Button>();
        button.onClick.AddListener(call);
        button.transform.Find("Text").GetComponent<Text>().text = text;
    }

    /// <summary>
    /// StreamingAssetsフォルダ上にあるファイルをファイルパスから動的にVideo Clipを割り当てる。
    /// </summary>
    void setVideoClip()
    {
        // 指定ディレクトリ配下の Unlimited(Hard).MP4 を再生する
        string filename = string.Format(@"Unlimited(Hard).MP4");
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, filename);
        video.url = filePath;
    }
}
