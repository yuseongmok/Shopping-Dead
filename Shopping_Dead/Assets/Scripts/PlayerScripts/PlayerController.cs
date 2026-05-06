using UnityEngine;
using UnityEngine.UI; // 스테미나 UI를 제어하기 위해 추가

public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2.5f;
    private float currentSpeed;
    private Rigidbody rb;
    private Vector3 moveDirection;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    private bool isGrounded;
    public Transform groundCheck; // 플레이어 발밑에 둘 빈 오브젝트
    public float groundDistance = 0.4f;
    public LayerMask groundMask;  // 바닥으로 인식할 레이어

    [Header("Crouch Settings")]
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    private bool isCrouching = false;
    private CapsuleCollider capsuleCollider;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrain = 20f;   // 달릴 때 초당 소모량
    public float staminaRegen = 15f;   // 쉴 때 초당 회복량
    private bool isRunning = false;

    [Header("Camera Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    private float xRotation = 0f;

    void Start()
    {
rb = GetComponent<Rigidbody>();
    capsuleCollider = GetComponent<CapsuleCollider>();
    currentSpeed = walkSpeed;
    currentStamina = maxStamina;
   
    rb.constraints = RigidbodyConstraints.FreezeRotation;

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    if (cameraTransform == null)
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }
    }

    void Update()
    {
        // 1. 마우스 시선 회전
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 2. 바닥 체크 (점프용)
        // groundCheck 위치 기준으로 반구를 그려 바닥(Ground) 레이어와 닿아있는지 확인합니다.
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 3. 앉기 (Crouch) 입력 - Left Control 키
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            StartCrouch();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            StopCrouch();
        }

        // 4. 달리기 (Run) 및 스테미나 처리
        // 쉬프트 키를 누르고 있고, 앞으로 움직이고 있으며, 앉아있지 않고, 스테미나가 있을 때만 달립니다.
        bool isMovingForward = Input.GetAxisRaw("Vertical") > 0;
        if (Input.GetKey(KeyCode.LeftShift) && isMovingForward && !isCrouching && currentStamina > 0)
        {
            isRunning = true;
            currentSpeed = runSpeed;
            currentStamina -= staminaDrain * Time.deltaTime; // 스테미나 감소
        }
        else
        {
            isRunning = false;
            currentSpeed = isCrouching ? crouchSpeed : walkSpeed;
            
            // 달리기를 안 할 때는 스테미나 서서히 회복
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegen * Time.deltaTime;
            }
        }
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        // 5. 점프 (Jump) 입력 - Space 키
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // 6. 이동 입력 값 받기
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 targetVelocity = moveDirection * currentSpeed;
            // Y축 속도(중력/점프)는 건드리지 않고 X, Z 속도만 적용합니다.
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    // 앉기 시작
    void StartCrouch()
    {
        isCrouching = true;
        currentSpeed = crouchSpeed;
        capsuleCollider.height = crouchHeight; // 콜라이더 크기를 줄여서 좁은 곳을 지나갈 수 있게 합니다.
        
        // 카메라 위치도 아래로 낮춰줍니다.
        cameraTransform.localPosition = new Vector3(0f, crouchHeight * 0.4f, 0.2f);
    }

    // 앉기 해제
    void StopCrouch()
    {
        isCrouching = false;
        currentSpeed = walkSpeed;
        capsuleCollider.height = standHeight;
        
        // 카메라 위치 원상복구
        cameraTransform.localPosition = new Vector3(0f, standHeight * 0.4f, 0.2f);
    }
}