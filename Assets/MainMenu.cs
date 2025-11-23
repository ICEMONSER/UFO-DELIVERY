using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("ชื่อ Scene ของเกมที่จะเล่นหลังจากจบวิดีโอ")]
    public string gameplaySceneName = "Level1";

    [Header("Video Settings")]
    [Tooltip("Video Player ที่จะใช้เล่น Cutscene")]
    public VideoPlayer videoPlayer;
    
    [Tooltip("UI ที่จะซ่อนเมื่อกด Play (เช่น ปุ่ม Play/Exit)")]
    public GameObject menuUI;
    
    [Tooltip("UI ที่จะแสดงเมื่อกด Play (เช่น RawImage ของวิดีโอ) - Optional")]
    public GameObject videoUI;

    [Tooltip("ปุ่มสำหรับข้าม Cutscene")]
    public KeyCode skipKey = KeyCode.Space;

    private bool isPlayingCutscene = false;

    void Start()
    {
        // Preload วิดีโอทันทีที่เข้าหน้าเมนู
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false; // ป้องกันการเล่นเอง
            videoPlayer.Prepare(); // โหลดรอไว้เลย
            videoPlayer.loopPointReached += OnVideoEnd;
            Debug.Log("MainMenu: Preloading video...");
        }
        
        if (videoUI != null)
        {
            videoUI.SetActive(false);
        }
        
        if (menuUI != null)
        {
            menuUI.SetActive(true);
        }
    }

    void Update()
    {
        if (isPlayingCutscene)
        {
            if (Input.GetKeyDown(skipKey))
            {
                Debug.Log("Skipped cutscene.");
                LoadGameplayScene();
            }
        }
    }

    public void PlayGame()
    {
        if (videoPlayer != null)
        {
            // ซ่อนเมนู
            if (menuUI != null) menuUI.SetActive(false);
            
            // แสดงจอวิดีโอ (ถ้ามี)
            if (videoUI != null) videoUI.SetActive(true);

            // เล่นวิดีโอที่ Preload ไว้
            isPlayingCutscene = true;
            videoPlayer.Play();
            Debug.Log("Playing Cutscene (Preloaded)...");
        }
        else
        {
            // ถ้าไม่ได้ตั้งค่า Video ให้เข้าเกมเลย
            Debug.LogWarning("Video Player not set! Loading game directly.");
            LoadGameplayScene();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video finished.");
        LoadGameplayScene();
    }

    void LoadGameplayScene()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game requested.");
        Application.Quit();
    }
}
