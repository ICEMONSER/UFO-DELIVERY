using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Tooltip("จำนวนดาเมจที่ทำใส่ผู้เล่น")]
    public int damageAmount = 10;

    [Tooltip("ทำลายตัวเองเมื่อชนผู้เล่นหรือไม่")]
    public bool destroyOnHit = false;

    [Tooltip("Tag ของผู้เล่น")]
    public string playerTag = "Player";

    [Header("Audio Settings")]
    [Tooltip("เสียงที่จะเล่นเมื่อชนผู้เล่น")]
    public AudioClip hitSound;

    [Tooltip("AudioSource สำหรับเล่นเสียง (ถ้าไม่ใส่จะหาในตัวนี้)")]
    public AudioSource audioSource;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckAndDealDamage(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckAndDealDamage(other.gameObject);
    }

    private void CheckAndDealDamage(GameObject target)
    {
        if (target.CompareTag(playerTag))
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // เล่นเสียงเมื่อชน
                if (hitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }

                playerHealth.TakeDamage(damageAmount);
                
                if (destroyOnHit)
                {
                    // ถ้าต้องทำลายตัวเอง ให้รอเสียงเล่นจบก่อน (ถ้ามีเสียง) หรือทำลายทันที
                    if (hitSound != null)
                    {
                        // ปิด Renderer และ Collider เพื่อให้ดูเหมือนหายไป
                        GetComponent<Renderer>().enabled = false;
                        GetComponent<Collider2D>().enabled = false;
                        Destroy(gameObject, hitSound.length);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
