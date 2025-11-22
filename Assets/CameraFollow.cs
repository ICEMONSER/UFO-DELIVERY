using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float FollowSpeed = 2f;
    public float yOffset = 1f;
    public Transform target;
    
    [Header("Lock Settings")]
    [Tooltip("ล็อก Y axis (ไม่ให้ Camera เลื่อนตาม Y ของ Player)")]
    public bool lockYAxis = true;

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            // ถ้าล็อก Y axis → ใช้ Y position เดิมของ Camera
            float newY = lockYAxis ? transform.position.y : (target.position.y + yOffset);
            Vector3 newPos = new Vector3(target.position.x, newY, -10f);
            transform.position = Vector3.Slerp(transform.position, newPos, FollowSpeed * Time.deltaTime);
        }
    }

    // ฟังก์ชันสำหรับอัพเดท Camera position ทันที (ไม่ใช่ค่อยๆ เคลื่อนที่)
    public void UpdateImmediately()
    {
        if (target != null)
        {
            // ถ้าล็อก Y axis → ใช้ Y position เดิมของ Camera
            float newY = lockYAxis ? transform.position.y : (target.position.y + yOffset);
            Vector3 newPos = new Vector3(target.position.x, newY, -10f);
            transform.position = newPos;
        }
    }
    
    // ฟังก์ชันสำหรับอัพเดท Camera position ทันที (พร้อมกำหนด Y position)
    public void UpdateImmediatelyWithY(float yPosition)
    {
        if (target != null)
        {
            Vector3 newPos = new Vector3(target.position.x, yPosition, -10f);
            transform.position = newPos;
        }
    }
    
    // ฟังก์ชันสำหรับอัพเดท Camera position ทันที (พร้อมกำหนด Y position และให้ Player อยู่ตรงกลาง)
    public void UpdateImmediatelyWithYCentered(float yPosition, Camera cam)
    {
        if (target != null && cam != null)
        {
            // เลื่อน Camera ให้ Player อยู่ตรงกลางของ Camera view (X axis)
            // Y position ใช้ค่าที่กำหนด (ล็อก Y axis)
            float newX = target.position.x;
            Vector3 newPos = new Vector3(newX, yPosition, -10f);
            transform.position = newPos;
        }
    }
}