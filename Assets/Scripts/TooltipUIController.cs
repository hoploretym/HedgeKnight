using TMPro;
using UnityEngine;

public class TooltipUIController : MonoBehaviour
{
    public static TooltipUIController Instance;

    public RectTransform tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private bool isShowing = false;
    private Vector3 offset = new Vector3(20f, -20f, 0f);

    void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
        if (isShowing)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition + new Vector3(20f, -20f, 100f)
            );
            tooltipPanel.position = Vector3.Lerp(
                tooltipPanel.position,
                worldPos,
                15f * Time.deltaTime
            );
        }
    }

    public void Show(string text)
    {
        tooltipText.text = text;
        tooltipPanel.gameObject.SetActive(true);
        isShowing = true;
        tooltipPanel.position = new Vector3(300, 300, 0); // Центр экрана
    }

    public void Hide()
    {
        tooltipPanel.gameObject.SetActive(false);
        isShowing = false;
    }
}
