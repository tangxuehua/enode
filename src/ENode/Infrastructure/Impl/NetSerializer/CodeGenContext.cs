using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace NetSerializer
{
    /// <summary>
    /// 类型定义。
    /// </summary>
	public sealed class TypeData
	{
        public TypeData(System.Type xtype)
        {
            this.TypeID = string.Format("{0},{1}",xtype.FullName,xtype.Assembly.FullName);  //改用字符串
            this.CompareMethodInfo = typeof(TypeData).GetMethod("Compare");
            TypeID2 = 0;
        }

		public readonly string TypeID;
        public int TypeID2;
		public bool IsDynamic;
		public MethodInfo WriterMethodInfo;
		public ILGenerator WriterILGen;
		public MethodInfo ReaderMethodInfo;
		public ILGenerator ReaderILGen;
        public MethodInfo CompareMethodInfo;

        /// <summary>
        /// 类型比较。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Compare(string a,string b)
        {
            return string.Compare(a, b) == 0;
        }
	}

	sealed class CodeGenContext
	{
		readonly Dictionary<Type, TypeData> m_typeMap;

		public CodeGenContext(Dictionary<Type, TypeData> typeMap, MethodInfo serializerSwitch, MethodInfo deserializerSwitch)
		{
			m_typeMap = typeMap;
			this.SerializerSwitchMethodInfo = serializerSwitch;
			this.DeserializerSwitchMethodInfo = deserializerSwitch;
		}

		public MethodInfo SerializerSwitchMethodInfo { get; private set; }
		public MethodInfo DeserializerSwitchMethodInfo { get; private set; }

		public MethodInfo GetWriterMethodInfo(Type type)
		{
            if (m_typeMap.ContainsKey(type))
            {
                return m_typeMap[type].WriterMethodInfo;
            }
            else
            {
                return null;
            }
		}

		public ILGenerator GetWriterILGen(Type type)
		{
            if (m_typeMap.ContainsKey(type))
            {
                return m_typeMap[type].WriterILGen;
            }
            else
            {
                return null;
            }
		}

		public MethodInfo GetReaderMethodInfo(Type type)
		{
            if (m_typeMap.ContainsKey(type))
            {
                return m_typeMap[type].ReaderMethodInfo;
            }
            else
            {
                return null;
            }
		}

		public ILGenerator GetReaderILGen(Type type)
		{
            if (m_typeMap.ContainsKey(type))
            {
                return m_typeMap[type].ReaderILGen;
            }
            else
            {
                return null;
            }
		}

		public bool IsDynamic(Type type)
		{
            if (m_typeMap.ContainsKey(type))
            {
                return m_typeMap[type].IsDynamic;
            }
            else
            {
                return false;
            }
		}
	}
}
