using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InteractionTooltipUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GameObject container;
    [Header("Styling")]
    [SerializeField] private Color keyColor    = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Color actionColor = Color.white;
    private void Awake()
    {
        if (container == null) container = gameObject;
        Hide();
    }
    public void Show(string keyHint, string actionText)
    {
        if (label != null)
        {
            string keyHex    = ColorUtility.ToHtmlStringRGB(keyColor);
            string actionHex = ColorUtility.ToHtmlStringRGB(actionColor);
            label.text = $"<color=#{keyHex}>Press {keyHint}</color> <color=#{actionHex}>to {actionText}</color>";
        }
        container.SetActive(true);
    }
    public void Hide()
    {
        if(container != null)
            container.SetActive(false);
    }
    public static InteractionTooltipUI CreateDefault()
    {
        var canvasGo = new GameObject("InteractionTooltipCanvas");
        DontDestroyOnLoad(canvasGo);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();
        var panelGo   = new GameObject("TooltipPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        var panelImg  = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.55f);
        var panelRect  = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(0.5f, 0.15f);
        panelRect.anchorMax        = new Vector2(0.5f, 0.15f);
        panelRect.pivot            = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta        = new Vector2(420f, 48f);
        panelRect.anchoredPosition = Vector2.zero;
        var textGo = new GameObject("TooltipText");
        textGo.transform.SetParent(panelGo.transform, false);
        var tmp       = textGo.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = 22f;
        var textRect  = tmp.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);
        var tooltip    = panelGo.AddComponent<InteractionTooltipUI>();
        tooltip.label     = tmp;
        tooltip.container = panelGo;
        panelGo.SetActive(false);
        return tooltip;
    }
}
