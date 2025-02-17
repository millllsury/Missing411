using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomExplorationManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bgSprite;
    [SerializeField] private GameObject interactableObjectsContainer; // Группа предметов
    //[SerializeField] private GameObject exitButton; // Кнопка выхода
    [SerializeField] private Button candle;
    private FeedbackManager feedbackManager;
    //private int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
    [SerializeField] private Canvas keysGroup;

    [SerializeField] private Image boxImage; // Ссылка на UI Image (не SpriteRenderer!)
    [SerializeField] private Button boxButton;

    private bool isOpen = false;
    private bool isRoomLit = false;

    [SerializeField] private GameObject keyObject1;
    [SerializeField] private GameObject keyObject2;

    [SerializeField] private GameObject arrowHint; // Ссылка на стрелку

    

    void Start()
    {
        FeedbackManager.Instance.ShowMessage("Look around the room!");
        keyObject1.gameObject.SetActive(false);
        keyObject2.gameObject.SetActive(false);
        bgSprite.sprite = Resources.Load<Sprite>("Backgrounds/houseMainRoomNight");
        interactableObjectsContainer.SetActive(false); // Скрываем предметы в тёмной комнате
        arrowHint.SetActive(false); // Скрываем стрелку изначально
        Invoke(nameof(ShowArrowHint), 2f); // Показываем через 2 секунды
    }

  

    void ShowArrowHint()
    {
        if (isRoomLit) return;
        arrowHint.SetActive(true);
    }
    // Функция, которая вызывается при клике на свечу
    public void LightCandle()
    {
        //BlinkEffect blink = candle.GetComponent<BlinkEffect>();
        //blink.StopBlinking();

        if (arrowHint != null)
        {
            arrowHint.SetActive(false);

        }

        if (!isRoomLit)
        {
            isRoomLit = true;
            bgSprite.sprite = Resources.Load<Sprite>("Backgrounds/houseMainRoom");
            interactableObjectsContainer.SetActive(true); // Делаем предметы доступными
            candle.gameObject.SetActive(false);
            keyObject1.gameObject.SetActive(true);
        }
    }

    private string boxOpenPath = "UI/boxOpen"; // Без "Resources/"
    private string boxClosedPath = "UI/boxClosed"; // Без "Resources/"

    public void OnBoxClick()
    {
        isOpen = !isOpen;

        if (boxImage != null)
        {
            string spritePath = isOpen ? boxOpenPath : boxClosedPath;
            Sprite newSprite = Resources.Load<Sprite>(spritePath);

            if (newSprite != null)
            {
                boxImage.sprite = newSprite;
            }
            else
            {
                Debug.LogError($"Ошибка: Спрайт не найден в Resources/{spritePath}");
            }
        }

        if (isOpen)
        {
            keyObject2.gameObject.SetActive(true);
        }
        else
        {
            keyObject2.gameObject.SetActive(false);
        }
    }

    // Добавление предметов в инвентарь
    public void GotKeyReward(Button button)
    {

        string keyID = button.gameObject.name;

        // Checking if the key has already been got
        /*if (GameStateManager.Instance.IsKeyCollected(selectedSlotIndex, keyID))
        {
            Debug.Log($"[GotKeyReward] Ключ {keyID} уже собран, пропускаем.");
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
            return;
        }*/

        // Add the key to the saved ones
        //GameStateManager.Instance.SaveKeyCollected(selectedSlotIndex, keyID);

        // We are giving out a reward
        CurrencyManager.Instance.AddKeys(1);
        feedbackManager.ShowMessage("You've found a key!");

        // Torn off the btn
        if (button != null)
        {
            button.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Error in GotKeyReward!");
        }
    }

    // Выход из мини-игры
    public void ExitRoom()
    {
        Debug.Log("Выход из комнаты");

    }
}
