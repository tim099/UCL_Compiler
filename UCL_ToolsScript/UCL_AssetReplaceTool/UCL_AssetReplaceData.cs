using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Reflection;
using UCL.Core.CsvLib;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UCL.ToolsLib
{
    [System.Serializable]
    public class AssetReplaceData
    {
        const int MaxSearchLayer = 10;
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
            public ReplaceData(string iOriginPath, string iOriginAsset, string iReplacePath, string iReplaceAsset)
            {
                m_OriginPath = iOriginPath;
                m_OriginAsset = iOriginAsset;
                m_ReplacePath = iReplacePath;
                m_ReplaceAsset = iReplaceAsset;
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

            public string m_OriginAsset;
            public string m_ReplaceAsset;
        }
        public class ReplaceSetting
        {
            public ReplaceSetting(string iReplacePath)
            {
                m_ReplacePath = iReplacePath;
            }
            public void AddReplaceAsset(string iOriginAsset, string iReplaceAsset, string iReplacePath)
            {
                if (m_ReplaceDic == null)
                {
                    m_ReplaceDic = new Dictionary<string, string>();
                }
                if (!m_ReplaceDic.ContainsKey(iOriginAsset))
                {
                    m_ReplaceDic.Add(iOriginAsset, iReplaceAsset);
                }
                if (m_PathReplaceDic == null)
                {
                    m_PathReplaceDic = new Dictionary<string, string>();
                }
                if (!m_PathReplaceDic.ContainsKey(iOriginAsset))
                {
                    m_PathReplaceDic.Add(iOriginAsset, iReplacePath);
                }
            }
            public string GetReplaceAsset(string iOriginAsset)
            {
                if (m_ReplaceDic == null)
                {
                    return iOriginAsset;
                }
                if (!m_ReplaceDic.ContainsKey(iOriginAsset))
                {
                    return iOriginAsset;
                }

                return m_ReplaceDic[iOriginAsset];
            }
            public string GetReplacePath(string iOriginAsset)
            {
                if (m_PathReplaceDic == null || !m_PathReplaceDic.ContainsKey(iOriginAsset))
                {
                    return m_ReplacePath;
                }
                return m_PathReplaceDic[iOriginAsset];
            }
            string m_ReplacePath;
            Dictionary<string, string> m_ReplaceDic = null;
            Dictionary<string, string> m_PathReplaceDic = null;
        }
        public List<ReplaceData> m_ReplaceList = new List<ReplaceData>();
        public string m_ReplaceRoot = string.Empty;
        protected Dictionary<string, ReplaceSetting> m_ReplaceDic = new Dictionary<string, ReplaceSetting>();
        protected List<UnityEngine.Object> m_MissingReferenceList = new List<UnityEngine.Object>();
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
                    if (aRow.Count >= 4)
                    {
                        string aOriginAsset = aRow.Get(1); 
                        string aReplaceAsset = aRow.Get(3);
                        if(!string.IsNullOrEmpty(aOriginAsset) && !string.IsNullOrEmpty(aReplaceAsset))
                        {
                            m_ReplaceList.Add(new ReplaceData(aRow.Get(0), aOriginAsset, aRow.Get(2), aReplaceAsset));
                        }
                        else
                        {
                            m_ReplaceList.Add(new ReplaceData(aRow.Get(0), aRow.Get(1)));
                        }             
                    }
                    else
                    {
                        m_ReplaceList.Add(new ReplaceData(aRow.Get(0), aRow.Get(1)));
                    }
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
                    ReplaceSetting aReplaceSetting = null;
                    if (!m_ReplaceDic.ContainsKey(aID))
                    {
                        aReplaceSetting = new ReplaceSetting(aReplace.ReplacePath);
                        m_ReplaceDic.Add(aID, aReplaceSetting);
                    }
                    else
                    {
                        aReplaceSetting = m_ReplaceDic[aID];
                    }

                    if (!string.IsNullOrEmpty(aReplace.m_OriginAsset) && !string.IsNullOrEmpty(aReplace.ReplacePath))
                    {
                        aReplaceSetting.AddReplaceAsset(aReplace.m_OriginAsset, aReplace.m_ReplaceAsset, aReplace.ReplacePath);
                    }
                }
            }
            else
            {
                foreach (var aReplace in m_ReplaceList)
                {
                    string aID = AssetDatabase.AssetPathToGUID(aReplace.ReplacePath);//aAsset.GetInstanceID();
                    ReplaceSetting aReplaceSetting = null;
                    if (!m_ReplaceDic.ContainsKey(aID))
                    {
                        aReplaceSetting = new ReplaceSetting(aReplace.OriginPath);
                        m_ReplaceDic.Add(aID, aReplaceSetting);
                    }
                    else
                    {
                        aReplaceSetting = m_ReplaceDic[aID];
                    }
                    if (!string.IsNullOrEmpty(aReplace.m_OriginAsset) && !string.IsNullOrEmpty(aReplace.OriginPath))
                    {
                        aReplaceSetting.AddReplaceAsset(aReplace.m_ReplaceAsset, aReplace.m_OriginAsset, aReplace.OriginPath);
                    }
                }
            }
        }
        public void SaveReplaceDataToCSV(string iPath)
        {
            CSVData aData = new CSVData();
            foreach (var aReplace in m_ReplaceList)
            {
                bool aIsReplaceAsset = (!string.IsNullOrEmpty(aReplace.m_OriginAsset) || !string.IsNullOrEmpty(aReplace.m_ReplaceAsset));
                var aRow = aData.AddRow();
                if (aIsReplaceAsset)
                {
                    aRow.AddColume(aReplace.m_OriginPath);
                    aRow.AddColume(aReplace.m_OriginAsset);
                    aRow.AddColume(aReplace.m_ReplacePath);
                    aRow.AddColume(aReplace.m_ReplaceAsset);
                }
                else
                {
                    aRow.AddColume(aReplace.m_OriginPath);
                    aRow.AddColume(aReplace.m_ReplacePath);
                }

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
                    var aReplaceSetting = m_ReplaceDic[aGUID];
                    if (iData is Sprite)
                    {
                        Sprite aSprite = iData as Sprite;
                        string aSpriteName = aReplaceSetting.GetReplaceAsset(aSprite.name);
                        //Debug.LogError("aSprite.name:" + aSprite.name + ",aSpriteName:" + aSpriteName);
                        Object[] aNewSprites = AssetDatabase.LoadAllAssetsAtPath(aReplaceSetting.GetReplacePath(aSprite.name));
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
                        var aAsset = AssetDatabase.LoadAssetAtPath(aReplaceSetting.GetReplacePath(iData.name), iType);
                        return aAsset;
                    }
                }
            }
            return null;
        }
        object ReplaceOnObject(object iObject, int iLayer)
        {
            if(iLayer > MaxSearchLayer)
            {
                return null;
            }
            if (iObject == null)
            {
                return null;
            }
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
                        aResultObj = ReplaceOnObject(aObj, iLayer+1);
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
                else if(aObj is string)
                {
                    //Don't replace string!!
                }
                else if(aObj is IList)
                {
                    object aResult = ReplaceOnObject(aObj, iLayer + 1);
                    if (aResult != null)
                    {
                        aField.SetValue(iObject, aResult);
                        aIsModified = true;
                    }
                }
                else if (aField.FieldType.IsStructOrClass())
                {
                    object aResult = ReplaceOnObject(aObj, iLayer + 1);
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
        bool CheckMissingReference(UnityEngine.Object iObject)
        {
            SerializedObject aSerializedObject = new SerializedObject(iObject);
            var aIterator = aSerializedObject.GetIterator();
            while(aIterator.Next(true))
            {
                if (aIterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (aIterator.objectReferenceValue == null && aIterator.objectReferenceInstanceIDValue != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        bool CheckMissingReference(GameObject iGameObj) {
            if (iGameObj == null) return false;
            PrefabAssetType aAssetType = PrefabUtility.GetPrefabAssetType(iGameObj);
            var aComponents = iGameObj.GetComponents<Component>();
            bool aIsMissingReference = false;
            foreach (var aComponent in aComponents)
            {
                if (CheckMissingReference(aComponent))
                {
                    aIsMissingReference = true;
                    break;
                }
            }
            if (aIsMissingReference)
            {
                Debug.LogWarning("Find Missing refrence at:" + iGameObj.name);
                m_MissingReferenceList.Add(iGameObj);
            }
            foreach (Transform aTran in iGameObj.transform)
            {
                var aObj = aTran.gameObject;
                CheckMissingReference(aObj);
            }
            return aIsMissingReference;
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
                if (ReplaceOnObject(aComponent, 0) != null)
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
        virtual public void FindMissingReference() {
            m_MissingReferenceList.Clear();
            int aTotalCount = 0;
            int aFinishedCount = 0;
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
                        var aPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(aPath);
                        if (aPrefab != null)
                        {
                            CheckMissingReference(aPrefab);
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
                            if (CheckMissingReference(aScriptableObject))
                            {
                                m_MissingReferenceList.Add(aScriptableObject);
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
            if (m_MissingReferenceList.Count > 0)
            {
                Rect buttonRect = new Rect(-200f, 0f, 0f, 0f);
                PopupWindow.Show(buttonRect, new UCL_MissingReferencePopUpWindow(m_MissingReferenceList.Clone()));
            }
        }
        virtual public void Replace()
        {
            m_MissingReferenceList.Clear();
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
                                UCL.Core.EditorLib.EditorUtilityMapper.SetDirty(aPrefab);
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
                            if (ReplaceOnObject(aScriptableObject, 0) != null)
                            {
                                aIsUpdated = true;
                                UCL.Core.EditorLib.EditorUtilityMapper.SetDirty(aScriptableObject);
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
                aPath = UCL.Core.EditorLib.EditorUtilityMapper.OpenFolderPanel("Explore ReplaceRoot", aPath, string.Empty);
                if (aPath.Length > aLen)
                {
                    m_ReplaceRoot = aPath.Substring(aLen);
                }
                else if (aPath.Length == aLen)
                {
                    m_ReplaceRoot = string.Empty;
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
            if (GUILayout.Button("Find MissingReference", GUILayout.Width(180f)))
            {
                FindMissingReference();
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
                    aPath = UCL.Core.EditorLib.EditorUtilityMapper.OpenFilePanel("Explore", aPath, string.Empty);
                    if(aPath.Length > aLen)
                    {
                        aReplace.m_OriginPath = aPath.Substring(aLen);
                    }
                }
                aReplace.m_OriginPath = GUILayout.TextField(aReplace.m_OriginPath, GUILayout.MinWidth(250));
                GUILayout.Label("Asset", GUILayout.Width(60)); 
                aReplace.m_OriginAsset = GUILayout.TextField(aReplace.m_OriginAsset, GUILayout.MinWidth(150));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ReplacePath", GUILayout.Width(80));
                if (GUILayout.Button("Explore", GUILayout.Width(80)))
                {
                    int aLen = Application.dataPath.Length + 1;
                    var aPath = Path.Combine(Application.dataPath, aReplace.m_ReplacePath);
                    aPath = UCL.Core.EditorLib.EditorUtilityMapper.OpenFilePanel("Explore", aPath, string.Empty);
                    if (aPath.Length > aLen)
                    {
                        aReplace.m_ReplacePath = aPath.Substring(aLen);
                    }
                }
                aReplace.m_ReplacePath = GUILayout.TextField(aReplace.m_ReplacePath, GUILayout.MinWidth(250));
                GUILayout.Label("Asset", GUILayout.Width(60));
                aReplace.m_ReplaceAsset = GUILayout.TextField(aReplace.m_ReplaceAsset, GUILayout.MinWidth(150));
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
