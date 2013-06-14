using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NetSerializer
{
    class TypeFilter
    {
        /// <summary>
        /// 判断字段的过虑条件。
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool ProccessField(FieldInfo field)
        {
            if(field.FieldType.IsSubclassOf(typeof(Delegate)))
                return  false;
            
            return true;
        }
    }
}
