using System.Linq;
using System.Collections.Generic;

namespace Resemble
{
    public class Text
    {
        public string userString = "";
        public List<Tag> tags = new List<Tag>();


        struct TagKey
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

        }

        public string BuildResembleString()
        {
            List<TagKey> keys = new List<TagKey>();
            keys.AddRange(tags.Select(x => new TagKey(x, true)));
            keys.AddRange(tags.Select(x => new TagKey(x, false)));
            keys.OrderBy(x => x.id);

            int offset = 0;
            string s = userString;
            for (int i = 0; i < keys.Count; i++)
            {
                string tagString = keys[i].open ? keys[i].tag.OpenTag() : keys[i].tag.CloseTag();
                s = s.Insert(keys[i].id + offset, tagString);
                offset += tagString.Length;
            }

            return s;
        }
    }
}