using UnityEngine;
using UnityEngine.UI;

public class SliderGradient : MonoBehaviour
{
    public Slider slider;
    public Image fillImage;
    public Color minColor = Color.red;
    public Color maxColor = Color.green;

    void Start()
    {
        slider.onValueChanged.AddListener(UpdateColor);
        UpdateColor(slider.value); // Обновляем цвет при старте
    }

    void UpdateColor(float value)
    {
        float normalizedValue = (value - slider.minValue) / (slider.maxValue - slider.minValue);
        fillImage.color = Color.Lerp(minColor, maxColor, normalizedValue);
    }
}
