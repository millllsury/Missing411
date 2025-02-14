using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;

public class WardrobeManager : MonoBehaviour
{
    // Названия сцен
    public string wardrobeSceneName = "WardrobeScene";

    [SerializeField] private BlinkingManager blinkingManager;
    
    // Ссылки на дочерние объекты персонажа
    public SpriteRenderer hairRenderer;    // Hair объект
    public SpriteRenderer clothesRenderer; // Clothes объект

    // Списки спрайтов
    public Sprite[] hairOptions;    // Массив доступных волос
    public Sprite[] clothesOptions; // Массив доступной одежды

    private int currentIndex = 0;       // Текущий индекс для выбранной категории
    private string currentCategory;    // Текущая выбранная категория ("Hair" или "Clothes")

    // Кнопки UI
    public Button hairButton;
    public Button clothesButton;

    [SerializeField] private CharacterManager characterManager;
    public SpriteRenderer eyesImage;


    private GameStateManager gameStateManager;

    private void Awake()
    {
        gameStateManager = FindFirstObjectByType<GameStateManager>();
        SoundManager.Instance.PlaySoundByName("WardrobeMusic");
        blinkingManager.StartBlinking("Alice", eyesImage);
        currentCategory = "Hair"; // По умолчанию выбрана категория "Hair"
        SetButtonState(hairButton, true);
        SetButtonState(clothesButton, false);
        var (savedHairIndex, savedClothesIndex) = GameStateManager.Instance.LoadAppearance();

        List<int> unlockedHairstyles = GameStateManager.Instance.GetUnlockedHairstyles();
        List<int> unlockedClothes = GameStateManager.Instance.GetUnlockedClothes();

        hairRenderer.sprite = hairOptions[savedHairIndex];
        clothesRenderer.sprite = clothesOptions[savedClothesIndex];

        if (unlockedHairstyles.Contains(savedHairIndex))
        {
            hairRenderer.sprite = hairOptions[savedHairIndex];
        }
        if (unlockedClothes.Contains(savedClothesIndex))
        {
            clothesRenderer.sprite = clothesOptions[savedClothesIndex];
        }

        characterManager.AdjustCharacterAppearance("WardrobeScene");
    }

    public void CloseWardrobe()
    {
        // Сохраняем текущие индексы в GameStateManager
        int currentHairIndex = GetCurrentSpriteIndex(hairRenderer.sprite, hairOptions);
        int currentClothesIndex = GetCurrentSpriteIndex(clothesRenderer.sprite, clothesOptions);

        GameStateManager.Instance.SaveAppearance(currentHairIndex, currentClothesIndex);
        int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
        GameStateManager.Instance.SaveGameToSlot(selectedSlotIndex);
        Debug.Log($"Индексы сохранены: Волосы={currentHairIndex}, Одежда={currentClothesIndex}");

        // Переходим на основную сцену
        string mainSceneName = GameStateManager.Instance.LoadSceneID();
        SoundManager.Instance.StopAllSounds();
        SceneManager.LoadScene("Scene"+ mainSceneName);
        
        
    }


    // Выбрать категорию "Hair"
    public void SelectHair()
    {
        currentCategory = "Hair";
        currentIndex = GetCurrentSpriteIndex(hairRenderer.sprite, hairOptions);
        SetButtonState(hairButton, true);
        SetButtonState(clothesButton, false);
    }

    // Выбрать категорию "Clothes"
    public void SelectClothes()
    {
        currentCategory = "Clothes";
        currentIndex = GetCurrentSpriteIndex(clothesRenderer.sprite, clothesOptions);
        SetButtonState(clothesButton, true);
        SetButtonState(hairButton, false);
    }

    // Следующий элемент
    public void NextItem()
    {
        if (currentCategory == "Hair")
        {
            List<int> unlockedHairstyles = GameStateManager.Instance.GetUnlockedHairstyles();
            int currentIndexInList = unlockedHairstyles.IndexOf(currentIndex);
            currentIndex = unlockedHairstyles[(currentIndexInList + 1) % unlockedHairstyles.Count];
            UpdateHair();
        }
        else if (currentCategory == "Clothes")
        {
            List<int> unlockedClothes = GameStateManager.Instance.GetUnlockedClothes();
            int currentIndexInList = unlockedClothes.IndexOf(currentIndex);
            currentIndex = unlockedClothes[(currentIndexInList + 1) % unlockedClothes.Count];
            UpdateClothes();
        }
    }

    public void PreviousItem()
    {
        if (currentCategory == "Hair")
        {
            List<int> unlockedHairstyles = GameStateManager.Instance.GetUnlockedHairstyles();
            int currentIndexInList = unlockedHairstyles.IndexOf(currentIndex);
            currentIndex = unlockedHairstyles[(currentIndexInList - 1 + unlockedHairstyles.Count) % unlockedHairstyles.Count];
            UpdateHair();
        }
        else if (currentCategory == "Clothes")
        {
            List<int> unlockedClothes = GameStateManager.Instance.GetUnlockedClothes();
            int currentIndexInList = unlockedClothes.IndexOf(currentIndex);
            currentIndex = unlockedClothes[(currentIndexInList - 1 + unlockedClothes.Count) % unlockedClothes.Count];
            UpdateClothes();
        }
    }


    // Обновить волосы
    private void UpdateHair()
    {
        string hairSpriteName = hairOptions[currentIndex].name;
        hairRenderer.sprite = hairOptions[currentIndex];
        GameStateManager.Instance.SaveAppearance(currentIndex, PlayerPrefs.GetInt("CurrentClothesIndex", 0));
    }

    private void UpdateClothes()
    {
        string clothesSpriteName = clothesOptions[currentIndex].name;
        clothesRenderer.sprite = clothesOptions[currentIndex];
        GameStateManager.Instance.SaveAppearance(PlayerPrefs.GetInt("CurrentHairIndex", 0), currentIndex);
    }


    // Получить индекс текущего спрайта
    private int GetCurrentSpriteIndex(Sprite currentSprite, Sprite[] options)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == currentSprite)
            {
                return i;
            }
        }
        return 0; // Если текущий спрайт не найден, возвращаем 0
    }


    // Установить состояние кнопки
    private void SetButtonState(Button button, bool isActive)
    {
        ColorBlock colors = button.colors;

        if (isActive)
        {
            colors.normalColor = new Color(0.6f, 0.6f, 0.6f);  // Серый цвет для активного состояния
            colors.highlightedColor = new Color(0.6f, 0.6f, 0.6f); // Такой же цвет для наведения
            colors.pressedColor = new Color(0.5f, 0.5f, 0.5f);     // Темнее для нажатия
            colors.selectedColor = new Color(0.6f, 0.6f, 0.6f);    // Совпадает с активным состоянием
        }
        else
        {
            colors.normalColor = new Color(1f, 1f, 1f);           // Белый цвет для неактивного состояния
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f); // Светло-серый для наведения
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);     // Немного темнее для нажатия
            colors.selectedColor = new Color(1f, 1f, 1f);         // Совпадает с неактивным состоянием
        }

        button.colors = colors;

        // Программно устанавливаем кнопку выбранной
        if (isActive)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
