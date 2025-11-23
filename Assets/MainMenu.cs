using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("ชื่อ Scene ของ Cutscene ที่จะเล่นหลังจากกด Play")]
    public string cutsceneSceneName = "Cutscene";

    public void PlayGame()
    {
        Debug.Log($"Loading Cutscene: {cutsceneSceneName}");
        SceneManager.LoadScene(cutsceneSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game requested.");
        Application.Quit();
    }
}
