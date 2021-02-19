using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UCL.CompilerLib
{
    [System.Serializable]
    public class AssetReplaceData
    {
        [System.Serializable]
        public class ReplaceData
        {
            public ReplaceData()
            {
                m_OriginPath = string.Empty;
                m_ReplacePath = string.Empty;
            }
            public ReplaceData(string iOriginPath, string iReplacePath)
            {
                m_OriginPath = iOriginPath;
                m_ReplacePath = iReplacePath;
            }
            public string OriginPath
            {
                get { return Path.Combine("Assets", m_OriginPath); }
            }
            public string ReplacePath
            {
                get { return Path.Combine("Assets", m_ReplacePath); }
            }
            /// <summary>
            /// Origin asset path
            /// </summary>
            public string m_OriginPath;

            /// <summary>
            /// Replace asset path
            /// </summary>
            public string m_ReplacePath;
        }
        public List<ReplaceData> m_ReplaceList = new List<ReplaceData>();
        public string m_ReplaceRoot = string.Empty;
        protected Dictionary<string, string> m_ReplaceDic = new Dictionary<string, string>();
#if UNITY_EDITOR
        public void LoadReplaceDataFromCSV(string iPath)
        {
            if (!File.Exists(iPath))
            {
                Debug.LogError("LoadReplaceDataFromCSV file not exist:" + iPath);
                return;
            }
            m_ReplaceList.Clear();
            CSVData aData = new CSVData(File.ReadAllText(iPath));
            for(int i = 0; i < aData.Count; i++)
            {
                var aRow = aData.GetRow(i);
                if (aRow.Count >= 2)
                {
                    m_ReplaceList.Add(new ReplaceData(aRow.Get(0), aRow.Get(1)));
                }
            }
        }
        public void GenerateReplaceDictionary(bool iIsRevert)
        {
            m_ReplaceDic.Clear();
            if (!iIsRevert)
            {
                foreach(var aReplace in m_ReplaceList)
                {
                    string aID = AssetDatabase.AssetPathToGUID(aReplace.OriginPath);//aAsset.GetInstanceID();
                    if (!m_ReplaceDic.ContainsKey(aID))
                    {
                        //Debug.LogWarning("aID:" + aID);
                        m_ReplaceDic.Add(aID, aReplace.ReplacePath);
                    }
                }
            }
            else
            {
                foreach (var aReplace in m_ReplaceList)
                {
                    string aID = AssetDatabase.AssetPathToGUID(aReplace.ReplacePath);//aAsset.GetInstanceID();
                    if (!m_ReplaceDic.ContainsKey(aID))
                    {
                        //Debug.LogWarning("aID:" + aID);
                        m_ReplaceDic.Add(aID, aReplace.OriginPath);
                    }
                }
            }
        }
        public void SaveReplaceDataToCSV(string iPath)
        {
            CSVData aData = new CSVData();
            foreach (var aReplace in m_ReplaceList)
            {
                var aRow = aData.AddRow();
                aRow.AddColume(aReplace.m_OriginPath);
                aRow.AddColume(aReplace.m_ReplacePath);
            }
            File.WriteAllText(iPath, aData.ToCSV());
        }
        UnityEngine.Object CheckReplace(UnityEngine.Object iData, System.Type iType)
        {
            if (iData == null) return null;
            string aGUID;
            long aFile;

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(iData, out aGUID, out aFile))
            {
                if (m_ReplaceDic.ContainsKey(aGUID))
                {
                    if (iData is Sprite)
                    {
                        Sprite aSprite = iData as Sprite;
                        string aSpriteName = aSprite.name;
                        Object[] aNewSprites = AssetDatabase.LoadAllAssetsAtPath(m_ReplaceDic[aGUID]);
                        //Debug.LogError("aSprite.name:" + aSprite.name+ ",aNewSprites:"+ aNewSprites.UCL_ToString());
                        if (aNewSprites.Length > 0)
                        {
                            Sprite aNewSprite = null;
                            bool aFind = false;
                            for (int i = 0; i < aNewSprites.Length && !aFind; i++)
                            {
                                var aTmpSprite = aNewSprites[i] as Sprite;
                                if (aTmpSprite != null)
                                {
                                    aNewSprite = aTmpSprite;
                                    if (aNewSprite.name == aSpriteName)
                                    {
                                        aFind = true;
                                    }
                                }
                            }
                            return aNewSprite;
                        }

                    }
                    else
                    {
                        var aAsset = AssetDatabase.LoadAssetAtPath(m_ReplaceDic[aGUID], iType);
                        return aAsset;
                    }
                }
            }
            return null;
        }
        object ReplaceOnObject(object iObject)
        {
            bool aIsModified = false;
            if (iObject is IList)
            {
                IList aList = iObject as IList;
                //Debug.LogWarning("Replace List!!");
                for (int i = 0; i < aList.Count; i++)
                {
                    var aObj = aList[i];
                    object aResultObj = null;
                    if (aObj is UnityEngine.Object)
                    {
                        aResultObj = CheckReplace(aObj as UnityEngine.Object, iObject.GetType());
                    }
                    else
                    {
                        aResultObj = ReplaceOnObject(aObj);
                    }
                    if (aResultObj != null)
                    {
                        aIsModified = true;
                        aList[i] = aResultObj;
                    }
                }
                if(aIsModified) return aList;
                return null;
            }
            var aType = iObject.GetType();
            var aFields = aType.GetAllFieldsUntil(typeof(Component), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var aField in aFields)
            {
                object aObj = aField.GetValue(iObject);
                if (aObj == null) continue;
                //var aFieldType = aField.FieldType;
                var aData = aObj as UnityEngine.Object;

                if (aData != null)
                {
                    UnityEngine.Object aResult = CheckReplace(aData, aField.FieldType);
                    if (aResult != null)
                    {
                        aIsModified = true;
                        //Debug.LogWarning("aResult:" + aResult.name);
                        aField.SetValue(iObject, aResult);
                    }

                }
                else if(aObj is IList)
                {
                    object aResult = ReplaceOnObject(aObj);
                    if (aResult != null)
                    {
                        aField.SetValue(iObject, aResult);
                        aIsModified = true;
                    }
                }
                else if (aField.FieldType.IsStructOrClass())
                {
                    object aResult = ReplaceOnObject(aObj);
                    if (aResult != null)
                    {
                        //Debug.LogWarning("Replace struct:" + aField.Name);
                        aField.SetValue(iObject, aResult);
                        aIsModified = true;
                    }
                }
            }
            if (aIsModified) return iObject;

            return null;
        }
        bool ReplaceOnGameObject(GameObject iGameObj)
        {
            if (iGameObj == null) return false;
            PrefabAssetType aAssetType = PrefabUtility.GetPrefabAssetType(iGameObj);
            bool aIsModified = false;
            //Debug.LogWarning("iGameObj:" + iGameObj.name+ ",aAssetType:"+ aAssetType.ToString()+
                //",IsAnyPrefabInstanceRoot:" + PrefabUtility.IsAnyPrefabInstanceRoot(iGameObj));
            var aComponents = iGameObj.GetComponents<Component>();
            foreach (var aComponent in aComponents)
            {
                if (ReplaceOnObject(aComponent) != null)
                {
                    aIsModified = true;
                }
            }
            foreach (Transform aTran in iGameObj.transform)
            {
                var aObj = aTran.gameObject;
                if (ReplaceOnGameObject(aObj))
                {
                    aIsModified = true;
                }
            }
            return aIsModified;
        }
        virtual public void Replace()
        {
            int aTotalCount = 0;
            int aFinishedCount = 0;
            bool aIsUpdated = false;
            int aLen = Application.dataPath.Length - 6;
            var aPrefabsPath = Directory.GetFiles(Path.Combine(Application.dataPath, m_ReplaceRoot), "*.prefab", SearchOption.AllDirectories);
            aTotalCount += aPrefabsPath.Length;
            var aScriptableObjectsPath = Directory.GetFiles(Path.Combine(Application.dataPath, m_ReplaceRoot), "*.asset", SearchOption.AllDirectories);
            aTotalCount += aScriptableObjectsPath.Length;
            {
                foreach (var aPrefabPath in aPrefabsPath)
                {
                    try
                    {
                        float aProgress = ++aFinishedCount / (float)aTotalCount;
                        string aPath = aPrefabPath.Substring(aLen);
                        UnityEditor.EditorUtility.DisplayProgressBar("AssetReplace", aPath, aProgress);
                        //Debug.LogWarning("aPath:" + aPath);
                        var aPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(aPath);
                        if (aPrefab != null)
                        {
                            if (ReplaceOnGameObject(aPrefab))
                            {
                                aIsUpdated = true;
                                //Debug.LogWarning("Save Prefab!!");
                                EditorUtility.SetDirty(aPrefab);
                            }

                            //Debug.LogWarning("aComponents:" + aComponents.UCL_ToString());
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
                        UnityEditor.EditorUtility.DisplayProgressBar("AssetReplace", aPath, aProgress);
                        var aScriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(aPath);
                        if (aScriptableObject != null)
                        {
                            if (ReplaceOnObject(aScriptableObject) != null)
                            {
                                aIsUpdated = true;
                                EditorUtility.SetDirty(aScriptableObject);
                            }
                        }

                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }

                }
            }


            if (aIsUpdated) AssetDatabase.SaveAssets();
            UnityEditor.EditorUtility.ClearProgressBar();
            //Debug.LogWarning("aFilesPath:" + aFilesPath.UCL_ToString());
        }
        virtual public void ReplaceAssets()
        {
            GenerateReplaceDictionary(false);
            Replace();
        }
        virtual public void RevertAssets()
        {
            GenerateReplaceDictionary(true);
            Replace();
        }
        virtual public void OnGUI()
        {
            if (string.IsNullOrEmpty(m_ReplaceRoot))
            {
                m_ReplaceRoot = Application.dataPath.Replace("Assets", string.Empty);
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("ReplaceRoot",GUILayout.Width(80));

            m_ReplaceRoot = GUILayout.TextField(m_ReplaceRoot);
            if (GUILayout.Button("Explore ReplaceRoot", GUILayout.Width(160f)))
            {
                int aLen = Application.dataPath.Length + 1;
                var aPath = Path.Combine(Application.dataPath, m_ReplaceRoot);
                aPath = EditorUtility.OpenFolderPanel("Explore ReplaceRoot", aPath, string.Empty);
                if (aPath.Length > aLen)
                {
                    m_ReplaceRoot = aPath.Substring(aLen);
                }
                //m_ReplaceRoot = EditorUtility.OpenFolderPanel("Explore ReplaceRoot", m_ReplaceRoot, string.Empty);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Replace", GUILayout.Width(80f)))
            {
                ReplaceAssets();
            }
            if (GUILayout.Button("Revert", GUILayout.Width(80f)))
            {
                RevertAssets();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Add New ReplaceData"))
            {
                m_ReplaceList.Add(new ReplaceData());
            }

            ReplaceData aAssetReplaceData = null;
            foreach(var aReplace in m_ReplaceList)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("X", GUILayout.Width(40))){
                    aAssetReplaceData = aReplace;
                }
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("OriginPath", GUILayout.Width(80));
                if (GUILayout.Button("Explore", GUILayout.Width(80)))
                {
                    int aLen = Application.dataPath.Length + 1;
                    var aPath = Path.Combine(Application.dataPath, aReplace.m_OriginPath);
                    aPath = EditorUtility.OpenFilePanel("Explore", aPath, string.Empty);
                    if(aPath.Length > aLen)
                    {
                        aReplace.m_OriginPath = aPath.Substring(aLen);
                    }
                }
                aReplace.m_OriginPath = GUILayout.TextField(aReplace.m_OriginPath, GUILayout.MinWidth(250));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ReplacePath", GUILayout.Width(80));
                if (GUILayout.Button("Explore", GUILayout.Width(80)))
                {
                    int aLen = Application.dataPath.Length + 1;
                    var aPath = Path.Combine(Application.dataPath, aReplace.m_ReplacePath);
                    aPath = EditorUtility.OpenFilePanel("Explore", aPath, string.Empty);
                    if (aPath.Length > aLen)
                    {
                        aReplace.m_ReplacePath = aPath.Substring(aLen);
                    }
                }
                aReplace.m_ReplacePath = GUILayout.TextField(aReplace.m_ReplacePath, GUILayout.MinWidth(250));
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

            }
            if (aAssetReplaceData != null)
            {
                m_ReplaceList.Remove(aAssetReplaceData);
            }
        }
#endif
    }
}
