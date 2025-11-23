using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Panel หรือ GameObject ที่เป็นหน้า Game Over")]
    public GameObject gameOverPanel;

    [Tooltip("ปุ่ม Restart (ถ้ามี)")]
    public Button restartButton;

    [Tooltip("PlayerHealth Script (ถ้าไม่ใส่จะหาจาก Tag 'Player')")]
    public PlayerHealth playerHealth;

    void Start()
    {
        // ซ่อนหน้า Game Over ตอนเริ่มเกม
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // หา PlayerHealth ถ้ายังไม่ได้ใส่
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        if (playerHealth != null)
        {
            // Subscribe event เมื่อผู้เล่นตาย
            playerHealth.OnDeath.AddListener(ShowGameOver);
        }

        // ตั้งค่าปุ่ม Restart
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            // หยุดเวลาเมื่อ Game Over
            Time.timeScale = 0f;
            Debug.Log("GameOverUI: Showing Game Over screen and pausing game.");
        }
    }

    public void RestartGame()
    {
        // คืนค่าเวลาก่อนรีโหลดฉาก
        Time.timeScale = 1f;
        Debug.Log("GameOverUI: Restarting game...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
