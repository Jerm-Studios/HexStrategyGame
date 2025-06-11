using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;

    [Header("Music Clips")]
    public AudioClip mainThemeMusic;
    public AudioClip battleMusic;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;

    [Header("UI Sound Effects")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    public AudioClip menuOpenSound;
    public AudioClip menuCloseSound;

    [Header("Unit Sound Effects")]
    public AudioClip unitSelectSound;
    public AudioClip unitMoveSound;
    public AudioClip unitAttackSound;
    public AudioClip unitDieSound;

    [Header("Game Sound Effects")]
    public AudioClip turnStartSound;
    public AudioClip turnEndSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;

    [Header("Volume Settings")]
    [Range(0, 1)] public float masterVolume = 1.0f;
    [Range(0, 1)] public float musicVolume = 0.7f;
    [Range(0, 1)] public float sfxVolume = 0.8f;
    [Range(0, 1)] public float uiVolume = 0.6f;

    // Singleton pattern
    public static AudioManager Instance { get; private set; }

    // Dictionary to store sound effects by name
    private Dictionary<string, AudioClip> soundEffects = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize audio sources if not set
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.parent = transform;
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.parent = transform;
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        if (uiSource == null)
        {
            GameObject uiObj = new GameObject("UISource");
            uiObj.transform.parent = transform;
            uiSource = uiObj.AddComponent<AudioSource>();
            uiSource.loop = false;
            uiSource.playOnAwake = false;
        }

        // Register all sound effects
        RegisterSoundEffects();

        // Apply initial volume settings
        UpdateVolumes();
    }

    private void RegisterSoundEffects()
    {
        // UI sounds
        RegisterSound("ButtonClick", buttonClickSound);
        RegisterSound("ButtonHover", buttonHoverSound);
        RegisterSound("MenuOpen", menuOpenSound);
        RegisterSound("MenuClose", menuCloseSound);

        // Unit sounds
        RegisterSound("UnitSelect", unitSelectSound);
        RegisterSound("UnitMove", unitMoveSound);
        RegisterSound("UnitAttack", unitAttackSound);
        RegisterSound("UnitDie", unitDieSound);

        // Game sounds
        RegisterSound("TurnStart", turnStartSound);
        RegisterSound("TurnEnd", turnEndSound);
        RegisterSound("Victory", victorySound);
        RegisterSound("Defeat", defeatSound);
    }

    private void RegisterSound(string name, AudioClip clip)
    {
        if (clip != null)
        {
            soundEffects[name] = clip;
        }
    }

    public void Initialize()
    {
        Debug.Log("AudioManager initialized");
        PlayMusic(mainThemeMusic);
    }

    public void PlayMusic(AudioClip music)
    {
        if (music == null || musicSource == null)
            return;

        StartCoroutine(CrossFadeMusic(music, 1.0f));
    }

    private IEnumerator CrossFadeMusic(AudioClip newMusic, float fadeDuration)
    {
        // If music is already playing, fade it out
        float startVolume = musicSource.volume;

        if (musicSource.isPlaying)
        {
            // Fade out current music
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
                yield return null;
            }
        }

        // Change clip and start new music
        musicSource.clip = newMusic;
        musicSource.Play();

        // Fade in new music
        float timer2 = 0;
        while (timer2 < fadeDuration)
        {
            timer2 += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, musicVolume * masterVolume, timer2 / fadeDuration);
            yield return null;
        }

        // Ensure final volume is correct
        musicSource.volume = musicVolume * masterVolume;
    }

    public void PlaySFX(string soundName)
    {
        if (!soundEffects.ContainsKey(soundName) || sfxSource == null)
            return;

        sfxSource.PlayOneShot(soundEffects[soundName], sfxVolume * masterVolume);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
    }

    public void PlayUISound(string soundName)
    {
        if (!soundEffects.ContainsKey(soundName) || uiSource == null)
            return;

        uiSource.PlayOneShot(soundEffects[soundName], uiVolume * masterVolume);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;

        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;

        if (uiSource != null)
            uiSource.volume = uiVolume * masterVolume;
    }

    public void PlayGameStateMusic(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.PlayerTurn:
            case GameManager.GameState.EnemyTurn:
                PlayMusic(battleMusic);
                break;
            case GameManager.GameState.Victory:
                PlayMusic(victoryMusic);
                PlaySFX("Victory");
                break;
            case GameManager.GameState.Defeat:
                PlayMusic(defeatMusic);
                PlaySFX("Defeat");
                break;
            default:
                PlayMusic(mainThemeMusic);
                break;
        }
    }

    public void PlayTurnSound(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.PlayerTurn:
                PlaySFX("TurnStart");
                break;
            case GameManager.GameState.EnemyTurn:
                PlaySFX("TurnEnd");
                break;
        }
    }
}
