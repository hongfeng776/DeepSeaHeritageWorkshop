using UnityEngine;
using UnityEngine.UI;

public class CaveExplorationUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Text locationText;
    [SerializeField] private TopResourceBar resourceBar;

    [Header("Settings")]
    [SerializeField] private string locationName = "神秘洞穴";

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        UpdateLocationText();
        resourceBar?.Refresh();
    }

    private void InitializeComponents()
    {
        if (returnButton == null)
        {
            GameObject buttonObj = new GameObject("ReturnButton");
            buttonObj.transform.SetParent(transform);
            returnButton = buttonObj.AddComponent<Button>();
            
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.95f);
            rect.anchorMax = new Vector2(0.12f, 0.99f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = "返回工坊";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 18;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.4f, 0.6f);
        }

        returnButton?.onClick.AddListener(OnReturnButtonClicked);
    }

    private void UpdateLocationText()
    {
        if (locationText != null)
        {
            locationText.text = locationName;
        }
    }

    private void OnReturnButtonClicked()
    {
        Debug.Log("正在返回主界面...");
        SceneLoader.Instance.ReturnToMainScene();
    }

    private void OnDestroy()
    {
        returnButton?.onClick.RemoveAllListeners();
    }
}
