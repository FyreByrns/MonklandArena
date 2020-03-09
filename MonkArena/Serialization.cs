using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MonkArena {
    public static partial class Utils {
        public static byte[] GetBytes(this object me) {
            var size = Marshal.SizeOf(me);
            byte[] bytes = new byte[size];

            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(me, ptr, false);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            return bytes;
        }

        public static object GetObject(this byte[] me, Type type) {
            var ptr = Marshal.AllocHGlobal(me.Length);
            Marshal.Copy(me, 0, ptr, me.Length);
            object from = Marshal.PtrToStructure(ptr, type);
            Marshal.FreeHGlobal(ptr);
            return from;
        }
    }
}
