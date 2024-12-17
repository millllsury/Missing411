using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private BlinkingManager blinkingManager;
    [SerializeField] private SpriteRenderer leftAvatar;
    [SerializeField] private SpriteRenderer rightAvatar;

    [SerializeField] private SpriteRenderer leftEyesImage;
    [SerializeField] private SpriteRenderer rightEyesImage;

    [SerializeField] private GameObject speakerPanelLeft;
    [SerializeField] private GameObject speakerPanelCenter;
    [SerializeField] private GameObject speakerPanelRight;

    private Coroutine leftBlinkCoroutine;
    private Coroutine rightBlinkCoroutine;
    private string currentLeftCharacter;
    private string currentRightCharacter;

    [SerializeField] private Animations animations;
    private bool isLeftAvatarAnimating = false;
    private bool isRightAvatarAnimating = false;

    [SerializeField]  private SpriteRenderer hairRenderer;
    [SerializeField]  private SpriteRenderer clothesRenderer;

    private void Start()
    {
       LoadAppearance();
        //LoadCharacters();
    }

    public void LoadCharacters()
    {
        var (leftCharacter, rightCharacter) = GameStateManager.Instance.LoadCharacterNames();

        Debug.Log($"��������� ����������: Left = {leftCharacter}, Right = {rightCharacter}");

        // ��������������� ������ ���� ��� ��������� ������
        if (!string.IsNullOrEmpty(leftCharacter))
            SetCharacter(leftCharacter, 1, false, leftCharacter);

        if (!string.IsNullOrEmpty(rightCharacter))
            SetCharacter(rightCharacter, 2, false, rightCharacter);
    }



    public void LoadAppearance()
    {
        var (hairIndex, clothesIndex) = GameStateManager.Instance.LoadAppearance();

        Debug.Log($"�������� �������� ����: HairIndex = {hairIndex}, ClothesIndex = {clothesIndex}");

        // ��������� ������� �� ����
        string hairPath = $"Characters/Alice/Hair/hair{hairIndex}";
        string clothesPath = $"Characters/Alice/Clothes/clothes{clothesIndex}";

        Sprite hairSprite = Resources.Load<Sprite>(hairPath);
        Sprite clothesSprite = Resources.Load<Sprite>(clothesPath);

        Debug.Log($"���� � ��������: Hair = {hairPath}, Clothes = {clothesPath}");

        if (hairSprite != null)
        {
            hairRenderer.sprite = hairSprite;
            Debug.Log("������ ��� ����� ������� ��������.");
        }
        else
        {
            Debug.LogError($"������ ��� ����� �� ������: {hairPath}");
        }

        if (clothesSprite != null)
        {
            clothesRenderer.sprite = clothesSprite;
            Debug.Log("������ ��� ������ ������� ��������.");
        }
        else
        {
            Debug.LogError($"������ ��� ������ �� ������: {clothesPath}");
        }
    }




    // ����� ��� ��������� ���������
    public void SetCharacter(string speaker, int place, bool isNarration, string character)
    {
        Debug.Log("SetCharacter ������ ��� ���������: " + speaker + " � �������: " + place);

        speakerPanelLeft.SetActive(false);
        speakerPanelCenter.SetActive(false);
        speakerPanelRight.SetActive(false);

        GameObject activePanel = null;

        if (place == 1)
        { 
            // �������� leftEyesImage � ��������� isLeft = true
            UpdateCharacter(ref currentLeftCharacter, leftAvatar, ref leftBlinkCoroutine, leftEyesImage, character, true);
            activePanel = speakerPanelLeft;
            
        }
        else if (place == 2)
        {
            
            UpdateCharacter(ref currentRightCharacter, rightAvatar, ref rightBlinkCoroutine, rightEyesImage, character, false);
            activePanel = speakerPanelRight;
            
        }
        else if (isNarration)
        {
            activePanel = speakerPanelCenter;
        }
        else
        {
            HideAvatars();
            Debug.LogWarning("������������ �������� 'place' ��� SetCharacter: " + place);
            return;
        }

        if (activePanel != null)
        {
            activePanel.SetActive(true);
            TextMeshProUGUI speakerText = activePanel.transform.Find("SpeakerText").GetComponent<TextMeshProUGUI>();
            speakerText.text = isNarration ? "..." : speaker;
        }
    }


    private void UpdateCharacter(ref string currentCharacter, SpriteRenderer avatar, ref Coroutine blinkCoroutine, SpriteRenderer eyesImage, string character, bool isLeft)
    {
        if (currentCharacter != character)
        {
            // ���������� �������� ��� ����������� ���������
            if (!string.IsNullOrEmpty(currentCharacter))
            {
                blinkingManager.StopBlinking(currentCharacter); // ������������ �����
            }

            currentCharacter = character;
            UpdateAvatar(avatar, character, isLeft);

            // ��������� �������� ��� ������ ���������
            blinkingManager.StartBlinking(character, eyesImage);
        }
    }


    private void UpdateAvatar(SpriteRenderer avatar, string character, bool isLeft)
    {


        if (avatar == null)
        {
            Debug.LogError("Avatar is null!");
            return;
        }

        if (string.IsNullOrEmpty(character))
        {
            Debug.LogWarning("��� ��������� �� �������.");
            StartCoroutine(SmoothDisappear(avatar));
            return;
        }

        Sprite loadedSprite = Resources.Load<Sprite>("Characters/" + character + "/" + character);
        if (loadedSprite == null)
        {
            Debug.LogError($"������ ��� {character} �� ������ � Resources/Characters!");
            return;
        }

        avatar.sprite = loadedSprite;

        if (avatar.sprite != null)
        {
            if (avatar.gameObject.activeSelf)
            {
                StartCoroutine(SmoothDisappear(avatar));
                StartCoroutine(WaitAndShowNewAvatar(avatar, character, isLeft));
            }
            else
            {
                float targetX = isLeft ? -3f : 3f;
                StartCoroutine(SmoothAppear(avatar, character, targetX));
            }

            Debug.Log("���������� ������ ��� ���������: " + character);
        }
        else
        {
            StartCoroutine(SmoothDisappear(avatar));
            Debug.LogWarning("������ �� ������ ��� ���������: " + character);
        }

    }



    private IEnumerator SmoothAppear(SpriteRenderer avatar, string character, float endPositionX)
    {
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        isLeftAvatarAnimating = true;

        Vector3 startPosition = avatar.transform.position;
        Vector3 endPosition = new Vector3(endPositionX, avatar.transform.position.y, avatar.transform.position.z);

        avatar.gameObject.SetActive(true);

        // �������� ���������
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            avatar.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, Mathf.Lerp(0f, 1f, elapsedTime / duration));

            yield return null;
        }

        avatar.transform.position = endPosition;
        avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, 1f);

        isLeftAvatarAnimating = false; // ��� isRightAvatarAnimating � ����������� �� ����, ����� ������ �����������
    }

    private IEnumerator SmoothDisappear(SpriteRenderer avatar)
    {
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        avatar.gameObject.SetActive(false);
        avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, 0f); // �������� ������������ �� ������ ���������� �������������
    }

 

    private IEnumerator WaitAndShowNewAvatar(SpriteRenderer avatar, string character, bool isLeft)
    {
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        float targetX = isLeft ? -3f : 3f;
        yield return StartCoroutine(SmoothAppear(avatar, character, targetX));
    }

    public void HideAvatars()
    {
        leftAvatar.gameObject.SetActive(false);
        rightAvatar.gameObject.SetActive(false);
        speakerPanelLeft.SetActive(false);
        speakerPanelCenter.SetActive(false);
        speakerPanelRight.SetActive(false);

        if (leftBlinkCoroutine != null)
            StopCoroutine(leftBlinkCoroutine);
        if (rightBlinkCoroutine != null)
            StopCoroutine(rightBlinkCoroutine);

        leftEyesImage.gameObject.SetActive(false);
        rightEyesImage.gameObject.SetActive(false);

        Debug.Log("������ ��� ������� � ������ ����������.");
    }

    private void OnDisable()
    {
        // ���������� ��� �������� ��� ����������
        blinkingManager.StopAllBlinking();
    }

    public string GetCurrentLeftCharacter()
    {
        return string.IsNullOrEmpty(currentLeftCharacter) ? null : currentLeftCharacter;
    }

    public string GetCurrentRightCharacter()
    {
        return string.IsNullOrEmpty(currentRightCharacter) ? null : currentRightCharacter;
    }


   


}




