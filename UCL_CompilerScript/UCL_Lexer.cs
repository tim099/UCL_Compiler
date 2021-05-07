using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UCL.CompilerLib
{
    /// <summary>
    /// UCL_Lexer takes text and breaks it up into tokens
    /// </summary>
    public class UCL_Lexer
    {
        public class Token
        {
            /// <summary>
            /// Try Parse token from iReader
            /// return true if success
            /// </summary>
            /// <param name="iReader"></param>
            /// <returns></returns>
            virtual public bool TryParse(StringReader iReader)
            {
                return false;
            }
            public int m_ID;
            public string m_Type;
            public int m_Line;
            public string m_Symbol;
        }
    }
}