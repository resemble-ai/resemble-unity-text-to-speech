using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble
{
    public static class ClipExtend
    {

        public static string GetSavePath(this Clip clip)
        {
            return GetSaveFolder(clip) + clip.clipName + ".wav";
        }

        public static string GetSaveFolder(this Clip clip)
        {
            string path = AssetDatabase.GetAssetPath(clip.speech);

            //Adapts the path according to the settings
            switch (Settings.pathMethode)
            {
                default:
                case Settings.PathMethode.SamePlace:
                    path = RemoveFilenameFromPath(path);
                    break;
                case Settings.PathMethode.Absolute:
                    path = Settings.folderPathA.Remove(0, Application.dataPath.Length - 6);
                    break;
                case Settings.PathMethode.MirrorHierarchy:
                    path = RemoveFilenameFromPath(path);
                    string dataPath = Application.dataPath;
                    string folderB = Settings.folderPathB.Remove(0, dataPath.Length);
                    string folderA = Settings.folderPathA.Remove(0, dataPath.Length);
                    if (path.Contains(folderB))
                        path = path.Replace(folderB, folderA);
                    break;
            }

            //Add sub folder and extension
            if (Settings.useSubDirectory)
                path += "/" + clip.speech.name + "/";
            else
                path += "/";

            //Return result
            return path;
        }

        private static string RemoveFilenameFromPath(string path)
        {
            return path.Remove(path.LastIndexOf("/"));
        }
    }
}