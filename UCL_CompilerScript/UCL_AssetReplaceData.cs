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
                var aType = aComponent.GetType();

                var aFields = aType.GetAllFieldsUntil(typeof(Component), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var aField in aFields)
                {
                    //var aFieldType = aField.FieldType;
                    var aData = aField.GetValue(aComponent) as UnityEngine.Object;

                    if (aData != null)
                    {
                        string aGUID;
                        long aFile;

                        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(aData, out aGUID, out aFile))
                        {
                            //int aID = aData.GetInstanceID();
                            //Debug.LogWarning("Find aData:" + aGUID);
                            if (m_ReplaceDic.ContainsKey(aGUID))
                            {
                                aIsModified = true;
                                
                                if (aData is Sprite)
                                {
                                    Sprite aSprite = aData as Sprite;
                                    Object[] aNewSprites = AssetDatabase.LoadAllAssetsAtPath(m_ReplaceDic[aGUID]);
                                    //Debug.LogError("aSprite.name:" + aSprite.name+ ",aNewSprites:"+ aNewSprites.UCL_ToString());
                                    if (aNewSprites.Length > 0)
                                    {
                                        Sprite aNewSprite = null;
                                        for (int i = 0; i < aNewSprites.Length; i++)
                                        {
                                            aNewSprite = aNewSprites[i] as Sprite;
                                            if (aNewSprite != null && aNewSprite.name == aSprite.name)
                                            {
                                                break;
                                            }
                                        }
                                        if (aNewSprite != null)
                                        {
                                            aField.SetValue(aComponent, aNewSprite);
                                        }
                                    }
                                    
                                }
                                else
                                {
                                    var aAsset = AssetDatabase.LoadAssetAtPath(m_ReplaceDic[aGUID], aField.FieldType);
                                    aField.SetValue(aComponent, aAsset);
                                }
                                //Debug.LogWarning("Replace aData:" + aData.name + "," + m_ReplaceDic[aGUID]);
                            }
                        }

                    }
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
            bool aIsUpdated = false;
            int aLen = Application.dataPath.Length - 6;
            var aFilesPath = Directory.GetFiles(Path.Combine(Application.dataPath, m_ReplaceRoot), "*.prefab", SearchOption.AllDirectories);
            int aTotalCount = aFilesPath.Length;
            int aFinishedCount = 0;
            foreach (var aPrefabPath in aFilesPath)
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
                catch(System.Exception e)
                {
                    Debug.LogError(e);
                }

            }
            UnityEditor.EditorUtility.ClearProgressBar();
            if (aIsUpdated) AssetDatabase.SaveAssets();
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
