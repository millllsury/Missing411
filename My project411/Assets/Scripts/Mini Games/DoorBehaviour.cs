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
    [SerializeField] private Sprite leftDoorOpenSprite;  
    [SerializeField] private Sprite rightDoorOpenSprite;
    private void Awake()
    {
        flagsManager = FindAnyObjectByType<GameFlagsManager>();
        dialogueManager = FindAnyObjectByType<DialogueManager>();
        doorAnimator = GetComponent<Animator>();

        if (doorAnimator == null)
        {
            Debug.LogError($"Animator отсутствует на {gameObject.name}!");
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
            // Используем спрайт из инспектора или загружаем из Resources
            leftDoorImage.sprite = leftDoorOpenSprite != null
                ? leftDoorOpenSprite
                : Resources.Load<Sprite>("Backgrounds/Doors/OpenDoor1");

            Debug.Log("Левая дверь уже была открыта, устанавливаем спрайт.");
        }

        if (GameStateManager.Instance.GetRightDoorOpened())
        {
            // Используем спрайт из инспектора или загружаем из Resources
            rightDoorImage.sprite = rightDoorOpenSprite != null
                ? rightDoorOpenSprite
                : Resources.Load<Sprite>("Backgrounds/Doors/OpenDoor2");

            Debug.Log("Правая дверь уже была открыта, устанавливаем спрайт.");
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
        dialogueManager.blockMovingForward = true;
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
