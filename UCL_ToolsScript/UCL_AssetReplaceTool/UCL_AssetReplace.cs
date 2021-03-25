using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UCL.ToolsLib
{
    [UCL.Core.ATTR.EnableUCLEditor]
    public class UCL_AssetReplace : MonoBehaviour
    {
        public string m_CSVPath;
        public AssetReplaceData m_AssetReplaceData;
#if UNITY_EDITOR
        [UCL.Core.ATTR.UCL_FunctionButton]
        public void OpenWindow()
        {
            UCL_AssetReplaceWindow.ShowWindow(this);
        }
#endif
    }
}