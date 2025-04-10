using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    // Game state
    public int currentLevel = 0;
    public bool isPaused = false;
    
    // Events
    public static event Action<string> OnGameStateChanged;
    public static event Action<string> OnError;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        try
        {
            // Initialize game state
            currentLevel = 0;
            isPaused = false;
            
            // Notify subscribers
            OnGameStateChanged?.Invoke("GameInitialized");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing game: {e.Message}");
            OnError?.Invoke("Failed to initialize game");
        }
    }

    public void StartGame()
    {
        try
        {
            currentLevel = 1;
            LoadMazeScene();
            OnGameStateChanged?.Invoke("GameStarted");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error starting game: {e.Message}");
            OnError?.Invoke("Failed to start game");
        }
    }

    public void LoadMazeScene()
    {
        try
        {
            SceneManager.LoadScene("MazeScene");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading maze scene: {e.Message}");
            OnError?.Invoke("Failed to load maze");
        }
    }

    public void LoadReflectionScene()
    {
        try
        {
            SceneManager.LoadScene("ReflectionScene");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading reflection scene: {e.Message}");
            OnError?.Invoke("Failed to load reflection");
        }
    }

    public void CompleteLevel()
    {
        try
        {
            currentLevel++;
            LoadReflectionScene();
            OnGameStateChanged?.Invoke("LevelCompleted");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error completing level: {e.Message}");
            OnError?.Invoke("Failed to complete level");
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        OnGameStateChanged?.Invoke("GamePaused");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        OnGameStateChanged?.Invoke("GameResumed");
    }

    public void QuitGame()
    {
        try
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
            
            OnGameStateChanged?.Invoke("GameQuit");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error quitting game: {e.Message}");
            OnError?.Invoke("Failed to quit game");
        }
    }
}
