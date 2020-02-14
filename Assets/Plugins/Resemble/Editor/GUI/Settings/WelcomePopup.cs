using UnityEditor;

namespace Resemble
{
    /// <summary> Open preferences window on the first load </summary>
    public class WelcomePopup
    {
        [InitializeOnLoadMethod]
        private static void Load()
        {
            if (Settings.instance.showWelcomePopup)
            {
                Settings.instance.showWelcomePopup = false;
                Settings.SetDirty();
                Settings.OpenWindow();
                RessembleSettingsProvider.pageID = 2;
            }
        }
    }
}