using System;
using System.Runtime.InteropServices;

namespace Base.Cil
{
    public sealed class OutStringMarshaler : ICustomMarshaler
    {
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero) return null;
            return Marshal.PtrToStringUni(pNativeData);
        }

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            return Marshal.StringToHGlobalUni((string)managedObj);
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            if (pNativeData != IntPtr.Zero)
                Marshal.FreeHGlobal(pNativeData);
        }

        public void CleanUpManagedData(object managedObj)
        {
        }

        public int GetNativeDataSize()
        {
            return -1;
        }

        static readonly OutStringMarshaler Instance = new OutStringMarshaler();

        static ICustomMarshaler GetInstance(string pstrCookie)
        {
            return Instance;
        }
    }
}
