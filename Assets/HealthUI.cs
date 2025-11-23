using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Script PlayerHealth (ถ้าไม่ใส่จะหาจาก Tag 'Player')")]
    public PlayerHealth playerHealth;
    
    [Tooltip("Slider สำหรับแสดงหลอดเลือด (ถ้าไม่ใส่จะหาในลูกของ GameObject นี้)")]
    public Slider healthSlider;
    
    [Tooltip("Image สำหรับแสดงหลอดเลือด (ถ้าใช้ Image Fill แทน Slider)")]
    public Image healthFillImage;

    [Header("UI Settings")]
    public Gradient healthColorGradient;
    public Image fillImageToColor;

    void Start()
    {
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
            // Subscribe event
            playerHealth.OnHealthChanged.AddListener(UpdateHealthUI);
            
            // อัพเดทครั้งแรก
            UpdateHealthUI(playerHealth.currentHealth, playerHealth.maxHealth);
        }
        else
        {
            Debug.LogWarning("HealthUI: ไม่พบ PlayerHealth script!");
        }

        // หา Slider ถ้ายังไม่ได้ใส่
        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
        }

        // หา Fill Image อัตโนมัติจาก Slider
        if (healthFillImage == null && healthSlider != null)
        {
            if (healthSlider.fillRect != null)
            {
                healthFillImage = healthSlider.fillRect.GetComponent<Image>();
            }
        }
    }

    public void UpdateHealthUI(int current, int max)
    {
        float fillAmount = (float)current / max;

        if (healthSlider != null)
        {
            healthSlider.value = fillAmount;
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = fillAmount;
            
            // ถ้าเลือดหมด (0) ให้ซ่อนรูปภาพไปเลย เพื่อไม่ให้เห็นขอบมนๆ
            if (fillAmount <= 0)
            {
                healthFillImage.enabled = false;
            }
            else
            {
                healthFillImage.enabled = true;
            }
        }

        // เปลี่ยนสีตามเลือด (ถ้ามีการตั้งค่า Gradient)
        if (fillImageToColor != null && healthColorGradient != null)
        {
            fillImageToColor.color = healthColorGradient.Evaluate(fillAmount);
        }
    }
}
