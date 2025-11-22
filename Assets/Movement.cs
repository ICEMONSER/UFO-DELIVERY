using UnityEngine;

public class Movement : MonoBehaviour
{
    public float walkSpeed = 5f;
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
        // Prevent rotation
        transform.rotation = Quaternion.identity;

        // Horizontal movement input
        float moveX = Input.GetAxisRaw("Horizontal");

        // Running check
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Choose speed
        float speed = isRunning ? runSpeed : walkSpeed;

        // Apply movement
        rb.velocity = new Vector2(moveX * speed, rb.velocity.y);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // 🎯 SEND ANIMATION VALUES
        // Speed = abs(moveX) (NOT multiplied by walkSpeed!)
        animator.SetFloat("Speed", Mathf.Abs(moveX));

        // isBack = true when moving left
        animator.SetBool("isBack", moveX < 0);
    }

    // Ground detection
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
            isGrounded = false;
    }
}
