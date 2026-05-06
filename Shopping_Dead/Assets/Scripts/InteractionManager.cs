using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Detection")]
    public float pickupRange = 3f;      // 물건을 집을 수 있는 거리
    public LayerMask itemLayer;         // 물건을 판정할 레이어 (필요 시 지정)

    [Header("Hold Settings")]
    public Transform holdPoint;         // 물건이 고정될 HoldPoint 위치
    public float throwForce = 15f;      // 던지는 힘의 세기

    private Camera cam;
    private GameObject heldItem;        // 현재 손에 들고 있는 물건
    private Rigidbody heldItemRb;       // 들고 있는 물건의 Rigidbody

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        // 1. 물건을 들고 있지 않을 때: 집기 시도
        if (heldItem == null)
        {
            // E 키를 누르면 내 시선 정면에 물건이 있는지 레이캐스트(Raycast)로 확인합니다.
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryPickupItem();
            }
        }
        // 2. 물건을 들고 있을 때: 던지기 또는 놓기
        else
        {
            // 마우스 왼쪽 클릭을 하면 조준 방향으로 힘차게 던집니다.
            if (Input.GetMouseButtonDown(0))
            {
                ThrowItem();
            }
            // 다시 E 키를 누르면 발 앞에 툭 내려놓습니다.
            else if (Input.GetKeyDown(KeyCode.E))
            {
                DropItem();
            }
        }

        // 3. 물건을 들고 있다면 물건을 HoldPoint 위치로 부드럽게 고정시킵니다.
        if (heldItem != null)
        {
            KeepItemInHand();
        }
    }

    void TryPickupItem()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // 화면 중앙 정방향 레이저
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            // 정면에 조준된 오브젝트의 태그가 "Item"인지 확인합니다.
            if (hit.collider.CompareTag("Item"))
            {
                heldItem = hit.collider.gameObject;
                heldItemRb = heldItem.GetComponent<Rigidbody>();

                if (heldItemRb != null)
                {
                    // 물건을 집어 올렸으므로 중력과 물리 충돌 반응을 잠시 끕니다.
                    heldItemRb.useGravity = false;
                    heldItemRb.linearDamping = 10f; // 손 안에서 너무 흔들리지 않게 마찰력을 높입니다.
                    heldItemRb.constraints = RigidbodyConstraints.FreezeRotation; // 손 안에서 회전 방지
                }
            }
        }
    }

    void KeepItemInHand()
    {
        // 물건을 내 손(HoldPoint) 위치로 부드럽게 이동시킵니다.
        Vector3 targetPosition = holdPoint.position;
        Vector3 moveDirection = targetPosition - heldItem.transform.position;
        
        // 물리에 기반하여 손 위치로 힘을 가해 끌어당깁니다.
        heldItemRb.linearVelocity = moveDirection * 15f; 
    }

    void DropItem()
    {
        if (heldItemRb != null)
        {
            // 물리의 원래 성질을 복구합니다.
            heldItemRb.useGravity = true;
            heldItemRb.linearDamping = 1f;
            heldItemRb.constraints = RigidbodyConstraints.None;
        }

        heldItem = null;
        heldItemRb = null;
    }

    void ThrowItem()
    {
        if (heldItemRb != null)
        {
            // 물리의 원래 성질을 복구하고 힘을 가해 던집니다.
            heldItemRb.useGravity = true;
            heldItemRb.linearDamping = 1f;
            heldItemRb.constraints = RigidbodyConstraints.None;

            // 카메라가 바라보는 정면 방향으로 힘을 가합니다.
            heldItemRb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
        }

        heldItem = null;
        heldItemRb = null;
    }
}