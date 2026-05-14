using UnityEngine;
using System.Collections.Generic;

public class MartLayoutManager : MonoBehaviour
{
    [Header("진열대 종류")]
    public GameObject[] shelfPrefabs; // 다양한 진열대 프리팹들

    [Header("배치 지점")]
    public Transform[] gridPoints;    // 마트 바닥에 미리 깔아둔 Empty Object들

    [Header("겹침 체크 설정")]
    public LayerMask obstructionLayer; // "Shelf" 레이어 선택
    public Vector3 shelfSize = new Vector3(2f, 2f, 4f); // 진열대의 평균 크기

    private List<GameObject> currentShelves = new List<GameObject>();

    void Update()
    {
        // G 키를 누르면 구조 변경
        if (Input.GetKeyDown(KeyCode.G))
        {
            ChangeLayout();
        }
    }

    public void ChangeLayout()
    {
        // 1. 기존 진열대 제거
        foreach (GameObject shelf in currentShelves)
        {
            if (shelf != null) Destroy(shelf);
        }
        currentShelves.Clear();

        // 2. 새로운 구조 배치
        foreach (Transform point in gridPoints)
        {
            // 어떤 진열대를 세울지 랜덤 선택
            int randomIndex = Random.Range(0, shelfPrefabs.Length);

            // 0도 또는 90도 랜덤 회전 (통로 방향 결정)
            float[] rotations = { 0f, 90f };
            float randomRot = rotations[Random.Range(0, rotations.Length)];
            Quaternion shelfRotation = Quaternion.Euler(0, randomRot, 0);

            // 💡 겹침 체크 박스 크기 계산
            Vector3 halfExtents = shelfSize * 0.5f;

            // 회전값에 따라 체크 박스의 가로/세로를 스왑
            if (Mathf.Approximately(randomRot, 90f))
            {
                halfExtents = new Vector3(halfExtents.z, halfExtents.y, halfExtents.x);
            }

            // 💡 [핵심] 해당 지점이 비어있는지 확인
            // 이미 다른 진열대가 이 공간을 침범했다면 생성하지 않음
            bool isOverlap = Physics.CheckBox(point.position, halfExtents, shelfRotation, obstructionLayer);

            if (!isOverlap)
            {
                GameObject newShelf = Instantiate(shelfPrefabs[randomIndex], point.position, shelfRotation);

                // 생성된 진열대의 레이어를 설정 (체크에 감지되도록)
                newShelf.layer = LayerMask.NameToLayer("Shelf");

                currentShelves.Add(newShelf);
            }
            else
            {
                Debug.Log($"{point.name} 위치는 공간 부족으로 건너뜁니다.");
            }
        }

        Debug.Log($"🛒 마트 진열대 {currentShelves.Count}개 재배치 완료!");
    }

    // 에디터 뷰에서 배치 영역을 미리 보기 위한 기능
    private void OnDrawGizmos()
    {
        if (gridPoints == null) return;
        Gizmos.color = Color.green;
        foreach (var pt in gridPoints)
        {
            if (pt != null) Gizmos.DrawWireCube(pt.position, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}