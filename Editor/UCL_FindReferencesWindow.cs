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
        List<Object> m_ReferencesList = new List<Object>();
        [MenuItem("UCL/Tools/Find References", false, 20)]
        [MenuItem("Assets/UCL Tools/Find References", false, 20)]
        static public void ShowWindow()
        {
            EditorWindow.GetWindow<UCL_FindReferencesWindow>();
        }
        public void Search()
        {
            m_ReferencesList.Clear();
        }
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            Selection.activeObject = EditorGUILayout.ObjectField("Target Object:", Selection.activeObject, typeof(Object), true);
            if (Selection.activeObject != null)
            {
                if (GUILayout.Button("Search",GUILayout.Width(80)))
                {
                    Search();
                }
            }

            EditorGUILayout.EndHorizontal();



            //GUILayout.Label("Reference Result :");
            //foreach (Object obj in _referenceObjectList)
                //EditorGUILayout.ObjectField(obj, typeof(Object), true);
            EditorGUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint) Repaint();
        }
    }
}

