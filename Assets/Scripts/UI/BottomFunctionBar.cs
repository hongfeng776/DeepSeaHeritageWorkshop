using UnityEngine;
using UnityEngine.UI;
using System;

public enum FunctionButtonType
{
    Workshop,
    Explore,
    Inventory,
    Mission,
    Shop,
    Settings
}

[Serializable]
public class FunctionButtonConfig
{
    public FunctionButtonType buttonType;
    public string buttonName;
    public Sprite icon;
    public bool isHighlighted;
}

public class BottomFunctionBar : MonoBehaviour
{
    [Header("Button Prefab")]
    [SerializeField] private FunctionButton functionButtonPrefab;

    [Header("Container")]
    [SerializeField] private Transform buttonContainer;

    [Header("Button Configs")]
    [SerializeField] private FunctionButtonConfig[] buttonConfigs;

    public event Action<FunctionButtonType> OnButtonClicked;

    private FunctionButtonType currentSelectedType = FunctionButtonType.Workshop;
    private bool isInitialized = false;

    private void Start()
    {
        if (!isInitialized && buttonConfigs != null && buttonConfigs.Length > 0)
        {
            InitializeButtons();
        }
        SelectButton(FunctionButtonType.Workshop);
    }

    public void SetConfigsAndInitialize(FunctionButtonConfig[] configs)
    {
        buttonConfigs = configs;
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        if (functionButtonPrefab == null || buttonContainer == null)
        {
            Debug.LogWarning("FunctionButton prefab or container not set!");
            return;
        }

        if (buttonConfigs == null || buttonConfigs.Length == 0)
        {
            Debug.LogWarning("ButtonConfigs is empty!");
            return;
        }

        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var config in buttonConfigs)
        {
            FunctionButton button = Instantiate(functionButtonPrefab, buttonContainer);
            button.Initialize(config, OnFunctionButtonClicked);
        }

        isInitialized = true;
        Debug.Log($"BottomFunctionBar initialized with {buttonConfigs.Length} buttons!");
    }

    private void OnFunctionButtonClicked(FunctionButtonType type)
    {
        Debug.Log($"Button clicked: {type}");
        
        if (currentSelectedType == type)
        {
            OnButtonClicked?.Invoke(type);
            return;
        }

        SelectButton(type);
        OnButtonClicked?.Invoke(type);
    }

    private void SelectButton(FunctionButtonType type)
    {
        currentSelectedType = type;
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        FunctionButton[] buttons = buttonContainer.GetComponentsInChildren<FunctionButton>();
        foreach (var button in buttons)
        {
            button.SetSelected(button.ButtonType == currentSelectedType);
        }
    }

    public void SetButtonHighlight(FunctionButtonType type, bool highlight)
    {
        FunctionButton[] buttons = buttonContainer.GetComponentsInChildren<FunctionButton>();
        foreach (var button in buttons)
        {
            if (button.ButtonType == type)
            {
                button.SetHighlight(highlight);
                break;
            }
        }
    }

    public void SetButtonInteractable(FunctionButtonType type, bool interactable)
    {
        FunctionButton[] buttons = buttonContainer.GetComponentsInChildren<FunctionButton>();
        foreach (var button in buttons)
        {
            if (button.ButtonType == type)
            {
                button.SetInteractable(interactable);
                break;
            }
        }
    }
}

public class FunctionButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Image selectedIndicator;
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private Animator animator;

    [Header("Animation Settings")]
    [SerializeField] private float pressedScale = 0.9f;
    [SerializeField] private float animationDuration = 0.15f;
    [SerializeField] private string clickSfxPath = "Audio/click";
    [SerializeField] private bool playSound = true;

    public FunctionButtonType ButtonType { get; private set; }
    private Action<FunctionButtonType> clickCallback;
    private Vector3 originalScale;
    private Coroutine currentAnimation;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        if (button == null)
        {
            button = GetComponentInChildren<Button>();
        }
        if (nameText == null)
        {
            nameText = GetComponentInChildren<Text>();
        }

        originalScale = transform.localScale;
    }

    public void Initialize(FunctionButtonConfig config, Action<FunctionButtonType> callback)
    {
        ButtonType = config.buttonType;
        clickCallback = callback;

        if (nameText != null)
        {
            nameText.text = config.buttonName;
        }

        if (iconImage != null && config.icon != null)
        {
            iconImage.sprite = config.icon;
        }

        SetHighlight(config.isHighlighted);
        SetSelected(false);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
        else
        {
            Debug.LogWarning($"FunctionButton {config.buttonName} has no Button component!");
        }
    }

    private void OnClick()
    {
        PlayClickAnimation();
        PlayClickSound();
        clickCallback?.Invoke(ButtonType);
        animator?.SetTrigger("Click");
    }

    private void PlayClickAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(ScaleAnimation());
    }

    private System.Collections.IEnumerator ScaleAnimation()
    {
        float elapsedTime = 0f;
        transform.localScale = originalScale * pressedScale;

        yield return new WaitForSeconds(animationDuration * 0.5f);

        elapsedTime = 0f;
        while (elapsedTime < animationDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (animationDuration * 0.5f);
            transform.localScale = Vector3.Lerp(originalScale * pressedScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        currentAnimation = null;
    }

    private void PlayClickSound()
    {
        if (playSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(clickSfxPath);
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
        {
            selectedIndicator.gameObject.SetActive(selected);
        }

        if (animator != null)
        {
            animator.SetBool("Selected", selected);
        }
    }

    public void SetHighlight(bool highlight)
    {
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(highlight);
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    private void OnDestroy()
    {
        button?.onClick.RemoveAllListeners();
    }
}
