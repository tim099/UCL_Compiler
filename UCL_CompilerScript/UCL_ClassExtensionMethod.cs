using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.CompilerLib {
    static public partial class ClassExtensionMethod {
        static public ClassModifier ToClassModifier(this string iClassModifier) {
            string aClassModifier = iClassModifier.ToLower();
            switch(aClassModifier) {
                case "const": return ClassModifier.Const;
                case "partial": return ClassModifier.Partial;
                case "static": return ClassModifier.Static;
            }
            return ClassModifier.None;
        }
        static public AccessModifier ToAccessModifier(this string iAccessModifier) {
            switch(iAccessModifier.ToLower()) {
                case "public": return AccessModifier.Public;
                case "protected": return AccessModifier.Protected;
                case "private": return AccessModifier.Private;
                case "internal": return AccessModifier.Internal;
            }
            return AccessModifier.Public;
        }
        static public string ParseToString(this AccessModifier iAccessModifier) {
            switch(iAccessModifier) {
                case AccessModifier.Public: return "public";
                case AccessModifier.Protected: return "protected";
                case AccessModifier.Private: return "private";
                case AccessModifier.Internal: return "internal";
            }
            return iAccessModifier.ToString().ToLower();
        }
        static public string ParseToString(this ClassModifier iClassModifier) {
            switch(iClassModifier) {
                case ClassModifier.None: return string.Empty;
                case ClassModifier.Const: return "const";
                case ClassModifier.Static: return "static";
                case ClassModifier.Partial: return "partial";
            }
            return iClassModifier.ToString().ToLower(); ;
        }
        static public string ParseToString(this HashSet<ClassModifier> iClassModifierSet) {
            if(iClassModifierSet == null || iClassModifierSet.Count == 0) return string.Empty;
            string result = string.Empty;
            bool first = true;
            foreach(var aClassModifier in iClassModifierSet) {
                if(first) {
                    first = false;
                } else {
                    result += "\t";
                }
                result += aClassModifier.ParseToString();
            }
            return result;
        }
    }
}