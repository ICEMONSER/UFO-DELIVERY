using UnityEngine;

public class ProximityAnimator : MonoBehaviour
{
    [Header("Animator Settings (Modern)")]
    [Tooltip("Animator ที่จะให้เล่น (ลากตัวที่ทำดาเมจมาใส่ที่นี่)")]
    public Animator targetAnimator;

    [Tooltip("ชื่อ Parameter ใน Animator ที่เป็นแบบ Trigger")]
    public string triggerName = "Activate";

    [Header("Animation Settings (Legacy)")]
    [Tooltip("Animation ที่จะให้เล่น (ถ้าใช้แบบเก่า)")]
    public Animation targetLegacyAnimation;

    [Tooltip("ชื่อ Clip ที่จะให้เล่น (สำหรับ Animation แบบเก่า)")]
    public string clipName;

    [Header("General Settings")]
    [Tooltip("เล่นแค่ครั้งเดียวหรือไม่")]
    public bool playOnce = false;

    [Tooltip("Tag ของผู้เล่น")]
    public string playerTag = "Player";

    [Header("Audio Settings")]
    [Tooltip("เสียงที่จะเล่นเมื่อ Animation เริ่ม (ลากไฟล์เสียงมาใส่)")]
    public AudioClip triggerSound;

    [Tooltip("ระยะเวลาที่จะเล่นเสียง (วินาที) - ใส่ 0 เพื่อเล่นจนจบไฟล์")]
    public float soundDuration = 0f;

    [Tooltip("AudioSource สำหรับเล่นเสียง (ถ้าไม่ใส่จะหาในตัวนี้)")]
    public AudioSource audioSource;

    private bool hasPlayed = false;

    private void Start()
    {
        // ... (existing code) ...
        // 1. ลองหา Animator (Modern)
        if (targetAnimator == null)
        {
            targetAnimator = GetComponent<Animator>();
            if (targetAnimator == null)
            {
                targetAnimator = GetComponentInParent<Animator>();
            }
        }

        // 2. ลองหา Animation (Legacy)
        if (targetLegacyAnimation == null)
        {
            targetLegacyAnimation = GetComponent<Animation>();
            if (targetLegacyAnimation == null)
            {
                targetLegacyAnimation = GetComponentInParent<Animation>();
            }
        }

        // 3. เตรียม AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // ถ้าไม่มี AudioSource ให้สร้างใหม่
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayed && playOnce) return;

        if (other.CompareTag(playerTag))
        {
            bool played = false;

            // ... (existing animation logic) ...
            // 1. เล่นแบบ Legacy Animation ก่อน (ถ้ามี)
            if (targetLegacyAnimation != null)
            {
                if (!string.IsNullOrEmpty(clipName))
                {
                    if (targetLegacyAnimation.GetClip(clipName) != null)
                    {
                        targetLegacyAnimation.Play(clipName);
                        Debug.Log($"ProximityAnimator: Played legacy animation clip '{clipName}'.");
                        played = true;
                    }
                    else
                    {
                        Debug.LogError($"ProximityAnimator: Animation clip '{clipName}' not found on object '{targetLegacyAnimation.name}'! Please check the name or add the clip to the Animation component.");
                    }
                }
                else
                {
                    if (targetLegacyAnimation.clip != null)
                    {
                        targetLegacyAnimation.Play();
                        Debug.Log($"ProximityAnimator: Played default legacy animation '{targetLegacyAnimation.clip.name}'.");
                        played = true;
                    }
                    else
                    {
                         Debug.LogError($"ProximityAnimator: No default animation clip found on object '{targetLegacyAnimation.name}'!");
                    }
                }
            }
            // 2. ถ้าไม่มี Legacy ให้เล่นแบบ Animator (Modern)
            else if (targetAnimator != null)
            {
                targetAnimator.SetTrigger(triggerName);
                Debug.Log($"ProximityAnimator: Triggered '{triggerName}' animator parameter.");
                played = true;
            }
            else
            {
                Debug.LogWarning("ProximityAnimator: No Animator or Animation assigned!");
            }

            if (played)
            {
                // เล่นเสียงถ้ามี
                if (triggerSound != null && audioSource != null)
                {
                    audioSource.clip = triggerSound;
                    audioSource.Play();

                    // ถ้ากำหนดระยะเวลาเสียงไว้ ให้หยุดเมื่อถึงเวลา
                    if (soundDuration > 0)
                    {
                        StartCoroutine(StopSoundAfterDelay(soundDuration));
                    }
                }

                hasPlayed = true;
            }
        }
    }

    private System.Collections.IEnumerator StopSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
