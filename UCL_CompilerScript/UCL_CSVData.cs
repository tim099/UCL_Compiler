using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.CompilerLib {
    [System.Serializable]
    public class CSVRowData {
        public int Count {
            get {
                if(m_Columes == null) return 0;
                return m_Columes.Count;
            }
        }
        public CSVRowData(string iData) {
            var columes = iData.Split(new[] { ',' });//, System.StringSplitOptions.RemoveEmptyEntries
            for(int i = 0; i < columes.Length; i++) {
                m_Columes.Add(columes[i]);
            }
        }
        public string Get(int iColume) {
            if(iColume < 0 || iColume >= m_Columes.Count) return string.Empty;
            return m_Columes[iColume];
        }
        public List<string> m_Columes = new List<string>();
    }
    [System.Serializable]
    public class CSVData {
        public int Count { get {
                if(m_Rows == null) return 0;
                return m_Rows.Count;
            } }
        public CSVData(string data) {
            var rows = data.SplitByLine();
            for(int i = 0; i < rows.Length; i++) {
                if(!string.IsNullOrEmpty(rows[i])) {
                    m_Rows.Add(new CSVRowData(rows[i]));
                }
            }
        }
        public string GetData(int row, int colume) {
            if(row < 0 || row >= m_Rows.Count) return string.Empty;
            return m_Rows[row].Get(colume);
        }
        public CSVRowData GetRow(int row) {
            if(row < 0 || row >= m_Rows.Count) return null;
            return m_Rows[row];
        }
        public List<CSVRowData> m_Rows = new List<CSVRowData>();
    }
}
