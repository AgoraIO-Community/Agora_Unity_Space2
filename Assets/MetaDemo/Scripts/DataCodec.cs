using System;
using System.Runtime.InteropServices;
namespace Agora.Demo.Meta.Model
{
    public class DataCodec<T> where T : new()
    {
        public byte[] Encode(T transformData)
        {
            int size = Marshal.SizeOf(transformData);
            byte[] arr = new byte[size];

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(transformData, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }

        public T Decode(byte[] bytes)
        {
            T str = new();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(bytes, 0, ptr, size);

                str = (T)Marshal.PtrToStructure(ptr, str.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return str;
        }
    }
}
