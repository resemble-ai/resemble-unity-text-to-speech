using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    public class Tag
    {
        public Type type;
        public int start;
        public int end;
        public Rect[] rects;
        public Color color;

        public Tag(Type type, int idA, int idB)
        {
            this.type = type;
            this.color = type.Color();
            start = Mathf.Min(idA, idB);
            end = Mathf.Max(idA, idB);
        }

        public enum Type
        {
            Wait,
            None,
            Angry,
            Happy,
            Sad,
            Confuse,
            COUNT,
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

        public static Tag.Type GetEmotion(string value)
        {
            switch (value)
            {
                case "Angry":
                    return Tag.Type.Angry;
                case "Happy":
                    return Tag.Type.Happy;
                case "Sad":
                    return Tag.Type.Sad;
                case "Confuse":
                    return Tag.Type.Confuse;
                default:
                    return Tag.Type.None;
            }
        }
    }
}