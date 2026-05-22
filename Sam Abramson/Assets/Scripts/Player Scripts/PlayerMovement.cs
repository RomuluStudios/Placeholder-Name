using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Rotation")]
    public float rotationSpeed = 12f;

    [Header("References")]
    public Transform cameraTransform;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Vector2 input;
    private Vector3 moveDir;

    private bool jumpPressed;
    private bool isGrounded;
    private bool isCrouching;

    private float originalHeight;
    private Vector3 originalCenter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        originalHeight = capsule.height;
        originalCenter = capsule.center;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;

        if (Input.GetKeyDown(KeyCode.C))
            ToggleCrouch();

        CheckGround();
        CalculateMoveDirection();
    }

    private void FixedUpdate()
    {
        Move();
        Jump();
        Rotate();
    }

    void ToggleCrouch()
    {
        isCrouching = !isCrouching;

        if (isCrouching)
        {
            capsule.height = originalHeight * 0.5f;
            capsule.center = originalCenter * 0.5f;
        }
        else
        {
            capsule.height = originalHeight;
            capsule.center = originalCenter;
        }
    }

    void CalculateMoveDirection()
    {
        if (cameraTransform == null)
        {
            moveDir = new Vector3(input.x, 0f, input.y);
            return;
        }

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 raw = new Vector3(input.x, 0f, input.y);

        moveDir = (camForward * raw.z + camRight * raw.x).normalized;
    }

    void Move()
    {
        float speed = Input.GetKey(KeyCode.LeftShift)
            ? sprintSpeed
            : walkSpeed;

        Vector3 horizontal = moveDir * speed;

        rb.linearVelocity = new Vector3(
            horizontal.x,
            rb.linearVelocity.y,
            horizontal.z
        );
    }

    void Rotate()
    {
        if (moveDir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(moveDir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    void Jump()
    {
        if (!jumpPressed || !isGrounded)
        {
            jumpPressed = false;
            return;
        }

        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        jumpPressed = false;
    }

    void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}