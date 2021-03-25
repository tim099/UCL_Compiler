using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.CompilerLib;
using UCL.Core.CsvLib;
using UnityEngine;

public static partial class UCL_CsvDataExtension
{
    public static ClassData CreateClassData(this UCL_CSVParser iCSVParser)
    {
        return new ClassData(iCSVParser.m_CSVData);
    }
    public static void SaveClassToFile(this UCL_CSVParser iCSVParser, ClassData iClassData)
    {
        File.WriteAllText(iCSVParser.SaveFilePath + ".cs", iClassData.ConvertToString());
    }
    public static void WriteToClass(this UCL_CSVParser iCSVParser)
    {
        ClassData aClassData = iCSVParser.CreateClassData();
        Debug.LogError(aClassData.UCL_ToString());
        iCSVParser.SaveClassToFile(aClassData);
        //File.WriteAllText(SaveFilePath + ".cs", aClassData.ConvertToString());
    }
}
