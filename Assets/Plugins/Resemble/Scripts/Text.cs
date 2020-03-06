using System.Linq;
using System.Collections.Generic;

namespace Resemble
{
    [System.Serializable]
    public class Text
    {
        /// <summary> Contains the text typed in by the user. </summary>
        public string userString = "";

        /// <summary> Contains the tags applied to the text. </summary>
        public List<Tag> tags = new List<Tag>();

        /// <summary> Position of the start of selection. </summary>
        public int selectID;

        /// <summary> Position of the user carret. </summary>
        public int carretID;

        /// <summary> Returns true if there is currently a selection. </summary>
        public bool haveSelection
        {
            get
            {
                return selectID != carretID;
            }
        }

        /// <summary> Max length of this text. </summary>
        public const int maxLength = 1000;

        /// <summary> Return a plain text string with SSML tags supported by the Resemble API. </summary>
        public string BuildResembleString()
        {
            //Order tags
            List<TagKey> keys = new List<TagKey>();
            keys.AddRange(tags.Select(x => new TagKey(x, true)));
            keys.AddRange(tags.Select(x => new TagKey(x, false)));
            keys = keys.OrderBy(x => (x.id + (x.open ? 0.5f : 0.0f))).ToList();

            //Insert tags in string
            int offset = 0;
            string s = userString;
            for (int i = 0; i < keys.Count; i++)
            {
                string tagString = keys[i].open ? keys[i].tag.OpenTag() : keys[i].tag.CloseTag();
                s = s.Insert(keys[i].id + offset, tagString);
                offset += tagString.Length;
            }

            //Return result
            return "<speak><p>" + s + "</p></speak>";
        }

        /// <summary> Override values by data from a string with SSML tags. </summary>
        public void ParseResembleString(string rawString)
        {
            List<TagParser> tagKeys = new List<TagParser>();
            string[] splited = rawString.Split('<');
            string withoutTags = "";
            for (int i = 0; i < splited.Length; i++)
            {
                tagKeys.Add(new TagParser(withoutTags.Length, splited[i]));

                //Add char for solo (like break tags)
                if (tagKeys[i].solo)
                    withoutTags += " ";
                withoutTags += tagKeys[i].next;
            }
            //TEMP - Debug
            /*
            for (int i = 0; i < tags.Count; i++)
            {
                UnityEngine.Debug.Log(tags[i]);
            }*/

            List<Tag> newTags = new List<Tag>();
            int count = tagKeys.Count;
            for (int i = 0; i < count; i++)
            {
                if (tagKeys[i].solo)
                {
                    //Build real tag
                    switch (tagKeys[i].type)
                    {
                        case "break":
                            newTags.Add(Tag.ParseBreak(tagKeys[i].data, tagKeys[i].id));
                            break;
                    }

                    //Remove tagParser
                    tagKeys.RemoveAt(i);
                    i--;
                    count--;
                    continue;
                }

                //Read list in reverse to find the opening tag
                if (i > 0)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (tagKeys[j].type == tagKeys[i].type)
                        {
                            switch (tagKeys[j].type)
                            {
                                case "style":
                                    newTags.Add(Tag.ParseEmotion(tagKeys[j].data, tagKeys[j].id, tagKeys[i].id));
                                    break;
                            }

                            //Remove two tag parser
                            tagKeys.RemoveAt(i);
                            tagKeys.RemoveAt(j);
                            i -= 2;
                            count -= 2;
                            break;
                        }
                    }
                }
            }

            userString = withoutTags;
            this.tags = newTags;
        }

        private struct TagParser
        {
            public TagParser(int id, string value)
            {
                this.id = id;
                next = "";
                if (value.Contains('>'))
                {
                    string[] s = value.Split('>');
                    value = s[0];
                    next = s[1];
                }
                string[] split = value.Split('=');
                open = !split[0].StartsWith("/");
                type = (open ? split[0] : split[0].Remove(0, 1)).Split(' ')[0];
                length = value.Length + 2;
                solo = split.Length > 1 && split[1].EndsWith("/");
                data = split.Length > 1 ? (solo ? split[1].Remove(split[1].Length -1) : split[1]) : "";
                closeId = -1;
            }

            public override string ToString()
            {
                return string.Format("|{1}|  |{0}|  Id : {2}  Length : {3}  Data : {4}", type, open ? '[' : ']', id, length, data);
            }

            public int id;
            public int length;
            public string type;
            public string data;
            public string next;
            public bool open;
            public bool solo;

            public int closeId;
        }

        /// <summary> Little struct used to merge the userStrings and tags easily. See BuildResembleString() methode. </summary>
        private struct TagKey
        {
            public int id;
            public Tag tag;
            public bool open;

            public TagKey(Tag tag, bool open)
            {
                this.tag = tag;
                this.open = open;
                id = open ? tag.start : tag.end;
            }

            public override string ToString()
            {
                return string.Format("(id : {0}, open : {1}, tag : {2})", id, open, tag);
            }
        }

        //Callbacks
        public delegate void Callback();
        public Callback onEdit;
        public Callback onChangeSelect;

        public void CallOnEdit()
        {
            if (onEdit != null)
                onEdit.Invoke();
        }

        public void CallOnChangeSelect()
        {
            if (onChangeSelect != null)
                onChangeSelect.Invoke();
        }

    }
}