using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.CompilerLib.Demo
{
    [UCL.Core.ATTR.EnableUCLEditor]
    public class UCL_AssetReplaceTest : MonoBehaviour
    {
        /// <summary>
        /// Field for asset replace test
        /// </summary>
        public Sprite m_Sprite = null;
        public AudioClip m_Clip = null;

        [UCL.Core.ATTR.UCL_DrawOnGUI]
        public void DrawInspectorGUI()
        {
            UCL.Core.UI.UCL_GUILayout.DrawSpriteFixedWidth(m_Sprite, 128);
            if (GUILayout.Button("Test"))
            {
                Debug.LogWarning("Test!!");
            }
        }

    }
}

