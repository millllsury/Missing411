using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;

public class Animations : MonoBehaviour
{
    [SerializeField] private SpriteRenderer emotionImageLeft;
    [SerializeField] private SpriteRenderer emotionImageRight;
    [SerializeField] private SpriteRenderer eyesImageLeft;
    [SerializeField] private SpriteRenderer eyesImageRight;

    private Dictionary<string, SpriteRenderer> emotionRenderers;
    private Dictionary<string, SpriteRenderer> eyesRenderers;

    private bool isLeftAvatarAnimation = false;
    private bool isRightAvatarAnimation = false;

    private BlinkingManager blinkingManager;
    private BackgroundController backgroundController;
    public bool IsLeftAvatarAnimating => isLeftAvatarAnimation;
    public bool IsRightAvatarAnimating => isRightAvatarAnimation;

    private Animator characterAnimator;


    private void Start()
    {
        emotionRenderers = new Dictionary<string, SpriteRenderer>
        {
            { "left", emotionImageLeft },
            { "right", emotionImageRight }
        };

        eyesRenderers = new Dictionary<string, SpriteRenderer>
        {
            { "left", eyesImageLeft },
            { "right", eyesImageRight }
        };

        blinkingManager = FindAnyObjectByType<BlinkingManager>();
        if (blinkingManager == null)
        {
            Debug.LogError("BlinkingManager �� ������ � �����!");
        }

        backgroundController = GetComponent<BackgroundController>();

        isLeftAvatarAnimation = false;
        isRightAvatarAnimation = false;
    }




    public void PlayAnimation(string characterPosition, string animationName, string character)
    {
        if (string.IsNullOrEmpty(character) || string.IsNullOrEmpty(animationName))
        {
            Debug.LogWarning("Not enough data to play the animation.");
            return;
        }

        // �������� ������� ��������� ����� GameStateManager
        characterPosition = GameStateManager.GetCharacterPosition(character) == 1 ? "left" : "right";

        if (!emotionRenderers.TryGetValue(characterPosition, out var emotionRenderer) ||
            !eyesRenderers.TryGetValue(characterPosition, out var eyesRenderer))
        {
            return;
        }

        Transform characterTransform = emotionRenderer.transform.parent; // emotions are inside the parent

        SpriteNull(emotionRenderer);
        SpriteNull(eyesRenderer);

        bool isAvatarAnimating = (characterPosition == "left") ? isLeftAvatarAnimation : isRightAvatarAnimation;
        if (isAvatarAnimating) return;

        string emotion = null;
        string eyes = null;
        string sound = null;
        bool leaveLastFrame = false;
        bool stopBlinkingPermanently = false;

        Debug.Log($"�������� animationName: {animationName}");

        switch (animationName.ToLower())
        {
            case "laugh":
                emotion = "happy";
                sound = emotion;
                break;
            case "sad":
                emotion = "sad";
                break;
            case "happy":
                emotion = "happy";
                eyes = "eyestothesidebase";
                break;
            case "fall":
                emotion = "ClosedEyes";
                leaveLastFrame = true;
                StartCoroutine(ShakeCharacter(characterTransform, 0.1f, 1f));
                sound = "body-fall";
                break;
            case "eyesToTheSideBase":
                eyes = "eyesToTheSideBase";
                break;
            case "moving":
                GameObject characterObject = null;

                if (characterPosition == "left")
                {
                    characterObject = GameObject.Find("CharacterAvatarLeft");
                }
                else if (characterPosition == "right")
                {
                    characterObject = GameObject.Find("CharacterAvatarRight");
                }

                if (characterObject != null)
                {
                    characterAnimator = characterObject.GetComponent<Animator>();

                    if (characterAnimator != null)
                    {
                        characterAnimator.SetTrigger("Moving");
                    }
                    else
                    {
                        Debug.LogError($"Animator �� ������ � {characterObject.name}!");
                    }
                }
                else
                {
                    Debug.LogError($"������ ��������� ({characterPosition}) �� ������!");
                }
                break;

            case "movingback":
                if (characterPosition == "left")
                {
                    characterObject = GameObject.Find("CharacterAvatarLeft");
                    if (characterObject != null)
                    {
                        StartCoroutine(MoveCharacter(characterObject, new Vector3(-3f, characterObject.transform.position.y, characterObject.transform.position.z)));
                    }
                }
                else if (characterPosition == "right")
                {
                    characterObject = GameObject.Find("CharacterAvatarRight");
                    if (characterObject != null)
                    {
                        StartCoroutine(MoveCharacter(characterObject, new Vector3(3f, characterObject.transform.position.y, characterObject.transform.position.z)));
                    }
                }
                break;
            case "shouting":
                emotion = "Shouting";
                sound = "Shouting";
                break;
            default:
                Debug.LogWarning($"�������� {animationName} �� �������.");
                return;
        }

        if (!string.IsNullOrEmpty(eyes) && eyesRenderer == null)
        {
            eyesRenderer.gameObject.SetActive(true);
            Debug.Log("�������� �����");
        }

        SetEmotionImage(emotionRenderer, eyesRenderer, character, emotion, eyes);
        PlaySoundForEmotion(character, sound);

        if (characterPosition == "left") isLeftAvatarAnimation = true;
        else if (characterPosition == "right") isRightAvatarAnimation = true;

       
        StartCoroutine(ShowEmotionForDuration(emotionRenderer, eyesRenderer, characterPosition, 2f, character, leaveLastFrame, stopBlinkingPermanently));
    }

    private IEnumerator MoveCharacter(GameObject character, Vector3 targetPosition)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3 startPosition = character.transform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            character.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            yield return null;
        }

        character.transform.position = targetPosition;
    }
    private IEnumerator ShakeCharacter(Transform characterTransform, float shakeAmount, float duration)
    {
        if (characterTransform == null) yield break;

        Vector3 originalPosition = characterTransform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float xOffset = Random.Range(-shakeAmount, shakeAmount);
            characterTransform.localPosition = new Vector3(originalPosition.x + xOffset, originalPosition.y, originalPosition.z);
            yield return null;
        }

        // Return the character to its original position
        characterTransform.localPosition = originalPosition;
    }



    private void PlaySoundForEmotion(string character, string sound = null)
    {
        if (sound == null) return;
        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager is null! Ensure it is assigned in the inspector.");
            return;
        }
     
        SoundManager.Instance.HandleSoundTrigger($"play:{character}_{sound}");
    }


    private void SpriteNull(SpriteRenderer renderer)
    {
        renderer.sprite = null;
    }


    public void CleanAllEmotionRenderers() {

        emotionImageLeft.sprite = null;
        emotionImageRight.sprite = null;

    }


    private void SetEmotionImage(SpriteRenderer emotionRenderer, SpriteRenderer eyesRenderer, string character, string emotion, string eyes)
    {
        SetSprite(emotionRenderer, !string.IsNullOrEmpty(emotion) ? $"Characters/{character}/{character}_{emotion}" : null);
        SetSprite(eyesRenderer, !string.IsNullOrEmpty(eyes) ? $"Characters/{character}/{character}_{eyes}" : null);
    }

    private void SetSprite(SpriteRenderer renderer, string path)
    {
        if (renderer == null) return;

        if (string.IsNullOrEmpty(path))
        {
            renderer.gameObject.SetActive(false);
            return;
        }

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            renderer.sprite = sprite;
            renderer.gameObject.SetActive(true);

        }
        else
        {
            Debug.Log($"������ �� ������ �� ����: {path}");
            renderer.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowEmotionForDuration(SpriteRenderer emotionRenderer, SpriteRenderer eyesRenderer, string characterPosition, float duration, string characterName, bool leaveLastFrame, bool stopBlinkingPermanently)
    {

        blinkingManager.StopBlinking(characterName);

        // �������� ���������� ��������
        yield return FadeIn(emotionRenderer);
        yield return FadeIn(eyesRenderer);

        yield return new WaitForSeconds(duration);

        // �������� ���������� �������� ����� ���������
        if (!leaveLastFrame)
        {
            yield return FadeOut(emotionRenderer);
        }

        // ��������� ��������
        if (characterPosition == "left") isLeftAvatarAnimation = false;
        else if (characterPosition == "right") isRightAvatarAnimation = false;

        Debug.Log($"�������� ���������, ������ ������. �������� �������� ��� {characterName}");

  
        if (!IsCharacterInSave(characterName))
        {
            Debug.Log($"[ShowEmotionForDuration] �������� {characterName} ����������� � ����������. �������� �� �����������.");
            yield break; 
        }

        // �������� ����� �������� ��������
        if (blinkingManager != null && eyesRenderers.ContainsKey(characterPosition) && eyesRenderers[characterPosition] != null && !leaveLastFrame && !stopBlinkingPermanently)
        {
            blinkingManager.StartBlinking(characterName, eyesRenderers[characterPosition]);
        }
        else if (stopBlinkingPermanently)
        {
            blinkingManager.StopBlinking(characterName);
            Debug.Log($"�������� ��� {characterName} ����������� ��-�� ������ 'shadow'.");
        }
    }

    private bool IsCharacterInSave(string characterName)
    {
        var (leftCharacter, rightCharacter) = GameStateManager.Instance.LoadCharacterNames();

        return leftCharacter == characterName || rightCharacter == characterName;
    }



    private IEnumerator FadeIn(SpriteRenderer renderer)
    {
        if (renderer == null) yield break;

        for (float alpha = 0f; alpha <= 1f; alpha += Time.deltaTime * 5) // ���������� ��������
        {
            SetAlpha(renderer, alpha);
            yield return null;
        }
        SetAlpha(renderer, 1f); // ��������, ��� �������� ����� 1
    }

    private IEnumerator FadeOut(SpriteRenderer renderer)
    {
        if (renderer == null) yield break;

        for (float alpha = 1f; alpha >= 0f; alpha -= Time.deltaTime * 5)
        {
            SetAlpha(renderer, alpha);
            yield return null;
        }
        SetAlpha(renderer, 0f); // ��������, ��� �������� ����� 0
        SpriteNull(renderer);
    }

    private void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer == null) return;

        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }

   

}


