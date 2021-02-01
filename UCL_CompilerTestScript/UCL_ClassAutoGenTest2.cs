using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UCL.CompilerLib.Test {
    [UCL.Core.ATTR.EnableUCLEditor]
    public class UCL_ClassAutoGenTest2 : MonoBehaviour {
        public string m_FolderPath = string.Empty;
        public string m_NameSpace = string.Empty;
        public UCL_CSVParser m_DataParser = null;
        public UCL_ScriptRefactor m_ScriptRefactor = null;
        [UCL.Core.ATTR.UCL_FunctionButton]
        public void Generate() {
            var aCSVData = m_DataParser.GetCSVData();
            var aParams = aCSVData.GetRow(0);
            m_ScriptRefactor.m_StringReplacement.Clear();
            for(int aCol = 0; aCol < 2; aCol++) {
                var aParam = aParams.Get(aCol);
                var aClassData = new ClassData();
                aClassData.SetNameSpace(m_NameSpace);
                aClassData.AddModifier(ClassModifier.Static);
                aClassData.SetName(aParam);
                for(int i = 1, count = aCSVData.Count; i < count; i++) {
                    var aRow = aCSVData.GetRow(i);
                    var aData = aRow.Get(aCol);
                    if(string.IsNullOrEmpty(aData)) continue;
                    var aField = new ClassFieldData(aData);
                    aField.AddModifier(ClassModifier.Const);
                    aField.AddSummary(aRow.Get(2));
                    aField.SetType("string");
                    aField.SetValue((object)aData);
                    aClassData.AddFieldData(aField);
                }
                File.WriteAllText(m_FolderPath + "/Firebase" + aParam + ".cs", aClassData.ConvertToString());
            }
            {
                var aParam = "ScreenTrigger";
                var aClassData = new ClassData();
                aClassData.SetNameSpace(m_NameSpace);
                aClassData.AddModifier(ClassModifier.Static);
                aClassData.SetName(aParam);

                var aInitMethod = aClassData.CreateClassMethodData("Init");
                {
                    aInitMethod.AddModifier(ClassModifier.Static);
                    aInitMethod.SetReturnType("void");
                    //string aStatement = string.Empty;

                    
                }
                {
                    var method = aClassData.CreateClassMethodData("SetCurrentScreen");
                    method.AddModifier(ClassModifier.Static);
                    method.AddParameter("string", "i" + aParams.Get(0));
                    method.SetReturnType("void");
                    method.AddStatement("if(!mScreenClassDic.ContainsKey(iScreenName)) {");
                    method.AddStatement("\tUnityEngine.Debug.LogError(\"!mScreenClassDic.ContainsKey(\" + iScreenName + \")\");");
                    method.AddStatement("\treturn;");
                    method.AddStatement("}");
                    method.AddStatement("Firebase.Analytics.FirebaseAnalytics.SetCurrentScreen(iScreenName, mScreenClassDic[iScreenName]);");
                    //string aStatement = string.Empty;

                    //method.AddStatement(aStatement);
                }
                {
                    var aField = aClassData.CreateFieldData("mScreenClassDic");
                    aField.SetType("System.Collections.Generic.Dictionary<string, string>");
                    aField.AddModifier(ClassModifier.Static);
                    aField.SetValue("new System.Collections.Generic.Dictionary<string, string>()");
                }
                for(int i = 1, count = aCSVData.Count; i < count; i++) {
                    var aRow = aCSVData.GetRow(i);
                    string aFuncName = aRow.Get(0);
                    string aStatement = string.Empty;
                    aInitMethod.AddStatement("//" + i + "." + aRow.Get(2));
                    aStatement += "mScreenClassDic.Add(" + aParams.Get(0) + "." + aFuncName + ", "+ aParams.Get(1) + "." + aRow.Get(1)+");";
                    //aStatement += System.Environment.NewLine;
                    aInitMethod.AddStatement(aStatement);

                    //if(aFuncName.Length > 0) {
                    //    var aNames = aFuncName.Split(new char[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
                    //    aFuncName = string.Empty;
                    //    for(int j = 0; j < aNames.Length; j++) {
                    //        var aName = aNames[j];
                    //        if(aName.Length > 0) {
                    //            aFuncName += char.ToUpper(aName[0]);
                    //            aFuncName += aName.Substring(1, aName.Length - 1);
                    //        }
                    //    }
                    //}
                    //m_ScriptRefactor.m_StringReplacement.Add(new UCL_ScriptRefactor.ReplaceData(
                    //    "SetCurrentScreen" + aFuncName + "()", "SetCurrentScreen(" + "Firebase." + aParams.Get(0) + "." + aRow.Get(0) + ")")
                    //);
                    //var method = aClassData.CreateClassMethodData("SetCurrentScreen" + aFuncName);
                    //method.AddModifier(ClassModifier.Static);
                    //method.SetReturnType("void");
                    //method.AddSummary(i + "." + aRow.Get(2));
                    //string aStatement = string.Empty;
                    //aStatement += "Firebase.Analytics.FirebaseAnalytics.SetCurrentScreen(";
                    //aStatement += aParams.Get(0) + "." + aRow.Get(0)+", ";
                    //aStatement += aParams.Get(1) + "." + aRow.Get(1);
                    //aStatement += ");";
                    //method.AddStatement(aStatement);
                }
                File.WriteAllText(m_FolderPath + "/Firebase" + aParam + ".cs", aClassData.ConvertToString());
            }

        }
    }
}