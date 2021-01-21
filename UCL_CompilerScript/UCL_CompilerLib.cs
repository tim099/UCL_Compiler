using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.CompilerLib {
    static public class TypeLib {
        /// <summary>
        /// Convert String to System.Type
        /// </summary>
        /// <param name="iTypeName"></param>
        /// <returns></returns>
        static public System.Type ConvertToType(string iTypeName) {
            switch(iTypeName) {
                case "int": return typeof(int);
                case "long": return typeof(long);
                case "float": return typeof(float);
                case "double": return typeof(double);
                case "string": return typeof(string);
            }

            return typeof(object);
        }
    }
}