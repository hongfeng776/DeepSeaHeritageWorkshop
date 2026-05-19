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

    private void Awake()
    {
        InitializeButtons();
    }

    private void Start()
    {
        SelectButton(FunctionButtonType.Workshop);
    }

    private void InitializeButtons()
    {
        if (functionButtonPrefab == null || buttonContainer == null)
        {
            Debug.LogWarning("FunctionButton prefab or container not set!");
            return;
        }

        foreach (var config in buttonConfigs)
        {
            FunctionButton button = Instantiate(functionButtonPrefab, buttonContainer);
            button.Initialize(config, OnFunctionButtonClicked);
        }
    }

    private void OnFunctionButtonClicked(FunctionButtonType type)
    {
        if (currentSelectedType == type)
        {
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

    public FunctionButtonType ButtonType { get; private set; }
    private Action<FunctionButtonType> clickCallback;

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

        button?.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        clickCallback?.Invoke(ButtonType);
        animator?.SetTrigger("Click");
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
