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

    private bool isGrounded;
    private bool jumpRequested;
    public Rigidbody rb;
    [SerializeField] private float movespeed = 8.0f;

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
            jumpRequested = false;
        }
    }
    void PlayerMove()
    {
        Vector3 currentVel = rb.velocity;
        Vector3 newVel = new Vector3(horizontal * movespeed, currentVel.y, vertical * movespeed);
        rb.velocity = Vector3.Lerp(rb.velocity, newVel, Time.deltaTime * 10.0f);
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}