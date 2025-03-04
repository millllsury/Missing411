using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DoorBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject mainScene;
    [SerializeField] private GameObject bedroom;
    [SerializeField] private GameObject mainRoom;
    [SerializeField] private Button doorButton;
    [SerializeField] private Canvas doors;
    [SerializeField] private Button backButton;
    [SerializeField] private Button wardrobeBtn;
    [SerializeField] private Button nextButton;
    private Animator doorAnimator;

    private GameFlagsManager flagsManager;
    private DialogueManager dialogueManager;
    [SerializeField] private Image leftDoorImage;  
    [SerializeField] private Image rightDoorImage;

    private void Awake()
    {
        flagsManager = FindAnyObjectByType<GameFlagsManager>();
        dialogueManager = FindAnyObjectByType<DialogueManager>();

        // Ищем Animator на дочерних объектах
        doorAnimator = GetComponentInChildren<Animator>();

        if (doorAnimator == null)
        {
            Debug.LogError($"Animator отсутствует на объекте {gameObject.name} или его дочерних объектах!");
        }
        else
        {
            Debug.Log($"Найден Animator на объекте: {doorAnimator.gameObject.name}");
        }
    }


    private void Start()
    {
        LoadDoorState();
    }

    private void Update()
    {
        if (GameStateManager.Instance.GetLeftDoorOpened() && GameStateManager.Instance.GetRightDoorOpened())
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    private void LoadDoorState()
    {
        if (GameStateManager.Instance.GetLeftDoorOpened())
        {
            Animator leftAnimator = leftDoorImage.GetComponent<Animator>();
            if (leftAnimator != null)
            {
                leftAnimator.enabled = false; // Отключаем анимацию
            }

            // Устанавливаем спрайт OpenDoor1
            Sprite leftSprite = Resources.Load<Sprite>("Backgrounds/doors/OpenDoor1");
            if (leftSprite != null)
            {
                leftDoorImage.sprite = leftSprite;
                Debug.Log("✅ Левая дверь уже была открыта, устанавливаем спрайт OpenDoor1.");
            }
            else
            {
                Debug.LogError("❌ Ошибка: Спрайт OpenDoor1 не найден в Resources!");
            }
        }

        if (GameStateManager.Instance.GetRightDoorOpened())
        {
            Animator rightAnimator = rightDoorImage.GetComponent<Animator>();
            if (rightAnimator != null)
            {
                rightAnimator.enabled = false; // Отключаем анимацию
            }

            // Устанавливаем спрайт OpenDoor2
            Sprite rightSprite = Resources.Load<Sprite>("Backgrounds/doors/OpenDoor2");
            if (rightSprite != null)
            {
                rightDoorImage.sprite = rightSprite;
                Debug.Log("✅ Правая дверь уже была открыта, устанавливаем спрайт OpenDoor2.");
            }
            else
            {
                Debug.LogError("❌ Ошибка: Спрайт OpenDoor2 не найден в Resources!");
            }
        }
    }


    public void HideObjects()
    {
        doors.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
        wardrobeBtn.interactable = false;
    }

    public void nextButtonClick()
    {
        dialogueManager.ShowNextDialogueText();
        dialogueManager.blockMovingForward = false;

    }

    // 🔹 Метод для открытия левой двери
    public void OpenLeftDoor()
    {
        if (GameStateManager.Instance.GetLeftDoorOpened())
        {
            Debug.Log("Левая дверь уже открыта. Сразу переключаем сцену.");

            // Переключаем сцену сразу, так как анимация уже была проиграна ранее
            bedroom.SetActive(true);
            mainScene.SetActive(false);
            mainRoom.SetActive(false);

            return;
        }

        if (!AnimatorHasParameter("LeftDoorOpen"))
        {
            Debug.LogError($"В `Animator` отсутствует параметр LeftDoorOpen");
            return;
        }

        StartCoroutine(PlayLeftDoorAnimation());
    }


    public void OpenRightDoor()
    {
        if (GameStateManager.Instance.GetRightDoorOpened())
        {
            Debug.Log("Правая дверь уже открыта. Сразу переключаем сцену.");

            // Переключаем сцену сразу, так как анимация уже была проиграна ранее
            bedroom.SetActive(false);
            mainScene.SetActive(false);
            mainRoom.SetActive(true);

            return;
        }

        if (!AnimatorHasParameter("RightDoorOpen"))
        {
            Debug.LogError($"В `Animator` отсутствует параметр RightDoorOpen");
            return;
        }

        StartCoroutine(PlayRightDoorAnimation());
    }


    private IEnumerator PlayLeftDoorAnimation()
    {
        doorAnimator.SetTrigger("LeftDoorOpen");
        GameStateManager.Instance.SetLeftDoorOpened(true);
        SoundManager.Instance.PlaySoundByName("door");

        yield return new WaitForEndOfFrame();

        AnimatorStateInfo stateInfo = doorAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("LeftDoorOpen"))
        {
            Debug.Log($"Анимация LeftDoorOpen запущена.");
        }

        yield return new WaitForSeconds(stateInfo.length); // Ждём завершения анимации

        // Переключаем сцену
        bedroom.SetActive(true);
        mainScene.SetActive(false);
        mainRoom.SetActive(false);

        yield return RestoreButtonState();

        doorAnimator.ResetTrigger("LeftDoorOpen");

        GameStateManager.Instance.SetLeftDoorOpened(true); // Фиксируем состояние двери
        flagsManager.SetFlag("leftBedroom", true);
    }


    private IEnumerator PlayRightDoorAnimation()
    {
        doorAnimator.SetTrigger("RightDoorOpen");
        GameStateManager.Instance.SetRightDoorOpened(true);
        SoundManager.Instance.PlaySoundByName("door");

        yield return new WaitForEndOfFrame();

        AnimatorStateInfo stateInfo = doorAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("RightDoorOpen"))
        {
            Debug.Log($"Анимация RightDoorOpen запущена.");
        }

        yield return new WaitForSeconds(stateInfo.length); // Ждём завершения анимации

        // Переключаем сцену
        bedroom.SetActive(false);
        mainScene.SetActive(false);
        mainRoom.SetActive(true);

        yield return RestoreButtonState();

        doorAnimator.ResetTrigger("RightDoorOpen");

        GameStateManager.Instance.SetRightDoorOpened(true); // Фиксируем состояние двери
        flagsManager.SetFlag("rightMainroom", true);
    }


    private bool AnimatorHasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in doorAnimator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    private IEnumerator RestoreButtonState()
    {
        yield return new WaitForSeconds(0.3f);

        if (doorButton != null)
        {
            doorButton.interactable = true;
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }

        if (wardrobeBtn != null)
        {
            wardrobeBtn.interactable = true;
        }
    }
}
