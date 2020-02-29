using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    [System.Serializable]
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
            this.color = type == Type.Emotion ? emotion.Color() : type.Color();
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

        public bool Contains(int id)
        {
            return id > start && id < end;
        }

        public ChangeState Contains(int id, int length)
        {
            if (length == 1 && id == start)
                return ChangeState.Before;

            int n = id + length;
            if (id <= start)
            {
                if (n <= start)
                    return ChangeState.Before;
                else if (n < end)
                    return ChangeState.ContainsStart;
                else
                    return ChangeState.ContainsAll;
            }
            else
            {
                if (id > end)
                    return ChangeState.After;
                else if (n > end + 1)
                    return ChangeState.ContainsEnd;
                else
                    return ChangeState.Inside;
            }
        }

        /// <summary> Call this when you add some characters of the string. </summary>
        public void AddCharacters(int id, int length)
        {
            if (!Contains(id))
            {
                if (id <= start)
                {
                    start += length;
                    end += length;
                }
            }
            else
            {
                end += length;
            }
        }

        /// <summary> Call this when you remove some characters of the string. Return true if need to be deleted </summary>
        public bool RemoveCharacters(int id, int length)
        {
            ChangeState state = Contains(id, length);

            switch (state)
            {
                case ChangeState.Before:
                    start -= length;
                    end -= length;
                    return false;
                case ChangeState.After:
                    return false;
                case ChangeState.Inside:
                    end -= length;
                    break;
                case ChangeState.ContainsAll:
                    return true;
                case ChangeState.ContainsStart:
                    start -= start - id;
                    end -= id + length - start - (start - id);
                    break;
                case ChangeState.ContainsEnd:
                    end -= end - id;
                    break;
            }
            return end - start <= 0;
        }

        /// <summary> Call this when you remove some characters of the tag area. Return true if need to be deleted. Can return a new tag to add. </summary>
        public bool ClearCharacters(int id, int length, out Tag otherTag)
        {
            otherTag = null;
            ChangeState state = Contains(id, length);

            switch (state)
            {
                case ChangeState.Before:
                    return false;
                case ChangeState.After:
                    return false;
                case ChangeState.Inside:
                    if ((end - (id + length)) > 0)
                        otherTag = new Tag(type, emotion, id + length, end);
                    end = id;
                    break;
                case ChangeState.ContainsAll:
                    return true;
                case ChangeState.ContainsStart:
                    start += id + length - start;
                    break;
                case ChangeState.ContainsEnd:
                    end -= end - id;
                    break;
            }
            return end - start <= 0;
        }

        public enum ChangeState
        {
            Before,
            After,
            Inside,
            ContainsAll,
            ContainsStart,
            ContainsEnd,
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