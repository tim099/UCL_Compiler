using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace UCL.CompilerLib {
    [UCL.Core.ATTR.EnableUCLEditor]
    public class UCL_ScriptRefactor : MonoBehaviour {
        [System.Serializable]
        public struct ReplaceData {
            public ReplaceData(string iReplaceTarget, string iReplaceValue) {
                m_ReplaceTarget = iReplaceTarget;
                m_ReplaceValue = iReplaceValue;
            }
            public string m_ReplaceTarget;
            public string m_ReplaceValue;
        }
        /// <summary>
        /// Root directory to refactor
        /// </summary>
        public string m_RefactorRoot = string.Empty;

        /// <summary>
        /// Refactor files extesion
        /// </summary>
        public string m_Extension = "cs";

        /// <summary>
        /// files to exclude from refactor
        /// </summary>
        public List<string> m_ExcludeFiles = new List<string>();
        /// <summary>
        /// Replace the string in scripts
        /// </summary>
        public List<ReplaceData> m_StringReplacement = new List<ReplaceData>();
        [UCL.Core.ATTR.UCL_FunctionButton]
        virtual public void Refactor() {
            var aFilePaths = System.IO.Directory.GetFiles(m_RefactorRoot, "*." + m_Extension, System.IO.SearchOption.AllDirectories);
            int aTotalCount = aFilePaths.Length;
            int aFinishedCount = 0;
            HashSet<string> aExcludeFileSet = new HashSet<string>();
            foreach(var aExclude in m_ExcludeFiles) {
                aExcludeFileSet.Add(aExclude);
            }
            foreach(var aFilePath in aFilePaths) {
                float aProgress = ++aFinishedCount / (float)aTotalCount;
                var aFileName = UCL.Core.FileLib.Lib.GetFileName(aFilePath);
                //var aFoldersName = UCL.Core.FileLib.Lib.GetFoldersName(aFilePath);
                if(aExcludeFileSet.Contains(aFileName)) {
                    continue;//Skip this file
                }
                string aData = File.ReadAllText(aFilePath);
                

                StringBuilder aStringBuilder = new StringBuilder(aData);
                foreach(var aReplace in m_StringReplacement) {
                    aStringBuilder.Replace(aReplace.m_ReplaceTarget, aReplace.m_ReplaceValue);
                }
                string aResult = aStringBuilder.ToString();
                if(!aResult.Equals(aData)) {
                    Debug.LogWarning(aFileName + ",Replace:" + aData);
                    File.WriteAllText(aFilePath, aResult);
                }
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar("Refactor", aFilePath, aProgress);
#endif
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }
#if UNITY_EDITOR
        /// <summary>
        /// Explore Refactor root folder
        /// </summary>
        [UCL.Core.ATTR.UCL_FunctionButton]
        public void ExploreRefactorRoot() {
            if(string.IsNullOrEmpty(m_RefactorRoot)) {
                var path = UnityEditor.AssetDatabase.GetAssetPath(this);
                m_RefactorRoot = Core.FileLib.Lib.RemoveFolderPath(path, 1);
            }
            var aDir = Core.FileLib.EditorLib.OpenFolderExplorer(m_RefactorRoot);
            if(!string.IsNullOrEmpty(aDir)) {
                m_RefactorRoot = aDir;
            }
        }
#endif
    }
}