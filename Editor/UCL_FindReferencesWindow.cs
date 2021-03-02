using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
namespace UCL.CompilerLib
{
    public class UCL_FindReferencesWindow : EditorWindow
    {
        [MenuItem("UCL/Tools/Find References", false, 20)]
        static public void ShowWindow()
        {
            EditorWindow.GetWindow<UCL_FindReferencesWindow>();
        }
    }
}

