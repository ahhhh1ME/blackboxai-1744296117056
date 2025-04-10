using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameHUDPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject reflectionPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Main Menu Elements")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("Game HUD Elements")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI emotionText;
    [SerializeField] private Image interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Reflection Elements")]
    [SerializeField] private TextMeshProUGUI quoteText;
    [SerializeField] private TextMeshProUGUI authorText;
    [SerializeField] private TextMeshProUGUI lessonText;
    [SerializeField] private Button continueButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float typewriterSpeed = 0.05f;

    // References to canvases for different UI states
    private CanvasGroup mainMenuCanvas;
    private CanvasGroup gameHUDCanvas;
    private CanvasGroup reflectionCanvas;
    private CanvasGroup loadingCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeUI()
    {
        // Get canvas group components
        mainMenuCanvas = mainMenuPanel.GetComponent<CanvasGroup>();
        gameHUDCanvas = gameHUDPanel.GetComponent<CanvasGroup>();
        reflectionCanvas = reflectionPanel.GetComponent<CanvasGroup>();
        loadingCanvas = loadingPanel.GetComponent<CanvasGroup>();

        // Initialize button listeners
        if (startGameButton) startGameButton.onClick.AddListener(OnStartGameClicked);
        if (optionsButton) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (quitButton) quitButton.onClick.AddListener(OnQuitClicked);
        if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);

        // Subscribe to events
        PlayerController.OnInteractableFound += ShowInteractionPrompt;
        PlayerController.OnInteractableLost += HideInteractionPrompt;
        GameManager.OnGameStateChanged += HandleGameStateChange;
        GameManager.OnError += ShowError;

        // Hide all panels initially
        HideAllPanels();
        ShowMainMenu();
    }

    private void HandleGameStateChange(string state)
    {
        switch (state)
        {
            case "GameStarted":
                ShowGameHUD();
                break;
            case "GamePaused":
                ShowPauseMenu();
                break;
            case "GameResumed":
                HidePauseMenu();
                break;
            case "LevelCompleted":
                ShowReflectionPanel();
                break;
        }
    }

    #region Panel Management

    public void ShowMainMenu()
    {
        StartCoroutine(FadeCanvas(mainMenuCanvas, 1f));
        HideGameHUD();
        HideReflectionPanel();
    }

    public void ShowGameHUD()
    {
        StartCoroutine(FadeCanvas(gameHUDCanvas, 1f));
        HideMainMenu();
        HideReflectionPanel();
        UpdateLevelText(GameManager.Instance.currentLevel);
    }

    public void ShowReflectionPanel()
    {
        StartCoroutine(FadeCanvas(reflectionCanvas, 1f));
        HideGameHUD();
    }

    public void ShowLoadingScreen(string message = "Loading...")
    {
        loadingPanel.GetComponentInChildren<TextMeshProUGUI>().text = message;
        StartCoroutine(FadeCanvas(loadingCanvas, 1f));
    }

    private void HideAllPanels()
    {
        mainMenuCanvas.alpha = 0f;
        gameHUDCanvas.alpha = 0f;
        reflectionCanvas.alpha = 0f;
        loadingCanvas.alpha = 0f;

        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(false);
        reflectionPanel.SetActive(false);
        loadingPanel.SetActive(false);
    }

    private void HideMainMenu() => StartCoroutine(FadeCanvas(mainMenuCanvas, 0f));
    private void HideGameHUD() => StartCoroutine(FadeCanvas(gameHUDCanvas, 0f));
    private void HideReflectionPanel() => StartCoroutine(FadeCanvas(reflectionCanvas, 0f));
    private void HideLoadingScreen() => StartCoroutine(FadeCanvas(loadingCanvas, 0f));

    #endregion

    #region UI Updates

    public void UpdateLevelText(int level)
    {
        if (levelText)
            levelText.text = $"Level {level}";
    }

    public void UpdateEmotionText(string emotion)
    {
        if (emotionText)
            emotionText.text = emotion;
    }

    public void ShowInteractionPrompt(GameObject interactable)
    {
        if (interactionPrompt)
        {
            interactionPrompt.gameObject.SetActive(true);
            promptText.text = $"Press E to interact with {interactable.name}";
        }
    }

    public void HideInteractionPrompt()
    {
        if (interactionPrompt)
            interactionPrompt.gameObject.SetActive(false);
    }

    public void DisplayQuote(StoicQuote quote)
    {
        StartCoroutine(TypewriterEffect(quoteText, quote.quote));
        authorText.text = $"- {quote.author}";
        StartCoroutine(TypewriterEffect(lessonText, quote.lesson));
    }

    public void ShowError(string message)
    {
        Debug.LogError($"UI Error: {message}");
        // Implement error popup here
    }

    #endregion

    #region Button Handlers

    private void OnStartGameClicked()
    {
        GameManager.Instance.StartGame();
    }

    private void OnOptionsClicked()
    {
        // Implement options menu
    }

    private void OnQuitClicked()
    {
        GameManager.Instance.QuitGame();
    }

    private void OnContinueClicked()
    {
        GameManager.Instance.LoadMazeScene();
    }

    #endregion

    #region Coroutines

    private IEnumerator FadeCanvas(CanvasGroup canvas, float targetAlpha)
    {
        if (canvas == null) yield break;

        canvas.gameObject.SetActive(true);
        
        float startAlpha = canvas.alpha;
        float elapsed = 0f;

        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeSpeed);
            yield return null;
        }

        canvas.alpha = targetAlpha;
        
        if (targetAlpha == 0f)
            canvas.gameObject.SetActive(false);
    }

    private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string text)
    {
        textComponent.text = "";
        foreach (char c in text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (PlayerController.OnInteractableFound != null)
            PlayerController.OnInteractableFound -= ShowInteractionPrompt;
        if (PlayerController.OnInteractableLost != null)
            PlayerController.OnInteractableLost -= HideInteractionPrompt;
        if (GameManager.OnGameStateChanged != null)
            GameManager.OnGameStateChanged -= HandleGameStateChange;
        if (GameManager.OnError != null)
            GameManager.OnError -= ShowError;
    }
}
