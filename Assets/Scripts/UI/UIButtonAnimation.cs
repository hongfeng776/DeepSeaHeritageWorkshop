using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonAnimation : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float pressedScale = 0.9f;
    [SerializeField] private float animationDuration = 0.1f;

    [Header("Sound Settings")]
    [SerializeField] private string clickSfxPath = "Audio/click";
    [SerializeField] private bool playSound = true;

    private Button button;
    private Vector3 originalScale;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        PlayClickAnimation();
        PlayClickSound();
    }

    private void PlayClickAnimation()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(ScaleAnimation());
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
        currentCoroutine = null;
    }

    private void PlayClickSound()
    {
        if (playSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(clickSfxPath);
        }
    }
}
