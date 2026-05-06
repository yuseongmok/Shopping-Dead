using UnityEngine;
using System.Collections.Generic;

public class CartDetector : MonoBehaviour
{
    [Header("Cart Inventory")]
    public List<GameObject> itemsInCart = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            if (!itemsInCart.Contains(other.gameObject))
            {
                itemsInCart.Add(other.gameObject);
                Debug.Log($"🛒 [카트] {other.gameObject.name} 담김");

                // 💡 GameManager에게 실시간 개수를 전달하여 UI를 갱신합니다!
                UpdateGameManager();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            if (itemsInCart.Contains(other.gameObject))
            {
                itemsInCart.Remove(other.gameObject);
                Debug.Log($"⚠️ [카트] {other.gameObject.name} 빠짐");

                // 💡 물건이 빠졌을 때도 UI를 실시간으로 갱신합니다!
                UpdateGameManager();
            }
        }
    }

    // 카트 속 목표 아이템 개수를 세어서 GameManager에 전달하는 함수
    private void UpdateGameManager()
    {
        int targetCount = 0;
        string targetName = GameManager.Instance.targetItemName;

        foreach (GameObject item in itemsInCart)
        {
            // 이름에 "Item_Milk"가 포함되어 있는지 검사합니다.
            if (item.name.Contains(targetName))
            {
                targetCount++;
            }
        }

        // UI 업데이트 지시
        GameManager.Instance.UpdateShoppingListUI(targetCount);
    }
}