using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

// 必須コンポーネントの指定
[RequireComponent(typeof(VideoPlayer))]
public class VideoClipCapture : MonoBehaviour
{
    void setVideoClip()
    {
        // VideoPlayerの参照を取得する
        VideoPlayer videoPlayer = this.gameObject.GetComponent<VideoPlayer>();

        // 指定ディレクトリ配下の drop pop candy(EXPERT).mp4 を再生する
        string filename = string.Format(@"drop pop candy(EXPERT).mov");
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, filename);

        // パスを指定する
        videoPlayer.url = filePath;
    }
}
