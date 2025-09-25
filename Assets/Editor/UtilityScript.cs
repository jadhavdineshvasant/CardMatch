#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
#endif
using UnityEngine;

namespace CyberSpeed.ExtraTools
{
    public static class UtilityScript
    {
#if UNITY_EDITOR
        static string streamingAssetsPath = Application.persistentDataPath;
        static string productName = Application.productName;

        /// <summary>
        /// It clears the consoles current output
        /// </summary>
        [MenuItem("Extra Tools/Clear Console #q")] // CTRL + Q
        private static void ClearConsole()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            Type type = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        /// <summary>
        /// It opens current project's root location into an Explorer
        /// </summary>
        [MenuItem("Extra Tools/Open Project Folder %e")] // CTRL + E
        public static void ShowExplorer()
        {
            string path = Directory.GetParent(Application.dataPath).FullName;
            EditorUtility.RevealInFinder(path);
        }
#endif
    }
}