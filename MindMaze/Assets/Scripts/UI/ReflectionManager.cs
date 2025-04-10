using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class ReflectionManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup reflectionCanvas;
    [SerializeField] private RectTransform contentContainer;
    
    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI quoteText;
    [SerializeField] private TextMeshProUGUI authorText;
    [SerializeField] private TextMeshProUGUI lessonText;
    [SerializeField] private TextMeshProUGUI emotionText;
    [SerializeField] private TextMeshProUGUI levelCompletionText;
    
    [Header("UI Controls")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button replayLevelButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image emotionIcon;
    
    [Header("Animation Settings")]
    [SerializeField] private float textRevealDelay = 0.5f;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem completionParticles;
    [SerializeField] private float pulseIntensity = 1.1f;
    [SerializeField] private float pulseSpeed = 2f;
    
    [Header("Audio")]
    [SerializeField] private string reflectionMusic = "ReflectionMusic";
    [SerializeField] private string textRevealSound = "TextReveal";
    [SerializeField] private string completionSound = "LevelComplete";

    private StoicQuote currentQuote;
    private EmotionType currentEmotion;
    private bool isRevealing;
    private Vector3 originalContentScale;

    private void Awake()
    {
        InitializeReflectionUI();
    }

    private void Start()
    {
        StartReflection();
    }

    private void InitializeReflectionUI()
    {
        // Store original content scale
        if (contentContainer != null)
        {
            originalContentScale = contentContainer.localScale;
        }

        // Initialize buttons
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
            continueButton.gameObject.SetActive(false);
        }
        
        if (replayLevelButton != null)
        {
            replayLevelButton.onClick.AddListener(OnReplayClicked);
            replayLevelButton.gameObject.SetActive(false);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            mainMenuButton.gameObject.SetActive(false);
        }

        // Hide text elements initially
        SetTextElementsVisible(false);
    }

    private void StartReflection()
    {
        // Get current level's quote and emotion
        currentQuote = GameManager.Instance.GetCurrentLevelQuote();
        currentEmotion = GameManager.Instance.GetCurrentEmotion();

        // Start background music
        AudioManager.Instance.PlayMusic(reflectionMusic, true);

        // Begin reveal sequence
        StartCoroutine(RevealSequence());
    }

    private IEnumerator RevealSequence()
    {
        isRevealing = true;

        // Fade in the canvas
        yield return StartCoroutine(FadeInCanvas());

        // Reveal emotion
        yield return StartCoroutine(RevealEmotion());

        // Reveal quote
        yield return StartCoroutine(RevealQuote());

        // Reveal lesson
        yield return StartCoroutine(RevealLesson());

        // Show completion text and particles
        yield return StartCoroutine(ShowCompletion());

        // Show buttons
        ShowControls();

        isRevealing = false;
    }

    private IEnumerator FadeInCanvas()
    {
        reflectionCanvas.alpha = 0f;
        reflectionCanvas.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            reflectionCanvas.alpha = revealCurve.Evaluate(elapsed / fadeSpeed);
            yield return null;
        }

        reflectionCanvas.alpha = 1f;
    }

    private IEnumerator RevealEmotion()
    {
        emotionText.gameObject.SetActive(true);
        emotionText.alpha = 0f;

        // Set emotion text and color
        emotionText.text = currentEmotion.ToString().ToUpper();
        emotionText.color = Utilities.GetEmotionColor(currentEmotion);

        // Fade in emotion text
        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            emotionText.alpha = revealCurve.Evaluate(elapsed / fadeSpeed);
            yield return null;
        }

        // Start pulsing animation
        StartCoroutine(PulseEmotionText());

        yield return new WaitForSeconds(textRevealDelay);
    }

    private IEnumerator RevealQuote()
    {
        quoteText.gameObject.SetActive(true);
        authorText.gameObject.SetActive(true);

        // Reveal quote with typewriter effect
        yield return StartCoroutine(TypewriterEffect(quoteText, currentQuote.quote));

        // Reveal author
        authorText.text = $"- {currentQuote.author}";
        authorText.alpha = 0f;
        
        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            authorText.alpha = revealCurve.Evaluate(elapsed / fadeSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(textRevealDelay);
    }

    private IEnumerator RevealLesson()
    {
        lessonText.gameObject.SetActive(true);
        
        // Reveal lesson with typewriter effect
        yield return StartCoroutine(TypewriterEffect(lessonText, currentQuote.lesson));
        
        yield return new WaitForSeconds(textRevealDelay);
    }

    private IEnumerator ShowCompletion()
    {
        levelCompletionText.gameObject.SetActive(true);
        levelCompletionText.alpha = 0f;

        // Play completion sound and particles
        AudioManager.Instance.PlaySFX(completionSound);
        if (completionParticles != null)
        {
            completionParticles.Play();
        }

        // Fade in completion text
        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            levelCompletionText.alpha = revealCurve.Evaluate(elapsed / fadeSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(textRevealDelay);
    }

    private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string text)
    {
        textComponent.text = "";
        textComponent.alpha = 1f;

        foreach (char c in text)
        {
            textComponent.text += c;
            AudioManager.Instance.PlaySFX(textRevealSound);
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    private IEnumerator PulseEmotionText()
    {
        Vector3 originalScale = emotionText.transform.localScale;

        while (true)
        {
            // Pulse scale
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * pulseSpeed;
                float scale = 1f + (Mathf.Sin(t * Mathf.PI) * (pulseIntensity - 1f));
                emotionText.transform.localScale = originalScale * scale;
                yield return null;
            }
        }
    }

    private void ShowControls()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            StartCoroutine(FadeInButton(continueButton));
        }
        
        if (replayLevelButton != null)
        {
            replayLevelButton.gameObject.SetActive(true);
            StartCoroutine(FadeInButton(replayLevelButton));
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
            StartCoroutine(FadeInButton(mainMenuButton));
        }
    }

    private IEnumerator FadeInButton(Button button)
    {
        CanvasGroup buttonCanvas = button.GetComponent<CanvasGroup>();
        if (buttonCanvas == null)
        {
            buttonCanvas = button.gameObject.AddComponent<CanvasGroup>();
        }

        buttonCanvas.alpha = 0f;
        
        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            buttonCanvas.alpha = revealCurve.Evaluate(elapsed / fadeSpeed);
            yield return null;
        }

        buttonCanvas.alpha = 1f;
    }

    private void SetTextElementsVisible(bool visible)
    {
        if (quoteText != null) quoteText.gameObject.SetActive(visible);
        if (authorText != null) authorText.gameObject.SetActive(visible);
        if (lessonText != null) lessonText.gameObject.SetActive(visible);
        if (emotionText != null) emotionText.gameObject.SetActive(visible);
        if (levelCompletionText != null) levelCompletionText.gameObject.SetActive(visible);
    }

    #region Button Handlers

    private void OnContinueClicked()
    {
        if (isRevealing) return;
        
        AudioManager.Instance.PlaySFX("ButtonClick");
        StartCoroutine(TransitionToNextLevel());
    }

    private void OnReplayClicked()
    {
        if (isRevealing) return;
        
        AudioManager.Instance.PlaySFX("ButtonClick");
        StartCoroutine(TransitionToReplay());
    }

    private void OnMainMenuClicked()
    {
        if (isRevealing) return;
        
        AudioManager.Instance.PlaySFX("ButtonClick");
        StartCoroutine(TransitionToMainMenu());
    }

    #endregion

    #region Transitions

    private IEnumerator TransitionToNextLevel()
    {
        // Fade out canvas
        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            reflectionCanvas.alpha = 1f - revealCurve.Evaluate(elapsed / fadeSpeed);
            yield return null;
        }

        // Load next level
        GameManager.Instance.LoadNextLevel();
    }

    private IEnumerator TransitionToReplay()
    {
        // Fade out canvas
        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            reflectionCanvas.alpha = 1f - revealCurve.Evaluate(elapsed / fadeSpeed);
            yield return null;
        }

        // Replay current level
        GameManager.Instance.ReplayCurrentLevel();
    }

    private IEnumerator TransitionToMainMenu()
    {
        // Fade out canvas
        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            reflectionCanvas.alpha = 1f - revealCurve.Evaluate(elapsed / fadeSpeed);
            yield return null;
        }

        // Return to main menu
        GameManager.Instance.ReturnToMainMenu();
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up any coroutines
        StopAllCoroutines();
    }
}
