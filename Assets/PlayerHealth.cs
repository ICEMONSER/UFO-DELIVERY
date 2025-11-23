using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("เลือดสูงสุดของผู้เล่น")]
    public int maxHealth = 100;
    
    [Tooltip("เลือดปัจจุบัน (แสดงผลเท่านั้น)")]
    public int currentHealth;

    [Header("Fall Settings")]
    [Tooltip("ระดับความสูงที่ถ้าต่ำกว่านี้จะถือว่าตกเหวตาย")]
    public float fallThreshold = -20f;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged; // current, max
    public UnityEvent OnDeath;

    [Header("Audio Settings")]
    [Tooltip("เสียงเมื่อได้รับดาเมจ")]
    public AudioClip damageSound;
    
    [Tooltip("AudioSource สำหรับเล่นเสียง (ถ้าไม่ใส่จะหาในตัวนี้)")]
    public AudioSource audioSource;

    private bool isDead = false;
    private Vector3 startPosition;

    private bool isTeleporting = false;

    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        
        // เตรียม AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        
        // แจ้งเตือน UI ครั้งแรก
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        if (isDead || isTeleporting) return;

        // ตรวจสอบการตกเหว
        if (transform.position.y < fallThreshold)
        {
            Die("Fell to death");
        }
    }

    public void SetTeleporting(bool teleporting)
    {
        isTeleporting = teleporting;
        if (isTeleporting)
        {
            Debug.Log("Player is teleporting... Fall damage disabled.");
        }
        else
        {
            Debug.Log("Player finished teleporting. Fall damage enabled.");
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {amount} damage. Current Health: {currentHealth}/{maxHealth}");
        
        // เล่นเสียงเจ็บ
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die("Health reached 0");
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        Debug.Log($"Player healed {amount}. Current Health: {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die(string reason)
    {
        if (isDead) return;
        
        isDead = true;
        
        // ปรับเลือดให้เป็น 0 และอัพเดท UI
        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"Game Over! Reason: {reason}");
        
        OnDeath?.Invoke();

        // ไม่รีโหลดฉากอัตโนมัติแล้ว ให้ GameOverUI จัดการแทน
        // StartCoroutine(ReloadSceneDelay(2f));
    }

    private System.Collections.IEnumerator ReloadSceneDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
