using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public float FollowSpeed = 2f;
    public float yOffset = 1f;
    public Transform target;

    [Header("Red Zone (Dead Zone)")]
    [Tooltip("ขนาดของ Red Zone (X, Y) - ตัวละครอยู่ในโซนนี้กล้องจะไม่เลื่อน")]
    public Vector2 redZoneSize = new Vector2(2f, 1.5f);

    [Tooltip("แสดง Red Zone ใน Scene View")]
    public bool showRedZone = true;

    [Tooltip("ล็อคแกน Y - กล้องจะเลื่อนเฉพาะแกน X")]
    public bool lockYAxis = true;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.position;

        // คำนวณตำแหน่งใหม่
        Vector3 newPos;

        if (lockYAxis)
        {
            // เลื่อนเฉพาะแกน X, คงค่า Y
            float offsetX = targetPos.x - currentPos.x;
            
            // ตรวจสอบว่าตัวละครอยู่นอก Red Zone หรือไม่
            if (Mathf.Abs(offsetX) > redZoneSize.x * 0.5f)
            {
                // ตัวละครอยู่นอก Red Zone → เลื่อนกล้อง
                float newX = targetPos.x - Mathf.Sign(offsetX) * (redZoneSize.x * 0.5f);
                newPos = new Vector3(newX, currentPos.y, -10f);
            }
            else
            {
                // ตัวละครอยู่ใน Red Zone → กล้องไม่เลื่อน
                newPos = new Vector3(currentPos.x, currentPos.y, -10f);
            }
        }
        else
        {
            // เลื่อนทั้งแกน X และ Y
            Vector3 offset = targetPos - currentPos;
            offset.y += yOffset;

            // ตรวจสอบว่าตัวละครอยู่นอก Red Zone หรือไม่
            bool outsideX = Mathf.Abs(offset.x) > redZoneSize.x * 0.5f;
            bool outsideY = Mathf.Abs(offset.y) > redZoneSize.y * 0.5f;

            if (outsideX || outsideY)
            {
                // ตัวละครอยู่นอก Red Zone → เลื่อนกล้อง
                float newX = outsideX ? targetPos.x - Mathf.Sign(offset.x) * (redZoneSize.x * 0.5f) : currentPos.x;
                float newY = outsideY ? targetPos.y + yOffset - Mathf.Sign(offset.y) * (redZoneSize.y * 0.5f) : currentPos.y;
                newPos = new Vector3(newX, newY, -10f);
            }
            else
            {
                // ตัวละครอยู่ใน Red Zone → กล้องไม่เลื่อน
                newPos = new Vector3(currentPos.x, currentPos.y, -10f);
            }
        }

        transform.position = Vector3.Slerp(transform.position, newPos, FollowSpeed * Time.deltaTime);
    }

    // วาด Red Zone ใน Scene View
    void OnDrawGizmosSelected()
    {
        if (!showRedZone || target == null) return;

        Vector3 cameraCenter = transform.position;
        Vector3 targetCenter = target.position;

        // วาด Red Zone ที่ตำแหน่งกล้อง
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // สีแดงโปร่งใส
        Gizmos.DrawCube(cameraCenter, new Vector3(redZoneSize.x, redZoneSize.y, 0.1f));

        // วาดขอบ Red Zone
        Gizmos.color = Color.red;
        DrawWireRect(cameraCenter, redZoneSize.x, redZoneSize.y);

        // ตรวจสอบว่าตัวละครอยู่นอก Red Zone หรือไม่
        Vector3 offset = targetCenter - cameraCenter;
        bool outsideX = Mathf.Abs(offset.x) > redZoneSize.x * 0.5f;
        bool outsideY = !lockYAxis && Mathf.Abs(offset.y) > redZoneSize.y * 0.5f;

        // วาดเส้นจากกล้องไปยังตัวละคร
        if (outsideX || outsideY)
        {
            // ถ้าอยู่นอก Red Zone → เส้นสีแดงเข้ม
            Gizmos.color = new Color(1f, 0f, 0f, 1f);
        }
        else
        {
            // ถ้าอยู่ใน Red Zone → เส้นสีแดงอ่อน
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        }
        Gizmos.DrawLine(cameraCenter, targetCenter);

        // วาดกรอบกล้อง (Camera Viewport)
        if (cam == null) cam = GetComponent<Camera>();
        if (cam != null)
        {
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawCube(cameraCenter, new Vector3(width, height, 0.1f));
            
            Gizmos.color = Color.green;
            DrawWireRect(cameraCenter, width, height);
        }
    }

    // Helper function: วาดสี่เหลี่ยมขอบ
    void DrawWireRect(Vector3 center, float width, float height)
    {
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        
        Vector3 topLeft = new Vector3(center.x - halfWidth, center.y + halfHeight, center.z);
        Vector3 topRight = new Vector3(center.x + halfWidth, center.y + halfHeight, center.z);
        Vector3 bottomLeft = new Vector3(center.x - halfWidth, center.y - halfHeight, center.z);
        Vector3 bottomRight = new Vector3(center.x + halfWidth, center.y - halfHeight, center.z);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}