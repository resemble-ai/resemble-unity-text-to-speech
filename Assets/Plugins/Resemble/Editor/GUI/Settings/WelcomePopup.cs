using UnityEditor;
using Resemble;

namespace Resemble.GUIEditor
{
    /// <summary> Open preferences window on the first load </summary>
    public class WelcomePopup
    {
        private static int frameCount = 0;

        [InitializeOnLoadMethod]
        private static void Load()
        {
            frameCount = 0;
            EditorApplication.update += WaitSomeUpdate;
        }

        private static void WaitSomeUpdate()
        {
            frameCount++;
            if (frameCount >= 10)
            {
                EditorApplication.update -= WaitSomeUpdate;
                if (Settings.showWelcomePopup)
                {
                    Settings.showWelcomePopup = false;
                    Settings.OpenWindow();
                    RessembleSettingsProvider.pageID = 2;
                }
            }
        }
    }
}