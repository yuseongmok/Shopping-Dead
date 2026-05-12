using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public GameObject lightSource; // 손전등 빛 오브젝트
    private bool isOn = false;     // 현재 켜져 있는지 확인

    void Start()
    {
        // 시작할 때는 손전등을 꺼둠
        if (lightSource != null)
            lightSource.SetActive(isOn);
    }

    void Update()
    {
        // F 키를 누르면 상태를 반전
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }
    }

    void ToggleFlashlight()
    {
        isOn = !isOn;
        lightSource.SetActive(isOn);

        //효과음 자리
        Debug.Log("손전등 상태: " + (isOn ? "ON" : "OFF"));
    }
}