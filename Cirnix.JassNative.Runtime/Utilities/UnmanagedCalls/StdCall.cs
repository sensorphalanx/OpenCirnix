﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Cirnix.JassNative.Runtime.Utilities.UnmanagedCalls
{
    public static class StdCall
    {
        private static Dictionary<IntPtr, DynamicMethod> methods = new Dictionary<IntPtr, DynamicMethod>();

        public static TReturned Invoke<TReturned>(IntPtr address, params object[] parameters) where TReturned : struct
        {
            if (!methods.TryGetValue(address, out DynamicMethod method))
            {
                var returnType = typeof(TReturned);
                var paramTypes = parameters.Select(o => o.GetType()).ToArray();

                method = new DynamicMethod($"StdCall_Invoke_{(int)address:X8}", returnType, paramTypes, typeof(StdCall).Module);
                var il = method.GetILGenerator();
                for (int i = 0; i < parameters.Length; i++)
                    il.Emit(OpCodes.Ldarg, i);
                il.Emit(OpCodes.Ldc_I4, (int)address);
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, CallingConvention.StdCall, returnType, paramTypes);
                il.Emit(OpCodes.Ret);

                methods.Add(address, method);
            }

            return (TReturned)method.Invoke(null, parameters);
        }
    }
}
