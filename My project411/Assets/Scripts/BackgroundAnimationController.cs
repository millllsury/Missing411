using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundAnimationController : MonoBehaviour
{
    [SerializeField] private Image animationFrame;
    [SerializeField] private float frameDelay = 3f; // ����� ����� �������
    [SerializeField] private int repeatCount = -1;
    [SerializeField] private List<Sprite> animationSprites;

    private Coroutine animationCoroutine;

    public string CurrentAnimation { get; private set; }

    private bool isAnimating = false;
    public bool IsAnimating => isAnimating;

    [SerializeField] private bool keepLastFrame = false;
    void Start()
    {
        if (animationFrame == null)
        {
            animationFrame = GetComponent<Image>();
        }
    }



    public void StartAnimation(List<Sprite> sprites, float delay, string animationName, int repeatCount = -1, bool keepLastFrame = false)
    {
        if (isAnimating)
        {
            Debug.LogWarning("������� ��������� ����� ��������, ���� ���������� ��� �����������.");
            StopAnimation();
        }

        CurrentAnimation = animationName; // ������������� ��� ������� ��������
        animationSprites = sprites;
        frameDelay = delay;
        this.repeatCount = repeatCount;
        this.keepLastFrame = keepLastFrame;

        if (animationSprites != null && animationSprites.Count > 0)
        {
            isAnimating = true;
            animationCoroutine = StartCoroutine(PlayAnimation());
        }
        else
        {
            Debug.LogError("������ �������� ��� �������� ����.");
        }
    }


    public void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        isAnimating = false;
    }

    private IEnumerator PlayAnimation()
    {
        if (animationSprites == null || animationSprites.Count == 0)
        {
            Debug.LogError("������ �������� ��� �������� ����.");
            yield break; // ��������� ���������� ��������, ���� ������ ����
        }

        if (animationFrame == null)
        {
            Debug.LogError("��������� Image �� ������. ���������, ��� �� ��������� � �������.");
            yield break; // ��������� ���������� ��������, ���� animationFrame �� ���������������
        }

        int currentFrame = 0;
        int playedCount = 0; // ������� ����������� ������
        int lastFrameIndex = 0; // ���������� ��� �������� ���������� �����, ������� ����� ��������

        while (repeatCount == -1 || playedCount < repeatCount)
        {
            // ���������� ������� ���� ��������
            animationFrame.sprite = animationSprites[currentFrame];
            //Debug.Log($"Frame {currentFrame}, repeatCount: {repeatCount}, playedCount: {playedCount}");

            // ��������� lastFrameIndex ������ ���, ����� ���������� ����� ����
            lastFrameIndex = currentFrame;

            currentFrame = (currentFrame + 1) % animationSprites.Count;

            if (currentFrame == 0)
            {
                playedCount++; // ����������� ������� ���������� ��� ���������� �����
            }

            yield return new WaitForSeconds(frameDelay);
        }

        // ����� ���������� �����, ��������� ���� keepLastFrame
        if (keepLastFrame)
        {
            Debug.Log($"Animation finished. Showing last frame: {lastFrameIndex} and playedCount: {playedCount}");
            animationFrame.sprite = animationSprites[lastFrameIndex];
        }
        else
        {
            // ��������� ������ �������� ����� ����������, ���� keepLastFrame == false
            Debug.Log("Disabling background animation object.");
            gameObject.SetActive(false);
        }

        isAnimating = false;
        animationCoroutine = null;
    }

}