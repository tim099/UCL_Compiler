#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UCL.CompilerLib
{
    public class UCL_AssetReplaceWindow : EditorWindow
    {
        public UCL_AssetReplace m_AssetReplace;
        public AssetReplaceData m_AssetReplaceData = null;
        public static UCL_AssetReplaceWindow Instance = null;
        protected Rect m_GridRegion = new Rect();
        protected Vector2 m_ScrollPos = default;
        protected string m_CSVPath;

        [MenuItem("UCL/Tools/AssetReplaceWindow")]
        public static void ShowWindow()
        {
            Instance = EditorWindow.GetWindow<UCL_AssetReplaceWindow>("AssetReplaceWindow");
            Instance.Show();
            Instance.Init(new AssetReplaceData());
        }
        public static void ShowWindow(UCL_AssetReplace iAssetReplace)
        {
            Instance = EditorWindow.GetWindow<UCL_AssetReplaceWindow>("AssetReplaceWindow");
            Instance.Show();
            Instance.SetAssetReplace(iAssetReplace);
        }
        virtual public void Init(AssetReplaceData iData)
        {
            m_AssetReplaceData = iData;
        }
        virtual public void SetAssetReplace(UCL_AssetReplace iAssetReplace)
        {
            m_AssetReplace = iAssetReplace;
            if (m_AssetReplace == null)
            {
                Init(new AssetReplaceData());
                return;
            }
            m_CSVPath = iAssetReplace.m_CSVPath;
            Init(iAssetReplace.m_AssetReplaceData);
        }
        virtual protected void OnGUI()
        {
            if (m_AssetReplaceData == null)
            {
                Init(new AssetReplaceData());
            }
            m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);
            GUILayout.BeginVertical();
            using (var scope = new GUILayout.VerticalScope("box"))
            {
                var aAssetReplace = EditorGUILayout.ObjectField(m_AssetReplace, typeof(UCL_AssetReplace), true) as UCL_AssetReplace;
                if (aAssetReplace != m_AssetReplace)
                {
                    SetAssetReplace(aAssetReplace);
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label("CSV", GUILayout.Width(40));
                m_CSVPath = GUILayout.TextField(m_CSVPath);
                if (GUILayout.Button("Explore CSV", GUILayout.Width(120f)))
                {
                    if (string.IsNullOrEmpty(m_CSVPath))
                    {
                        m_CSVPath = Application.dataPath.Replace("Assets", string.Empty);
                    }
                    m_CSVPath = EditorUtility.OpenFilePanel("Explore CSV", m_CSVPath, "csv");
                    m_AssetReplaceData.LoadReplaceDataFromCSV(m_CSVPath);
                }
                if (GUILayout.Button("Reload CSV", GUILayout.Width(120f)))
                {
                    m_AssetReplaceData.LoadReplaceDataFromCSV(m_CSVPath);
                }
                if (GUILayout.Button("Save CSV", GUILayout.Width(120f)))
                {
                    m_AssetReplaceData.SaveReplaceDataToCSV(m_CSVPath);
                }
                GUILayout.EndHorizontal();
            }
            using (var scope = new GUILayout.VerticalScope("box"))
            {
                m_AssetReplaceData.OnGUI();
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();





            if (Event.current.type == EventType.Repaint)
            {
                var newRgn = GUILayoutUtility.GetLastRect();
                if (newRgn != m_GridRegion)
                {
                    m_GridRegion = newRgn;
                    Repaint();
                }
            }
        }
    }

}

#endif