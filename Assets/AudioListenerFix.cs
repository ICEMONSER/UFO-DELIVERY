using UnityEngine;

/// <summary>
/// สคริปต์สำหรับแก้ไขปัญหา Audio Listener หลายตัวใน Scene
/// ควรมี Audio Listener เพียงตัวเดียว (ปกติอยู่ที่ Main Camera)
/// </summary>
public class AudioListenerFix : MonoBehaviour
{
    [Header("Auto Fix Settings")]
    [Tooltip("แก้ไขอัตโนมัติเมื่อ Start (ปิด Audio Listener ที่ไม่ใช่ Main Camera)")]
    public bool autoFixOnStart = true;

    [Tooltip("แสดงข้อความเมื่อแก้ไข")]
    public bool showDebugMessages = true;

    void Start()
    {
        if (autoFixOnStart)
        {
            FixAudioListeners();
        }
    }

    /// <summary>
    /// แก้ไขปัญหา Audio Listener หลายตัว
    /// </summary>
    [ContextMenu("Fix Audio Listeners")]
    public void FixAudioListeners()
    {
        // หา Audio Listener ทั้งหมดใน Scene
        AudioListener[] allListeners = FindObjectsOfType<AudioListener>();

        if (allListeners.Length == 0)
        {
            if (showDebugMessages)
            {
                Debug.LogWarning("AudioListenerFix: ไม่พบ Audio Listener ใน Scene! ควรมีอย่างน้อย 1 ตัวที่ Main Camera");
            }
            return;
        }

        if (allListeners.Length == 1)
        {
            if (showDebugMessages)
            {
                Debug.Log($"AudioListenerFix: พบ Audio Listener 1 ตัว (ปกติ) - {allListeners[0].gameObject.name}");
            }
            return;
        }

        // มี Audio Listener มากกว่า 1 ตัว → ต้องแก้ไข
        if (showDebugMessages)
        {
            Debug.LogWarning($"AudioListenerFix: พบ Audio Listener {allListeners.Length} ตัว! กำลังแก้ไข...");
        }

        // หา Main Camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }

        AudioListener mainCameraListener = null;
        if (mainCamera != null)
        {
            mainCameraListener = mainCamera.GetComponent<AudioListener>();
        }

        // ปิด Audio Listener ที่ไม่ใช่ Main Camera
        int disabledCount = 0;
        foreach (AudioListener listener in allListeners)
        {
            if (listener == mainCameraListener)
            {
                // เก็บ Audio Listener ของ Main Camera ไว้
                if (showDebugMessages)
                {
                    Debug.Log($"AudioListenerFix: เก็บ Audio Listener ของ Main Camera: {listener.gameObject.name}");
                }
            }
            else
            {
                // ปิด Audio Listener ที่ไม่ใช่ Main Camera
                listener.enabled = false;
                disabledCount++;
                if (showDebugMessages)
                {
                    Debug.Log($"AudioListenerFix: ปิด Audio Listener ที่: {listener.gameObject.name}");
                }
            }
        }

        // ถ้า Main Camera ไม่มี Audio Listener → เพิ่มให้
        if (mainCamera != null && mainCameraListener == null)
        {
            mainCameraListener = mainCamera.gameObject.AddComponent<AudioListener>();
            if (showDebugMessages)
            {
                Debug.Log($"AudioListenerFix: เพิ่ม Audio Listener ให้ Main Camera: {mainCamera.gameObject.name}");
            }
        }

        if (showDebugMessages)
        {
            Debug.Log($"AudioListenerFix: แก้ไขเสร็จแล้ว! ปิด {disabledCount} Audio Listener, เก็บไว้ 1 ตัวที่ Main Camera");
        }
    }

    /// <summary>
    /// ตรวจสอบ Audio Listener ใน Scene
    /// </summary>
    [ContextMenu("Check Audio Listeners")]
    public void CheckAudioListeners()
    {
        AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
        
        Debug.Log($"=== Audio Listener Check ===");
        Debug.Log($"พบ Audio Listener ทั้งหมด: {allListeners.Length} ตัว");

        for (int i = 0; i < allListeners.Length; i++)
        {
            AudioListener listener = allListeners[i];
            Debug.Log($"{i + 1}. {listener.gameObject.name} - Enabled: {listener.enabled}");
        }

        if (allListeners.Length > 1)
        {
            Debug.LogWarning("⚠️ มี Audio Listener มากกว่า 1 ตัว! ควรมีเพียง 1 ตัวที่ Main Camera");
        }
        else if (allListeners.Length == 1)
        {
            Debug.Log("✅ มี Audio Listener 1 ตัว (ปกติ)");
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่พบ Audio Listener! ควรมี 1 ตัวที่ Main Camera");
        }
    }
}

