using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickSoundHandler : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ��������� ���� ����� �������
        {
            if (EventSystem.current.IsPointerOverGameObject()) // ���� ���� �� UI
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.UIClickSound();
                }
                else
                {
                    Debug.LogError("SoundManager.Instance is null in UIClickSoundHandler!");
                }
            }
        }
    }

}
