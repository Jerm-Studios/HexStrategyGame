using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [Header("Slider References")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider uiVolumeSlider;

    [Header("Text References")]
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    public TextMeshProUGUI uiVolumeText;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found!");
            return;
        }

        // Initialize sliders with current values
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = audioManager.masterVolume;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = audioManager.musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = audioManager.sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (uiVolumeSlider != null)
        {
            uiVolumeSlider.value = audioManager.uiVolume;
            uiVolumeSlider.onValueChanged.AddListener(OnUIVolumeChanged);
        }

        // Update text displays
        UpdateVolumeTexts();
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetMasterVolume(value);
            UpdateVolumeTexts();
            audioManager.PlayUISound("ButtonClick");
        }
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetMusicVolume(value);
            UpdateVolumeTexts();
            audioManager.PlayUISound("ButtonClick");
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(value);
            UpdateVolumeTexts();
            audioManager.PlayUISound("ButtonClick");
        }
    }

    public void OnUIVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetUIVolume(value);
            UpdateVolumeTexts();
            audioManager.PlayUISound("ButtonClick");
        }
    }

    private void UpdateVolumeTexts()
    {
        if (audioManager == null)
            return;

        if (masterVolumeText != null)
            masterVolumeText.text = $"Master: {Mathf.RoundToInt(audioManager.masterVolume * 100)}%";

        if (musicVolumeText != null)
            musicVolumeText.text = $"Music: {Mathf.RoundToInt(audioManager.musicVolume * 100)}%";

        if (sfxVolumeText != null)
            sfxVolumeText.text = $"SFX: {Mathf.RoundToInt(audioManager.sfxVolume * 100)}%";

        if (uiVolumeText != null)
            uiVolumeText.text = $"UI: {Mathf.RoundToInt(audioManager.uiVolume * 100)}%";
    }
}
    