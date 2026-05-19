using UnityEngine;
using System.Collections.Generic;

public class MartLayoutManager : MonoBehaviour
{
    [Header("진열대 종류")]
    public GameObject[] shelfPrefabs; // 다양한 크기의 진열대 프리팹들

    [Header("마트 격자(Grid) 설정")]
    public int columns = 6;
    public int rows = 8;
    public Vector2 gridSpacing = new Vector2(5f, 7f); // 격자 간격

    [Header("겹침 체크 설정")]
    public LayerMask obstructionLayer; // "Shelf" 레이어 선택

    private List<GameObject> currentShelves = new List<GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ChangeLayout();
        }
    }

    public void ChangeLayout()
    {
        // 1. 기존 진열대 완전히 제거
        foreach (GameObject shelf in currentShelves)
        {
            if (shelf != null) Destroy(shelf);
        }
        currentShelves.Clear();

        // 2. 마트 중심 기준 격자 시작점 계산
        Vector3 startPosition = GetGridStartPosition();

        // 3. 격자 배치 시작
        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 spawnPos = startPosition + new Vector3(x * gridSpacing.x, 0f, z * gridSpacing.y);

                if (shelfPrefabs == null || shelfPrefabs.Length == 0) return;

                // 어떤 진열대를 세울지 랜덤 선택
                int randomIndex = Random.Range(0, shelfPrefabs.Length);
                GameObject selectedPrefab = shelfPrefabs[randomIndex];

                BoxCollider prefabCollider = selectedPrefab.GetComponent<BoxCollider>();
                if (prefabCollider == null) continue;

                // 실제 절반 크기(halfExtents) 구하기
                Vector3 halfExtents = GetPrefabHalfExtents(selectedPrefab, prefabCollider);

                // 0도 또는 90도 랜덤 회전
                float[] rotations = { 0f, 90f };
                float randomRot = rotations[Random.Range(0, rotations.Length)];
                Quaternion shelfRotation = Quaternion.Euler(0, randomRot, 0);

                // 회전값에 따라 가로세로 스왑
                if (Mathf.Approximately(randomRot, 90f))
                {
                    halfExtents = new Vector3(halfExtents.z, halfExtents.y, halfExtents.x);
                }

                // 해당 프리팹 고유의 크기로 주변을 검사합니다.
                bool isOverlap = Physics.CheckBox(spawnPos, halfExtents, shelfRotation, obstructionLayer);

                if (!isOverlap)
                {
                    GameObject newShelf = Instantiate(selectedPrefab, spawnPos, shelfRotation);
                    newShelf.layer = LayerMask.NameToLayer("Shelf");
                    currentShelves.Add(newShelf);
                }
            }
        }

        Debug.Log($"🛒 제각각 크기 대응 자동 배치 완료! (총 {currentShelves.Count}개)");
    }

    // 💡 [추가] 에디터 뷰에서 배치될 자리를 실시간으로 시각화해주는 코드
    private void OnDrawGizmos()
    {
        // 프리팹 배열이 비어있으면 그리지 않음
        if (shelfPrefabs == null || shelfPrefabs.Length == 0) return;

        Vector3 startPosition = GetGridStartPosition();

        // 격자 포인트를 순회하며 박스를 그립니다.
        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 pt = startPosition + new Vector3(x * gridSpacing.x, 0f, z * gridSpacing.y);

                // 순서대로 프리팹들의 크기를 매칭해서 보여줍니다 (비율 확인용)
                int prefabIndex = (x * rows + z) % shelfPrefabs.Length;
                GameObject prefab = shelfPrefabs[prefabIndex];

                if (prefab != null)
                {
                    BoxCollider col = prefab.GetComponent<BoxCollider>();
                    if (col != null)
                    {
                        // 실제 프리팹 크기 계산
                        Vector3 actualSize = Vector3.Scale(col.size, prefab.transform.localScale);

                        // 🟢 초록색 선으로 배치될 진열대 크기 영역 표시
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(pt + col.center, actualSize);
                    }
                }

                // 🔵 노란색 점으로 격자의 중심점(스폰 포인트 위치) 표시
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(pt, 0.2f);
            }
        }
    }

    // 코드 중복을 줄이기 위한 격자 시작점 계산 함수
    private Vector3 GetGridStartPosition()
    {
        return transform.position - new Vector3(
            (columns - 1) * gridSpacing.x * 0.5f,
            0f,
            (rows - 1) * gridSpacing.y * 0.5f
        );
    }

    // 프리팹의 스케일이 반영된 겹침 체크용 halfExtents 계산 함수
    private Vector3 GetPrefabHalfExtents(GameObject prefab, BoxCollider col)
    {
        Vector3 actualSize = Vector3.Scale(col.size, prefab.transform.localScale);
        return actualSize * 0.5f;
    }
}