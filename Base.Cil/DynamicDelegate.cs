using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Base.Cil.Attributes;
using Base.Collections;
using Base.Collections.Impl;
using Base.Lang;

namespace Base.Cil
{
	public class DynamicDelegateParameter
	{
		internal static readonly DynamicDelegateParameter Void = new DynamicDelegateParameter(typeof(void), 0, 0, null, 0);

		internal readonly Type Type, CustomMarshaler;
		internal readonly UnmanagedType UnmanagedType, ArraySubType;
		internal readonly ParameterAttributes Attributes;

		public DynamicDelegateParameter(Type type, UnmanagedType unmanagedType, UnmanagedType arraySubType, Type customMarshaler, ParameterAttributes attributes)
		{
			Type = type;
			UnmanagedType = unmanagedType;
			ArraySubType = arraySubType;
			CustomMarshaler = customMarshaler;
			Attributes = attributes;
		}

		internal void AppendId(StringBuilder id)
		{
			id.Append(Type.FullName);
			if (UnmanagedType != 0)
			{
				id.Append('[').Append(UnmanagedType);
				if (ArraySubType != 0)
					id.Append(':').Append(ArraySubType);
				id.Append(']');
			}
			if (CustomMarshaler != null)
				id.Append(" by ").Append(CustomMarshaler.FullName);
		}

		private static readonly ConstructorInfo MarshalAsConstructor = typeof(MarshalAsAttribute).GetConstructor(new[] { typeof(UnmanagedType) });
		private static readonly FieldInfo MarshalAsArraySubType = typeof(MarshalAsAttribute).GetField("ArraySubType");
		private static readonly FieldInfo MarshalAsTypeRef = typeof(MarshalAsAttribute).GetField("MarshalTypeRef");

		internal void Build(ParameterBuilder builder)
		{
			if (UnmanagedType != 0)
			{
				var fields = new List<FieldInfo>();
				var values = new List<object>();
				if (ArraySubType != 0)
				{
					fields.Add(MarshalAsArraySubType);
					values.Add(ArraySubType);
				}
				if (CustomMarshaler != null)
				{
					fields.Add(MarshalAsTypeRef);
					values.Add(CustomMarshaler);
				}
				CustomAttributeBuilder cab;
				if (fields.Count > 0)
					cab = new CustomAttributeBuilder(MarshalAsConstructor, new object[] { UnmanagedType }, fields.ToArray(), values.ToArray());
				else
					cab = new CustomAttributeBuilder(MarshalAsConstructor, new object[] { UnmanagedType });
				builder.SetCustomAttribute(cab);
			}
		}

		internal static DynamicDelegateParameter FromParameterInfo(ParameterInfo par, PInvokeAttribute pia)
		{
			var pt = par?.ParameterType;
			if (pt == null || pt == typeof(void)) return Void;
			var mar = par.GetAttribute<MarshalAsAttribute>();
			UnmanagedType ut = 0; UnmanagedType at = 0;
			Type cm = null;
			if (mar != null)
			{
				ut = mar.Value;
				at = mar.ArraySubType;
				cm = mar.MarshalTypeRef;
			}
			else if (pia != null)
			{
				var realType = pt.IsByRef ? pt.GetElementType() : pt;
				var isOutOrRetval = par.IsOut || pt.IsByRef || par.Position == -1;
				if (realType == typeof(byte[]))
				{
					if (isOutOrRetval && pia.ByteArrayMarshaler != null)
					{
						ut = UnmanagedType.CustomMarshaler;
						cm = pia.ByteArrayMarshaler;
					}
				}
				else if (realType.IsEnum)
				{
					var et = realType.GetEnumUnderlyingType();
					switch (Type.GetTypeCode(et))
					{
						case TypeCode.Byte:
							ut = UnmanagedType.U1;
							break;
						case TypeCode.SByte:
							ut = UnmanagedType.I1;
							break;
						case TypeCode.UInt16:
							ut = UnmanagedType.U2;
							break;
						case TypeCode.Int16:
							ut = UnmanagedType.I2;
							break;
						case TypeCode.UInt32:
							ut = UnmanagedType.U4;
							break;
						case TypeCode.Int32:
							ut = UnmanagedType.I4;
							break;
						case TypeCode.UInt64:
							ut = UnmanagedType.U8;
							break;
						case TypeCode.Int64:
							ut = UnmanagedType.I8;
							break;
						default:
							throw new CodeBug("unsupported enum underlying type: " + et);
					}
				}
				else switch (Type.GetTypeCode(realType))
					{
						case TypeCode.Boolean:
							if (pia.NativeBool != 0) ut = pia.NativeBool;
							break;
						case TypeCode.String:
							if (isOutOrRetval)
							{
								if (pia.OutStringMarshaler != null)
								{
									ut = UnmanagedType.CustomMarshaler;
									cm = pia.OutStringMarshaler;
								}
							}
							else if (pia.NativeString != 0) ut = pia.NativeString;
							break;
					}
			}
			return new DynamicDelegateParameter(pt, ut, at, cm, par.Attributes);
		}
	}

	public class DynamicDelegate
	{
		private static readonly Dictionary<CallingConvention, Type> CallConvModifiers = new Dictionary<CallingConvention, Type>
		{
			[CallingConvention.Cdecl] = typeof(CallConvCdecl),
			[CallingConvention.FastCall] = typeof(CallConvFastcall),
			[CallingConvention.StdCall] = typeof(CallConvStdcall),
			[CallingConvention.ThisCall] = typeof(CallConvThiscall)
		};

		private readonly string Id;
		private readonly DynamicDelegateParameter ReturnParameter;
		private readonly DynamicDelegateParameter[] Parameters;
		private readonly CallingConvention CallingConvention;

		public DynamicDelegate(DynamicDelegateParameter[] parameters, DynamicDelegateParameter returnParameter, CallingConvention callingConvention)
		{
			ReturnParameter = returnParameter ?? DynamicDelegateParameter.Void;
			Parameters = parameters;
			CallingConvention = callingConvention;
			var id = new StringBuilder("D_");
			ReturnParameter.AppendId(id);
			foreach (var par in Parameters)
			{
				id.Append('_');
				par.AppendId(id);
			}
			Id = id.ToString();
		}

		private static readonly ICache<string, Type> _delegates = new StrongCache<string, Type>();

		public Type GetOrCreate()
		{
			return _delegates.Get(Id, (id) =>
			{
				var mb = CilUtils.ModuleBuilder;
				var typeBuilder = mb.DefineType(id, TypeAttributes.Sealed | TypeAttributes.Public, typeof(MulticastDelegate));
				var constructor = typeBuilder.DefineConstructor(
							MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
							CallingConventions.Standard, new[] { typeof(object), typeof(IntPtr) });
				constructor.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
				CallConvModifiers.TryGetValue(CallingConvention, out var callConvModifier);
				var invokeMethod = typeBuilder.DefineMethod(
						   "Invoke",
						   MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public,
						   CallingConventions.Standard,
						   ReturnParameter.Type,
						   null,
						   callConvModifier == null ? null : new[] { callConvModifier },
						   Parameters.Select(p => p.Type).ToArray(),
						   null,
						   null
						   );

				invokeMethod.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
				var rbuilder = invokeMethod.DefineParameter(0, ReturnParameter.Attributes, "return");
				ReturnParameter.Build(rbuilder);
				for (var i = 0; i < Parameters.Length; i++)
				{
					var parameter = Parameters[i];
					var idx = i + 1;
					var builder = invokeMethod.DefineParameter(idx, parameter.Attributes, "a" + idx);
					parameter.Build(builder);
				}
				return typeBuilder.CreateTypeInfo();
			});
		}

		public static Type GetOrCreate(MethodInfo mi)
		{
			var mpia = mi.GetAttribute<PInvokeAttribute>() ?? mi.DeclaringType.GetAttribute<PInvokeAttribute>();
			var dyndelegate =
				new DynamicDelegate(
					mi.GetParameters().Select(p => DynamicDelegateParameter.FromParameterInfo(p, mpia)).ToArray(),
					DynamicDelegateParameter.FromParameterInfo(mi.ReturnParameter, mpia), mpia?.CallingConvention ?? 0);
			return dyndelegate.GetOrCreate();
		}


	}
}
