using UnityEngine;
using TMPro;
using System.Collections;

public class CharacterManager : MonoBehaviour
{
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

    [SerializeField] private Transform leftPosition;
    [SerializeField] private Transform rightPosition;

    private bool isLeftAvatarAnimating = false;
    private bool isRightAvatarAnimating = false;

    public bool IsLeftAvatarAnimating => isLeftAvatarAnimating;
    public bool IsRightAvatarAnimating => isRightAvatarAnimating;

    public void SetCharacter(string speaker, int place, bool isNarration, string character)
    {
        Debug.Log("SetCharacter ������ ��� ���������: " + speaker + " � �������: " + place);

        speakerPanelLeft.SetActive(false);
        speakerPanelCenter.SetActive(false);
        speakerPanelRight.SetActive(false);

        GameObject activePanel = null;

        if (place == 1)
        {
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
            currentCharacter = character;
            UpdateAvatar(avatar, character, isLeft);

            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);

            blinkCoroutine = StartCoroutine(BlinkCoroutine(eyesImage, character));
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

        Sprite loadedSprite = Resources.Load<Sprite>("Characters/" + character);
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
                // ������ ���������� Y � ����������� �� ����, ����� ��� �������� ��� ������
                float targetX = isLeft ? -5f : 5f;  // ������� �� X
                float targetY = isLeft ? -1.5f : -1f;  // ������� �� Y ��� ������ � ������� ���������

                // �������� X � Y ��� ������ SmoothAppear
                StartCoroutine(SmoothAppear(avatar, character, targetX, targetY));
            }

            Debug.Log("���������� ������ ��� ���������: " + character);
        }
        else
        {
            StartCoroutine(SmoothDisappear(avatar));
            Debug.LogWarning("������ �� ������ ��� ���������: " + character);
        }
    }


    private IEnumerator SmoothAppear(SpriteRenderer avatar, string character, float endPositionX, float endPositionY)
    {
        // ����, ���� �������� ������ ���������� �� ����������
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        // �������� ��������
        isLeftAvatarAnimating = true;

        // �������� ������� ������� ��������� � ������ �������
        Vector3 startPosition = avatar.transform.position;
        Vector3 endPosition = new Vector3(endPositionX, endPositionY, avatar.transform.position.z); // ��������� � ��� Y

        float duration = 0.5f; // ����������������� ��������
        float elapsedTime = 0f;

        // ��������� ���� ��������� (������������)
        avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, 0f);
        avatar.gameObject.SetActive(true);

        // ������� �������� � ��������� ������������
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // �������� ������������ ��� ������� � �����-������
            avatar.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, Mathf.Lerp(0f, 1f, elapsedTime / duration));

            yield return null;
        }

        // ������������� ��������� ������� � ����
        avatar.transform.position = endPosition;
        avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, 1f);

        isLeftAvatarAnimating = false;
    }

    private IEnumerator WaitAndShowNewAvatar(SpriteRenderer avatar, string character, bool isLeft)
    {
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        // ������ ���� ��� X � Y � ����������� �� ����, ����� ��� ������ ��������
        float targetX = isLeft ? -5f : 5f; // ����� �������� �� -5, ������ �� 5
        float targetY = isLeft ? -1.5f : -1f; // ����� �� -1.5, ������ �� -1

        // �������� �������� ��� ��������� ���������
        yield return StartCoroutine(SmoothAppear(avatar, character, targetX, targetY));
    }


    private IEnumerator SmoothDisappear(SpriteRenderer avatar)
    {
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        float duration = 0.5f;
        float elapsedTime = 0f;

        Color startColor = avatar.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            avatar.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, elapsedTime / duration));
            yield return null;
        }

        avatar.gameObject.SetActive(false);
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

    private IEnumerator BlinkCoroutine(SpriteRenderer eyesImage, string character)
    {
        while (true)
        {
            if (!isLeftAvatarAnimating && !isRightAvatarAnimating) // ���������, �� ���� �� ��������
            {
                Sprite closedEyesSprite = Resources.Load<Sprite>("Characters/" + character + "_ClosedEyes");
                if (closedEyesSprite != null)
                {
                    eyesImage.sprite = closedEyesSprite;
                    eyesImage.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"������ �������� ���� �� ������ ���: {character}");
                }

                // �������� (�������� �����)
                yield return new WaitForSeconds(0.2f);

                eyesImage.gameObject.SetActive(false);
                yield return new WaitForSeconds(Random.Range(3f, 5f));
            }
            else
            {
                yield return null;
            }
        }
    }
}