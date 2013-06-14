/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Linq;

namespace NetSerializer
{
	static class SerializerCodegen
	{
		public static DynamicMethod GenerateDynamicSerializerStub(Type type)
		{
			var dm = new DynamicMethod("Serialize", null,
				new Type[] { typeof(Stream), type },
				typeof(Serializer), true);

			dm.DefineParameter(1, ParameterAttributes.None, "stream");
			dm.DefineParameter(2, ParameterAttributes.None, "value");

			return dm;
		}

#if GENERATE_DEBUGGING_ASSEMBLY
		public static MethodBuilder GenerateStaticSerializerStub(TypeBuilder tb, Type type)
		{
			var mb = tb.DefineMethod("Serialize", MethodAttributes.Public | MethodAttributes.Static, null, new Type[] { typeof(Stream), type });
			mb.DefineParameter(1, ParameterAttributes.None, "stream");
			mb.DefineParameter(2, ParameterAttributes.None, "value");
			return mb;
		}
#endif

		public static void GenerateSerializerBody(CodeGenContext ctx, Type type, ILGenerator il)
		{
			// arg0: Stream, arg1: value

			if (type.IsArray)
				GenSerializerBodyForArray(ctx, type, il);
			else
				GenSerializerBody(ctx, type, il);
		}

		static void GenSerializerBody(CodeGenContext ctx, Type type, ILGenerator il)
		{
			var fields = Helpers.GetFieldInfos(type);

			foreach (var field in fields)
			{
				// Note: the user defined value type is not passed as reference. could cause perf problems with big structs
                if (TypeFilter.ProccessField(field))  //类型过虑器
                {
                    il.Emit(OpCodes.Ldarg_0);
                    if (type.IsValueType)
                        il.Emit(OpCodes.Ldarga_S, 1);
                    else
                        il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldfld, field);

                    GenSerializerCall(ctx, il, field.FieldType);
                }
			}

			il.Emit(OpCodes.Ret);
		}

		static void GenSerializerBodyForArray(CodeGenContext ctx, Type type, ILGenerator il)
		{
			var elemType = type.GetElementType();

			var notNullLabel = il.DefineLabel();

			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Brtrue_S, notNullLabel);

			// if value == null, write 0
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldc_I4_0);
			il.EmitCall(OpCodes.Call, ctx.GetWriterMethodInfo(typeof(uint)), null);
			il.Emit(OpCodes.Ret);

			il.MarkLabel(notNullLabel);

			// write array len + 1
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldlen);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Add);
			il.EmitCall(OpCodes.Call, ctx.GetWriterMethodInfo(typeof(uint)), null);

			// declare i
			var idxLocal = il.DeclareLocal(typeof(int));

			// i = 0
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Stloc_S, idxLocal);

			var loopBodyLabel = il.DefineLabel();
			var loopCheckLabel = il.DefineLabel();

			il.Emit(OpCodes.Br_S, loopCheckLabel);

			// loop body
			il.MarkLabel(loopBodyLabel);

			// write element at index i
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Ldelem, elemType);

			GenSerializerCall(ctx, il, elemType);

			// i = i + 1
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Stloc_S, idxLocal);

			il.MarkLabel(loopCheckLabel);

			// loop condition
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldlen);
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Clt);
			il.Emit(OpCodes.Brtrue_S, loopBodyLabel);

			il.Emit(OpCodes.Ret);
		}

		static void GenSerializerCall(CodeGenContext ctx, ILGenerator il, Type type)
		{
			// We can call the Serializer method directly for:
			// - Value types
			// - Array types
			// - Sealed types with static Serializer method, as the method will handle null
			// Other reference types go through the SerializesSwitch

			bool direct;

			if (type.IsValueType || type.IsArray)
				direct = true;
			else if (type.IsSealed && ctx.IsDynamic(type) == false)
				direct = true;
			else
				direct = false;

			var method = direct ? ctx.GetWriterMethodInfo(type) : ctx.SerializerSwitchMethodInfo;
            if (method != null)  //非空
            {
                il.EmitCall(OpCodes.Call, method, null);
            }
		}

        public static void GenerateSerializerSwitch(CodeGenContext ctx, ILGenerator il, IDictionary<Type, TypeData> map)
        {
            // arg0: Stream, arg1: object

            var idLocal = il.DeclareLocal(typeof(int));
            var strLocal = il.DeclareLocal(typeof(string));

            // 读对象类型GetTypeID。
            //var getTypeIDMethod = typeof(Serializer).GetMethod("GetTypeID", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null);
            var getTypeIDMethod = typeof(Serializer).GetMethod("GetTypeID", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null);
            
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, getTypeIDMethod, null);
            il.Emit(OpCodes.Stloc_S, strLocal);

            //读对象类型GetTypeID2(整数)。
            var getTypeIDMethod2 = typeof(Serializer).GetMethod("GetTypeID2", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null);

            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, getTypeIDMethod2, null);
            il.Emit(OpCodes.Stloc_S, idLocal);

            // write typeID/字符串
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_S, strLocal);
            il.EmitCall(OpCodes.Call, ctx.GetWriterMethodInfo(typeof(string)), null);

            // +1 for 0 (null)
            //定义标签
            var jumpTable = new Label[map.Count + 1];
            jumpTable[0] = il.DefineLabel();
            foreach (var kvp in map)
            {
                int index = Serializer.GetTypeID2(kvp.Key);
                jumpTable[index] = il.DefineLabel();
            }

            il.Emit(OpCodes.Ldloc_S, idLocal);
            il.Emit(OpCodes.Switch, jumpTable);

            ConstructorInfo exceptionCtor = typeof(Exception).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);

            /* null case */
            il.MarkLabel(jumpTable[0]);
            il.Emit(OpCodes.Ret);

            /* cases for types */
            foreach (var kvp in map)
            {
                var type = kvp.Key;
                var data = kvp.Value;

                int index = Serializer.GetTypeID2(kvp.Key);
                il.MarkLabel(jumpTable[index]);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);

                il.EmitCall(OpCodes.Call, data.WriterMethodInfo, null);

                il.Emit(OpCodes.Ret);
            }
        }
	}
}
