using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    /// <summary> Utility class to quickly open a web page related to the plugin. </summary>
    public static class WebPageExtensions
    {

        /// <summary> Opens a web page in the user's default browser. </summary>
        /// <param name="webPage">Page to open.</param>
        public static void Open(this WebPage webPage)
        {
            Application.OpenURL(urls[webPage]);
        }

        /// <summary> Opens a web page in the user's default browser. </summary>
        /// <param name="webPage">Page to open.</param>
        /// <param name="addtional">Part of the url that will be added at the end of the web page url.</param>
        public static void Open(this WebPage webPage, string addtional)
        {
            Application.OpenURL(urls[webPage] + "/" + addtional);
        }

        /// <summary> List all plugin related webpages. </summary>
        private static Dictionary<WebPage, string> urls = new Dictionary<WebPage, string>()
        {
            { WebPage.ResembleHome, "https://www.resemble.ai"},
            { WebPage.ResembleProjects, "https://app.resemble.ai/projects"},
            { WebPage.ResembleAPIDoc, "https://app.resemble.ai/docs"},
            { WebPage.ResembleToken, "https://app.resemble.ai/account/api"},
            { WebPage.ResembleSignUp, "https://app.resemble.ai/users/sign_up"},
            { WebPage.PluginDoc, "https://www.resemble.ai/unity-docs/"},
            { WebPage.PluginScriptingDoc, "https://www.resemble.ai/unity-scripting/"},
            { WebPage.PluginSettings, "https://www.resemble.ai/unity-docs#settings"},
            { WebPage.PluginWindow, "https://www.resemble.ai/unity-docs#window"},
            { WebPage.PluginCharacterSet, "https://www.resemble.ai/unity-docs#characterset"},
            { WebPage.PluginClip, "https://www.resemble.ai/unity-docs#cliphelp"},
            { WebPage.PluginGithub, "https://github.com/resemble-ai/resemble-unity-text-to-speech/"},
            { WebPage.PluginGithubPackage, "https://github.com/resemble-ai/resemble-unity-text-to-speech/blob/master/Output/ResemblePlugin.unitypackage"},
        };
    }

    /// <summary> Enum of all pages related to the plugin. </summary>
    /// <example> Example : <c>WebPage.ResembleHome.Open();</c> </example>
    public enum WebPage
    {
        ResembleHome,
        ResembleProjects,
        ResembleAPIDoc,
        ResembleToken,
        ResembleSignUp,
        PluginDoc,
        PluginScriptingDoc,
        PluginSettings,
        PluginWindow,
        PluginCharacterSet,
        PluginClip,
        PluginGithub,
        PluginGithubPackage,
    }
}