using UnityEngine;

public class CollectableBox : MonoBehaviour
{
    [Header("Box Settings")]
    [Tooltip("ID ของกล่อง (ใช้สำหรับตรวจสอบว่าต้องส่งกล่องไหนบ้าง)")]
    public int boxID = 0;

    [Tooltip("ชื่อของกล่อง")]
    public string boxName = "Box";

    [Header("Visual Feedback")]
    [Tooltip("แสดงข้อความเมื่อเก็บกล่อง")]
    public bool showCollectMessage = true;

    [Tooltip("สีของกล่องเมื่อถูกคลิก")]
    public Color highlightColor = Color.yellow;

    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private static int totalCollectedBoxes = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // เพิ่ม Collider2D ถ้ายังไม่มี (สำหรับการคลิก)
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
        }
    }

    void OnMouseDown()
    {
        if (!isCollected)
        {
            CollectBox();
        }
    }

    void CollectBox()
    {
        isCollected = true;
        totalCollectedBoxes++;

        // ซ่อนกล่อง
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // ปิด Collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // แสดงข้อความ
        if (showCollectMessage)
        {
            Debug.Log($"Collected: {boxName} (ID: {boxID})");
        }

        // เพิ่มกล่องเข้า Inventory (ผ่าน DeliveryNPC)
        DeliveryNPC.AddBoxToInventory(boxID, boxName);
    }

    // ฟังก์ชันสำหรับรีเซ็ตกล่อง (ถ้าต้องการ)
    public void ResetBox()
    {
        isCollected = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    // ฟังก์ชันสำหรับตรวจสอบว่าถูกเก็บแล้วหรือยัง
    public bool IsCollected()
    {
        return isCollected;
    }

    // ฟังก์ชันสำหรับรีเซ็ตจำนวนกล่องที่เก็บทั้งหมด
    public static void ResetTotalCollected()
    {
        totalCollectedBoxes = 0;
    }
}

