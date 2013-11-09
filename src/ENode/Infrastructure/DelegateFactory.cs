using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ENode.Infrastructure
{
    /// <summary>A dynamic delegate factory.
    /// </summary>
    public class DelegateFactory
    {
        /// <summary>Creates a delegate from the given methodInfo and parameterTypes.
        /// </summary>
        /// <typeparam name="D"></typeparam>
        /// <param name="methodInfo"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        public static D CreateDelegate<D>(MethodInfo methodInfo, Type[] parameterTypes) where D : class
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }
            if (parameterTypes == null)
            {
                throw new ArgumentNullException("parameterTypes");
            }
            var parameters = methodInfo.GetParameters();
            var dynamicMethod = new DynamicMethod(
                methodInfo.Name,
                MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard,
                methodInfo.ReturnType,
                parameterTypes,
                typeof(object),
                true)
            {
                InitLocals = false
            };
            var dynamicEmit = new DynamicEmit(dynamicMethod);
            if (!methodInfo.IsStatic)
            {
                dynamicEmit.LoadArgument(0);
                dynamicEmit.CastTo(typeof(object), methodInfo.DeclaringType);
            }
            for (int index = 0; index < parameters.Length; index++)
            {
                dynamicEmit.LoadArgument(index + 1);
                dynamicEmit.CastTo(parameterTypes[index + 1], parameters[index].ParameterType);
            }
            dynamicEmit.Call(methodInfo);
            dynamicEmit.Return();

            return dynamicMethod.CreateDelegate(typeof(D)) as D;
        }

        class DynamicEmit
        {
            private ILGenerator _ilGenerator;
            private static readonly Dictionary<Type, OpCode> _converts = new Dictionary<Type, OpCode>();

            static DynamicEmit()
            {
                _converts.Add(typeof(sbyte), OpCodes.Conv_I1);
                _converts.Add(typeof(short), OpCodes.Conv_I2);
                _converts.Add(typeof(int), OpCodes.Conv_I4);
                _converts.Add(typeof(long), OpCodes.Conv_I8);
                _converts.Add(typeof(byte), OpCodes.Conv_U1);
                _converts.Add(typeof(ushort), OpCodes.Conv_U2);
                _converts.Add(typeof(uint), OpCodes.Conv_U4);
                _converts.Add(typeof(ulong), OpCodes.Conv_U8);
                _converts.Add(typeof(float), OpCodes.Conv_R4);
                _converts.Add(typeof(double), OpCodes.Conv_R8);
                _converts.Add(typeof(bool), OpCodes.Conv_I1);
                _converts.Add(typeof(char), OpCodes.Conv_U2);
            }
            public DynamicEmit(DynamicMethod dynamicMethod)
            {
                this._ilGenerator = dynamicMethod.GetILGenerator();
            }
            public DynamicEmit(ILGenerator ilGen)
            {
                this._ilGenerator = ilGen;
            }

            public void LoadArgument(int argumentIndex)
            {
                switch (argumentIndex)
                {
                    case 0:
                        this._ilGenerator.Emit(OpCodes.Ldarg_0);
                        break;
                    case 1:
                        this._ilGenerator.Emit(OpCodes.Ldarg_1);
                        break;
                    case 2:
                        this._ilGenerator.Emit(OpCodes.Ldarg_2);
                        break;
                    case 3:
                        this._ilGenerator.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        if (argumentIndex < 0x100)
                        {
                            this._ilGenerator.Emit(OpCodes.Ldarg_S, (byte)argumentIndex);
                        }
                        else
                        {
                            this._ilGenerator.Emit(OpCodes.Ldarg, argumentIndex);
                        }
                        break;
                }
            }
            public void CastTo(Type fromType, Type toType)
            {
                if (fromType != toType)
                {
                    if (toType == typeof(void))
                    {
                        if (!(fromType == typeof(void)))
                        {
                            this.Pop();
                        }
                    }
                    else
                    {
                        if (fromType.IsValueType)
                        {
                            if (toType.IsValueType)
                            {
                                this.Convert(toType);
                                return;
                            }
                            this._ilGenerator.Emit(OpCodes.Box, fromType);
                        }
                        this.CastTo(toType);
                    }
                }
            }
            public void CastTo(Type toType)
            {
                if (toType.IsValueType)
                {
                    this._ilGenerator.Emit(OpCodes.Unbox_Any, toType);
                }
                else
                {
                    this._ilGenerator.Emit(OpCodes.Castclass, toType);
                }
            }
            public void Pop()
            {
                this._ilGenerator.Emit(OpCodes.Pop);
            }
            public void Convert(Type toType)
            {
                this._ilGenerator.Emit(_converts[toType]);
            }
            public void Return()
            {
                this._ilGenerator.Emit(OpCodes.Ret);
            }
            public void Call(MethodInfo method)
            {
                if (method.IsFinal || !method.IsVirtual)
                {
                    this._ilGenerator.EmitCall(OpCodes.Call, method, null);
                }
                else
                {
                    this._ilGenerator.EmitCall(OpCodes.Callvirt, method, null);
                }
            }
        }
    }
}
