using UnityEngine;
using UnityEngine.UI;

public class SettingsWindow : UIWindowBase
{
    [Header("Settings Elements")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button closeButton;

    protected override void Awake()
    {
        base.Awake();
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        musicVolumeSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider?.onValueChanged.AddListener(OnSfxVolumeChanged);
        closeButton?.onClick.AddListener(OnCloseClicked);
    }

    protected override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);
        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.SfxVolume;
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.MusicVolume = value;
    }

    private void OnSfxVolumeChanged(float value)
    {
        AudioManager.Instance.SfxVolume = value;
    }

    private void OnCloseClicked()
    {
        UIManager.Instance.CloseWindow<SettingsWindow>();
    }

    private void OnDestroy()
    {
        musicVolumeSlider?.onValueChanged.RemoveAllListeners();
        sfxVolumeSlider?.onValueChanged.RemoveAllListeners();
        closeButton?.onClick.RemoveAllListeners();
    }
}
