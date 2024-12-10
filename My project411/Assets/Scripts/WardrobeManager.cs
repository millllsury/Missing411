using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WardrobeManager : MonoBehaviour
{
    // �������� ����
    public string wardrobeSceneName = "WardrobeScene";
    private DialogueManager dialogueManager;

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

    private void Awake()
    {

        currentCategory = "Hair"; // �� ��������� ������� ��������� "Hair"
        SetButtonState(hairButton, true);
        SetButtonState(clothesButton, false);

        // ������������ ��������� ���������, ���� ���� ����������� ������
        if (hairRenderer != null && clothesRenderer != null)
        {
            int savedHairIndex = PlayerPrefs.GetInt("CurrentHairIndex", 0);
            int savedClothesIndex = PlayerPrefs.GetInt("CurrentClothesIndex", 0);

            hairRenderer.sprite = hairOptions[savedHairIndex];
            clothesRenderer.sprite = clothesOptions[savedClothesIndex];
        }
    }

    // ������� � ����� ���������
    public void OpenWardrobe()
    {
        //mainSceneName = SceneManager.GetActiveScene().name;
        //Debug.Log($"������� �����: {mainSceneName}");
        // ��������� ������� ������� � PlayerPrefs
        PlayerPrefs.SetInt("CurrentHairIndex", GetCurrentSpriteIndex(hairRenderer.sprite, hairOptions));
        PlayerPrefs.SetInt("CurrentClothesIndex", GetCurrentSpriteIndex(clothesRenderer.sprite, clothesOptions));
        PlayerPrefs.Save();

        // ��������� ����� ���������
        //SceneManager.LoadScene(wardrobeSceneName);
    }

    // ������� �������� � ��������� � �������� �����
    public void CloseWardrobe()
    {
        // ��������� ������� �����
       
        PlayerPrefs.SetInt("CurrentHairIndex", currentIndex);
        PlayerPrefs.SetInt("CurrentClothesIndex", currentIndex);
        PlayerPrefs.Save();
        string mainSceneName = PlayerPrefs.GetString("MainSceneName", "DefaultScene");
        Debug.Log(mainSceneName);
        // ��������� �������� �����

        SceneManager.LoadScene(mainSceneName);
    }

    // ������� ��������� "Hair"
    public void SelectHair()
    {
        currentCategory = "Hair";
        currentIndex = GetCurrentSpriteIndex(hairRenderer.sprite, hairOptions);
        //UpdateHair();
        SetButtonState(hairButton, true);
        SetButtonState(clothesButton, false);
    }

    // ������� ��������� "Clothes"
    public void SelectClothes()
    {
        currentCategory = "Clothes";
        currentIndex = GetCurrentSpriteIndex(clothesRenderer.sprite, clothesOptions);
        //UpdateClothes();
        SetButtonState(clothesButton, true);
        SetButtonState(hairButton, false);
    }

    // ��������� �������
    public void NextItem()
    {
        if (currentCategory == "Hair")
        {
            currentIndex = (currentIndex + 1) % hairOptions.Length;
            UpdateHair();
        }
        else if (currentCategory == "Clothes")
        {
            currentIndex = (currentIndex + 1) % clothesOptions.Length;
            UpdateClothes();
        }
    }

    // ���������� �������
    public void PreviousItem()
    {
        if (currentCategory == "Hair")
        {
            currentIndex = (currentIndex - 1 + hairOptions.Length) % hairOptions.Length;
            UpdateHair();
        }
        else if (currentCategory == "Clothes")
        {
            currentIndex = (currentIndex - 1 + clothesOptions.Length) % clothesOptions.Length;
            UpdateClothes();
        }
    }

    // �������� ������
    private void UpdateHair()
    {
        hairRenderer.sprite = hairOptions[currentIndex];
    }

    // �������� ������
    private void UpdateClothes()
    {
        clothesRenderer.sprite = clothesOptions[currentIndex];
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
            colors.normalColor = new Color(0.7f, 0.7f, 0.7f); // ����� ����, ��������� �� ����������
        }
        else
        {
            colors.normalColor = new Color(1f, 1f, 1f); // ����� ���� ��� ���������� ������
        }
        button.colors = colors;
    }
}
