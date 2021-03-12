using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

namespace UCL.CompilerLib
{
    public class UCL_FindReferencesWindow : EditorWindow
    {
        static UCL_FindReferencesWindow Ins = null;
        const int MaxSearchLayer = 10;
        const string FindReferencesMenuItem = "UCL/Tools/Find References In Project";
        public string m_SearchRoot = string.Empty;
        List<Object> m_ReferencesList = new List<Object>();
        Object m_Target = null;

        [MenuItem(FindReferencesMenuItem, false, 20)]
        static public void ShowWindow()
        {
            //Menu.SetChecked(FindReferencesMenuItem, Ins != null);
            Ins = EditorWindow.GetWindow<UCL_FindReferencesWindow>();
            Ins.Init(Selection.activeObject);
        }

        [MenuItem("Assets/UCL Tools/Find References In Project", false, 20)]
        static public void ShowWindowAndFindReference()
        {
            ShowWindow();
            Ins.SearchByGUID();
        }
        public void SearchComponentScriptReference(System.Type iComponentType)
        {
            Debug.LogWarning("SearchComponentScriptReference() iComponentType:"+ iComponentType.FullName);
            int aTotalCount = 0;
            int aFinishedCount = 0;
            int aLen = Application.dataPath.Length - 6;
            var aPrefabsPath = Directory.GetFiles(Path.Combine(Application.dataPath, m_SearchRoot), "*.prefab", SearchOption.AllDirectories);
            aTotalCount += aPrefabsPath.Length;
            {
                foreach (var aPrefabPath in aPrefabsPath)
                {
                    try
                    {
                        float aProgress = ++aFinishedCount / (float)aTotalCount;
                        string aPath = aPrefabPath.Substring(aLen);
                        UnityEditor.EditorUtility.DisplayProgressBar("Search", aPath, aProgress);
                        var aPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(aPath);
                        if (aPrefab != null)
                        {
                            if (CheckComponentReference(aPrefab, iComponentType))
                            {
                                m_ReferencesList.Add(aPrefab);
                            }
                        }

                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            UnityEditor.EditorUtility.ClearProgressBar();
        }
        bool CheckReferenceObject(object iObject, UnityEngine.Object iTarget, int iLayer)
        {
            if (iLayer > MaxSearchLayer)
            {
                return false;
            }
            if (iObject == null)
            {
                return false;
            }
            var aType = iObject.GetType();
            if (aType.IsNumber())
            {
                return false;
            }
            if (iObject is string)
            {
                return false;
            }
            if(iObject is UnityEngine.Object)
            {
                if (iObject.Equals(iTarget)) return true;

            }
            if (iObject is IList)
            {
                IList aList = iObject as IList;
                for (int i = 0; i < aList.Count; i++)
                {
                    var aObj = aList[i];
                    if(CheckReferenceObject(aObj, iTarget, iLayer + 1))
                    {
                        return true;
                    }
                }
                return false;
            }

            var aFields = aType.GetAllFieldsUntil(typeof(Component), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var aField in aFields)
            {
                object aObj = aField.GetValue(iObject);
                if (aObj == null) continue;
                UnityEngine.Object aUnityObj = aObj as UnityEngine.Object;
                if (aObj != null)
                {

                }
                if (CheckReferenceObject(aObj, iTarget, iLayer + 1))
                {
                    return true;
                }
            }

            return false;
        }
        public void SearchPrefabReference(UnityEngine.GameObject iPrefab)
        {

        }
        public void SearchByGUID()
        {
            m_ReferencesList.Clear();
            var aSerializationMode = EditorSettings.serializationMode;
            EditorSettings.serializationMode = SerializationMode.ForceText;
            var aGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_Target));
            Regex aRegex = new Regex(aGUID, RegexOptions.Compiled);
            List<string> aPaths = new List<string>();
            aPaths.Append(Directory.GetFiles(Path.Combine(Application.dataPath, m_SearchRoot), "*.prefab", SearchOption.AllDirectories));
            aPaths.Append(Directory.GetFiles(Path.Combine(Application.dataPath, m_SearchRoot), "*.unity", SearchOption.AllDirectories));
            aPaths.Append(Directory.GetFiles(Path.Combine(Application.dataPath, m_SearchRoot), "*.mat", SearchOption.AllDirectories));
            aPaths.Append(Directory.GetFiles(Path.Combine(Application.dataPath, m_SearchRoot), "*.asset", SearchOption.AllDirectories));
            for(int i = 0; i < aPaths.Count; i++)
            {
                float aProgress = (float)i / aPaths.Count;
                string aPath = aPaths[i];
                string aStr = File.ReadAllText(aPath);
                if (aRegex.IsMatch(aStr))
                {
                    string aAssetPath = UCL.Core.FileLib.Lib.ConvertToAssetsPath(aPath);
                    //Debug.LogError("aAssetPath:" + aAssetPath + ",aPath:" + aPath);
                    m_ReferencesList.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(aAssetPath));
                }
                if(UnityEditor.EditorUtility.DisplayCancelableProgressBar("Search:"+ (100f * aProgress).ToString("N2")+"%", aPath, aProgress))
                {
                    break;
                }
            }


            EditorSettings.serializationMode = aSerializationMode;
            UnityEditor.EditorUtility.ClearProgressBar();
        }
        public void SearchByReflection()
        {
            m_ReferencesList.Clear();
            var aType = m_Target.GetType();
            Debug.LogWarning("m_Target.GetType():" + aType.FullName);
            if(m_Target is UnityEditor.MonoScript)
            {
                UnityEditor.MonoScript aScript = m_Target as UnityEditor.MonoScript;
                System.Type aClassType = aScript.GetClass();
                if (aClassType.IsSubclassOf(typeof(Component))){
                    SearchComponentScriptReference(aClassType);
                    return;
                }
                //else scriptable object!!
            }
            if (m_Target is UnityEngine.GameObject)
            {
                UnityEngine.GameObject aObj = m_Target as UnityEngine.GameObject;
                if (aObj != null)
                {
                    SearchPrefabReference(aObj);
                    return;
                }
            }
            //EditorSettings.serializationMode = SerializationMode.ForceText;

            int aTotalCount = 0;
            int aFinishedCount = 0;
            int aLen = Application.dataPath.Length - 6;
            var aPrefabsPath = Directory.GetFiles(Path.Combine(Application.dataPath, m_SearchRoot), "*.prefab", SearchOption.AllDirectories);
            aTotalCount += aPrefabsPath.Length;
            var aScriptableObjectsPath = Directory.GetFiles(Path.Combine(Application.dataPath, m_SearchRoot), "*.asset", SearchOption.AllDirectories);
            aTotalCount += aScriptableObjectsPath.Length;
            {
                foreach (var aPrefabPath in aPrefabsPath)
                {
                    try
                    {
                        float aProgress = ++aFinishedCount / (float)aTotalCount;
                        string aPath = aPrefabPath.Substring(aLen);
                        UnityEditor.EditorUtility.DisplayProgressBar("Search", aPath, aProgress);
                        var aPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(aPath);
                        if (aPrefab != null)
                        {
                            if (CheckReference(aPrefab, m_Target))
                            {
                                m_ReferencesList.Add(aPrefab);
                            }
                        }

                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }

                }
            }
            {

                foreach (var aScriptableObjectPath in aScriptableObjectsPath)
                {
                    try
                    {
                        float aProgress = ++aFinishedCount / (float)aTotalCount;
                        string aPath = aScriptableObjectPath.Substring(aLen);
                        UnityEditor.EditorUtility.DisplayProgressBar("Search", aPath, aProgress);
                        var aScriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(aPath);
                        if (aScriptableObject != null)
                        {
                            if (CheckReference(aScriptableObject, m_Target))
                            {
                                m_ReferencesList.Add(aScriptableObject);
                            }
                        }

                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }

                }
            }
            UnityEditor.EditorUtility.ClearProgressBar();
        }
        /// <summary>
        /// return true if iObject has reference to iTarget
        /// </summary>
        /// <param name="iObject"></param>
        /// <param name="iTarget"></param>
        /// <returns></returns>
        bool CheckReference(UnityEngine.Object iObject, UnityEngine.Object iTarget)
        {
            //var aTargetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(iTarget));
            SerializedObject aSerializedObject = new SerializedObject(iObject);
            var aIterator = aSerializedObject.GetIterator();
            while (aIterator.Next(true))
            {
                //Debug.LogWarning("aIterator.propertyType:" + aIterator.propertyType.ToString());
                if (aIterator.propertyType == SerializedPropertyType.ObjectReference && aIterator.objectReferenceValue != null)
                {
                    //Debug.LogWarning("aIterator.objectReferenceValue:" + aIterator.objectReferenceValue.ToString());
                    if (aIterator.objectReferenceValue.Equals(iTarget))
                    {
                        return true;
                    }
                }
                //if(aIterator.propertyType == SerializedPropertyType.ObjectReference && aIterator.objectReferenceValue != null)
                //{
                //    var aGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(aIterator.objectReferenceValue));
                //    Debug.LogWarning("aTargetGUID:" + aTargetGUID + ",aGUID:" + aGUID+ 
                //        ",objectReferenceValue:" + aIterator.objectReferenceValue.ToString());
                //    //Debug.LogWarning("aIterator.objectReferenceValue:" + aIterator.objectReferenceValue.UCL_ToString());
                //    Debug.LogWarning("objectReferenceValue.GetInstanceID:" + aIterator.objectReferenceValue.GetInstanceID() + ",iTarget.GetInstanceID():" + iTarget.GetInstanceID());
                //    if (aTargetGUID == aGUID)
                //    {
                //        //Debug.LogWarning("aIterator.objectReferenceValue:" + aIterator.objectReferenceValue.ToString());
                //        return true;
                //    }
                //}

            }
            return false;
        }
        bool CheckComponentReference(GameObject iObject,System.Type iType)
        {
            if (iObject.GetComponent(iType) != null)
            {
                return true;
            }
            foreach(Transform aChild in iObject.transform)
            {
                if (CheckComponentReference(aChild.gameObject, iType))
                {
                    return true;
                }
            }
            return false;
        }
        public void Init(Object iTarget)
        {
            m_Target = iTarget;
            //m_SearchRoot = Application.dataPath.Replace("Assets", string.Empty);
        }
        void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("SearchRoot", GUILayout.Width(80));
            m_SearchRoot = GUILayout.TextField(m_SearchRoot);
            if (GUILayout.Button("Explore SearchRoot", GUILayout.Width(160f)))
            {
                int aLen = Application.dataPath.Length + 1;
                var aPath = Path.Combine(Application.dataPath, m_SearchRoot);
                aPath = EditorUtility.OpenFolderPanel("Explore SearchRoot", aPath, string.Empty);
                if (aPath.Length > aLen)
                {
                    m_SearchRoot = aPath.Substring(aLen);
                }else
                {
                    m_SearchRoot = string.Empty;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            bool aDoSearch = false;
            m_Target = EditorGUILayout.ObjectField("Target Object:", m_Target, typeof(Object), true);
            if (m_Target != null)
            {
                if (GUILayout.Button("Search",GUILayout.Width(80)))
                {
                    aDoSearch = true;
                    //SearchByReflection();
                }
            }
            GUILayout.EndHorizontal();

            if (m_ReferencesList.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Reference result");
                if (GUILayout.Button("Clear", GUILayout.Width(120f)))
                {
                    m_ReferencesList.Clear();
                }
                GUILayout.EndHorizontal();
                for (int i = 0; i < m_ReferencesList.Count; i++)
                {
                    var aObj = m_ReferencesList[i];
                    EditorGUILayout.ObjectField(aObj, typeof(Object), true);
                }
            }


            GUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint) Repaint();
            if (aDoSearch)
            {
                SearchByGUID();
            }
        }
    }
}

