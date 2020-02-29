using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Resemble;

namespace Resemble.GUIEditor
{
    public static class TextExtend
    {

        //Text edition

        /// <summary> Type on character in the string. </summary>
        public static void TypeChar(this Text text, char c)
        {
            //Remove existing selection if any
            int a, b, l; text.GetIDs(out a, out b, out l);
            if (text.haveSelection)
                RemoveChars(text, a, l);

            //Add char to string
            AddChars(text, a, c.ToString());
            text.SetIDs(a + 1);
        }

        /// <summary> Remove characters from the string. Negate count remove char before the carret. </summary>
        public static void RemoveChars(this Text text, int count)
        {
            if (count == 0)
            {
                return;
            }

            int id = text.carretID;
            int length = Mathf.Abs(count);

            if (count > 0)
            {
                if (text.carretID < 0)
                    Debug.LogError("Index is out of bounds.");
                else if (text.carretID + count > text.userString.Length)
                    Debug.LogError("Index is out of bounds.");
            }
            else
            {
                if (text.carretID + count < 0)
                    Debug.LogError("Index is out of bounds.");
                else if (text.carretID > text.userString.Length)
                    Debug.LogError("Index is out of bounds.");
                id += count;
            }
            RemoveChars(text, id, length);
            text.SetIDs(id);
        }

        /// <summary> Select all text. </summary>
        public static void SelectAll(this Text text)
        {
            text.SetIDs(0, text.userString.Length);
        }

        /// <summary> Copy the substring in selection to the clipboard. </summary>
        public static void Copy(this Text text)
        {
            int a, b, l; text.GetIDs(out a, out b, out l);
            GUIUtility.systemCopyBuffer = text.userString.Substring(a, l);
        }

        /// <summary> Cut the substring in selection to the clipboard. </summary>
        public static void Cut(this Text text)
        {
            int a, b, l; text.GetIDs(out a, out b, out l);
            GUIUtility.systemCopyBuffer = text.userString.Substring(a, l);
            RemoveChars(text, a, l);
            text.SetIDs(a);
        }

        /// <summary> Override characters in selection by the string in the clipboard. </summary>
        public static void Paste(this Text text)
        {
            int a, b, l; text.GetIDs(out a, out b, out l);
            RemoveChars(text, a, l);
            string pastString = CleanString(GUIUtility.systemCopyBuffer, "\r", System.Environment.NewLine);
            AddChars(text, a, pastString);
            text.SetIDs(b);
        }

        /// <summary> Delete characters in selection. </summary>
        public static void Delete(this Text text)
        {
            //check selection
            if (!text.haveSelection)
                return;

            //Remove characters
            int a, b, l; text.GetIDs(out a, out b, out l);
            RemoveChars(text, a, l);
            text.SetIDs(a);
        }

        /// <summary> Return true if the selection contain the id. </summary>
        public static bool SelectionContains(this Text text, int id)
        {
            int a, b, l; text.GetIDs(out a, out b, out l);
            return id >= a && id <= b;
        }


        //Tag edition

        /// <summary> Return the tag under the carret. If you have a selection, use GetTags() instead. </summary>
        public static Tag GetTag(this Text text)
        {
            //Check selection
            if (text.haveSelection)
                return null;

            //Return the first tag that contains the carret index
            for (int i = 0; i < text.tags.Count; i++)
            {
                if (text.tags[i].Contains(text.carretID))
                    return text.tags[i];
            }

            //No tag find
            return null;
        }

        /// <summary> Return the tag under the selection. If you don't have a selection, use GetTag() instead. </summary>
        public static List<Tag> GetTags(this Text text)
        {
            //Check selection
            if (!text.haveSelection)
                return null;

            //Get Tags under the selection
            List<Tag> tags = new List<Tag>();
            int a, b, l; text.GetIDs(out a, out b, out l);
            for (int i = 0; i < text.tags.Count; i++)
            {
                switch (text.tags[i].Contains(a, l))
                {
                    case Tag.ChangeState.Before:
                    case Tag.ChangeState.After:
                        break;
                    default:
                        tags.Add(text.tags[i]);
                        break;
                }
            }

            //Return result
            return tags;
        }

        /// <summary> Create tag on selection. Can delete or split tag if needed. </summary>
        public static Tag AddTag(this Text text)
        {
            //Check is selection exist
            if (!text.haveSelection)
                return null;

            //Remove tags in area
            text.RemoveTag();

            //Add new tag
            int a, b, l; text.GetIDs(out a, out b, out l);
            Tag tag = new Tag(Tag.Type.Emotion, Emotion.Neutral, a, b);
            text.tags.Add(tag);

            //Adapt selection
            text.SetIDs(b);

            //Invoke edit callback
            text.CallOnEdit();

            return tag;
        }

        /// <summary> Create a break tag at carret.</summary>
        public static Tag AddBreak(this Text text)
        {
            Debug.LogError("Breaks are not implemented yet.");
            return null;

            /* Old stuff
             userString = userString.Insert(carretID, "      ");
            OnEditText(carretID, 6, true);
            text.tags.Add(new Tag(Tag.Type.Wait, Emotion.Neutral, carretID, carretID + 6));
            RefreshTagsRects();
            carretID++;
            selectID++;
            dirty = true;*/
        }

        /// <summary> Remove tag in selection. Can delete or split tag if needed. </summary>
        public static void RemoveTag(this Text text)
        {
            int a, b, l; text.GetIDs(out a, out b, out l);
            int count = text.tags.Count;
            for (int i = 0; i < count; i++)
            {
                Tag otherTag = null;
                if (text.tags[i].ClearCharacters(a, l, out otherTag))
                {
                    text.tags.RemoveAt(i);
                    i--;
                    count--;
                }
                if (otherTag != null)
                {
                    text.tags.Add(otherTag);
                }
            }

            text.CallOnEdit();
        }

        /// <summary> Remove a tag. </summary>
        public static void RemoveTag(this Text text, Tag tag)
        {
            text.tags.Remove(tag);
            text.CallOnEdit();
        }

        /// <summary> Remove all tags. </summary>
        public static void ClearTags(this Text text)
        {
            text.tags.Clear();
            text.CallOnEdit();
        }



        //Private

        /// <summary> Add characters to the text. Handle the tag offset. </summary>
        private static void AddChars(Text text, int id, string value)
        {
            //Modify string
            text.userString = text.userString.Insert(id, value);

            //Apply offset on tags
            int length = value.Length;
            for (int i = 0; i < text.tags.Count; i++)
                text.tags[i].AddCharacters(id, length);

            text.CallOnEdit();
        }

        /// <summary> Remove characters from text. Handle the tag offset. </summary>
        private static void RemoveChars(Text text, int id, int length)
        {
            //Modify string
            text.userString = text.userString.Remove(id, length);

            //Apply offset on tags
            int count = text.tags.Count;
            for (int i = 0; i < count; i++)
            {
                if (text.tags[i].RemoveCharacters(id, length))
                {
                    //Delete tag if needed
                    text.tags.RemoveAt(i);
                    i--;
                    count--;
                }
            }

            text.CallOnEdit();
        }

        /// <summary> Remove items in string. </summary>
        private static string CleanString(string target, params string[] itemsToRemove)
        {
            for (int i = 0; i < itemsToRemove.Length; i++)
                target = target.Replace(itemsToRemove[i], "");
            return target;
        }

        /// <summary> Utils function for reorder to integer. Return length between a and b.</summary>
        private static int Order(ref int a, ref int b)
        {
            int min = Mathf.Min(a, b);
            int max = Mathf.Max(a, b);
            a = min;
            b = max;
            return b - a;
        }

        /// <summary> Returns useful indexes of the text selection. </summary>
        private static void GetIDs(this Text text, out int min, out int max, out int length)
        {
            min = Mathf.Min(text.selectID, text.carretID);
            max = Mathf.Max(text.selectID, text.carretID);
            length = max - min;
        }

        /// <summary> Set selectID and carretID at id value. </summary>
        private static void SetIDs(this Text text, int id)
        {
            text.selectID = text.carretID = id;
            text.CallOnChangeSelect();
        }

        /// <summary> Set text ids to select characters from id to id + length. </summary>
        private static void SetIDs(this Text text, int id, int length)
        {
            text.selectID = id;
            text.carretID = id + length;
            text.CallOnChangeSelect();
        }

    }
}
