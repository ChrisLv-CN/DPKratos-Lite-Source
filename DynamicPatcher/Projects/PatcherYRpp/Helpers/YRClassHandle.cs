using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    public partial class YRClassHandle<T> : CriticalFinalizerObject, IDisposable
    {
        public YRClassHandle(params object[] list)
        {
            _pointer = YRMemory.Create<T>(list);
        }
        public ref T Ref
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Pointer.Ref;
        }
        public Pointer<T> Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Pointer<T>(YRClassHandle<T> obj) => obj.Pointer;

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                
                YRMemory.Delete(_pointer);
                _pointer = Pointer<T>.Zero;
                disposedValue = true;
            }
        }

        ~YRClassHandle()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private Pointer<T> _pointer;
    }
}
