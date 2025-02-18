using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Security.Cryptography;

public class Bedroom : MonoBehaviour
{

    [SerializeField] private SpriteRenderer closet;
    [SerializeField] private Button closedDoor;
    [SerializeField] private Button openDoor;
    [SerializeField] private Button boxButton;

    [SerializeField] private CanvasGroup BoxCanvas;

    void Start()
    {
        CloseCanvas();
    }

    public void OnClosedDoorClick()
    {
        ToggleCloset(true); // Открываем шкаф
    }

    public void OnOpenDoorClick()
    {
        ToggleCloset(false); // Закрываем шкаф
    }

    public void ToggleCloset(bool isOpen)
    {
        closet.sprite = Resources.Load<Sprite>(isOpen ? "Backgrounds/Bedroom/closetOpen" : "Backgrounds/Bedroom/closet");

        closedDoor.gameObject.SetActive(!isOpen); 
        openDoor.gameObject.SetActive(isOpen);    
        boxButton.gameObject.SetActive(isOpen);
    }


    public void OnBoxClick()
    {
        StartCoroutine(FadeCanvas(BoxCanvas, 0f, 1f, 0.2f));
    }

    private IEnumerator FadeCanvas(CanvasGroup canvas, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        canvas.alpha = endAlpha;
        BoxCanvas.interactable = true;
        BoxCanvas.blocksRaycasts = true;
    }

    public void CloseCanvas() {

        BoxCanvas.alpha = 0f;
        BoxCanvas.interactable = false;
        BoxCanvas.blocksRaycasts = false;
    }

    void Update()
    {
        
    }
}
