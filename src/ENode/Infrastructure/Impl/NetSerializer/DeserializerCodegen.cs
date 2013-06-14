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
using System.Runtime.Serialization;
using System.Linq;

namespace NetSerializer
{
	static class DeserializerCodegen
	{
		public static DynamicMethod GenerateDynamicDeserializerStub(Type type)
		{
			var dm = new DynamicMethod("Deserialize", null,
				new Type[] { typeof(Stream), type.MakeByRefType() },
				typeof(Serializer), true);
			dm.DefineParameter(1, ParameterAttributes.None, "stream");
			dm.DefineParameter(2, ParameterAttributes.Out, "value");

			return dm;
		}

#if GENERATE_DEBUGGING_ASSEMBLY
		public static MethodBuilder GenerateStaticDeserializerStub(TypeBuilder tb, Type type)
		{
			var mb = tb.DefineMethod("Deserialize", MethodAttributes.Public | MethodAttributes.Static, null, new Type[] { typeof(Stream), type.MakeByRefType() });
			mb.DefineParameter(1, ParameterAttributes.None, "stream");
			mb.DefineParameter(2, ParameterAttributes.Out, "value");
			return mb;
		}
#endif

		public static void GenerateDeserializerBody(CodeGenContext ctx, Type type, ILGenerator il)
		{
			// arg0: stream, arg1: out value

			if (type.IsArray)
				GenDeserializerBodyForArray(ctx, type, il);
			else
				GenDeserializerBody(ctx, type, il);
		}

		static void GenDeserializerBody(CodeGenContext ctx, Type type, ILGenerator il)
		{
			if (type.IsClass)
			{
				// instantiate empty class
				il.Emit(OpCodes.Ldarg_1);

				var gtfh = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
				var guo = typeof(System.Runtime.Serialization.FormatterServices).GetMethod("GetUninitializedObject", BindingFlags.Public | BindingFlags.Static);
				il.Emit(OpCodes.Ldtoken, type);
				il.Emit(OpCodes.Call, gtfh);
				il.Emit(OpCodes.Call, guo);
				il.Emit(OpCodes.Castclass, type);

				il.Emit(OpCodes.Stind_Ref);
			}

			var fields = Helpers.GetFieldInfos(type);

			foreach (var field in fields)
            {
                if (TypeFilter.ProccessField(field))  //类型过虑器
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    if (type.IsClass)
                        il.Emit(OpCodes.Ldind_Ref);
                    il.Emit(OpCodes.Ldflda, field);

                    GenDeserializerCall(ctx, il, field.FieldType);
                }
			}

			if (typeof(IDeserializationCallback).IsAssignableFrom(type))
			{
				var miOnDeserialization = typeof(IDeserializationCallback).GetMethod("OnDeserialization",
										BindingFlags.Instance | BindingFlags.Public,
										null, new[] { typeof(Object) }, null);

				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldnull);
				il.Emit(OpCodes.Constrained, type);
				il.Emit(OpCodes.Callvirt, miOnDeserialization);
			}

			il.Emit(OpCodes.Ret);
		}

		static void GenDeserializerBodyForArray(CodeGenContext ctx, Type type, ILGenerator il)
		{
			var elemType = type.GetElementType();

			var lenLocal = il.DeclareLocal(typeof(uint));

			// read array len
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldloca_S, lenLocal);
			il.EmitCall(OpCodes.Call, ctx.GetReaderMethodInfo(typeof(uint)), null);

			var notNullLabel = il.DefineLabel();

			/* if len == 0, return null */
			il.Emit(OpCodes.Ldloc_S, lenLocal);
			il.Emit(OpCodes.Brtrue_S, notNullLabel);

			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Stind_Ref);
			il.Emit(OpCodes.Ret);

			il.MarkLabel(notNullLabel);

			var arrLocal = il.DeclareLocal(type);

			// create new array with len - 1
			il.Emit(OpCodes.Ldloc_S, lenLocal);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Sub);
			il.Emit(OpCodes.Newarr, elemType);
			il.Emit(OpCodes.Stloc_S, arrLocal);

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

			// read element to arr[i]
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldloc_S, arrLocal);
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Ldelema, elemType);

			GenDeserializerCall(ctx, il, elemType);

			// i = i + 1
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Stloc_S, idxLocal);

			il.MarkLabel(loopCheckLabel);

			// loop condition
			il.Emit(OpCodes.Ldloc_S, idxLocal);
			il.Emit(OpCodes.Ldloc_S, arrLocal);
			il.Emit(OpCodes.Ldlen);
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Clt);
			il.Emit(OpCodes.Brtrue_S, loopBodyLabel);


			// store new array to the out value
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldloc_S, arrLocal);
			il.Emit(OpCodes.Stind_Ref);

			il.Emit(OpCodes.Ret);
		}

		static void GenDeserializerCall(CodeGenContext ctx, ILGenerator il, Type type)
		{
			// We can call the Deserializer method directly for:
			// - Value types
			// - Array types
			// - Sealed types with static Deserializer method, as the method will handle null
			// Other reference types go through the DeserializesSwitch

			bool direct;

			if (type.IsValueType || type.IsArray)
				direct = true;
			else if (type.IsSealed && ctx.IsDynamic(type) == false)
				direct = true;
			else
				direct = false;

			var method = direct ? ctx.GetReaderMethodInfo(type) : ctx.DeserializerSwitchMethodInfo;
            if (method != null)
            {
                il.EmitCall(OpCodes.Call, method, null);
            }
		}

        public static void GenerateDeserializerSwitch(CodeGenContext ctx, ILGenerator il, IDictionary<Type, TypeData> map)
        {
            // arg0: stream, arg1: out object

            var idLocal = il.DeclareLocal(typeof(int));
            var strLocal = il.DeclareLocal(typeof(string));

            // read typeID //读字符串ID。
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloca_S, strLocal);
            il.EmitCall(OpCodes.Call, ctx.GetReaderMethodInfo(typeof(string)), null);
            il.Emit(OpCodes.Stloc_S, strLocal);

            //读对象类型GetTypeID2(整数)。
            var getTypeIDMethod2 = typeof(Serializer).GetMethod("GetTypeID2", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null);

            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, getTypeIDMethod2, null);
            il.Emit(OpCodes.Stloc_S, idLocal);

            // +1 for 0 (null)
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

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stind_Ref);
            il.Emit(OpCodes.Ret);

            /* cases for types */
            foreach (var kvp in map)
            {
                var type = kvp.Key;
                var data = kvp.Value;

                int index = Serializer.GetTypeID2(kvp.Key);
                il.MarkLabel(jumpTable[index]);

                var local = il.DeclareLocal(type);

                // call deserializer for this typeID
                il.Emit(OpCodes.Ldarg_0);
                if (local.LocalIndex < 256)
                    il.Emit(OpCodes.Ldloca_S, local);
                else
                    il.Emit(OpCodes.Ldloca, local);

                il.EmitCall(OpCodes.Call, data.ReaderMethodInfo, null);

                // write result object to out object
                il.Emit(OpCodes.Ldarg_1);
                if (local.LocalIndex < 256)
                    il.Emit(OpCodes.Ldloc_S, local);
                else
                    il.Emit(OpCodes.Ldloc, local);
                if (type.IsValueType)
                    il.Emit(OpCodes.Box, type);
                il.Emit(OpCodes.Stind_Ref);

                il.Emit(OpCodes.Ret);
            }
        }
	}
}
