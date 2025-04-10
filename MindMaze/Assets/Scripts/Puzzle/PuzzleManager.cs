using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Puzzle Settings")]
    [SerializeField] private QuoteDatabase quoteDatabase;
    [SerializeField] private float solveTime = 30f;
    [SerializeField] private int maxAttempts = 3;

    [Header("Difficulty Settings")]
    [SerializeField] private float difficultyMultiplier = 1.2f;
    [SerializeField] private bool adaptiveDifficulty = true;

    // Events
    public static event Action<StoicQuote> OnPuzzleStarted;
    public static event Action<bool> OnPuzzleCompleted;
    public static event Action<int> OnAttemptsChanged;
    public static event Action<float> OnTimeChanged;

    // Current puzzle state
    private StoicQuote currentQuote;
    private EmotionType currentEmotion;
    private int currentAttempts;
    private float remainingTime;
    private bool isPuzzleActive;
    private List<StoicQuote> usedQuotes = new List<StoicQuote>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePuzzleManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePuzzleManager()
    {
        if (quoteDatabase == null)
        {
            quoteDatabase = ScriptableObject.CreateInstance<QuoteDatabase>();
            quoteDatabase.InitializeDefaultQuotes();
            Debug.LogWarning("QuoteDatabase not assigned. Using default quotes.");
        }

        ResetPuzzleState();
    }

    private void Update()
    {
        if (!isPuzzleActive) return;

        // Update timer
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            OnTimeChanged?.Invoke(remainingTime);

            if (remainingTime <= 0)
            {
                HandlePuzzleTimeout();
            }
        }
    }

    public void StartPuzzle(EmotionType emotion)
    {
        try
        {
            currentEmotion = emotion;
            currentQuote = GetNextQuote(emotion);
            
            if (currentQuote == null)
            {
                throw new System.Exception("Failed to get valid quote for puzzle");
            }

            isPuzzleActive = true;
            currentAttempts = maxAttempts;
            remainingTime = solveTime * GetDifficultyMultiplier();

            OnPuzzleStarted?.Invoke(currentQuote);
            OnAttemptsChanged?.Invoke(currentAttempts);
            OnTimeChanged?.Invoke(remainingTime);

            UIManager.Instance.DisplayQuote(currentQuote);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error starting puzzle: {e.Message}");
            GameManager.Instance.OnError?.Invoke("Failed to start puzzle");
        }
    }

    public void AttemptSolution(string playerAnswer)
    {
        if (!isPuzzleActive) return;

        currentAttempts--;
        OnAttemptsChanged?.Invoke(currentAttempts);

        bool isCorrect = ValidateSolution(playerAnswer);

        if (isCorrect)
        {
            CompletePuzzle(true);
        }
        else if (currentAttempts <= 0)
        {
            CompletePuzzle(false);
        }
    }

    private bool ValidateSolution(string playerAnswer)
    {
        // Basic validation - can be expanded based on puzzle types
        if (string.IsNullOrEmpty(playerAnswer)) return false;

        // Convert both strings to lowercase and trim for comparison
        string normalizedAnswer = playerAnswer.Trim().ToLower();
        string normalizedSolution = currentQuote.lesson.Trim().ToLower();

        // Check if the player's answer contains key words from the lesson
        string[] solutionKeywords = normalizedSolution.Split(' ');
        int requiredKeywords = Mathf.Max(3, solutionKeywords.Length / 3);
        int matchedKeywords = 0;

        foreach (string keyword in solutionKeywords)
        {
            if (keyword.Length > 3 && normalizedAnswer.Contains(keyword))
            {
                matchedKeywords++;
                if (matchedKeywords >= requiredKeywords)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void CompletePuzzle(bool success)
    {
        isPuzzleActive = false;
        OnPuzzleCompleted?.Invoke(success);

        if (success)
        {
            GameManager.Instance.CompleteLevel();
        }
        else
        {
            RestartPuzzle();
        }
    }

    private void HandlePuzzleTimeout()
    {
        currentAttempts--;
        OnAttemptsChanged?.Invoke(currentAttempts);

        if (currentAttempts <= 0)
        {
            CompletePuzzle(false);
        }
        else
        {
            remainingTime = solveTime * GetDifficultyMultiplier();
            OnTimeChanged?.Invoke(remainingTime);
        }
    }

    private StoicQuote GetNextQuote(EmotionType emotion)
    {
        StoicQuote quote = quoteDatabase.GetQuoteForEmotion(emotion);
        
        // Ensure we don't repeat quotes unless we've used them all
        if (usedQuotes.Contains(quote))
        {
            if (usedQuotes.Count >= quoteDatabase.GetQuoteCount())
            {
                usedQuotes.Clear();
            }
            else
            {
                quote = quoteDatabase.GetQuoteForEmotion(emotion);
            }
        }

        usedQuotes.Add(quote);
        return quote;
    }

    private float GetDifficultyMultiplier()
    {
        if (!adaptiveDifficulty) return 1f;

        // Adjust difficulty based on player performance
        float levelMultiplier = 1f + (GameManager.Instance.currentLevel - 1) * 0.1f;
        return Mathf.Clamp(levelMultiplier * difficultyMultiplier, 0.5f, 2f);
    }

    public void RestartPuzzle()
    {
        ResetPuzzleState();
        StartPuzzle(currentEmotion);
    }

    private void ResetPuzzleState()
    {
        isPuzzleActive = false;
        currentAttempts = maxAttempts;
        remainingTime = solveTime;
        currentQuote = null;
    }

    public void CancelPuzzle()
    {
        if (!isPuzzleActive) return;

        isPuzzleActive = false;
        OnPuzzleCompleted?.Invoke(false);
        ResetPuzzleState();
    }

    private void OnDestroy()
    {
        // Cleanup
        Instance = null;
    }
}
