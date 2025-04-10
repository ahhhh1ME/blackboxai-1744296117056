using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class StoicQuote
{
    public string quote;
    public string author;
    public string lesson;
    public EmotionType associatedEmotion;
    public int difficulty;
}

[CreateAssetMenu(fileName = "QuoteDatabase", menuName = "MindMaze/Quote Database")]
public class QuoteDatabase : ScriptableObject
{
    [SerializeField]
    private List<StoicQuote> quotes = new List<StoicQuote>();

    private static readonly string[] defaultQuotes = {
        "The chief task in life is simply this: to identify and separate matters so that I can say clearly to myself which are externals not under my control, and which have to do with the choices I actually control.",
        "You have power over your mind - not outside events. Realize this, and you will find strength.",
        "The happiness of your life depends upon the quality of your thoughts.",
        "Waste no more time arguing about what a good man should be. Be one.",
        "It's not what happens to you, but how you react to it that matters.",
        "First say to yourself what you would be; then do what you have to do.",
        "He who fears death will never do anything worthy of a living man.",
        "The best revenge is to be unlike him who performed the injury."
    };

    private static readonly string[] defaultAuthors = {
        "Epictetus",
        "Marcus Aurelius",
        "Marcus Aurelius",
        "Marcus Aurelius",
        "Epictetus",
        "Epictetus",
        "Seneca",
        "Marcus Aurelius"
    };

    public void InitializeDefaultQuotes()
    {
        quotes.Clear();
        
        for (int i = 0; i < defaultQuotes.Length; i++)
        {
            quotes.Add(new StoicQuote
            {
                quote = defaultQuotes[i],
                author = defaultAuthors[i],
                lesson = GenerateDefaultLesson(i),
                associatedEmotion = (EmotionType)(i % System.Enum.GetValues(typeof(EmotionType)).Length),
                difficulty = (i % 3) + 1
            });
        }
    }

    private string GenerateDefaultLesson(int index)
    {
        switch (index)
        {
            case 0:
                return "Focus on what you can control, accept what you cannot.";
            case 1:
                return "Your mind is your strongest asset and the key to inner peace.";
            case 2:
                return "Your perspective shapes your reality.";
            case 3:
                return "Action speaks louder than words.";
            case 4:
                return "Your response to events determines their impact on you.";
            case 5:
                return "Self-improvement begins with clear intention and follows with dedicated action.";
            case 6:
                return "Fear of death prevents truly living.";
            case 7:
                return "Rise above negativity through noble character.";
            default:
                return "Reflect on the wisdom within.";
        }
    }

    public StoicQuote GetRandomQuote()
    {
        if (quotes == null || quotes.Count == 0)
        {
            Debug.LogWarning("No quotes available. Initializing defaults.");
            InitializeDefaultQuotes();
        }
        
        int randomIndex = UnityEngine.Random.Range(0, quotes.Count);
        return quotes[randomIndex];
    }

    public StoicQuote GetQuoteForEmotion(EmotionType emotion)
    {
        if (quotes == null || quotes.Count == 0)
        {
            Debug.LogWarning("No quotes available. Initializing defaults.");
            InitializeDefaultQuotes();
        }

        var matchingQuotes = quotes.FindAll(q => q.associatedEmotion == emotion);
        
        if (matchingQuotes.Count == 0)
        {
            Debug.LogWarning($"No quotes found for emotion {emotion}. Returning random quote.");
            return GetRandomQuote();
        }

        int randomIndex = UnityEngine.Random.Range(0, matchingQuotes.Count);
        return matchingQuotes[randomIndex];
    }

    public void AddQuote(StoicQuote quote)
    {
        if (string.IsNullOrEmpty(quote.quote) || string.IsNullOrEmpty(quote.author))
        {
            throw new ArgumentException("Quote and author cannot be empty");
        }
        
        quotes.Add(quote);
    }

    private void OnValidate()
    {
        if (quotes == null || quotes.Count == 0)
        {
            InitializeDefaultQuotes();
        }
    }
}
