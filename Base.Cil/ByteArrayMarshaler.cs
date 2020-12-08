using System;
using System.Runtime.InteropServices;

namespace Base.Cil
{
    public sealed class ByteArrayMarshaler : ICustomMarshaler
    {
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero) return null;
            var gch = GCHandle.FromIntPtr(pNativeData);
            var result = (byte[])gch.Target;
            gch.Free();
            return result;
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            var gch = GCHandle.Alloc(ManagedObj, GCHandleType.Pinned);
            return GCHandle.ToIntPtr(gch);
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public int GetNativeDataSize()
        {
            return -1;
        }

        static readonly ByteArrayMarshaler Instance = new ByteArrayMarshaler();

        static ICustomMarshaler GetInstance(string pstrCookie)
        {
            return Instance;
        }
    }
}
