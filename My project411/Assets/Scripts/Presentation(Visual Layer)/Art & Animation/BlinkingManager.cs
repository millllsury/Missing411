using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;

public class BlinkingManager : MonoBehaviour
{


    private Dictionary<string, Coroutine> blinkingCoroutines = new Dictionary<string, Coroutine>();

    public bool IsLeftAvatarAnimating { get; set; } = false; // ���� �������� ������ �������
    public bool IsRightAvatarAnimating { get; set; } = false; // ���� �������� ������� �������

    public delegate bool AnimationCheckDelegate();
    public AnimationCheckDelegate IsExternalAnimationPlaying;




    public void StartBlinking(string characterName, SpriteRenderer eyesImage)
    {
       // var (leftCharacter, rightCharacter) = GameStateManager.Instance.LoadCharacterNames();

        //if(leftCharacter == null || rightCharacter == null) { return; }
        Debug.Log($"StartBlinking called for {characterName} at {Time.time}");

        if (string.IsNullOrEmpty(characterName) || eyesImage == null)
        {
            Debug.LogWarning("Character name or eyesImage is null. Blinking will not start.");
            return;
        }

        if (shouldBlink.ContainsKey(characterName) && !shouldBlink[characterName])
        {
            Debug.Log($"[StartBlinking] �������� ��������� ��� {characterName}, ������ ������.");
            return;
        }

        // ���������� ������ ��������, ���� ��� ����
        if (blinkingCoroutines.ContainsKey(characterName))
        {
            StopBlinking(characterName);
        }

        // ���������, ���� �������� ��� ��������
        if (blinkingCoroutines.ContainsKey(characterName))
        {
            Debug.Log($"Blinking for {characterName} is already running.");
            return; // �� ��������� ����� ��������
        }

        //Debug.Log($"�������� � blinkingManager.StartBlinking() characterName: {characterName} ");

        // ������ ����� ��������
        Coroutine coroutine = StartCoroutine(BlinkCoroutine(eyesImage, characterName));
        blinkingCoroutines[characterName] = coroutine;
        globalCoroutineCount++;
        Debug.Log($"�������� ��� {characterName} ������� ���������. globalCoroutineCount: {globalCoroutineCount} ");
    }


    public void StopBlinking(string characterName)
    {
        if (blinkingCoroutines.TryGetValue(characterName, out Coroutine coroutine) && coroutine != null)
        {
            StopCoroutine(coroutine);
            blinkingCoroutines.Remove(characterName);
            Debug.Log($"[StopBlinking] �������� ����������� ��� {characterName}");
        }

        // ������������ ����� �������, ����� ��� �� ���������� ����� ��������� ��������
        foreach (var renderer in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (renderer.name.Contains("Eyes") && renderer.gameObject.activeSelf && renderer.name.Contains(characterName))
            {
                renderer.sprite = null; // ������� ������ ��������
                renderer.gameObject.SetActive(false); // ��������� ������ ����
                Debug.Log($"[StopBlinking] ����� ������ ��� {characterName}");
            }
        }
        globalCoroutineCount--;
    }




    public void StopAllBlinking()
    {
        foreach (var coroutine in blinkingCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        globalCoroutineCount = 0;
        blinkingCoroutines.Clear();
    }

    private static int globalCoroutineCount = 0;

    public static int GetGlobalCoroutineCount()
    {
        return globalCoroutineCount;
    }

    private Dictionary<string, bool> shouldBlink = new Dictionary<string, bool>();

    private IEnumerator BlinkCoroutine(SpriteRenderer eyesImage, string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogWarning("[BlinkCoroutine] Character name is null or empty. Exiting coroutine.");
            yield break;
        }

        shouldBlink[characterName] = true; // ��������� ��������

        while (shouldBlink.ContainsKey(characterName) && shouldBlink[characterName])
        {
            Sprite closedEyesSprite = Resources.Load<Sprite>($"Characters/{characterName}/{characterName}_ClosedEyes");
            if (closedEyesSprite == null)
            {
                Debug.LogWarning($"[BlinkCoroutine] Closed eyes sprite for {characterName} not found.");
                yield return new WaitForSeconds(1f);
                continue;
            }

            // ����� �������� ����
            eyesImage.sprite = closedEyesSprite;
            eyesImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);

            // ������� ����
            eyesImage.gameObject.SetActive(false);
            yield return new WaitForSeconds(Random.Range(5f, 7f));
        }

        Debug.Log($"[BlinkCoroutine] �������� ��������� ��� {characterName}");
    }



}