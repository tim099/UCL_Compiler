#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace UCL.CompilerLib
{
    public class UCL_MissingReferencePopUpWindow : PopupWindowContent
    {
        protected Vector2 m_ScrollPos = default;
        protected List<UnityEngine.Object> m_MissingReferenceList = null;
        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 200);
        }
        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("MissingReference");
            m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);
            if (m_MissingReferenceList != null)
            {
                for (int i = 0; i < m_MissingReferenceList.Count; i++)
                {
                    var aMissingReference = m_MissingReferenceList[i];
                    m_MissingReferenceList[i] = EditorGUILayout.ObjectField(aMissingReference, aMissingReference.GetType(), true);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        public override void OnOpen()
        {
        }

        public override void OnClose()
        {
            //Debug.LogWarning("MissingReferencePopUpWindow OnClose()");
        }
        public UCL_MissingReferencePopUpWindow(List<UnityEngine.Object> iMissingReferenceList)
        {
            m_MissingReferenceList = iMissingReferenceList;
        }
    }
}
#endif