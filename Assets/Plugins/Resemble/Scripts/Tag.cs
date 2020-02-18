using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    public class Tag
    {
        public Type type;
        public Emotion emotion;
        public int start;
        public int end;
        public Rect[] rects;
        public Color color;

        public Tag(Type type, Emotion emotion, int idA, int idB)
        {
            this.type = type;
            this.emotion = emotion;
            this.color = type.Color();
            start = Mathf.Min(idA, idB);
            end = Mathf.Max(idA, idB);
        }

        public enum Type
        {
            Wait,
            Emotion,
            COUNT,
        }

        public string OpenTag()
        {
            switch (type)
            {
                case Type.Wait:
                    return "<break time=\"1S\"/>";
                case Type.Emotion:
                    return emotion.OpenTag();
                default:
                    return "";
            }
        }

        public string CloseTag()
        {
            switch (type)
            {
                case Type.Wait:
                    return "";
                case Type.Emotion:
                    return emotion.CloseTag();
                default:
                    return "";
            }
        }

    }

    public static class Tags
    {
        private static Color[] colors = new Color[]
        {
            new Color(0.8f, 0.1f, 0.1f, 1.0f),
            new Color(0.5f, 0.5f, 0.5f, 1.0f),
            new Color(1.0f, 0.3f, 0.1f, 1.0f),
            new Color(0.5f, 1.0f, 0.3f, 1.0f),
            new Color(0.2f, 0.5f, 1.0f, 1.0f),
            new Color(1.0f, 0.6f, 0.0f, 1.0f),
        };

        public static Color Color(this Tag.Type tag)
        {
            return colors[(int)tag];
        }
    }
}