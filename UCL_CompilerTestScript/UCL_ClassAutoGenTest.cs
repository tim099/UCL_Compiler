using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace UCL.CompilerLib.Test {
    [UCL.Core.ATTR.EnableUCLEditor]
    public class UCL_ClassAutoGenTest : MonoBehaviour {
        public string m_FolderPath = string.Empty;
        public string m_NameSpace = string.Empty;
        public UCL_CSVParser m_ClassParser;
        public UCL_CSVParser m_DataParser;
        public UCL_CSVParser m_EventNameParser;
        [UCL.Core.ATTR.UCL_FunctionButton]
        public void Generate() {
            string[] aParams = new string[] {
                    "EventName",
                    "EventLabel",
                    "EventAction",
                    "EventCategory",
                    "EventCustomID",
                    "EventItemID",
                    "EventErrorID",
                    "EventBuyID",
                };
            var aClassData = m_ClassParser.CreateClassData();
            var aCSVData = m_DataParser.GetCSVData();
            {
                var method = aClassData.CreateClassMethodData("TriggerEvent");
                method.AddModifier(ClassModifier.Static);

                method.SetReturnType("void");
                foreach(var aParam in aParams) {
                    method.AddParameter("string", "i"+aParam);
                }
                string aStatement = "MarketingService.Instance.TriggerEvent(";
                bool aIsFirst = true;
                foreach(var aParam in aParams) {
                    if(aIsFirst) {
                        aIsFirst = false;
                    } else {
                        aStatement += ", ";
                    }
                    aStatement += ("i" + aParam);
                }
                aStatement += ");";
                method.AddStatement(aStatement);
            }
            {
                for(int i = 0, count = aCSVData.Count; i < count; i++) {
                    var aRow = aCSVData.GetRow(i);
                    var method = aClassData.CreateClassMethodData("FirebaseEvent"+(i+1));
                    method.AddModifier(ClassModifier.Static);
                    method.AddSummary(aRow.Get(1));
                    method.SetReturnType("void");
                    string aStatement = string.Empty;
                    aStatement += "TriggerEvent(";
                    string aPara;
                    aPara = aRow.Get(0);
                    if(string.IsNullOrEmpty(aPara)) {
                        aStatement += "string.Empty, ";
                    } else {
                        aStatement += aParams[0] + "." + aPara + ", ";
                    }
                    for(int j = 2; j < 5; j++) {
                        aPara = aRow.Get(j);//Label
                        if(aPara.Contains("(")) {
                            string aParam = "i" + aParams[j - 1];
                            ClassMethodParameter aParaData = method.AddParameter("string", aParam);
                            aParaData.m_Summary = aPara;
                            aStatement += aParam + ", ";
                        } else {
                            if(string.IsNullOrEmpty(aPara)) {
                                aStatement += "string.Empty, ";
                            } else {
                                aStatement += aParams[j - 1] + "." + aPara + ", ";
                            }
                        }

                    }
                    for(int j = 5; j < 9; j++) {
                        aPara = aRow.Get(j);//Label
                        if(string.IsNullOrEmpty(aPara)) {
                            aStatement += "string.Empty";
                        } else {
                            string aParam = "i" + aParams[j - 1];
                            ClassMethodParameter aParaData = method.AddParameter("string", aParam);
                            aParaData.m_Summary = aPara;
                            //<param name="iEventCustomID"></param>
                            aStatement += aParam;
                        }
                        if(j < 8) aStatement += ", ";
                    }
                    aStatement += ");";
                    method.AddStatement(aStatement);

                }
            }
            //m_ClassParser.SaveClassToFile(aClassData);
            {
                int aParseColume = 0;
                var aParam = aParams[aParseColume];//"EventName"
                var aEventClassData = new ClassData();
                aEventClassData.SetNameSpace(m_NameSpace);
                aEventClassData.AddModifier(ClassModifier.Static);
                aEventClassData.SetName(aParam);
                for(int i = 0, count = aCSVData.Count; i < count; i++) {
                    var aRow = aCSVData.GetRow(i);
                    var aData = aRow.Get(aParseColume);
                    if(string.IsNullOrEmpty(aData)) continue;
                    var aField = new ClassFieldData(aData);
                    aField.AddModifier(ClassModifier.Static);
                    aField.AddSummary(aRow.Get(1));
                    aField.SetType("string");
                    aField.SetValue(aData);
                    aEventClassData.AddFieldData(aField);
                }
                File.WriteAllText(m_FolderPath + "/Firebase" + aParam + ".cs", aEventClassData.ConvertToString());
            }
            for(int aParseColume = 2; aParseColume < 5; aParseColume++) {
                var aParam = aParams[aParseColume - 1];
                var aEventClassData = new ClassData();
                aEventClassData.SetNameSpace(m_NameSpace);
                aEventClassData.AddModifier(ClassModifier.Static);
                aEventClassData.SetName(aParam);
                for(int i = 0, count = aCSVData.Count; i < count; i++) {
                    var aRow = aCSVData.GetRow(i);
                    var aData = aRow.Get(aParseColume);
                    if(string.IsNullOrEmpty(aData)) continue;
                    if(aData.Contains("(")) {
                        aData = aData.Replace(' ', '\0');
                        var aDatas = aData.Split(new char[] { '、' , '(' , ')' }, System.StringSplitOptions.RemoveEmptyEntries);
                        for(int j = 1; j < aDatas.Length; j++) {
                            aData = aDatas[j];
                            var aField = new ClassFieldData(aData);
                            aField.AddModifier(ClassModifier.Static);
                            if(aParseColume != 3) aField.AddSummary(aRow.Get(1));
                            aField.SetType("string");
                            aField.SetValue(aData);
                            aEventClassData.AddFieldData(aField);
                        }
                    } else {
                        var aField = new ClassFieldData(aData);
                        aField.AddModifier(ClassModifier.Static);
                        if(aParseColume != 3) aField.AddSummary(aRow.Get(1));
                        aField.SetType("string");
                        aField.SetValue(aData);
                        aEventClassData.AddFieldData(aField);
                    }

                }
                File.WriteAllText(m_FolderPath + "/Firebase" + aParam + ".cs", aEventClassData.ConvertToString());
            }

            File.WriteAllText(m_FolderPath + "/FirebaseEventTrigger.cs", aClassData.ConvertToString());
        }
        public void Event1234() {

        }
    }
}