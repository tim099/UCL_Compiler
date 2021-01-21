using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UCL.CompilerLib {
    public enum AccessModifier : int {
        Public = 0,
        Private,
        Protected,
        Internal,
    }
    public enum ClassModifier : int {
        None = 0,
        Static,
        Const,
        Partial,
    }
    public enum FieldType {
        Int,
        Float,
        Long,
        Double,
        String,
    }
    public class ClassModifierSet {
        public int Count { get { return m_ModifierSet.Count; } }
        public ClassModifierSet() {

        }
        public HashSet<ClassModifier> m_ModifierSet = new HashSet<ClassModifier>();
        public void AddModifier(ClassModifier iModifier) {
            if(m_ModifierSet.Contains(iModifier) || iModifier == ClassModifier.None) return;
            m_ModifierSet.Add(iModifier);
        }
        public void AddModifier(string iModifier) {
            AddModifier(iModifier.ToClassModifier());
        }
        public string ParseToString() {
            if(m_ModifierSet == null || m_ModifierSet.Count == 0) return string.Empty;
            string result = string.Empty;
            bool first = true;
            foreach(var aClassModifier in m_ModifierSet) {
                if(first) {
                    first = false;
                } else {
                    result += ' ';
                }
                result += aClassModifier.ParseToString();
            }
            return result;
        }
    }
    public class ClassFieldData {
        public AccessModifier m_AccessModifier = AccessModifier.Public;
        public ClassModifierSet m_ModifierSet = new ClassModifierSet();
        public string m_FieldName = string.Empty;
        public System.Type m_Type = typeof(object);
        public string m_TypeName = string.Empty;
        public HashSet<string> m_Summarys = new HashSet<string>();
        public object m_Value = null;

        public ClassFieldData(CSVRowData iColumeData) {
            m_FieldName = iColumeData.Get(1);
            ParseFieldData(2, iColumeData);
        }
        public void AddSummary(string iSummary) {
            if(string.IsNullOrEmpty(iSummary) || m_Summarys.Contains(iSummary)) return;
            m_Summarys.Add(iSummary);
        }
        public void ParseFieldData(int at, CSVRowData iColumeData) {
            string aTitle = iColumeData.Get(at++);
            switch(aTitle.ToLower()) {
                case "type": {
                        SetType(iColumeData.Get(at++));
                        break;
                    }
                case "accessmodifier": {
                        m_AccessModifier = iColumeData.Get(at++).ToAccessModifier();
                        break;
                    }
                case "classmodifier": {
                        m_ModifierSet.AddModifier(iColumeData.Get(at++));
                        break;
                    }
                case "value": {
                        SetValue(iColumeData.Get(at++));
                        break;
                    }
                case "summary": {
                        AddSummary(iColumeData.Get(at++));
                        break;
                    }
                    
            }
            if(at < iColumeData.Count) {
                ParseFieldData(at, iColumeData);
            }
        }
        public void SetValue(string iValue) {
            if(m_Type == typeof(object) || m_Type == typeof(string)) {
                m_Value = iValue;
                return;
            }
            if(m_Type.IsNumber()) {
                object res_val;
                if(Core.MathLib.Num.TryParse(iValue, m_Type, out res_val)) {
                    m_Value = res_val;
                } else {
                    m_Value = iValue;
                }
            }
        }

        public void SetValue(object iValue) {
            m_Value = iValue;
        }
        public void SetType(string iTypeName) {
            m_TypeName = iTypeName;
            m_Type = TypeLib.ConvertToType(iTypeName);
        }
        public void ConvertToString(StringBuilder iStringBuilder, string aLayerStr) {
            if(m_Summarys.Count > 0) {
                iStringBuilder.AppendLine(aLayerStr + "/// <summary>");
                foreach(var aSummary in m_Summarys) {
                    var aLines = aSummary.SplitByLine();
                    foreach(var aLine in aLines) {
                        iStringBuilder.Append(aLayerStr + "/// ");
                        iStringBuilder.AppendLine(aLine);
                    }
                }
                iStringBuilder.AppendLine(aLayerStr + "/// </summary>");
            }
            iStringBuilder.Append(aLayerStr + m_AccessModifier.ParseToString() + " ");
            if(m_ModifierSet.Count > 0) iStringBuilder.Append(m_ModifierSet.ParseToString() + " ");
            iStringBuilder.Append(m_TypeName + " ");
            iStringBuilder.Append(m_FieldName + " = ");
            if(m_TypeName == "string") {
                iStringBuilder.Append("\"" + m_Value + "\"");
            } else {
                iStringBuilder.Append(m_Value.ToString());
            }
            iStringBuilder.Append(";" + System.Environment.NewLine);
        }
    }
    public class ClassData {
        public AccessModifier m_AccessModifier = AccessModifier.Public;
        public ClassModifierSet m_ModifierSet = new ClassModifierSet();
        public Dictionary<string, ClassFieldData> m_ClassFieldDatas = new Dictionary<string, ClassFieldData>();
        public string m_NameSpace = string.Empty;
        public string m_ClassName = string.Empty;

        public ClassData() { }
        public ClassData(CSVData csv_data) {
            for(int i = 0, count = csv_data.Count; i < count; i++) {
                ParseCSVColume(csv_data.GetRow(i));
            }
        }
        #region Add
        virtual public void AddFieldData(ClassFieldData iClassFieldData) {
            if(m_ClassFieldDatas.ContainsKey(iClassFieldData.m_FieldName)) {                
                if(iClassFieldData.m_Summarys.Count > 0) {
                    var aField = m_ClassFieldDatas[iClassFieldData.m_FieldName];
                    foreach(var aSummary in iClassFieldData.m_Summarys) {
                        aField.AddSummary(aSummary);
                    }
                }
                
                Debug.LogWarning("AddFieldData FieldName:" + iClassFieldData.m_FieldName + ",already exist!!");
            } else {
                m_ClassFieldDatas.Add(iClassFieldData.m_FieldName, iClassFieldData);
            }
        }
        #endregion
        #region Convert
        virtual public string ConvertToString() {
            StringBuilder aStringBuilder = new StringBuilder();
            Stack<string> m_ConvertStack = new Stack<string>();
            int aLayer = 0;
            string aLayerStr = string.Empty;
            if(!string.IsNullOrEmpty(m_NameSpace)) {
                aStringBuilder.Append("namespace ");
                aStringBuilder.Append(m_NameSpace);
                aStringBuilder.AppendLine(" {");
                m_ConvertStack.Push(aLayerStr + "}" + System.Environment.NewLine);
                aLayerStr = new string('\t', ++aLayer);
            }
            aStringBuilder.Append(aLayerStr);
            aStringBuilder.Append(m_AccessModifier.ParseToString() + " ");
            if(m_ModifierSet.Count > 0) aStringBuilder.Append(m_ModifierSet.ParseToString() + " ");
            aStringBuilder.AppendLine("class " + m_ClassName + " {");
            m_ConvertStack.Push(aLayerStr + "}" + System.Environment.NewLine);
            aLayerStr = new string('\t', ++aLayer);
            //string aFieldLayerStr = aLayerStr + '\t';
            foreach(var aFieldName in m_ClassFieldDatas.Keys) {
                m_ClassFieldDatas[aFieldName].ConvertToString(aStringBuilder, aLayerStr);
            }

            while(m_ConvertStack.Count > 0) {
                aStringBuilder.Append(m_ConvertStack.Pop());
            }
            return aStringBuilder.ToString();
        }
        #endregion
        virtual public void ParseCSVColume(CSVRowData iColumeData) {
            if(iColumeData.Count == 0) return;
            string title = iColumeData.Get(0).ToLower();
            switch(title) {
                case "namespace": {
                        m_NameSpace = iColumeData.Get(1);
                        break;
                    }
                case "classname": {
                        m_ClassName = iColumeData.Get(1);
                        break;
                    }
                case "classmodifier": {
                        for(int i = 1, count = iColumeData.Count; i < count; i++) {
                            m_ModifierSet.AddModifier(iColumeData.Get(i));
                        }
                        break;
                    }
                case "accessmodifier": {
                        m_AccessModifier = iColumeData.Get(1).ToAccessModifier();
                        break;
                    }
                    
                case "field": {
                        AddFieldData(new ClassFieldData(iColumeData));
                        break;
                    }
            }
        }
    }
}