using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopLevelDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Text levelText;
    [SerializeField] private Image expBarFill;
    [SerializeField] private Text expText;
    [SerializeField] private Animator levelUpAnimator;

    [Header("Settings")]
    [SerializeField] private float barAnimationDuration = 0.5f;
    [SerializeField] private bool showLevelNumber = true;
    [SerializeField] private bool showExpText = true;

    [Header("Level Up Popup")]
    [SerializeField] private GameObject levelUpPopup;
    [SerializeField] private Text levelUpLevelText;
    [SerializeField] private float popupDisplayDuration = 2f;
    [SerializeField] private bool playSoundOnLevelUp = true;

    private Coroutine currentBarAnimation;
    private float targetExpPercentage;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        UpdateDisplay();
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener<WorkshopLevelData>(GameEventNames.OnWorkshopLevelUpdated, OnLevelUpdated);
        EventManager.Instance.AddListener<int>(GameEventNames.OnWorkshopLevelUp, OnLevelUp);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<WorkshopLevelData>(GameEventNames.OnWorkshopLevelUpdated, OnLevelUpdated);
        EventManager.Instance.RemoveListener<int>(GameEventNames.OnWorkshopLevelUp, OnLevelUp);
    }

    private void InitializeComponents()
    {
        if (levelText == null)
        {
            GameObject textObj = new GameObject("LevelText");
            textObj.transform.SetParent(transform);
            levelText = textObj.AddComponent<Text>();
            levelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            levelText.fontSize = 24;
            levelText.fontStyle = FontStyle.Bold;
            levelText.alignment = TextAnchor.MiddleCenter;
            levelText.color = Color.white;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0.15f, 1f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        if (expBarFill == null)
        {
            GameObject barObj = new GameObject("ExpBar");
            barObj.transform.SetParent(transform);
            RectTransform barRect = barObj.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.16f, 0.6f);
            barRect.anchorMax = new Vector2(0.9f, 0.9f);
            barRect.sizeDelta = Vector2.zero;
            barRect.anchoredPosition = Vector2.zero;

            Image barBg = barObj.AddComponent<Image>();
            barBg.color = new Color(0.2f, 0.2f, 0.2f);

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(barObj.transform);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            expBarFill = fillObj.AddComponent<Image>();
            expBarFill.color = new Color(0.2f, 0.8f, 0.4f);
            expBarFill.fillMethod = Image.FillMethod.Horizontal;
            expBarFill.fillAmount = 0f;
        }

        if (expText == null)
        {
            GameObject textObj = new GameObject("ExpText");
            textObj.transform.SetParent(transform);
            expText = textObj.AddComponent<Text>();
            expText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            expText.fontSize = 16;
            expText.alignment = TextAnchor.MiddleCenter;
            expText.color = new Color(0.8f, 0.8f, 0.8f);

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.16f, 0.3f);
            rect.anchorMax = new Vector2(0.9f, 0.55f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }
    }

    private void UpdateDisplay()
    {
        var levelData = WorkshopLevelManager.Instance.GetLevelData();
        UpdateLevelText(levelData.Level);
        UpdateExpBar(levelData.GetExpPercentage(), false);
        UpdateExpText(levelData.CurrentExp, levelData.ExpToNextLevel);
    }

    private void UpdateLevelText(int level)
    {
        if (levelText != null && showLevelNumber)
        {
            levelText.text = $"Lv.{level}";
        }
    }

    private void UpdateExpBar(float percentage, bool animate = true)
    {
        if (expBarFill == null) return;

        targetExpPercentage = percentage;

        if (animate && gameObject.activeInHierarchy)
        {
            if (currentBarAnimation != null)
            {
                StopCoroutine(currentBarAnimation);
            }
            currentBarAnimation = StartCoroutine(AnimateExpBar());
        }
        else
        {
            expBarFill.fillAmount = percentage;
        }
    }

    private IEnumerator AnimateExpBar()
    {
        float startValue = expBarFill.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < barAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / barAnimationDuration;
            expBarFill.fillAmount = Mathf.Lerp(startValue, targetExpPercentage, t);
            yield return null;
        }

        expBarFill.fillAmount = targetExpPercentage;
        currentBarAnimation = null;
    }

    private void UpdateExpText(long currentExp, long expToNext)
    {
        if (expText != null && showExpText)
        {
            expText.text = $"{currentExp} / {expToNext}";
        }
    }

    private void OnLevelUpdated(WorkshopLevelData data)
    {
        UpdateLevelText(data.Level);
        UpdateExpBar(data.GetExpPercentage());
        UpdateExpText(data.CurrentExp, data.ExpToNextLevel);
    }

    private void OnLevelUp(int newLevel)
    {
        ShowLevelUpPopup(newLevel);
        PlayLevelUpAnimation();
        
        if (playSoundOnLevelUp)
        {
            PlayLevelUpSound();
        }
    }

    private void ShowLevelUpPopup(int newLevel)
    {
        if (levelUpPopup == null)
        {
            CreateLevelUpPopup();
        }

        if (levelUpLevelText != null)
        {
            levelUpLevelText.text = $"工坊升级到 Lv.{newLevel}！";
        }

        levelUpPopup.SetActive(true);
        StartCoroutine(HideLevelUpPopupAfterDelay());
    }

    private void CreateLevelUpPopup()
    {
        levelUpPopup = new GameObject("LevelUpPopup");
        levelUpPopup.transform.SetParent(transform.root);
        levelUpPopup.SetActive(false);

        RectTransform popupRect = levelUpPopup.AddComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.3f, 0.4f);
        popupRect.anchorMax = new Vector2(0.7f, 0.6f);
        popupRect.sizeDelta = Vector2.zero;
        popupRect.anchoredPosition = Vector2.zero;

        Image bgImage = levelUpPopup.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.3f, 0.9f);

        GameObject textObj = new GameObject("LevelUpText");
        textObj.transform.SetParent(levelUpPopup.transform);
        levelUpLevelText = textObj.AddComponent<Text>();
        levelUpLevelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        levelUpLevelText.fontSize = 32;
        levelUpLevelText.fontStyle = FontStyle.Bold;
        levelUpLevelText.alignment = TextAnchor.MiddleCenter;
        levelUpLevelText.color = Color.yellow;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        levelUpPopup.AddComponent<CanvasRenderer>();
    }

    private IEnumerator HideLevelUpPopupAfterDelay()
    {
        yield return new WaitForSeconds(popupDisplayDuration);
        levelUpPopup?.SetActive(false);
    }

    private void PlayLevelUpAnimation()
    {
        if (levelUpAnimator != null)
        {
            levelUpAnimator.SetTrigger("LevelUp");
        }
        else
        {
            StartCoroutine(ScaleLevelTextAnimation());
        }
    }

    private IEnumerator ScaleLevelTextAnimation()
    {
        if (levelText == null) yield break;

        Vector3 originalScale = levelText.transform.localScale;
        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
            levelText.transform.localScale = originalScale * scale;
            yield return null;
        }

        levelText.transform.localScale = originalScale;
    }

    private void PlayLevelUpSound()
    {
    }

    public void Refresh()
    {
        UpdateDisplay();
    }
}
