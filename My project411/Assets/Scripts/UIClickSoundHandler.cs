using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickSoundHandler : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Проверяем клик левой кнопкой
        {
            if (EventSystem.current.IsPointerOverGameObject()) // Если клик по UI
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
