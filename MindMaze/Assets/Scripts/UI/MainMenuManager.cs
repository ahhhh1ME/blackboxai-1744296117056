using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup mainMenuCanvas;
    [SerializeField] private CanvasGroup optionsCanvas;
    [SerializeField] private CanvasGroup creditsCanvas;
    
    [Header("Main Menu Elements")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI versionText;
    
    [Header("Options Menu Elements")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Button backFromOptionsButton;
    
    [Header("Credits Elements")]
    [SerializeField] private ScrollRect creditsScrollRect;
    [SerializeField] private Button backFromCreditsButton;
    
    [Header("Visual Effects")]
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float titlePulseSpeed = 1f;
    [SerializeField] private float titlePulseAmount = 0.1f;
    
    [Header("Audio")]
    [SerializeField] private string backgroundMusicName = "MenuMusic";
    [SerializeField] private string buttonClickSound = "ButtonClick";
    [SerializeField] private string startGameSound = "GameStart";

    private Vector3 originalTitleScale;
    private bool isTransitioning;

    private void Awake()
    {
        InitializeMainMenu();
    }

    private void Start()
    {
        PlayMenuMusic();
        StartCoroutine(AnimateTitleText());
    }

    private void InitializeMainMenu()
    {
        // Store original title scale
        if (titleText != null)
        {
            originalTitleScale = titleText.transform.localScale;
        }

        // Set version text
        if (versionText != null)
        {
            versionText.text = $"Version {Application.version}";
        }

        // Initialize main menu buttons
        if (startGameButton != null) startGameButton.onClick.AddListener(OnStartGameClicked);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (creditsButton != null) creditsButton.onClick.AddListener(OnCreditsClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

        // Initialize options menu elements
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        if (backFromOptionsButton != null) backFromOptionsButton.onClick.AddListener(OnBackFromOptionsClicked);

        // Initialize credits elements
        if (backFromCreditsButton != null) backFromCreditsButton.onClick.AddListener(OnBackFromCreditsClicked);

        // Initialize UI states
        ShowMainMenu();
        HideOptions();
        HideCredits();

        // Load saved settings
        LoadSettings();
    }

    private void LoadSettings()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (musicVolumeSlider != null) musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        if (fullscreenToggle != null) fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (qualityDropdown != null) qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("QualityLevel", qualityDropdown.value);
        PlayerPrefs.Save();
    }

    #region Button Click Handlers

    private void OnStartGameClicked()
    {
        if (isTransitioning) return;
        
        AudioManager.Instance.PlaySFX(startGameSound);
        StartCoroutine(StartGameTransition());
    }

    private void OnOptionsClicked()
    {
        if (isTransitioning) return;
        
        AudioManager.Instance.PlaySFX(buttonClickSound);
        StartCoroutine(TransitionToOptions());
    }

    private void OnCreditsClicked()
    {
        if (isTransitioning) return;
        
        AudioManager.Instance.PlaySFX(buttonClickSound);
        StartCoroutine(TransitionToCredits());
    }

    private void OnQuitClicked()
    {
        if (isTransitioning) return;
        
        AudioManager.Instance.PlaySFX(buttonClickSound);
        StartCoroutine(QuitGameTransition());
    }

    private void OnBackFromOptionsClicked()
    {
        if (isTransitioning) return;
        
        AudioManager.Instance.PlaySFX(buttonClickSound);
        SaveSettings();
        StartCoroutine(TransitionToMainMenu());
    }

    private void OnBackFromCreditsClicked()
    {
        if (isTransitioning) return;
        
        AudioManager.Instance.PlaySFX(buttonClickSound);
        StartCoroutine(TransitionToMainMenu());
    }

    #endregion

    #region Settings Handlers

    private void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }

    private void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void OnQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    #endregion

    #region UI State Management

    private void ShowMainMenu()
    {
        mainMenuCanvas.gameObject.SetActive(true);
        Utilities.SetCanvasGroupState(mainMenuCanvas, true);
    }

    private void HideMainMenu()
    {
        Utilities.SetCanvasGroupState(mainMenuCanvas, false);
    }

    private void ShowOptions()
    {
        optionsCanvas.gameObject.SetActive(true);
        Utilities.SetCanvasGroupState(optionsCanvas, true);
    }

    private void HideOptions()
    {
        Utilities.SetCanvasGroupState(optionsCanvas, false);
    }

    private void ShowCredits()
    {
        creditsCanvas.gameObject.SetActive(true);
        Utilities.SetCanvasGroupState(creditsCanvas, true);
    }

    private void HideCredits()
    {
        Utilities.SetCanvasGroupState(creditsCanvas, false);
    }

    #endregion

    #region Transitions

    private IEnumerator StartGameTransition()
    {
        isTransitioning = true;
        
        // Fade out main menu
        Utilities.FadeCanvasGroup(mainMenuCanvas, 0f, fadeSpeed);
        
        yield return new WaitForSeconds(fadeSpeed);
        
        // Start the game
        GameManager.Instance.StartGame();
        
        isTransitioning = false;
    }

    private IEnumerator TransitionToOptions()
    {
        isTransitioning = true;
        
        // Fade out main menu
        Utilities.FadeCanvasGroup(mainMenuCanvas, 0f, fadeSpeed);
        
        yield return new WaitForSeconds(fadeSpeed);
        
        // Show options
        ShowOptions();
        Utilities.FadeCanvasGroup(optionsCanvas, 1f, fadeSpeed);
        
        yield return new WaitForSeconds(fadeSpeed);
        
        isTransitioning = false;
    }

    private IEnumerator TransitionToCredits()
    {
        isTransitioning = true;
        
        // Fade out main menu
        Utilities.FadeCanvasGroup(mainMenuCanvas, 0f, fadeSpeed);
        
        yield return new WaitForSeconds(fadeSpeed);
        
        // Show credits
        ShowCredits();
        Utilities.FadeCanvasGroup(creditsCanvas, 1f, fadeSpeed);
        
        yield return new WaitForSeconds(fadeSpeed);
        
        isTransitioning = false;
    }

    private IEnumerator TransitionToMainMenu()
    {
        isTransitioning = true;
        
        // Fade out current menu
        if (optionsCanvas.gameObject.activeSelf)
        {
            Utilities.FadeCanvasGroup(optionsCanvas, 0f, fadeSpeed);
        }
        if (creditsCanvas.gameObject.activeSelf)
        {
            Utilities.FadeCanvasGroup(creditsCanvas, 0f, fadeSpeed);
        }
        
        yield return new WaitForSeconds(fadeSpeed);
        
        // Show main menu
        ShowMainMenu();
        Utilities.FadeCanvasGroup(mainMenuCanvas, 1f, fadeSpeed);
        
        yield return new WaitForSeconds(fadeSpeed);
        
        isTransitioning = false;
    }

    private IEnumerator QuitGameTransition()
    {
        isTransitioning = true;
        
        // Fade out everything
        Utilities.FadeCanvasGroup(mainMenuCanvas, 0f, fadeSpeed);
        
        yield return new WaitForSeconds(fadeSpeed);
        
        // Quit the game
        GameManager.Instance.QuitGame();
    }

    #endregion

    #region Visual Effects

    private IEnumerator AnimateTitleText()
    {
        if (titleText == null) yield break;

        while (true)
        {
            // Pulse the title text scale
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * titlePulseSpeed;
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * titlePulseAmount;
                titleText.transform.localScale = originalTitleScale * scale;
                yield return null;
            }
        }
    }

    #endregion

    private void PlayMenuMusic()
    {
        AudioManager.Instance.PlayMusic(backgroundMusicName, true);
    }

    private void OnDestroy()
    {
        // Save settings when destroying the menu
        SaveSettings();
    }
}
