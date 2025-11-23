using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryNPC : MonoBehaviour
{
    [Header("NPC Settings")]
    [Tooltip("ชื่อของ NPC (แสดงในข้อความต่างๆ)")]
    public string npcName = "Delivery NPC";

    [Tooltip("จำนวนกล่องที่ต้องส่งเพื่อผ่านด่าน (เฉพาะ NPC ตัวนี้)")]
    public int requiredBoxes = 3;

    [Tooltip("ID ของกล่องที่ต้องส่ง (ถ้าว่าง = รับกล่องทุก ID)")]
    public int[] requiredBoxIDs = new int[0];

    [Tooltip("NPC ID (ใช้แยก NPC แต่ละตัว ถ้าไม่กำหนดจะใช้ชื่อ NPC)")]
    public int npcID = 0;

    [Header("Interaction")]
    [Tooltip("ระยะห่างที่สามารถส่งกล่องได้ (ระยะที่ player เดินไปหา NPC ได้)")]
    public float interactionDistance = 5f;

    [Tooltip("ตรวจสอบระยะห่างก่อนส่งกล่อง (ถ้าปิด = ส่งได้ทุกระยะห่าง)")]
    public bool checkDistance = true;

    [Tooltip("แสดงข้อความเมื่ออยู่ใกล้พอที่จะส่งกล่องได้")]
    public bool showProximityMessage = true;

    [Tooltip("Tag ของผู้เล่น")]
    public string playerTag = "Player";

    [Tooltip("Player Object (ลาก Player GameObject มาวางที่นี่ หรือปล่อยว่างเพื่อหาโดยใช้ Tag)")]
    public GameObject playerObjectReference;

    [Header("Teleport Settings")]
    [Tooltip("ตำแหน่งที่ต้องการ Teleport ผู้เล่น (ลาก GameObject ที่มีตำแหน่งที่ต้องการมาวาง)")]
    public Transform teleportTarget;

    [Tooltip("หรือกำหนดตำแหน่ง Teleport โดยตรง (X, Y)")]
    public Vector2 teleportPosition = Vector2.zero;

    [Tooltip("ใช้ Teleport Position แทน Teleport Target")]
    public bool useTeleportPosition = false;

    [Tooltip("เวลาที่รอก่อน Teleport (วินาที)")]
    public float delayBeforeTeleport = 2f;

    [Header("UI Settings")]
    [Tooltip("แสดงข้อความ Level Successful")]
    public bool showSuccessMessage = true;

    [Tooltip("ข้อความ Level Successful")]
    public string successMessage = "Level Successful!";

    [Tooltip("แสดงข้อความเมื่อส่งกล่อง")]
    public bool showDeliveryMessage = true;

    [Tooltip("แสดงข้อความเมื่อผ่านด่าน")]
    public bool showLevelCompleteMessage = true;

    // Inventory เก็บกล่องที่ผู้เล่นเก็บได้
    private static List<int> collectedBoxIDs = new List<int>();
    private static List<string> collectedBoxNames = new List<string>();
    private int deliveredBoxes = 0;
    private bool levelCompleted = false;

    private GameObject successUI;
    private UnityEngine.UI.Text successText;
    private GameObject playerObject;
    private MonoBehaviour playerMovementScript;
    private Rigidbody2D playerRigidbody;
    private Vector2 savedVelocity;

    void Start()
    {
        // เพิ่ม Collider2D ถ้ายังไม่มี (สำหรับการคลิก)
        Collider2D existingCollider = GetComponent<Collider2D>();
        if (existingCollider == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            // ไม่ต้องเป็น trigger เพื่อให้ OnMouseDown() ทำงาน
            collider.isTrigger = false;
            
            // ตั้งค่าขนาด Collider ให้พอดีกับ Sprite
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                collider.size = spriteRenderer.bounds.size;
            }
            else
            {
                // ถ้าไม่มี Sprite → ใช้ขนาดเริ่มต้น
                collider.size = new Vector2(1f, 1f);
            }
            
            Debug.Log($"{npcName}: เพิ่ม BoxCollider2D สำหรับการคลิก (ขนาด: {collider.size})");
        }
        else
        {
            // ถ้ามี Collider อยู่แล้ว ให้ตรวจสอบว่าไม่ใช่ trigger
            if (existingCollider.isTrigger)
            {
                Debug.LogWarning($"{npcName}: Collider2D is set as trigger. OnMouseDown() may not work. Setting isTrigger to false...");
                existingCollider.isTrigger = false;
            }
            
            // ตรวจสอบขนาด Collider
            if (existingCollider is BoxCollider2D boxCollider)
            {
                if (boxCollider.size.x < 0.5f || boxCollider.size.y < 0.5f)
                {
                    Debug.LogWarning($"{npcName}: Collider size is too small ({boxCollider.size}). Increasing size...");
                    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        boxCollider.size = spriteRenderer.bounds.size;
                    }
                    else
                    {
                        boxCollider.size = new Vector2(1f, 1f);
                    }
                }
            }
        }

        // สร้าง UI สำหรับแสดงข้อความ Level Successful
        CreateSuccessUI();

        if (showDeliveryMessage)
        {
            Debug.Log($"{npcName} พร้อมแล้ว! คลิกที่ {npcName} เพื่อส่งกล่อง (ต้องการ {requiredBoxes} กล่อง)");
        }
    }

    void Update()
    {
        // ตรวจสอบการคลิกด้วย Raycast (สำรองในกรณี OnMouseDown() ไม่ทำงาน)
        if (Input.GetMouseButtonDown(0) && !levelCompleted)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = FindObjectOfType<Camera>();
            }

            if (cam == null) return;

            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z; // ใช้ z position ของ NPC

            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);
            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                Debug.Log($"{npcName}: ถูกคลิกผ่าน Raycast!");
                
                // ตรวจสอบระยะห่าง
                bool canDeliver = true;
                
                if (checkDistance)
                {
                    GameObject player = GetPlayer();
                    if (player != null)
                    {
                        // คำนวณระยะห่างแบบ 2D (ไม่คิดแกน Z)
                        float distance = Vector2.Distance(
                            new Vector2(transform.position.x, transform.position.y), 
                            new Vector2(player.transform.position.x, player.transform.position.y)
                        );
                        Debug.Log($"{npcName}: ระยะห่างจากผู้เล่น (Raycast): {distance:F2}, ต้องอยู่ใกล้: {interactionDistance}");
                        
                        if (distance <= interactionDistance)
                        {
                            // อยู่ใกล้แล้ว (ระยะที่เดินไปหาได้) → ส่งกล่อง
                            if (showProximityMessage)
                            {
                                Debug.Log($"✅ อยู่ใกล้ {npcName} แล้ว! ส่งกล่องได้ (ระยะห่าง: {distance:F1})");
                            }
                            canDeliver = true;
                        }
                        else
                        {
                            // อยู่ไกลเกินไป (ระยะที่เดินไปหาไม่ได้)
                            if (showDeliveryMessage || showProximityMessage)
                            {
                                Debug.LogWarning($"อยู่ไกลเกินไป! เดินเข้าใกล้ {npcName} ก่อน (ระยะห่าง: {distance:F1}, ต้องอยู่ใกล้: {interactionDistance})");
                            }
                            canDeliver = false;
                        }
                    }
                    else
                    {
                        // ไม่มี Player → ส่งได้เลย
                        Debug.Log($"{npcName}: Clicked via Raycast! (No player found)");
                        canDeliver = true;
                    }
                }
                else
                {
                    // ไม่ตรวจสอบระยะห่าง → ส่งได้เลย
                    Debug.Log($"{npcName}: Clicked via Raycast! (checkDistance disabled)");
                    canDeliver = true;
                }
                
                // ส่งกล่อง
                if (canDeliver)
                {
                    Debug.Log($"{npcName}: กำลังส่งกล่องผ่าน Raycast...");
                    DeliverBoxes();
                }
            }
        }
    }

    void CreateSuccessUI()
    {
        // สร้าง Canvas ถ้ายังไม่มี
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // สร้าง Panel สำหรับข้อความ
        GameObject panel = new GameObject("SuccessPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f); // สีดำโปร่งใส

        // สร้าง Text สำหรับข้อความ
        GameObject textObj = new GameObject("SuccessText");
        textObj.transform.SetParent(panel.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(600, 100);
        textRect.anchoredPosition = Vector2.zero;

        successText = textObj.AddComponent<UnityEngine.UI.Text>();
        successText.text = successMessage;
        
        // ไม่ต้องกำหนด font (Unity จะใช้ default font อัตโนมัติ)
        // หรือถ้าต้องการกำหนด font ให้ใช้ LegacyRuntime.ttf
        try
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont != null)
            {
                successText.font = defaultFont;
            }
        }
        catch (System.Exception)
        {
            // ถ้าไม่มี font ให้ใช้ default font ของ Unity (ไม่ต้องกำหนด)
        }
        
        successText.fontSize = 48;
        successText.color = Color.green;
        successText.alignment = TextAnchor.MiddleCenter;
        successText.fontStyle = FontStyle.Bold;

        successUI = panel;
        successUI.SetActive(false);
    }

    // ฟังก์ชันสำหรับหา Player
    GameObject GetPlayer()
    {
        // ใช้ playerObjectReference ก่อน
        if (playerObjectReference != null)
        {
            return playerObjectReference;
        }

        // ถ้าไม่มี ให้หาโดยใช้ Tag
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            // ลองหาชื่อ "Player" หรือ "MainCR" (ชื่อตัวละครที่อาจจะใช้)
            player = GameObject.Find("Player");
            if (player == null)
            {
                player = GameObject.Find("MainCR");
            }
        }

        if (player == null)
        {
            Debug.LogWarning($"{npcName}: Player not found! Please set 'Player Object Reference' in Inspector or make sure Player has tag '{playerTag}' or name 'Player'/'MainCR'");
        }

        return player;
    }

    void OnMouseDown()
    {
        Debug.Log($"{npcName}: OnMouseDown() ถูกเรียก!");
        
        if (levelCompleted)
        {
            if (showDeliveryMessage)
            {
                Debug.Log($"{npcName}: สำเร็จแล้ว! (ส่งกล่องครบ {requiredBoxes} กล่องแล้ว)");
            }
            return;
        }

        // ตรวจสอบระยะห่าง (ถ้าเปิดใช้งาน)
        bool canDeliver = true;
        
                if (checkDistance)
                {
                    GameObject player = GetPlayer();
                    if (player != null)
                    {
                        // คำนวณระยะห่างแบบ 2D (ไม่คิดแกน Z)
                        float distance = Vector2.Distance(
                            new Vector2(transform.position.x, transform.position.y), 
                            new Vector2(player.transform.position.x, player.transform.position.y)
                        );
                        Debug.Log($"{npcName}: ระยะห่างจากผู้เล่น: {distance:F2}, ต้องอยู่ใกล้: {interactionDistance}");
                
                if (distance > interactionDistance)
                {
                    // แสดงข้อความเมื่ออยู่ไกลเกินไป (ระยะที่เดินไปหาไม่ได้)
                    if (showDeliveryMessage || showProximityMessage)
                    {
                        Debug.LogWarning($"อยู่ไกลเกินไป! เดินเข้าใกล้ {npcName} ก่อน (ระยะห่าง: {distance:F1}, ต้องอยู่ใกล้: {interactionDistance})");
                    }
                    canDeliver = false; // ไม่ส่งกล่องถ้าอยู่ไกล
                }
                else
                {
                    // ถ้าอยู่ใกล้แล้ว (distance <= interactionDistance) → ส่งกล่องได้เลย
                    if (showProximityMessage)
                    {
                        Debug.Log($"✅ อยู่ใกล้ {npcName} แล้ว! ส่งกล่องได้ (ระยะห่าง: {distance:F1})");
                    }
                    canDeliver = true;
                }
            }
            else
            {
                // ถ้าไม่มี Player → ส่งได้เลย (ไม่ต้องตรวจสอบระยะห่าง)
                if (showDeliveryMessage)
                {
                    Debug.LogWarning($"{npcName}: Player not found. Delivering anyway...");
                }
                canDeliver = true;
            }
        }
        else
        {
            Debug.Log($"{npcName}: checkDistance ปิดอยู่ → ส่งกล่องได้เลย");
            canDeliver = true;
        }

        // ส่งกล่อง (เมื่ออยู่ใกล้หรือปิด checkDistance)
        if (canDeliver)
        {
            Debug.Log($"{npcName}: กำลังส่งกล่อง...");
            DeliverBoxes();
        }
        else
        {
            Debug.LogWarning($"{npcName}: ไม่สามารถส่งกล่องได้ (อยู่ไกลเกินไป)");
        }
    }

    void DeliverBoxes()
    {
        if (showDeliveryMessage)
        {
            Debug.Log($"{npcName}: กำลังส่งกล่อง... (มีกล่องในกระเป๋า: {collectedBoxIDs.Count} กล่อง)");
        }

        if (collectedBoxIDs.Count == 0)
        {
            if (showDeliveryMessage)
            {
                Debug.Log($"{npcName}: คุณยังไม่มีกล่อง! ไปเก็บกล่องก่อนโดยการคลิกที่กล่อง");
            }
            return;
        }

        // ตรวจสอบว่ามีกล่องที่ต้องการหรือไม่
        int validBoxes = 0;
        List<int> boxesToRemove = new List<int>();
        List<string> namesToRemove = new List<string>();

        for (int i = 0; i < collectedBoxIDs.Count; i++)
        {
            int boxID = collectedBoxIDs[i];
            string boxName = collectedBoxNames[i];

            // ตรวจสอบว่าเป็นกล่องที่ต้องการหรือไม่
            bool isValidBox = requiredBoxIDs.Length == 0; // ถ้าไม่มีกำหนด = รับทุกกล่อง
            if (!isValidBox)
            {
                foreach (int requiredID in requiredBoxIDs)
                {
                    if (boxID == requiredID)
                    {
                        isValidBox = true;
                        break;
                    }
                }
            }

            if (isValidBox)
            {
                validBoxes++;
                boxesToRemove.Add(boxID);
                namesToRemove.Add(boxName);

                if (showDeliveryMessage)
                {
                    Debug.Log($"{npcName}: ส่งกล่อง {boxName} (ID: {boxID}) ให้ {npcName} แล้ว!");
                }
            }
        }

        // ลบกล่องที่ส่งแล้วออกจาก Inventory
        foreach (int boxID in boxesToRemove)
        {
            collectedBoxIDs.Remove(boxID);
        }
        foreach (string boxName in namesToRemove)
        {
            collectedBoxNames.Remove(boxName);
        }

        deliveredBoxes += validBoxes;

        if (showDeliveryMessage)
        {
            if (validBoxes == 0)
            {
                Debug.LogWarning($"{npcName}: ไม่มีกล่องที่ส่งได้! กล่องที่ต้องการ: {(requiredBoxIDs.Length > 0 ? string.Join(", ", requiredBoxIDs) : "ทุกกล่อง")}");
            }
            else
            {
                Debug.Log($"{npcName}: ส่งกล่อง {validBoxes} กล่องแล้ว! รวมทั้งหมด: {deliveredBoxes}/{requiredBoxes} กล่อง (สำหรับ {npcName})");
            }
        }

        // ตรวจสอบว่าผ่านด่านหรือไม่ (เฉพาะ NPC ตัวนี้)
        if (deliveredBoxes >= requiredBoxes)
        {
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        levelCompleted = true;

        if (showLevelCompleteMessage)
        {
            Debug.Log($"สำเร็จ! {npcName} รับกล่อง {deliveredBoxes} กล่องแล้ว! (ต้องการ {requiredBoxes} กล่อง)");
        }

        // หยุดการเคลื่อนที่ของผู้เล่น
        DisablePlayerMovement();

        // แสดงข้อความ Level Successful
        if (showSuccessMessage && successUI != null)
        {
            successUI.SetActive(true);
            if (successText != null)
            {
                successText.text = successMessage;
            }
        }

        // เรียกฟังก์ชันเมื่อผ่านด่าน
        StartCoroutine(OnLevelComplete());
    }

    // ฟังก์ชันที่เรียกเมื่อผ่านด่าน (Teleport ผู้เล่น)
    protected virtual IEnumerator OnLevelComplete()
    {
        // รอสักครู่เพื่อให้ผู้เล่นเห็นข้อความ
        yield return new WaitForSeconds(delayBeforeTeleport);

        // Teleport ผู้เล่น
        TeleportPlayer();
    }

    void TeleportPlayer()
    {
        GameObject player = GetPlayer();
        if (player != null)
        {
            Vector3 targetPosition;

            // ตรวจสอบว่าจะใช้ Teleport Target หรือ Teleport Position
            if (useTeleportPosition)
            {
                // ใช้ตำแหน่งที่กำหนด
                targetPosition = new Vector3(teleportPosition.x, teleportPosition.y, player.transform.position.z);
            }
            else if (teleportTarget != null)
            {
                // ใช้ตำแหน่งจาก Transform
                targetPosition = new Vector3(teleportTarget.position.x, teleportTarget.position.y, player.transform.position.z);
            }
            else
            {
                Debug.LogWarning($"{npcName}: No teleport target or position set! Player will not be teleported.");
                Debug.LogWarning($"Please set either 'Teleport Target' or enable 'Use Teleport Position' and set 'Teleport Position' in Inspector!");
                // เปิดการเคลื่อนที่อีกครั้งแม้ว่าจะ Teleport ไม่ได้
                EnablePlayerMovement();
                // ซ่อนข้อความ
                if (successUI != null)
                {
                    successUI.SetActive(false);
                }
                return;
            }

            // Teleport ผู้เล่น
            Vector3 oldPosition = player.transform.position;
            
            // ตรวจสอบ z-position ของ Player (ควรเป็น 0 สำหรับ 2D)
            targetPosition.z = player.transform.position.z;
            
            // ตรวจสอบว่า Player GameObject ยัง active อยู่
            if (!player.activeSelf)
            {
                Debug.LogWarning($"{npcName}: Player GameObject is inactive! Activating it...");
                player.SetActive(true);
            }
            
            // ตรวจสอบว่า Player มี Renderer และเปิดอยู่
            SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
            if (playerRenderer != null && !playerRenderer.enabled)
            {
                Debug.LogWarning($"{npcName}: Player SpriteRenderer is disabled! Enabling it...");
                playerRenderer.enabled = true;
            }
            
            // ป้องกันการตายจากการตกเหวชั่วคราว
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.SetTeleporting(true);
                // คืนค่าหลังจาก 1 วินาที (รอให้ทุกอย่างเข้าที่)
                StartCoroutine(ResetTeleportingStatus(playerHealth, 1f));
            }

            // Teleport ผู้เล่น
            player.transform.position = targetPosition;

            if (showLevelCompleteMessage)
            {
                Debug.Log($"Player teleported from {oldPosition} to {targetPosition}");
                Debug.Log($"Player active: {player.activeSelf}, Position: {player.transform.position}");
            }

            // อัพเดท Camera ให้ตาม Player ทันที (ไม่ใช่ค่อยๆ เคลื่อนที่)
            UpdateCameraPosition(player.transform);
            
            // อัพเดท Camera อีกครั้งใน frame ถัดไป (เพื่อให้แน่ใจว่า Camera ตาม Player)
            StartCoroutine(UpdateCameraNextFrame(player.transform));
            
            // อัพเดท Camera อีกครั้งหลังจาก 0.1 วินาที (เพื่อให้แน่ใจว่า Camera ตาม Player)
            StartCoroutine(UpdateCameraDelayed(player.transform, 0.1f));

            // ซ่อนข้อความ Level Successful หลังจาก Teleport
            if (successUI != null)
            {
                successUI.SetActive(false);
            }

            // เปิดการเคลื่อนที่ของผู้เล่นอีกครั้ง
            EnablePlayerMovement();

            // รีเซ็ต Inventory และ NPC สำหรับ Level ถัดไป
            // Commented out to keep inventory across levels
            // ResetInventory();
            // ResetNPC();
            
            // รีเซ็ตกล่องทั้งหมดที่ถูกเก็บไปแล้ว (ให้กลับมาแสดงอีกครั้ง)
            // Commented out to prevent boxes from respawning
            // ResetAllCollectedBoxes();
        }
        else
        {
            Debug.LogWarning($"{npcName}: Player not found! Cannot teleport.");
        }
    }

    IEnumerator ResetTeleportingStatus(PlayerHealth playerHealth, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerHealth != null)
        {
            playerHealth.SetTeleporting(false);
        }
    }

    // อัพเดท Camera ใน frame ถัดไป (เพื่อให้แน่ใจว่า Camera ตาม Player)
    IEnumerator UpdateCameraNextFrame(Transform playerTransform)
    {
        yield return null; // รอ frame ถัดไป
        UpdateCameraPosition(playerTransform);
        
        if (showLevelCompleteMessage)
        {
            Debug.Log($"Camera updated again in next frame for player at {playerTransform.position}");
        }
    }

    // อัพเดท Camera หลังจาก delay (เพื่อให้แน่ใจว่า Camera ตาม Player)
    IEnumerator UpdateCameraDelayed(Transform playerTransform, float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateCameraPosition(playerTransform);
        
        if (showLevelCompleteMessage)
        {
            Debug.Log($"Camera updated again after {delay}s for player at {playerTransform.position}");
        }
    }

    // อัพเดทตำแหน่ง Camera ให้ตาม Player ทันที
    void UpdateCameraPosition(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning($"{npcName}: Player Transform is null! Cannot update camera.");
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }

        if (mainCamera == null)
        {
            Debug.LogWarning($"{npcName}: Camera not found! Cannot update camera position.");
            return;
        }

        Vector3 playerPos = playerTransform.position;
        Vector3 oldCameraPos = mainCamera.transform.position;

        // หา CameraFollow script
        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            // ตรวจสอบว่า target ถูกต้องหรือไม่
            if (cameraFollow.target == null || cameraFollow.target != playerTransform)
            {
                // อัพเดท target ให้ชี้ไปที่ Player
                cameraFollow.target = playerTransform;
                if (showLevelCompleteMessage)
                {
                    Debug.Log($"CameraFollow target updated to player: {playerTransform.name}");
                }
            }
            
            // คำนวณ Y position ใหม่ตาม Player (เพื่อให้มุมมองเหมือนเดิม)
            float newY = playerPos.y + cameraFollow.yOffset;
            
            // ใช้ UpdateImmediatelyWithYCentered() เพื่ออัพเดท Camera position ทันที
            cameraFollow.UpdateImmediatelyWithYCentered(newY, mainCamera);
            
            Vector3 newCameraPos = mainCamera.transform.position;
            if (showLevelCompleteMessage)
            {
                Debug.Log($"Camera (CameraFollow) updated immediately (centered): {oldCameraPos} → {newCameraPos} (Player: {playerPos})");
                Debug.Log($"Camera Y updated to: {newY} (Player Y: {playerPos.y}, Offset: {cameraFollow.yOffset})");
                Debug.Log($"Player centered in camera view");
            }
            return;
        }

        // หา CameraController script
        CameraController cameraController = mainCamera.GetComponent<CameraController>();
        if (cameraController != null)
        {
            // CameraController ใช้ private field player ดังนั้นเราต้องอัพเดท Camera position โดยตรง
            // อัพเดท Camera position ทันที (ไม่ใช่ค่อยๆ เคลื่อนที่)
            // ใช้ playerPos.y + 0.5f เพื่อให้มุมมองใกล้เคียงกับ CameraFollow (ที่มี yOffset = 1f) แต่ต่ำลงมาหน่อย
            float newY = playerPos.y + 0.5f;
            
            Vector3 newCameraPos = new Vector3(
                playerPos.x,
                newY, 
                mainCamera.transform.position.z
            );
            mainCamera.transform.position = newCameraPos;
            
            if (showLevelCompleteMessage)
            {
                Debug.Log($"Camera (CameraController) updated: {oldCameraPos} → {newCameraPos} (Player: {playerPos})");
                Debug.Log($"Camera Y updated to: {newY} (Player Y: {playerPos.y})");
                Debug.LogWarning($"Note: CameraController uses private player field. Make sure player reference is set in Inspector!");
            }
            return;
        }

        // ถ้าไม่มี Camera script → อัพเดท Camera position โดยตรง
        // ใช้ playerPos.y + 0.5f เพื่อให้มุมมองใกล้เคียงกับ CameraFollow
        Vector3 newCameraPosDirect = new Vector3(
            playerPos.x,
            playerPos.y + 0.5f,
            mainCamera.transform.position.z
        );
        mainCamera.transform.position = newCameraPosDirect;
        
        if (showLevelCompleteMessage)
        {
            Debug.Log($"Camera updated directly: {oldCameraPos} → {newCameraPosDirect} (Player: {playerPos})");
        }
    }

    // หยุดการเคลื่อนที่ของผู้เล่น
    void DisablePlayerMovement()
    {
        playerObject = GetPlayer();
        if (playerObject != null)
        {
            // หา Movement script
            playerMovementScript = playerObject.GetComponent<Movement>();
            if (playerMovementScript != null)
            {
                playerMovementScript.enabled = false;
            }

            // หยุด Rigidbody2D (ถ้ามี)
            playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
            if (playerRigidbody != null)
            {
                savedVelocity = playerRigidbody.linearVelocity;
                playerRigidbody.linearVelocity = Vector2.zero;
                playerRigidbody.isKinematic = true;
            }
        }
    }

    // เปิดการเคลื่อนที่ของผู้เล่นอีกครั้ง
    void EnablePlayerMovement()
    {
        if (playerObject != null)
        {
            // ตรวจสอบว่า Player GameObject ยัง active อยู่
            if (!playerObject.activeSelf)
            {
                Debug.LogWarning($"{npcName}: Player GameObject is inactive in EnablePlayerMovement! Activating it...");
                playerObject.SetActive(true);
            }
            
            // ตรวจสอบว่า Player มี Renderer และเปิดอยู่
            SpriteRenderer playerRenderer = playerObject.GetComponent<SpriteRenderer>();
            if (playerRenderer != null && !playerRenderer.enabled)
            {
                Debug.LogWarning($"{npcName}: Player SpriteRenderer is disabled in EnablePlayerMovement! Enabling it...");
                playerRenderer.enabled = true;
            }
            
            // เปิด Movement script
            if (playerMovementScript != null)
            {
                playerMovementScript.enabled = true;
            }

            // เปิด Rigidbody2D อีกครั้ง
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = false;
                playerRigidbody.linearVelocity = savedVelocity;
                
                // ตรวจสอบว่า Rigidbody2D ยังอยู่
                if (playerRigidbody.gameObject != playerObject)
                {
                    Debug.LogWarning($"{npcName}: Rigidbody2D reference mismatch! Re-fetching...");
                    playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
                }
            }
            
            if (showLevelCompleteMessage)
            {
                Debug.Log($"Player movement enabled. Active: {playerObject.activeSelf}, Position: {playerObject.transform.position}");
            }
        }

        // รีเซ็ตตัวแปร
        playerObject = null;
        playerMovementScript = null;
        playerRigidbody = null;
    }

    // ฟังก์ชันสำหรับเพิ่มกล่องเข้า Inventory (เรียกจาก CollectableBox)
    public static void AddBoxToInventory(int boxID, string boxName)
    {
        collectedBoxIDs.Add(boxID);
        collectedBoxNames.Add(boxName);
        Debug.Log($"Added to inventory: {boxName} (ID: {boxID}) - Total: {collectedBoxIDs.Count}");
    }

    // ฟังก์ชันสำหรับดู Inventory
    public static int GetInventoryCount()
    {
        return collectedBoxIDs.Count;
    }

    // ฟังก์ชันสำหรับรีเซ็ต Inventory
    public static void ResetInventory()
    {
        Debug.Log($"ResetInventory called! Stack: {System.Environment.StackTrace}");
        collectedBoxIDs.Clear();
        collectedBoxNames.Clear();
    }

    // ฟังก์ชันสำหรับรีเซ็ต NPC
    public void ResetNPC()
    {
        deliveredBoxes = 0;
        levelCompleted = false;
    }

    // ฟังก์ชันสำหรับรีเซ็ตกล่องทั้งหมดที่ถูกเก็บไปแล้ว (ให้กลับมาแสดงอีกครั้ง)
    void ResetAllCollectedBoxes()
    {
        // หา CollectableBox ทั้งหมดใน Scene (รวมทั้งที่ inactive อยู่)
        CollectableBox[] allBoxes = FindObjectsOfType<CollectableBox>(true);
        
        int resetCount = 0;
        foreach (CollectableBox box in allBoxes)
        {
            if (box != null && box.IsCollected())
            {
                box.ResetBox();
                resetCount++;
            }
        }
        
        if (showLevelCompleteMessage)
        {
            Debug.Log($"รีเซ็ตกล่อง {resetCount} กล่องให้กลับมาแสดงอีกครั้ง");
        }
    }

    // วาด Gizmo แสดงระยะห่างที่สามารถส่งกล่องได้และตำแหน่ง Teleport
    void OnDrawGizmosSelected()
    {
        // วาดระยะห่างที่สามารถส่งกล่องได้ (สีเขียว)
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // วาดตำแหน่ง Teleport (สีฟ้า)
        Vector3 teleportPos;
        if (useTeleportPosition)
        {
            teleportPos = new Vector3(teleportPosition.x, teleportPosition.y, transform.position.z);
        }
        else if (teleportTarget != null)
        {
            teleportPos = teleportTarget.position;
        }
        else
        {
            return; // ไม่มีตำแหน่ง Teleport
        }

        // วาดวงกลมที่ตำแหน่ง Teleport
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
        Gizmos.DrawWireSphere(teleportPos, 0.5f);

        // วาดเส้นจาก NPC ไปยังตำแหน่ง Teleport
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.8f);
        Gizmos.DrawLine(transform.position, teleportPos);

        // วาดลูกศรชี้ทิศทาง
        Vector3 direction = (teleportPos - transform.position).normalized;
        Vector3 arrowTip = teleportPos;
        Vector3 arrowBase1 = arrowTip - direction * 0.3f + new Vector3(-direction.y, direction.x, 0) * 0.2f;
        Vector3 arrowBase2 = arrowTip - direction * 0.3f - new Vector3(-direction.y, direction.x, 0) * 0.2f;
        Gizmos.DrawLine(arrowTip, arrowBase1);
        Gizmos.DrawLine(arrowTip, arrowBase2);
        Gizmos.DrawLine(arrowBase1, arrowBase2);
    }
}

