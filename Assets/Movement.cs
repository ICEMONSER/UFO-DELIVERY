using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 12f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float speed = isRunning ? runSpeed : moveSpeed;

        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Keep character facing right (no horizontal flipping)
        transform.localScale = new Vector3(1, 1, 1);

        // Rotate character to face backward when moving left (A key)
        if (moveX < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180); // Rotate 180 degrees to face backward
        }
        else if (moveX > 0)
        {
            transform.rotation = Quaternion.identity; // Face forward
        }

        // Animation Controller
        animator.SetFloat("Speed", Mathf.Abs(moveX * speed));
        animator.SetBool("isBack", moveX < 0);
    }

    private void FixedUpdate()
    {
        // รีเซ็ตสถานะพื้นทุกเฟรม (จะถูก set เป็น true ใน OnCollisionStay2D ถ้ายังเหยียบพื้นอยู่)
        isGrounded = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // ตรวจสอบว่าจุดที่ชนอยู่ด้านล่างหรือไม่ (เหยียบอยู่บนสิ่งของ)
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // ถ้า Normal ชี้ขึ้น (y > 0.7) แสดงว่าเป็นพื้น
            if (contact.normal.y > 0.7f)
            {
                isGrounded = true;
                return; // เจอพื้นแล้ว จบการทำงาน
            }
        }
    }
}
