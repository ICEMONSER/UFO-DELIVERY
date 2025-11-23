using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoCutsceneController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ชื่อ Scene ของเกมที่จะเล่นหลังจากจบวิดีโอ")]
    public string gameplaySceneName = "Level1";
    
    [Tooltip("ปุ่มสำหรับข้าม Cutscene")]
    public KeyCode skipKey = KeyCode.Space;

    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        if (videoPlayer != null)
        {
            // ลงทะเบียน event เมื่อเล่นจบ
            videoPlayer.loopPointReached += OnVideoEnd;
            Debug.Log("VideoCutsceneController: Waiting for video to finish...");
        }
        else
        {
            Debug.LogError("VideoCutsceneController: No VideoPlayer component found!");
        }
    }

    void Update()
    {
        // กดปุ่มเพื่อข้าม
        if (Input.GetKeyDown(skipKey))
        {
            Debug.Log("VideoCutsceneController: Skipped by user.");
            LoadGameplayScene();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("VideoCutsceneController: Video finished.");
        LoadGameplayScene();
    }

    void LoadGameplayScene()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }
}
