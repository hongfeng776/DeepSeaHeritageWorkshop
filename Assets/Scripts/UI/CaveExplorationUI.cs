using UnityEngine;
using UnityEngine.UI;

public class CaveExplorationUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Text locationText;
    [SerializeField] private Text oxygenText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text healthText;
    [SerializeField] private TopResourceBar resourceBar;
    [SerializeField] private Image oxygenFillImage;
    [SerializeField] private Image healthFillImage;

    [Header("Settings")]
    [SerializeField] private string locationName = "神秘洞穴";
    [SerializeField] private Color oxygenFullColor = new Color(0.2f, 0.8f, 0.4f);
    [SerializeField] private Color oxygenLowColor = new Color(0.9f, 0.3f, 0.2f);
    [SerializeField] private Color healthFullColor = new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] private Color healthLowColor = new Color(0.6f, 0.1f, 0.1f);

    private float updateInterval = 0.5f;
    private float lastUpdateTime;

    private void Awake()
    {
        InitializeComponents();
        InitializeEventListeners();
    }

    private void Start()
    {
        UpdateLocationText();
        resourceBar?.Refresh();
        UpdateUI();
    }

    private void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateUI();
            lastUpdateTime = Time.time;
        }
    }

    private void OnDestroy()
    {
        RemoveEventListeners();
    }

    private void InitializeComponents()
    {
        if (returnButton == null)
        {
            CreateReturnButton();
        }

        if (healthText == null)
        {
            CreateHealthDisplay();
        }

        if (oxygenText == null)
        {
            CreateOxygenDisplay();
        }

        if (timeText == null)
        {
            CreateTimeDisplay();
        }

        if (locationText == null)
        {
            CreateLocationText();
        }

        returnButton?.onClick.AddListener(OnReturnButtonClicked);
    }

    private void CreateHealthDisplay()
    {
        GameObject healthObj = new GameObject("HealthDisplay");
        healthObj.transform.SetParent(transform);

        RectTransform rect = healthObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.945f);
        rect.anchorMax = new Vector2(0.25f, 0.985f);
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(healthObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(healthObj.transform);
        healthFillImage = fillObj.AddComponent<Image>();
        healthFillImage.color = healthFullColor;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(healthObj.transform);
        healthText = textObj.AddComponent<Text>();
        healthText.text = "生命: 100/100";
        healthText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        healthText.fontSize = 14;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    private void CreateReturnButton()
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

    private void CreateOxygenDisplay()
    {
        GameObject oxygenObj = new GameObject("OxygenDisplay");
        oxygenObj.transform.SetParent(transform);

        RectTransform rect = oxygenObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.88f);
        rect.anchorMax = new Vector2(0.25f, 0.94f);
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(oxygenObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(oxygenObj.transform);
        oxygenFillImage = fillObj.AddComponent<Image>();
        oxygenFillImage.color = oxygenFullColor;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(oxygenObj.transform);
        oxygenText = textObj.AddComponent<Text>();
        oxygenText.text = "氧气: 100%";
        oxygenText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        oxygenText.fontSize = 16;
        oxygenText.alignment = TextAnchor.MiddleCenter;
        oxygenText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    private void CreateTimeDisplay()
    {
        GameObject timeObj = new GameObject("TimeDisplay");
        timeObj.transform.SetParent(transform);

        RectTransform rect = timeObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.82f);
        rect.anchorMax = new Vector2(0.15f, 0.87f);
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        timeText = timeObj.AddComponent<Text>();
        timeText.text = "时间: 00:00";
        timeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        timeText.fontSize = 16;
        timeText.alignment = TextAnchor.MiddleLeft;
        timeText.color = Color.white;
    }

    private void CreateLocationText()
    {
        GameObject locationObj = new GameObject("LocationText");
        locationObj.transform.SetParent(transform);

        RectTransform rect = locationObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.95f);
        rect.anchorMax = new Vector2(0.5f, 0.99f);
        rect.sizeDelta = new Vector2(200f, 0f);
        rect.anchoredPosition = Vector2.zero;

        locationText = locationObj.AddComponent<Text>();
        locationText.text = locationName;
        locationText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        locationText.fontSize = 20;
        locationText.alignment = TextAnchor.MiddleCenter;
        locationText.color = new Color(0.9f, 0.85f, 0.7f);
    }

    private void InitializeEventListeners()
    {
        EventManager.Instance.AddListener<int>(GameEventNames.OnCaveOxygenChanged, OnOxygenChanged);
    }

    private void RemoveEventListeners()
    {
        EventManager.Instance.RemoveListener<int>(GameEventNames.OnCaveOxygenChanged, OnOxygenChanged);
        returnButton?.onClick.RemoveAllListeners();
    }

    private void UpdateLocationText()
    {
        if (locationText != null)
        {
            locationText.text = locationName;
        }
    }

    private void UpdateUI()
    {
        if (CaveExplorationManager.Instance == null) return;

        UpdateOxygenDisplay(CaveExplorationManager.Instance.CurrentOxygen);
        UpdateTimeDisplay(CaveExplorationManager.Instance.ExplorationTime);
        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            int currentHealth = playerHealth.CurrentHealth;
            int maxHealth = playerHealth.MaxHealth;

            if (healthText != null)
            {
                healthText.text = $"生命: {currentHealth}/{maxHealth}";
            }

            if (healthFillImage != null)
            {
                float fillAmount = (float)currentHealth / maxHealth;
                healthFillImage.fillAmount = fillAmount;
                healthFillImage.color = Color.Lerp(healthLowColor, healthFullColor, fillAmount);
            }
        }
    }

    private void UpdateOxygenDisplay(int oxygen)
    {
        if (oxygenText != null)
        {
            oxygenText.text = $"氧气: {oxygen}%";
        }

        if (oxygenFillImage != null)
        {
            float fillAmount = oxygen / 100f;
            oxygenFillImage.fillAmount = fillAmount;
            oxygenFillImage.color = Color.Lerp(oxygenLowColor, oxygenFullColor, fillAmount);
        }
    }

    private void UpdateTimeDisplay(float time)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timeText.text = $"时间: {minutes:00}:{seconds:00}";
        }
    }

    private void OnOxygenChanged(int oxygen)
    {
        UpdateOxygenDisplay(oxygen);
    }

    private void OnReturnButtonClicked()
    {
        Debug.Log("正在返回主界面...");
        CaveExplorationManager.Instance?.ExitExploration();
    }
}
