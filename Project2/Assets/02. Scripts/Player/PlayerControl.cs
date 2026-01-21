using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("점프")]
    [SerializeField] private float jumpForce = 7.0f;

    [Header("바닥 체크")]
    public Transform groundCheck;                               //발밑 위치를 나타내는 트랜스폼
    [SerializeField] private float groundCheckRadius = 0.3f;    //감지용 반지름
    [SerializeField] private LayerMask groundLayer;             //레이어 설정

    [Header("Animation (Direct Assignment)")]
    [SerializeField] private Animator animator;

    private bool isGrounded;
    private bool jumpRequested;
    public Rigidbody rb;

    [SerializeField] private float movespeed = 8.0f;
    [SerializeField] private float sprintMultiplier = 2.0f;
    private bool isSprinting = false;

    private float horizontal;
    private float vertical;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    //Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        isSprinting = Input.GetKey(KeyCode.LeftShift) && (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f);

        if (animator != null)
        {
            float moveSpeedForAnim = new Vector2(horizontal, vertical).magnitude;
            animator.SetFloat("Speed", moveSpeedForAnim); //
            animator.SetBool("IsGround", isGrounded); //
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Debug.Log($"[Update] Space pressed | isGrounded = {isGrounded}");
            jumpRequested = true;
        }
    }
    private void FixedUpdate()
    {
        PlayerMove();

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        if (jumpRequested && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
            jumpRequested = false;
        }
    }
    void PlayerMove()
    {
        Vector3 currentVel = rb.velocity;
        Vector3 moveDir = (transform.forward * vertical) + (transform.right * horizontal);
        if (moveDir.magnitude > 1.0f)
        {
            moveDir.Normalize();
        }

        float finalSpeed = movespeed * (isSprinting ? sprintMultiplier : 1.0f);
        Vector3 newVel = new Vector3(moveDir.x * finalSpeed, currentVel.y, moveDir.z * finalSpeed);

        rb.velocity = Vector3.Lerp(rb.velocity, newVel, Time.deltaTime * 10.0f);
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}