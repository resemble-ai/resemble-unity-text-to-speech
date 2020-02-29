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