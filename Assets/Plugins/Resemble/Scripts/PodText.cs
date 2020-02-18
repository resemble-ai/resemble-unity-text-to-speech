using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
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
                BuildTagList();
            }
        }
        public string userString { get; private set; } = "";
        public string richString { get; private set; } = "";

        public void Refresh()
        {
            BuildTagList();
        }

        private void BuildTagList()
        {
            string[] tags = originString.Split('<', '>');
            List<int> tagsIds = IndexesOfAny(originString, '<', '>');

            /*
            string temp = originString;
            for (int i = 0; i < tagsIds.Count; i++)
            {
                temp = temp.Insert(tagsIds[i] + i, "*");
            }
            Debug.Log(temp);
            */

            tagsIds.Insert(0, 0);
            tagsIds.Add(originString.Length);

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
            for (int i = 0; i < tags.Length - 1; i += 4)
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

            /*
            for (int i = 0; i < tagList.Count; i++)
            {
                Debug.Log(tagList[i]);
            }*/
        }

        private List<int> IndexesOfAny(string target, params char[] chars)
        {
            List<int> ids = new List<int>();
            int id = 0;
            while (true)
            {
                int n = target.IndexOfAny(new char[] { '<', '>' }, id);
                if (n == -1)
                    return ids;
                ids.Add(n);
                id = n + 1;
            }
        }

        public void SetTagToSelection(int rawA, int rawB, int userA, int userB, Emotion type)
        {
            int rawStart = Mathf.Min(rawA, rawB);
            int rawEnd = Mathf.Max(rawA, rawB);
            int userStart = Mathf.Min(userA, userB);
            int userEnd = Mathf.Max(userA, userB);

            /*
            //Select a specific tag
            for (int i = 0; i < tags.Length; i++)
            {
                //Debug.Log(userStart + "  " + userEnd + "  " + tags[i]);
                if (tags[i].startIndex == userStart && tags[i].endIndex == userEnd)
                {
                    Debug.Log("Specific tag : " + originString.Substring(tags[i].rawStartIndex, tags[i].rawEndIndex - tags[i].rawStartIndex));
                    //int replacePos = tags[i].rawStartIndex
                    //tags[i].emotion = type;
                    return;
                }
            }*/
            /*
            List<Tag> containsTags;
            if (SelectionContainsTags(rawStart, rawEnd, out containsTags))
            {
                string newString = "";
                for (int i = 0; i < tags.Length; i++)
                {
                    bool bypass = false;
                    for (int j = 0; j < containsTags.Count; j++)
                    {
                        if (tags[i] == containsTags[j])
                        {
                            bypass = true;
                            break;
                        }
                    }

                    if (bypass || tags[i].emotion == Emotion.None)
                    {
                        Debug.Log("Bypass " + tags[i].content);
                        newString += tags[i].content;
                    }
                    else
                    {
                        Debug.Log("Include " + tags[i].content);
                        newString += "<size=" + tags[i].emotion.ToString() + ">" + tags[i].content + "</size>";
                    }
                }
                _originString = newString;
            }
            */

            originString = originString.Insert(rawEnd, "</size>").Insert(rawStart, "<size=" + type.ToString() + ">");
        }

        public void RemoveTag(Tag tag)
        {
            Debug.Log(originString.Substring(tag.startIndex, tag.endIndex));
        }

        public bool SelectionContainsTags(int start, int end, out List<Tag> result)
        {
            result = new List<Tag>();
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i].rawStartIndex >= start && tags[i].rawEndIndex <= end)
                {
                    result.Add(tags[i]);
                    Debug.Log(start + "  " + end + "  " + tags[i].rawStartIndex + "  " + tags[i].rawEndIndex + "  " + tags[i].content);
                }
            }
            return result.Count > 0;
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
        }

        public static string RemoveTags(string value)
        {
            int a = 0;
            bool haveTag = true;
            while (haveTag && a < 20)
            {
                a++;
                int next = value.IndexOf('<');
                if (next == -1)
                {
                    if (value.IndexOf('>') != -1)
                        next = 0;
                    else
                        return value;
                }
                int nextClose = value.IndexOf('>', next);
                if (nextClose == -1)
                    value = value.Remove(next);
                else
                    value = value.Remove(next, nextClose - next + 1);
            }
            return value;
        }

        public Tag BuildTag(string content, string type, ref int carretIndex, int rawStartIndex, int rawEndIndex)
        {
            int start = carretIndex;
            carretIndex += content.Length;
            return new Tag(content, Emotions.GetEmotion(type), start, carretIndex, rawStartIndex, rawEndIndex);
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

            public static bool operator ==(Tag a, Tag b)
            {
                return a.rawStartIndex == b.rawStartIndex && a.rawEndIndex == b.rawEndIndex;
            }

            public static bool operator !=(Tag a, Tag b)
            {
                return a.rawStartIndex != b.rawStartIndex || a.rawEndIndex != b.rawEndIndex;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [System.Serializable]
        public struct RectTag
        {
            public Rect rect;
            public Emotion emotion;
        }
    }
}