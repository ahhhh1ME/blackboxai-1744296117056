using UnityEngine;
using System.Collections.Generic;
using System;

public static class Utilities
{
    #region Math Utilities

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
            UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public static float GetAngleBetweenPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(b.z - a.z, b.x - a.x) * Mathf.Rad2Deg;
    }

    #endregion

    #region Color Utilities

    public static Color GetEmotionColor(EmotionType emotion)
    {
        switch (emotion)
        {
            case EmotionType.Fear:
                return new Color(0.7f, 0.7f, 0.7f); // Gray
            case EmotionType.Anger:
                return new Color(0.9f, 0.2f, 0.2f); // Red
            case EmotionType.Desire:
                return new Color(0.2f, 0.2f, 0.9f); // Blue
            case EmotionType.Anxiety:
                return new Color(0.8f, 0.8f, 0.2f); // Yellow
            case EmotionType.Pride:
                return new Color(0.2f, 0.9f, 0.2f); // Green
            case EmotionType.Grief:
                return new Color(0.5f, 0.5f, 0.5f); // Dark Gray
            case EmotionType.Envy:
                return new Color(0.6f, 0.2f, 0.6f); // Purple
            case EmotionType.Attachment:
                return new Color(0.4f, 0.4f, 0.4f); // Medium Gray
            default:
                return Color.white;
        }
    }

    public static Color LerpColorWithIntensity(Color baseColor, float intensity)
    {
        return Color.Lerp(Color.white, baseColor, Mathf.Clamp01(intensity));
    }

    #endregion

    #region String Utilities

    public static string GetEmotionDescription(EmotionType emotion)
    {
        switch (emotion)
        {
            case EmotionType.Fear:
                return "Fear clouds judgment and prevents growth.";
            case EmotionType.Anger:
                return "Anger is temporary madness.";
            case EmotionType.Desire:
                return "Desire chains us to external things.";
            case EmotionType.Anxiety:
                return "Anxiety comes from wanting to control the uncontrollable.";
            case EmotionType.Pride:
                return "Pride blinds us to our own faults.";
            case EmotionType.Grief:
                return "Grief reminds us of the impermanence of all things.";
            case EmotionType.Envy:
                return "Envy poisons contentment with what we have.";
            case EmotionType.Attachment:
                return "Attachment leads to disappointment.";
            default:
                return "Unknown emotion";
        }
    }

    public static string FormatTime(float timeInSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);
        return string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
    }

    #endregion

    #region Gameplay Utilities

    public static bool IsPlayerInRange(Transform player, Transform target, float range)
    {
        if (player == null || target == null) return false;
        return Vector3.Distance(player.position, target.position) <= range;
    }

    public static Vector3 GetSafeSpawnPosition(Vector3 desiredPosition, float checkRadius, LayerMask obstacleLayer)
    {
        if (!Physics.CheckSphere(desiredPosition, checkRadius, obstacleLayer))
        {
            return desiredPosition;
        }

        // If desired position is not safe, try to find a safe position nearby
        for (int i = 1; i <= 8; i++)
        {
            float angle = i * (360f / 8);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * checkRadius;
            Vector3 testPosition = desiredPosition + offset;

            if (!Physics.CheckSphere(testPosition, checkRadius, obstacleLayer))
            {
                return testPosition;
            }
        }

        Debug.LogWarning("Could not find safe spawn position!");
        return desiredPosition;
    }

    #endregion

    #region UI Utilities

    public static void SetCanvasGroupState(CanvasGroup canvasGroup, bool active)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = active ? 1f : 0f;
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }

    public static void FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration, 
        System.Action onComplete = null)
    {
        if (canvasGroup == null) return;

        MonoBehaviour host = GameObject.FindObjectOfType<MonoBehaviour>();
        if (host != null)
        {
            host.StartCoroutine(FadeCanvasGroupCoroutine(canvasGroup, targetAlpha, duration, onComplete));
        }
    }

    private static System.Collections.IEnumerator FadeCanvasGroupCoroutine(
        CanvasGroup canvasGroup, float targetAlpha, float duration, System.Action onComplete)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
    }

    #endregion

    #region Debug Utilities

    public static void DrawDebugBounds(Bounds bounds, Color color, float duration = 0f)
    {
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        Vector3[] points = new Vector3[]
        {
            center + new Vector3(-size.x, -size.y, -size.z) * 0.5f,
            center + new Vector3(size.x, -size.y, -size.z) * 0.5f,
            center + new Vector3(size.x, -size.y, size.z) * 0.5f,
            center + new Vector3(-size.x, -size.y, size.z) * 0.5f,
            center + new Vector3(-size.x, size.y, -size.z) * 0.5f,
            center + new Vector3(size.x, size.y, -size.z) * 0.5f,
            center + new Vector3(size.x, size.y, size.z) * 0.5f,
            center + new Vector3(-size.x, size.y, size.z) * 0.5f
        };

        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(points[i], points[(i + 1) % 4], color, duration);
            Debug.DrawLine(points[i + 4], points[((i + 1) % 4) + 4], color, duration);
            Debug.DrawLine(points[i], points[i + 4], color, duration);
        }
    }

    public static void LogWithContext(string message, UnityEngine.Object context = null)
    {
        if (context != null)
            Debug.Log($"[{context.GetType().Name}] {message}", context);
        else
            Debug.Log($"[Utilities] {message}");
    }

    #endregion

    #region Scene Management Utilities

    public static T GetOrCreateObject<T>(string name = null) where T : Component
    {
        T component = GameObject.FindObjectOfType<T>();
        
        if (component == null)
        {
            GameObject obj = new GameObject(name ?? typeof(T).Name);
            component = obj.AddComponent<T>();
        }
        
        return component;
    }

    public static void SafeDestroy(UnityEngine.Object obj)
    {
        if (Application.isPlaying)
            UnityEngine.Object.Destroy(obj);
        else
            UnityEngine.Object.DestroyImmediate(obj);
    }

    #endregion

    #region Input Utilities

    public static bool IsPointerOverUIObject()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    public static Vector2 GetMousePositionInCanvas(Canvas canvas)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, Input.mousePosition, canvas.worldCamera, out mousePos);
        return mousePos;
    }

    #endregion
}
