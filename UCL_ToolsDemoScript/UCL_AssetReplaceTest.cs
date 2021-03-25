using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.ToolsLib.Demo
{
    [UCL.Core.ATTR.EnableUCLEditor]
    public class UCL_AssetReplaceTest : MonoBehaviour
    {
        [System.Serializable]
        public class TestData
        {
            public Sprite m_Sprite2 = null;
            public TestData2 m_TestData2;
        }
        [System.Serializable]
        public struct TestData2
        {
            public AudioClip m_Clip2;
        }
        /// <summary>
        /// Field for asset replace test
        /// </summary>
        public Sprite m_Sprite = null;
        public AudioClip m_Clip = null;
        public List<Sprite> m_Sprites = new List<Sprite>();
        public Sprite[] m_SpriteArray = null;
        public TestData m_TestData = new TestData();
        public TestData2 m_TestData2 = default;
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

