using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WardrobeManager : MonoBehaviour
{
    // Названия сцен
    public string wardrobeSceneName = "WardrobeScene";
    private DialogueManager dialogueManager;

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

    private void Awake()
    {

        currentCategory = "Hair"; // По умолчанию выбрана категория "Hair"
        SetButtonState(hairButton, true);
        SetButtonState(clothesButton, false);

        // Восстановить состояние персонажа, если есть сохраненные данные
        if (hairRenderer != null && clothesRenderer != null)
        {
            int savedHairIndex = PlayerPrefs.GetInt("CurrentHairIndex", 0);
            int savedClothesIndex = PlayerPrefs.GetInt("CurrentClothesIndex", 0);

            hairRenderer.sprite = hairOptions[savedHairIndex];
            clothesRenderer.sprite = clothesOptions[savedClothesIndex];
        }
    }

    // Перейти в сцену гардероба
    public void OpenWardrobe()
    {
        //mainSceneName = SceneManager.GetActiveScene().name;
        //Debug.Log($"Текущая сцена: {mainSceneName}");
        // Сохранить текущие индексы в PlayerPrefs
        PlayerPrefs.SetInt("CurrentHairIndex", GetCurrentSpriteIndex(hairRenderer.sprite, hairOptions));
        PlayerPrefs.SetInt("CurrentClothesIndex", GetCurrentSpriteIndex(clothesRenderer.sprite, clothesOptions));
        PlayerPrefs.Save();

        // Загрузить сцену гардероба
        //SceneManager.LoadScene(wardrobeSceneName);
    }

    // Закрыть гардероб и вернуться в основную сцену
    public void CloseWardrobe()
    {
        // Сохранить текущий выбор
       
        PlayerPrefs.SetInt("CurrentHairIndex", currentIndex);
        PlayerPrefs.SetInt("CurrentClothesIndex", currentIndex);
        PlayerPrefs.Save();
        string mainSceneName = PlayerPrefs.GetString("MainSceneName", "DefaultScene");
        Debug.Log(mainSceneName);
        // Загрузить основную сцену

        SceneManager.LoadScene(mainSceneName);
    }

    // Выбрать категорию "Hair"
    public void SelectHair()
    {
        currentCategory = "Hair";
        currentIndex = GetCurrentSpriteIndex(hairRenderer.sprite, hairOptions);
        //UpdateHair();
        SetButtonState(hairButton, true);
        SetButtonState(clothesButton, false);
    }

    // Выбрать категорию "Clothes"
    public void SelectClothes()
    {
        currentCategory = "Clothes";
        currentIndex = GetCurrentSpriteIndex(clothesRenderer.sprite, clothesOptions);
        //UpdateClothes();
        SetButtonState(clothesButton, true);
        SetButtonState(hairButton, false);
    }

    // Следующий элемент
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

    // Предыдущий элемент
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

    // Обновить волосы
    private void UpdateHair()
    {
        hairRenderer.sprite = hairOptions[currentIndex];
    }

    // Обновить одежду
    private void UpdateClothes()
    {
        clothesRenderer.sprite = clothesOptions[currentIndex];
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
            colors.normalColor = new Color(0.7f, 0.7f, 0.7f); // Серый цвет, указывает на активность
        }
        else
        {
            colors.normalColor = new Color(1f, 1f, 1f); // Белый цвет для неактивной кнопки
        }
        button.colors = colors;
    }
}
