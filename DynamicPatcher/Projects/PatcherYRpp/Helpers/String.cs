using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [Serializable]
    public class AnsiString : CriticalFinalizerObject, IDisposable, ISerializable
    {
        IntPtr hGlobal;
        bool allocated;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AnsiString(string str)
        {
            hGlobal = Marshal.StringToHGlobalAnsi(str);
            allocated = true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AnsiString(IntPtr buffer, bool allocate = false)
        {
            hGlobal = allocate ? Marshal.StringToHGlobalAnsi(Marshal.PtrToStringAnsi(buffer)) : buffer;
            allocated = allocate;
        }

        [SecurityPermission(SecurityAction.LinkDemand,
            Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AnsiString::String", (string)this);
        }

        private AnsiString(SerializationInfo info, StreamingContext context) : this(info.GetString("AnsiString::String"))
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator IntPtr(AnsiString ansiStr) => ansiStr.hGlobal;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(AnsiString ansiStr) => Marshal.PtrToStringAnsi((IntPtr)ansiStr);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AnsiString(string str) => new AnsiString(str);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AnsiString(IntPtr ptr) => new AnsiString(ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AnsiString(Pointer<byte> ptr) => new AnsiString(ptr);
        
        public override string ToString() => this;

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                if (hGlobal != IntPtr.Zero && allocated)
                {
                    Marshal.FreeHGlobal(hGlobal);
                }
                disposedValue = true;
            }
        }

        ~AnsiString()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct AnsiStringPointer
    {
        IntPtr buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AnsiStringPointer(IntPtr ptr)
        {
            buffer = ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AnsiString(AnsiStringPointer pointer) => new AnsiString(pointer.buffer);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator IntPtr(AnsiStringPointer pointer) => pointer.buffer;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(AnsiStringPointer pointer) => (AnsiString)pointer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AnsiStringPointer(IntPtr ptr) => new AnsiStringPointer(ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AnsiStringPointer(Pointer<byte> ptr) => new AnsiStringPointer(ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AnsiStringPointer(AnsiString str) => (IntPtr)str;


        public override string ToString() => this;
    }


    [Serializable]
    public class UniString : CriticalFinalizerObject, IDisposable, ISerializable
    {
        IntPtr hGlobal;
        bool allocated;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniString(string str)
        {
            hGlobal = Marshal.StringToHGlobalUni(str);
            allocated = true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniString(IntPtr buffer, bool allocate = false)
        {
            hGlobal = allocate ? Marshal.StringToHGlobalUni(Marshal.PtrToStringUni(buffer)) : buffer;
            allocated = allocate;
        }

        [SecurityPermission(SecurityAction.LinkDemand,
            Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("UniString::String", (string)this);
        }

        private UniString(SerializationInfo info, StreamingContext context) : this(info.GetString("UniString::String"))
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator IntPtr(UniString uniStr) => uniStr.hGlobal;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(UniString uniStr) => Marshal.PtrToStringUni((IntPtr)uniStr);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UniString(string str) => new UniString(str);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UniString(IntPtr ptr) => new UniString(ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UniString(Pointer<char> ptr) => new UniString(ptr);
        
        public override string ToString() => this;

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                if (hGlobal != IntPtr.Zero && allocated)
                {
                    Marshal.FreeHGlobal(hGlobal);
                }
                disposedValue = true;
            }
        }

        ~UniString()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct UniStringPointer
    {
        IntPtr buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniStringPointer(IntPtr ptr)
        {
            buffer = ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UniString(UniStringPointer pointer) => new UniString(pointer.buffer);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator IntPtr(UniStringPointer pointer) => pointer.buffer;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(UniStringPointer pointer) => (UniString)pointer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UniStringPointer(IntPtr ptr) => new UniStringPointer(ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UniStringPointer(Pointer<char> ptr) => new UniStringPointer(ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UniStringPointer(UniString str) => (IntPtr)str;


        public override string ToString() => this;
    }
}
