using UnityEngine;

[System.Serializable]
public class EmotionData
{
    public string name;
    public Color color = Color.white;
    public string description;
    public float intensity = 1.0f;
}

public enum EmotionType
{
    Fear,
    Anger,
    Desire,
    Anxiety,
    Pride,
    Grief,
    Envy,
    Attachment
}

[CreateAssetMenu(fileName = "EmotionConfig", menuName = "MindMaze/Emotion Configuration")]
public class EmotionConfiguration : ScriptableObject
{
    [SerializeField]
    private EmotionData[] emotions;

    private static readonly Color[] defaultColors = {
        new Color(0.7f, 0.7f, 0.7f), // Fear - Gray
        new Color(0.9f, 0.2f, 0.2f), // Anger - Red tint
        new Color(0.2f, 0.2f, 0.9f), // Desire - Blue tint
        new Color(0.8f, 0.8f, 0.2f), // Anxiety - Yellow tint
        new Color(0.2f, 0.9f, 0.2f), // Pride - Green tint
        new Color(0.5f, 0.5f, 0.5f), // Grief - Dark gray
        new Color(0.6f, 0.2f, 0.6f), // Envy - Purple tint
        new Color(0.4f, 0.4f, 0.4f)  // Attachment - Medium gray
    };

    public void InitializeDefaultEmotions()
    {
        emotions = new EmotionData[System.Enum.GetValues(typeof(EmotionType)).Length];
        
        for (int i = 0; i < emotions.Length; i++)
        {
            emotions[i] = new EmotionData
            {
                name = System.Enum.GetName(typeof(EmotionType), i),
                color = defaultColors[i],
                description = GetDefaultDescription((EmotionType)i),
                intensity = 1.0f
            };
        }
    }

    public EmotionData GetEmotionData(EmotionType type)
    {
        int index = (int)type;
        if (emotions != null && index < emotions.Length)
        {
            return emotions[index];
        }
        
        Debug.LogWarning($"Emotion data not found for {type}. Returning default.");
        return new EmotionData
        {
            name = type.ToString(),
            color = defaultColors[index],
            description = GetDefaultDescription(type),
            intensity = 1.0f
        };
    }

    private string GetDefaultDescription(EmotionType type)
    {
        switch (type)
        {
            case EmotionType.Fear:
                return "The anticipation of future suffering";
            case EmotionType.Anger:
                return "The desire for revenge or punishment";
            case EmotionType.Desire:
                return "The attachment to temporary pleasures";
            case EmotionType.Anxiety:
                return "Worry about uncertain outcomes";
            case EmotionType.Pride:
                return "Excessive self-regard";
            case EmotionType.Grief:
                return "Pain from loss or disappointment";
            case EmotionType.Envy:
                return "Desire for others' possessions or qualities";
            case EmotionType.Attachment:
                return "Clinging to impermanent things";
            default:
                return "Unknown emotion";
        }
    }

    public void ValidateEmotionData()
    {
        if (emotions == null || emotions.Length == 0)
        {
            Debug.LogWarning("Emotion data is empty. Initializing with defaults.");
            InitializeDefaultEmotions();
        }
    }

    private void OnValidate()
    {
        ValidateEmotionData();
    }
}
