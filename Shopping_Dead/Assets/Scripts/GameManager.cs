using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트를 제어하기 위해 필수 추가

public class GameManager : MonoBehaviour
{
    // 싱글톤 패턴 (어디서나 GameManager에 쉽게 접근할 수 있게 합니다)
    public static GameManager Instance;

    [Header("Shopping Goal")]
    public string targetItemName = "Item_Milk"; // 우리가 찾아야 할 목표 물건 이름
    public int requiredAmount = 1;               // 찾아야 할 개수

    [Header("UI Reference")]
    public Text shoppingListText;                // 화면에 표시할 UI 텍스트 연결

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 게임 시작 시 UI 초기화
        UpdateShoppingListUI(0);
    }

    // 카트 안의 물건 개수를 받아와 UI를 갱신해 주는 함수
    public void UpdateShoppingListUI(int currentCount)
    {
        string statusSymbol = currentCount >= requiredAmount ? "[V]" : "[ ]";
        
        // UI에 띄울 텍스트 조립
        shoppingListText.text = $"<b>🛒 쇼핑 리스트</b>\n\n" +
                                $"{statusSymbol} 우유 ({currentCount}/{requiredAmount})\n\n";

        // 목표를 달성했을 때 문구 추가
        if (currentCount >= requiredAmount)
        {
            shoppingListText.text += "<color=green><b>★ 미션 완료! 마트를 탈출하세요!</b></color>";
            Debug.Log("🎉 쇼핑 목표 달성! 탈출구가 열렸습니다!");
        }
    }
}