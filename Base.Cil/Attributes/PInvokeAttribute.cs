using System;
using System.Runtime.InteropServices;

namespace Base.Cil.Attributes
{
    /// <summary>
    /// To be used over an interface to indicate that it is a PInvoke proxy.
    /// May also be applied to a specific method to adjust default marshaling.
    /// Observations for default marshalling on PInvoke delegate methods:
    /// 1. [In, Out] have no effect so don't use these (they seem to affect only COM interop)
    /// 2. Classes marked with [StructLayout] are treated as UnmanagedType.LPStruct, so there's no sense to use that attribute
    /// 3. "ref" and "out" for [StructLayout] classes probably expect that native code CoTaskMemAlloc's structure and returns a pointer, 
    /// so in most cases it's OK to just pass the class without any extra attributes or byrefs, default marshaler seems to copy class contents to 
    /// native structure back and forth correctly
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    public class PInvokeAttribute : Attribute
    {
        public CallingConvention CallingConvention { get; }

        public UnmanagedType NativeBool { get; set; } = UnmanagedType.I1;
        public UnmanagedType NativeString { get; set; } = UnmanagedType.LPWStr;
        public Type OutStringMarshaler { get; set; } = typeof(OutStringMarshaler);
        public Type ByteArrayMarshaler { get; set; } = typeof(ByteArrayMarshaler);

        public PInvokeAttribute(CallingConvention cc)
        {
            CallingConvention = cc;
        }
    }
}
