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
        public string m_Name = string.Empty;
        public System.Type m_Type = typeof(object);
        public string m_TypeName = string.Empty;
        public HashSet<string> m_Summarys = new HashSet<string>();
        public object m_Value = null;
        public ClassFieldData() { }
        public ClassFieldData(CSVRowData iColumeData) {
            Init(iColumeData.m_Columes);
        }
        public ClassFieldData(string iName) {
            m_Name = iName;
        }
        public void Init(List<string> iFieldDatas) {
            if(iFieldDatas == null || iFieldDatas.Count == 0) return;
            m_Name = iFieldDatas.Get(1);
            ParseFieldData(2, iFieldDatas);
        }
        public void AddSummary(string iSummary) {
            if(string.IsNullOrEmpty(iSummary) || m_Summarys.Contains(iSummary)) return;
            m_Summarys.Add(iSummary);
        }
        public void AddModifier(ClassModifier iModifier) {
            m_ModifierSet.AddModifier(iModifier);
        }
        public void ParseFieldData(int at, List<string> iFieldDatas) {
            string aTitle = iFieldDatas.Get(at++);
            switch(aTitle.ToLower()) {
                case "type": {
                        SetType(iFieldDatas.Get(at++));
                        break;
                    }
                case "accessmodifier": {
                        m_AccessModifier = iFieldDatas.Get(at++).ToAccessModifier();
                        break;
                    }
                case "classmodifier": {
                        m_ModifierSet.AddModifier(iFieldDatas.Get(at++));
                        break;
                    }
                case "value": {
                        SetValue(iFieldDatas.Get(at++));
                        break;
                    }
                case "summary": {
                        AddSummary(iFieldDatas.Get(at++));
                        break;
                    }
                    
            }
            if(at < iFieldDatas.Count) {
                ParseFieldData(at, iFieldDatas);
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
            iStringBuilder.Append(m_Name + " = ");
            if(m_TypeName == "string") {
                if(m_Value == null) {
                    iStringBuilder.Append("string.Empty");
                } else {
                    iStringBuilder.Append("\"" + m_Value + "\"");
                }
                
            } else {
                if(m_Value == null) {
                    iStringBuilder.Append("null");
                } else {
                    iStringBuilder.Append(m_Value.ToString());
                }
            }
            iStringBuilder.Append(";" + System.Environment.NewLine);
        }
    }
    public class ClassMethodParameter {
        public string m_Name = string.Empty;
        public string m_Type = string.Empty;
        public string m_Summary = string.Empty;
        public ClassMethodParameter(string iType, string iName) {
            m_Type = iType;
            m_Name = iName;
        }
        public void ConvertToString(StringBuilder iStringBuilder) {
            iStringBuilder.Append(m_Type);
            iStringBuilder.Append(' ');
            iStringBuilder.Append(m_Name);
        }
        public void WriteSummary(StringBuilder iStringBuilder, string iLayerStr) {
            if(string.IsNullOrEmpty(m_Summary)) return;
            iStringBuilder.Append(iLayerStr);
            iStringBuilder.Append("/// <param name=\"");
            iStringBuilder.Append(m_Name);
            iStringBuilder.AppendLine("\">" + m_Summary + "</param>");
        }
    }
    public class ClassMethodStatement {
        public ClassMethodStatement() { }
        public ClassMethodStatement(string aStatement) {
            m_Statement = aStatement;
        }
        public void ConvertToString(StringBuilder iStringBuilder, string aLayerStr) {
            iStringBuilder.Append(aLayerStr);
            iStringBuilder.AppendLine(m_Statement);
        }
        public string m_Statement = string.Empty;
    }
    public class ClassMethodData {
        public AccessModifier m_AccessModifier = AccessModifier.Public;
        public ClassModifierSet m_ModifierSet = new ClassModifierSet();
        public List<ClassMethodParameter> m_Parameters = new List<ClassMethodParameter>();
        public List<ClassMethodStatement> m_Statements = new List<ClassMethodStatement>();
        public HashSet<string> m_Summarys = new HashSet<string>();
        public string m_ReturnType = "void";
        public string m_Name = string.Empty;
        public ClassMethodData() { }
        public ClassMethodData(string iName) {
            SetMethodName(iName);
        }
        public void AddSummary(string iSummary) {
            if(string.IsNullOrEmpty(iSummary) || m_Summarys.Contains(iSummary)) return;
            m_Summarys.Add(iSummary);
        }
        public void AddModifier(ClassModifier iModifier) {
            m_ModifierSet.AddModifier(iModifier);
        }
        public ClassMethodParameter AddParameter(string iType, string iName) {
            ClassMethodParameter aClassMethodParameter = new ClassMethodParameter(iType, iName);
            m_Parameters.Add(aClassMethodParameter);
            return aClassMethodParameter;
        }
        public ClassMethodStatement AddStatement() {
            ClassMethodStatement aClassMethodStatement = new ClassMethodStatement();
            m_Statements.Add(aClassMethodStatement);
            return aClassMethodStatement;
        }
        public ClassMethodStatement AddStatement(string iStatement) {
            ClassMethodStatement aClassMethodStatement = new ClassMethodStatement(iStatement);
            m_Statements.Add(aClassMethodStatement);
            return aClassMethodStatement;
        }
        public void SetReturnType(string iReturnType) {
            m_ReturnType = iReturnType;
        }
        public void ParseMethodData(int at, CSVRowData iColumeData) {
            string aTitle = iColumeData.Get(at++);
            switch(aTitle.ToLower()) {
                case "accessmodifier": {
                        m_AccessModifier = iColumeData.Get(at++).ToAccessModifier();
                        break;
                    }
                case "classmodifier": {
                        m_ModifierSet.AddModifier(iColumeData.Get(at++));
                        break;
                    }
                case "returntype": {
                        m_ReturnType = iColumeData.Get(at++);
                        break;
                    }
            }
            if(at < iColumeData.Count) {
                ParseMethodData(at, iColumeData);
            }
        }
        public void SetMethodName(string iName) {
            m_Name = iName;
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
            foreach(var aParam in m_Parameters) {
                aParam.WriteSummary(iStringBuilder, aLayerStr);
            }
            iStringBuilder.Append(aLayerStr + m_AccessModifier.ParseToString() + " ");
            if(m_ModifierSet.Count > 0) iStringBuilder.Append(m_ModifierSet.ParseToString() + " ");
            iStringBuilder.Append(m_ReturnType + " ");
            iStringBuilder.Append(m_Name + "(");
            int at = 0;
            foreach(var aParam in m_Parameters) {
                if(at > 0) {
                    iStringBuilder.Append(", ");
                    if(at % 4 == 0) {
                        iStringBuilder.AppendLine();
                        iStringBuilder.Append(aLayerStr + '\t');
                    }
                }
                ++at;
                aParam.ConvertToString(iStringBuilder);
            }
            iStringBuilder.Append(") {");
            iStringBuilder.AppendLine();
            string aStatementLayerStr = aLayerStr + '\t';
            foreach(var aStatement in m_Statements) {
                aStatement.ConvertToString(iStringBuilder, aStatementLayerStr);
            }
            
            iStringBuilder.AppendLine(aLayerStr + "}");

        }
    }
    public class ClassData {
        public AccessModifier m_AccessModifier = AccessModifier.Public;
        public ClassModifierSet m_ModifierSet = new ClassModifierSet();
        public Dictionary<string, ClassFieldData> m_ClassFieldDatas = new Dictionary<string, ClassFieldData>();
        public Dictionary<string, ClassMethodData> m_ClassMethodDatas = new Dictionary<string, ClassMethodData>();
        public string m_NameSpace = string.Empty;
        public string m_ClassName = string.Empty;

        public ClassData() { }
        public ClassData(CSVData csv_data) {
            for(int i = 0, count = csv_data.Count; i < count; i++) {
                ParseCSVColume(csv_data.GetRow(i));
            }
        }
        #region Get & Set
        public void SetName(string iName) {
            m_ClassName = iName;
        }
        public void SetNameSpace(string iNameSpace) {
            m_NameSpace = iNameSpace;
        }
        #endregion
        #region Add & Create
        public void AddModifier(string iModifier) {
            m_ModifierSet.AddModifier(iModifier);
        }
        public void AddModifier(ClassModifier iModifier) {
            m_ModifierSet.AddModifier(iModifier);
        }
        virtual public ClassMethodData CreateClassMethodData(string iName) {
            ClassMethodData aClassFieldData = new ClassMethodData(iName);
            AddClassMethodData(aClassFieldData);
            return aClassFieldData;
        }
        virtual public void AddClassMethodData(ClassMethodData iClassMethodData) {
            if(m_ClassMethodDatas.ContainsKey(iClassMethodData.m_Name)) {//Field with same name already exist!!              
                Debug.LogWarning("AddClassMethodData Name:" + iClassMethodData.m_Name + ",already exist!!");
            } else {
                m_ClassMethodDatas.Add(iClassMethodData.m_Name, iClassMethodData);
            }
        }
        virtual public ClassFieldData CreateFieldData(string iName) {
            ClassFieldData aClassFieldData = new ClassFieldData(iName);
            AddFieldData(aClassFieldData);
            return aClassFieldData;
        }
        virtual public void AddFieldData(ClassFieldData iClassFieldData) {
            if(m_ClassFieldDatas.ContainsKey(iClassFieldData.m_Name)) {//Field with same name already exist!!              
                if(iClassFieldData.m_Summarys.Count > 0) {//Combine summarys
                    var aField = m_ClassFieldDatas[iClassFieldData.m_Name];
                    foreach(var aSummary in iClassFieldData.m_Summarys) {
                        aField.AddSummary(aSummary);
                    }
                }
                Debug.LogWarning("AddFieldData FieldName:" + iClassFieldData.m_Name + ",already exist!!");
            } else {
                m_ClassFieldDatas.Add(iClassFieldData.m_Name, iClassFieldData);
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
            foreach(var aName in m_ClassFieldDatas.Keys) {
                m_ClassFieldDatas[aName].ConvertToString(aStringBuilder, aLayerStr);
            }
            foreach(var aName in m_ClassMethodDatas.Keys) {
                m_ClassMethodDatas[aName].ConvertToString(aStringBuilder, aLayerStr);
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
                case "name":
                case "classname": {
                        SetName(iColumeData.Get(1));
                        break;
                    }
                case "classmodifier": {
                        for(int i = 1, count = iColumeData.Count; i < count; i++) {
                            AddModifier(iColumeData.Get(i));
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