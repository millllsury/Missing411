using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using System.Collections.Generic;

public class ScreenRipple : MonoBehaviour
{
    public PostProcessVolume postProcessVolume;

    private Grain grain;
    private DepthOfField depthOfField;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private bool isEffectActive = false;
    private bool effectApplied = false;

    // Настройки для плавности
    public float transitionSpeed = 2f;

    void Start()
    {
        if (postProcessVolume.profile.TryGetSettings(out grain) &&
            postProcessVolume.profile.TryGetSettings(out depthOfField) &&
            postProcessVolume.profile.TryGetSettings(out chromaticAberration) &&
            postProcessVolume.profile.TryGetSettings(out lensDistortion))
        {
            // Изначально эффекты выключены
            lensDistortion.intensity.value = 0f;
            lensDistortion.scale.value= 1f;

            grain.intensity.value = 0f;
            grain.size.value = 1f;
            grain.lumContrib.value = 0.8f;
            depthOfField.focusDistance.value = 10f;
            depthOfField.aperture.value = 5.6f;
            depthOfField.focalLength.value = 50f;
            chromaticAberration.intensity.value = 0f;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isEffectActive)
                StopEffect();
            else
                StartEffect();
        }


        if (isEffectActive && !effectApplied)
        {
            ApplyEffect();
        }
        else if (!isEffectActive && effectApplied)
        {
            RemoveEffect();
        }
    }

    // Плавное включение эффекта
    private void ApplyEffect()
    {

        grain.intensity.value = Mathf.MoveTowards(grain.intensity.value, 1f, Time.deltaTime * transitionSpeed);
        grain.size.value = Mathf.MoveTowards(grain.size.value, 3f, Time.deltaTime * transitionSpeed);
        grain.lumContrib.value = Mathf.MoveTowards(grain.lumContrib.value, 0.4f, Time.deltaTime * transitionSpeed);
        depthOfField.focusDistance.value = Mathf.MoveTowards(depthOfField.focusDistance.value, 10f, Time.deltaTime * transitionSpeed);
        depthOfField.aperture.value = Mathf.MoveTowards(depthOfField.aperture.value, 30f, Time.deltaTime * transitionSpeed);
        depthOfField.focalLength.value = Mathf.MoveTowards(depthOfField.focalLength.value, 50f, Time.deltaTime * transitionSpeed);
        chromaticAberration.intensity.value = Mathf.MoveTowards(chromaticAberration.intensity.value, 1f, Time.deltaTime * transitionSpeed);

        if (grain.intensity.value == 1f && grain.size.value == 3f && grain.lumContrib.value == 0.4f &&
            depthOfField.focusDistance.value == 10f && depthOfField.aperture.value == 30f && depthOfField.focalLength.value == 50f &&
            chromaticAberration.intensity.value == 1f)
        {
            effectApplied = true;
        }
    }

    // Плавное отключение эффекта
    private void RemoveEffect()
    {
        grain.intensity.value = Mathf.MoveTowards(grain.intensity.value, 0f, Time.deltaTime * transitionSpeed);
        grain.size.value = Mathf.MoveTowards(grain.size.value, 0f, Time.deltaTime * transitionSpeed);
        grain.lumContrib.value = Mathf.MoveTowards(grain.lumContrib.value, 0f, Time.deltaTime * transitionSpeed);
        depthOfField.focusDistance.value = Mathf.MoveTowards(depthOfField.focusDistance.value, 0f, Time.deltaTime * transitionSpeed);
        depthOfField.aperture.value = Mathf.MoveTowards(depthOfField.aperture.value, 0f, Time.deltaTime * transitionSpeed);
        depthOfField.focalLength.value = Mathf.MoveTowards(depthOfField.focalLength.value, 0f, Time.deltaTime * transitionSpeed);
        chromaticAberration.intensity.value = Mathf.MoveTowards(chromaticAberration.intensity.value, 0f, Time.deltaTime * transitionSpeed);

        if (grain.intensity.value == 0f && grain.size.value == 0f && grain.lumContrib.value == 0f &&
            depthOfField.focusDistance.value == 0f && depthOfField.aperture.value == 0f && depthOfField.focalLength.value == 0f &&
            chromaticAberration.intensity.value == 0f)
        {
            effectApplied = false;
        }
    }

    // Включение эффекта
    public void StartEffect()
    {
        isEffectActive = true;
        SoundManager.Instance.PlaySoundByName("interference");
        StartCoroutine(RepeatLensDistortion(2));
    }
    private IEnumerator RepeatLensDistortion(int repeatCount)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            yield return StartCoroutine(TemporaryLensDistortion());
        }
    }

    private IEnumerator TemporaryLensDistortion()
    {
        
        float[] intensityValues = { -100f, 0f, -100f, 8f };
        float[] scaleValues = { 0.01f, 0.5f, 1.8f, 1f };

        float duration = 0.05f; // Очень быстрые переходы

        // Переходы для Intensity
        for (int i = 0; i < intensityValues.Length; i++)
        {
            float startIntensity = lensDistortion.intensity.value;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                lensDistortion.intensity.value = Mathf.Lerp(startIntensity, intensityValues[i], elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            lensDistortion.intensity.value = intensityValues[i];
        }

        // Переходы для Scale
        for (int i = 0; i < scaleValues.Length; i++)
        {
            float startScale = lensDistortion.scale.value;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                lensDistortion.scale.value = Mathf.Lerp(startScale, scaleValues[i], elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            lensDistortion.scale.value = scaleValues[i];
        }

        // Плавное возвращение к нормальному состоянию
        float fadeDuration = 0.1f;
        float fadeElapsed = 0f;
        float initialIntensity = lensDistortion.intensity.value;
        float initialScale = lensDistortion.scale.value;

        while (fadeElapsed < fadeDuration)
        {
            lensDistortion.intensity.value = Mathf.Lerp(initialIntensity, 0f, fadeElapsed / fadeDuration);
            lensDistortion.scale.value = Mathf.Lerp(initialScale, 1f, fadeElapsed / fadeDuration);
            fadeElapsed += Time.deltaTime;
            yield return null;
        }

        lensDistortion.intensity.value = 0f;
        lensDistortion.scale.value = 1f;
    }
    // Отключение эффекта
    public void StopEffect()
    {
        isEffectActive = false;
    }

    // Полный сброс эффекта
    public void ResetEffect()
    {
        grain.intensity.value = 0f;
        grain.size.value = 0f;
        grain.lumContrib.value = 0f;
        depthOfField.focusDistance.value = 0f;
        depthOfField.aperture.value = 0f;
        depthOfField.focalLength.value = 0f;
        chromaticAberration.intensity.value = 0f;
        isEffectActive = false;
        effectApplied = false;
    }
}
