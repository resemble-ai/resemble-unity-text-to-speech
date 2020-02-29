using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Resemble;

namespace Resemble.GUIEditor
{
    public static class TextExtend
    {

        //Exposed

        /// <summary> Copy the substring between a and b to the clipboard. </summary>
        public static void Copy(this Text text, int a, int b)
        {
            int l = Order(ref a, ref b);
            GUIUtility.systemCopyBuffer = text.userString.Substring(a, l);
        }

        /// <summary> Cut the substring between a and b to the clipboard. </summary>
        public static void Cut(this Text text, ref int a, ref int b)
        {
            int l = Order(ref a, ref b);
            GUIUtility.systemCopyBuffer = text.userString.Substring(a, l);
            RemoveChars(text, a, l);
            b = a;
        }

        /// <summary> Override characters between a and b by the string in the clipboard. </summary>
        public static void Paste(this Text text, ref int a, ref int b)
        {
            int l = Order(ref a, ref b);
            RemoveChars(text, a, l);
            string pastString = CleanString(GUIUtility.systemCopyBuffer, "\r", System.Environment.NewLine);
            AddChars(text, a, pastString);
            a = b;
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

    }
}
