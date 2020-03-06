using UnityEngine;

namespace Resemble
{
    public enum Emotion
    {
        Neutral,
        Angry,
        Annoyed,
        Question,
        Confuse,
        Happy,
        COUNT,
    }

    public static class Emotions
    {
        private static Color[] colors = new Color[]
        {
            new Color(0.500f, 0.500f, 0.500f, 1.0f),    //Neutral
            new Color(0.741f, 0.250f, 0.278f, 1.0f),    //Angry
            new Color(0.369f, 0.349f, 0.663f, 1.0f),    //Annoyed
            new Color(1.000f, 0.737f, 0.427f, 1.0f),    //Question
            new Color(0.588f, 0.341f, 0.663f, 1.0f),    //Confuse
            new Color(0.667f, 0.729f, 0.239f, 1.0f),    //Happy
        };

        public static Color Color(this Emotion emotion)
        {
            return colors[(int)emotion];
        }

        public static string OpenTag(this Emotion emotion)
        {
            return string.Format("<style emotions={0}{1}{0}>", '\'', emotion.ToString().ToLower());
        }

        public static string CloseTag(this Emotion emotion)
        {
            return "</style>";
        }
    }
}
 