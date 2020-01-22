using UnityEngine;

public enum Emotion
{
    None,
    Angry,
    Happy,
    Sad,
    Confuse,
}

public static class Emotions
{
    private static Color[] colors = new Color[]
    {
        new Color(0.5f, 0.5f, 0.5f, 1.0f),
        new Color(1.0f, 0.3f, 0.1f, 1.0f),
        new Color(0.5f, 1.0f, 0.3f, 1.0f),
        new Color(0.2f, 0.5f, 1.0f, 1.0f),
        new Color(1.0f, 0.6f, 0.0f, 1.0f),
    };

    public static Color Color(this Emotion emotion)
    {
        return colors[(int)emotion];
    }

    public static Emotion GetEmotion(string value)
    {
        switch (value)
        {
            case "Angry":
                return Emotion.Angry;
            case "Happy":
                return Emotion.Happy;
            case "Sad":
                return Emotion.Sad;
            case "Confuse":
                return Emotion.Confuse;
            default:
                return Emotion.None;
        }
    }
}