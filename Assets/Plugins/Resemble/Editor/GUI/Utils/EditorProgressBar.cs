using UnityEditor;
using System.Linq;
using System.Reflection;
using Resemble;

namespace Resemble.GUIEditor
{
    public static class EditorProgressBar
    {
        static MethodInfo display;
        static MethodInfo clear;
        static EditorProgressBar()
        {
            var type = typeof(Editor).Assembly.GetTypes().Where(t => t.Name == "AsyncProgressBar").FirstOrDefault();
            if (type != null)
            {
                display = type.GetMethod("Display");
                clear = type.GetMethod("Clear");
            }
        }

        public static void Display(string text, float progress)
        {
            if (display != null)
                display.Invoke(null, new object[] { text, progress });
        }

        public static void Clear()
        {
            if (clear != null)
                clear.Invoke(null, null);
        }
    }
}