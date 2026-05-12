using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2.5f;
    private float currentSpeed;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool readyToJump = false; // 점프 입력을 받았는지 저장하는 변수

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    private bool isGrounded;
    public Transform groundCheck;
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
    public float staminaDrain = 30f;   // 달릴 때 초당 소모량
    public float staminaRegen = 15f;   // 쉴 때 초당 회복량
    private bool isRunning = false;
    public float jumpStaminaCost = 30f; //점프 시 소모할 스테미나 양
    public float regenDelay = 2f;      // 스테미나 사용 후 회복 시작까지 걸리는 시간 (초)
    private float lastStaminaUseTime;  // 마지막으로 스테미나를 사용한 시점 저장
    private bool isExhausted = false;

    [Header("Camera Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    private float xRotation = 0f;

    [Header("UI Reference")]
    public Slider staminaSlider;

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

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = maxStamina;
        }

        currentStamina = maxStamina;
    }

    void Update()
    {
        //마우스 시선 회전
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //바닥 체크 (점프용)
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        //앉기
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            StartCrouch();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            StopCrouch();
        }


        //달리기 및 스테미나 처리
        bool isMovingForward = Input.GetAxisRaw("Vertical") > 0;
        if (Input.GetKey(KeyCode.LeftShift) && isMovingForward && !isCrouching && currentStamina > 0)
        {
            isRunning = true;
            currentSpeed = runSpeed;
            currentStamina -= staminaDrain * Time.deltaTime;

            //스테미나를 사용 중이므로 시간을 현재 시점으로 계속 갱신합니다.
            lastStaminaUseTime = Time.time;
        }
        else
        {
            isRunning = false;
            currentSpeed = isCrouching ? crouchSpeed : walkSpeed;

            //스테미나 회복 로직: 마지막 사용 시점으로부터 regenDelay만큼 지났을 때만 회복
            if (currentStamina < maxStamina && Time.time > lastStaminaUseTime + regenDelay)
            {
                currentStamina += staminaRegen * Time.deltaTime;
            }
        }

        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;

            // 스테미나가 가득 찼을 때는 바를 숨김
            staminaSlider.gameObject.SetActive(currentStamina < maxStamina);
        }
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        //점프 입력
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching && currentStamina > 0)
        {
            readyToJump = true;

            // 점프를 했으니 즉시 사용 시간을 갱신해서 회복 지연을 발생
            lastStaminaUseTime = Time.time;

            // 만약 남은 스테미나가 점프 비용보다 적다면, 남은 걸 0으로 다 털어버립니다
            // 아니면 원래 비용만큼만 뜁니다
            if (currentStamina < jumpStaminaCost)
            {
                currentStamina = 0f;
                // 여기서 탈진 상태(isExhausted)를 사용 중이라면 true로 만들어주는 게 좋습니다
                isExhausted = true;
            }
            else
            {
                currentStamina -= jumpStaminaCost;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        // 6. 이동 입력 값 받기
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;
    }

    void FixedUpdate()
    {
        Move();

        // 물리 연산 주기에 맞춰서 딱 한 번만 점프를 실행
        if (readyToJump)
        {
            Jump();
        }
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

    void Jump()
    {
        // 현재 위아래 속도를 완전히 초기화 (중첩 방지 핵심)
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // 힘 가하기
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // 스테미나 차감
        currentStamina -= jumpStaminaCost;

        readyToJump = false;
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