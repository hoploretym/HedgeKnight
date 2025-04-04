using TMPro;
using UnityEngine;

public class TooltipUIController : MonoBehaviour
{
    public static TooltipUIController Instance;

    public RectTransform tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private bool isShowing = false;
    private Vector2 offset = new Vector2(20f, 30f);

    void Awake()
    {
        Instance = this;
        Hide();

        // Убедимся, что CanvasGroup настроен
        CanvasGroup cg = tooltipPanel.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = tooltipPanel.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 1f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    void Update()
    {
        if (isShowing)
        {
            Vector2 tooltipSize = tooltipPanel.sizeDelta;
            Vector3 mousePos = Input.mousePosition + (Vector3)offset;

            float padding = 10f;
            float clampedX = Mathf.Clamp(
                mousePos.x,
                padding,
                Screen.width - tooltipSize.x - padding
            );
            float clampedY = Mathf.Clamp(
                mousePos.y,
                padding,
                Screen.height - tooltipSize.y - padding
            );

            tooltipPanel.position = new Vector2(clampedX, clampedY);
        }
    }

    public void Show(string text)
    {
        tooltipText.text = text;
        tooltipPanel.gameObject.SetActive(true);
        isShowing = true;
    }

    public void Hide()
    {
        tooltipPanel.gameObject.SetActive(false);
        isShowing = false;
    }
}
