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
    


    public void HideObjects()
    {
        doors.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
        wardrobeBtn.interactable = false;
    }

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

    private void Update()
    {
        if (flagsManager.GetFlag("leftBedroom") && flagsManager.GetFlag("rightMainRoom"))
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    public void nextButtonClick()
    {
        dialogueManager.ShowNextDialogueText();
        dialogueManager.blockMovingForward = true;
    }

    // 🔹 Метод для открытия левой двери
    public void OpenLeftDoor()
    {
        if (!AnimatorHasParameter("LeftDoorOpen"))
        {
            Debug.LogError($"В `Animator` отсутствует параметр LeftDoorOpen");
            return;
        }

        Debug.Log($"Триггер `LeftDoorOpen` вызван.");
        doorAnimator.SetTrigger("LeftDoorOpen");

        StartCoroutine(PlayLeftDoorAnimation());

        SoundManager.Instance.PlaySoundByName("door");
        flagsManager.SetFlag("leftBedroom", true);
    }


    public void OpenRightDoor()
    {
        if (!AnimatorHasParameter("RightDoorOpen"))
        {
            Debug.LogError($"В `Animator` отсутствует параметр RightDoorOpen");
            return;
        }

        Debug.Log($"Триггер `RightDoorOpen` вызван.");
        doorAnimator.SetTrigger("RightDoorOpen");

        StartCoroutine(PlayRightDoorAnimation());

        SoundManager.Instance.PlaySoundByName("door");
        flagsManager.SetFlag("rightBedroom", true);
    }

  
    private IEnumerator PlayLeftDoorAnimation()
    {


        yield return new WaitForEndOfFrame();

        AnimatorStateInfo stateInfo = doorAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("LeftDoorOpen"))
        {
            Debug.Log($" Анимация LeftDoorOpen запущена.");
        }

        yield return new WaitForSeconds(stateInfo.length);
        
        // Переключаем сцену
        bedroom.SetActive(true);
        mainScene.SetActive(false);
        mainRoom.SetActive(false);

        if (doorButton != null)
        {
            yield return new WaitForSeconds(0.3f);
            doorButton.interactable = true;
        }

        doorAnimator.ResetTrigger("LeftDoorOpen");
      
    }


    private IEnumerator PlayRightDoorAnimation()
    {
        yield return new WaitForEndOfFrame();

        AnimatorStateInfo stateInfo = doorAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("RightDoorOpen"))
        {
            Debug.Log($"Анимация RightDoorOpen запущена.");
        }
        

        yield return new WaitForSeconds(stateInfo.length);
       
        // Переключаем сцену
        bedroom.SetActive(false);
        mainScene.SetActive(false);
        mainRoom.SetActive(true);

        if (doorButton != null)
        {
            yield return new WaitForSeconds(0.3f);
            doorButton.interactable = true;
        }

        doorAnimator.ResetTrigger("RightDoorOpen");
       //doorAnimator.SetTrigger("ResetDoor");
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
