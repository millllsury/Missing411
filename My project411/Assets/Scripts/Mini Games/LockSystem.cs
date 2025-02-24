using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;


public class LockSystem : MonoBehaviour
{
    [SerializeField] private TMP_Text[] digitTexts; // 4 текста для цифр
    [SerializeField] private GameObject digits;
    [SerializeField] private GameObject laggageClose;

    [SerializeField] private Button laggageButton; // Ссылка на кнопку
    [SerializeField] private ButtonMover buttonMover; // Ссылка на объект с ToggleMove

    [SerializeField] private GameObject clothes;
    [SerializeField] private GameObject openLaggage;

    [SerializeField] private Button[] increaseButtons; // Кнопки "+"
    [SerializeField] private Button[] decreaseButtons; // Кнопки "-"
    [SerializeField] private Button unlockButton; // Кнопка проверки кода

    private int[] currentDigits = new int[4]; // Храним текущие цифры
    private int[] correctCode = { 7, 2, 4, 9 }; // Правильный код

    public bool isPulledOut = true;
    [SerializeField] private CanvasGroup lockGameCanvas;
    [SerializeField] private Button closeCanvas;
    private void Start()
    {
        UpdateDigits(); // Обновляем начальное отображение цифр

        // Привязываем кнопки к изменениям чисел
        for (int i = 0; i < increaseButtons.Length; i++)
        {
            int index = i; // Создаем локальную копию индекса для лямбды
            increaseButtons[i].onClick.AddListener(() => ChangeDigit(index, 1));
            decreaseButtons[i].onClick.AddListener(() => ChangeDigit(index, -1));
        }

        unlockButton.onClick.AddListener(CheckCode);
    }

    private void ChangeDigit(int index, int change)
    {
        currentDigits[index] = (currentDigits[index] + change + 10) % 10; // Меняем цифру (0-9)
        UpdateDigits();
    }

    public void OnCaseClick()
    {
        isPulledOut = !isPulledOut;
        if (isPulledOut == true)
        {
            OpenCanvas();
            return;
        }
        SoundManager.Instance.PlaySoundByName("pullLaggage");
    }

  
    public void OpenCanvas() {

        lockGameCanvas.alpha = 1f;
        lockGameCanvas.interactable = true;
        lockGameCanvas.blocksRaycasts = true;
    }

    public void CloseCanvas()
    {
        lockGameCanvas.alpha = 0f;
        lockGameCanvas.interactable = false;
        lockGameCanvas.blocksRaycasts = false;
    }

    private void UpdateDigits()
    {
        for (int i = 0; i < digitTexts.Length; i++)
        {
            digitTexts[i].text = currentDigits[i].ToString();
        }
    }

    private void CheckCode()
    {
        for (int i = 0; i < correctCode.Length; i++)
        {
            if (currentDigits[i] != correctCode[i])
            {
                Debug.Log(" Код неверный! Попробуйте ещё раз.");
                return;
            }
        }
        FeedbackManager.Instance.ShowMessage("The lock is open!");
        LaggageOpening();
    }

    private void LaggageOpening() {

        SoundManager.Instance.PlaySoundByName("lockOpen");
        Animator laggageAnimator = FindFirstObjectByType<Animator>();

        if (laggageAnimator != null)
        {
            laggageAnimator.SetTrigger("laggageOpen"); // Устанавливаем триггер анимации
        }
        else
        {
            Debug.LogError("Аниматор не найден!");
        }

        StartCoroutine(EndGameRoutine());

    }


    private IEnumerator EndGameRoutine()
    {
        yield return new WaitForSeconds(2f); // Ждем 5 секунд
        Debug.Log("5 секунд прошло, продолжаем выполнение кода.");
        SoundManager.Instance.PlaySoundByName("laggageOpen");
        GettingReward();
    }

    private void GettingReward() {

        digits.SetActive(false);
        laggageClose.SetActive(false);
        openLaggage.SetActive(true);
        unlockButton.gameObject.SetActive(false);
        closeCanvas.gameObject.SetActive(true);

    }

    public void OnClothesClick()
    {
        //GameStateManager.Instance.UnlockNextItem();
        FeedbackManager.Instance.ShowMessage("You've got a new outfit!");
        clothes.SetActive(false);
    }
}
