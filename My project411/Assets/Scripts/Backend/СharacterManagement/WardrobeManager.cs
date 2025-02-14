using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;

public class WardrobeManager : MonoBehaviour
{
    // �������� ����
    public string wardrobeSceneName = "WardrobeScene";

    [SerializeField] private BlinkingManager blinkingManager;
    
    // ������ �� �������� ������� ���������
    public SpriteRenderer hairRenderer;    // Hair ������
    public SpriteRenderer clothesRenderer; // Clothes ������

    // ������ ��������
    public Sprite[] hairOptions;    // ������ ��������� �����
    public Sprite[] clothesOptions; // ������ ��������� ������

    private int currentIndex = 0;       // ������� ������ ��� ��������� ���������
    private string currentCategory;    // ������� ��������� ��������� ("Hair" ��� "Clothes")

    // ������ UI
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
        currentCategory = "Hair"; // �� ��������� ������� ��������� "Hair"
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
        // ��������� ������� ������� � GameStateManager
        int currentHairIndex = GetCurrentSpriteIndex(hairRenderer.sprite, hairOptions);
        int currentClothesIndex = GetCurrentSpriteIndex(clothesRenderer.sprite, clothesOptions);

        GameStateManager.Instance.SaveAppearance(currentHairIndex, currentClothesIndex);
        int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
        GameStateManager.Instance.SaveGameToSlot(selectedSlotIndex);
        Debug.Log($"������� ���������: ������={currentHairIndex}, ������={currentClothesIndex}");

        // ��������� �� �������� �����
        string mainSceneName = GameStateManager.Instance.LoadSceneID();
        SoundManager.Instance.StopAllSounds();
        SceneManager.LoadScene("Scene"+ mainSceneName);
        
        
    }


    // ������� ��������� "Hair"
    public void SelectHair()
    {
        currentCategory = "Hair";
        currentIndex = GetCurrentSpriteIndex(hairRenderer.sprite, hairOptions);
        SetButtonState(hairButton, true);
        SetButtonState(clothesButton, false);
    }

    // ������� ��������� "Clothes"
    public void SelectClothes()
    {
        currentCategory = "Clothes";
        currentIndex = GetCurrentSpriteIndex(clothesRenderer.sprite, clothesOptions);
        SetButtonState(clothesButton, true);
        SetButtonState(hairButton, false);
    }

    // ��������� �������
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


    // �������� ������
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


    // �������� ������ �������� �������
    private int GetCurrentSpriteIndex(Sprite currentSprite, Sprite[] options)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == currentSprite)
            {
                return i;
            }
        }
        return 0; // ���� ������� ������ �� ������, ���������� 0
    }


    // ���������� ��������� ������
    private void SetButtonState(Button button, bool isActive)
    {
        ColorBlock colors = button.colors;

        if (isActive)
        {
            colors.normalColor = new Color(0.6f, 0.6f, 0.6f);  // ����� ���� ��� ��������� ���������
            colors.highlightedColor = new Color(0.6f, 0.6f, 0.6f); // ����� �� ���� ��� ���������
            colors.pressedColor = new Color(0.5f, 0.5f, 0.5f);     // ������ ��� �������
            colors.selectedColor = new Color(0.6f, 0.6f, 0.6f);    // ��������� � �������� ����������
        }
        else
        {
            colors.normalColor = new Color(1f, 1f, 1f);           // ����� ���� ��� ����������� ���������
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f); // ������-����� ��� ���������
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);     // ������� ������ ��� �������
            colors.selectedColor = new Color(1f, 1f, 1f);         // ��������� � ���������� ����������
        }

        button.colors = colors;

        // ���������� ������������� ������ ���������
        if (isActive)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
