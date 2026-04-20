using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class LowResScaler : MonoBehaviour
{
    public int targetWidth = 320;
    public int targetHeight = 180;

    private RawImage img;
    private RectTransform rect;

    void Awake()
    {
        img = GetComponent<RawImage>();
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = (float)targetWidth / targetHeight;

        if (screenRatio > targetRatio)
        {
            float scale = (float)Screen.height / targetHeight;
            float width = targetWidth * scale;
            rect.sizeDelta = new Vector2(width, Screen.height);
        }
        else
        {
            float scale = (float)Screen.width / targetWidth;
            float height = targetHeight * scale;
            rect.sizeDelta = new Vector2(Screen.width, height);
        }
    }
}