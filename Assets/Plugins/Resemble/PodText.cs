using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PodText
{
    public int carretPosition;
    public Tag[] tags = new Tag[0];

    public string _originString = "";
    public string originString
    {
        get
        {
            return _originString;
        }

        set
        {
            if (_originString == value)
                return;
            _originString = value;
            userString = FormatResembleToUserString(value);
            richString = FormatResembleToRichString(value);
            BuildTagList();
        }
    }
    public string userString { get; private set; } = "";
    public string richString { get; private set; } = "";

    private string FormatResembleToUserString(string value)
    {
        value = value.Replace("<emotion=angry>", "");
        value = value.Replace("</emotion>", "");
        return value;
    }

    private string FormatResembleToRichString(string value)
    {
        return value;
    }

    private void BuildTagList()
    {
        string[] tags = originString.Split('<', '>');
        List<int> tagsIds = IndexesOfAny(originString, '<', '>');
        tagsIds.Insert(0, 0);
        tagsIds.Add(originString.Length);

        Debug.Log(tags.Length + "  " + tagsIds.Count);
        List<Tag> tagList = new List<Tag>();
        userString = "";

        //No text
        if (tags.Length == 0)
        {
            this.tags = tagList.ToArray();
            return;
        }

        //No tags
        if (tags.Length < 4)
        {
            tagList.Add(new Tag(tags[0], Emotion.Neutral, 0, tags[0].Length, tagsIds[0], tagsIds[1]));
            userString += tags[0];
            this.tags = tagList.ToArray();
            return;
        }

        //Get tags
        int carretId = 0;
        int rt = 0;
        for (int i = 0; i < tags.Length - 1; i+=4)
        {
            if (!string.IsNullOrEmpty(tags[i + 0]))
            {
                tagList.Add(BuildTag(tags[i + 0], Emotion.Neutral, ref carretId, tagsIds[rt * 2], tagsIds[rt * 2 + 1]));
                userString += tags[i + 0];
                rt++;
            }
            string type = tags[i + 1].Remove(0, 5);
            if (!string.IsNullOrEmpty(tags[i + 2]))
            {
                tagList.Add(BuildTag(tags[i + 2], type, ref carretId, tagsIds[rt * 2], tagsIds[rt * 2 + 1]));
                userString += tags[i + 2];
                rt++;
            }
        }
        if (!string.IsNullOrEmpty(tags[tags.Length - 1]))
        {
            tagList.Add(BuildTag(tags[tags.Length - 1], Emotion.Neutral, ref carretId, tagsIds[tagsIds.Count - 2], tagsIds[tagsIds.Count - 1]));
            userString += tags[tags.Length - 1];
        }

        this.tags = tagList.ToArray();

        
        for (int i = 0; i < tagList.Count; i++)
        {
            Debug.Log(tagList[i]);
        }
    }

    private List<int> IndexesOfAny(string target, params char[] chars)
    {
        List<int> ids = new List<int>();
        int id = 0;
        while(true)
        {
            int n = target.IndexOfAny(new char[] { '<', '>' }, id);
            if (n == -1)
                return ids;
            ids.Add(n);
            id = n+1;
        }
    }

    public void SetTagToSelection(int selectID, int cursorID, Emotion type)
    {
        int start = Mathf.Min(selectID, cursorID);
        int end = Mathf.Max(selectID, cursorID);

        originString = originString.Insert(end, "</size>").Insert(start, "<size=Angry>");
    }

    public string HighlightedTags(int selectID, int cursorID)
    {
        int start = Mathf.Min(selectID, cursorID);
        int end = Mathf.Max(selectID, cursorID);

        string t = "";
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i].rawStartIndex >= start && tags[i].rawEndIndex <= end)
                t += tags[i].ToString() + " | ";
        }

        return t;
    }

    public void RemoveAllTags()
    {
        originString = userString;
        return;
        string s = originString;
        Debug.Log(s.Replace("<", "<*"));
        int a = 0;
        while (true)
        {
            a++;
            int start = s.IndexOf('<');
            if (start == -1)
                break;
            int end = s.IndexOf('>', start) + 1;
            Debug.Log(start + "  " + end + "  " + s.Replace("<", "<*"));
            s = s.Remove(start, end - start);
            if (a == 50)
                break;
        }
        Debug.Log(s.Replace("<", "<*"));
        originString = s;
    }

    public Tag BuildTag(string content, string type, ref int carretIndex, int rawStartIndex, int rawEndIndex)
    {
        int start = carretIndex;
        carretIndex += content.Length;
        return new Tag(content, stringToEmotionTag[type], start, carretIndex, rawStartIndex, rawEndIndex);
    }

    public Tag BuildTag(string content, Emotion type, ref int carretIndex, int rawStartIndex, int rawEndIndex)
    {
        int start = carretIndex;
        carretIndex += content.Length;
        return new Tag(content, type, start, carretIndex, rawStartIndex, rawEndIndex);
    }

    [System.Serializable]
    public struct Tag
    {
        public string content;
        public Emotion emotion;
        public int startIndex;
        public int endIndex;
        public int rawStartIndex;
        public int rawEndIndex;

        public Tag(string content, Emotion emotion, int startIndex, int endIndex, int rawStartIndex, int rawEndIndex)
        {
            this.content = content;
            this.emotion = emotion;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.rawStartIndex = rawStartIndex;
            this.rawEndIndex = rawEndIndex;
        }

        public override string ToString()
        {
            return string.Format("Tag[{2} - {3}][{4} - {5}] ({0}, {1}))", emotion, content, startIndex, endIndex, rawStartIndex, rawEndIndex);
        }

    }

    [System.Serializable]
    public struct RectTag
    {
        public Rect rect;
        public Emotion emotion;
    }

    public enum Emotion
    {
        Neutral,
        Angry,
        Happy,
        Sad,
        Confuse,
    }

    private static Dictionary<string, Emotion> stringToEmotionTag = new Dictionary<string, Emotion>()
    {
        { "Neutral", Emotion.Neutral },
        { "Angry", Emotion.Angry },
        { "Happy", Emotion.Happy },
        { "Sad", Emotion.Sad },
        { "Confuse", Emotion.Confuse },
    };

}
