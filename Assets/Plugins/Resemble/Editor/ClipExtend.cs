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
            return GetSaveFolder(clip) + clip.name + ".wav";
        }

        public static string GetSaveFolder(this Clip clip)
        {
            string path = AssetDatabase.GetAssetPath(clip.set);

            //Adapts the path according to the settings
            switch (Settings.instance.pathMethode)
            {
                default:
                case Settings.PathMethode.SamePlace:
                    path = RemoveFilenameFromPath(path);
                    break;
                case Settings.PathMethode.Absolute:
                    path = Settings.instance.folderPathA.Remove(0, Application.dataPath.Length);
                    break;
                case Settings.PathMethode.MirrorHierarchy:
                    path = RemoveFilenameFromPath(path);
                    string dataPath = Application.dataPath;
                    string folderB = Settings.instance.folderPathB.Remove(0, dataPath.Length);
                    string folderA = Settings.instance.folderPathA.Remove(0, dataPath.Length);
                    if (path.Contains(folderB))
                        path = path.Replace(folderB, folderA);
                    break;
            }

            //Add sub folder and extension
            if (Settings.instance.useSubFolder)
                path += "/" + clip.set.name + "/";
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