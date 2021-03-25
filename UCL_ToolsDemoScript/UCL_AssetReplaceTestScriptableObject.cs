using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.ToolsLib.Demo
{
    [CreateAssetMenu(fileName = "New AssetReplaceTest", menuName = "UCL_Demo/AssetReplaceTest")]
    public class UCL_AssetReplaceTestScriptableObject : ScriptableObject
    {
        public Sprite m_Sprite = null;
        public AudioClip m_Clip = null;
    }
}