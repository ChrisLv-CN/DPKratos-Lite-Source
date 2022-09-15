using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Coroutines
{
    [Serializable]
    public sealed class Coroutine : YieldInstruction
    {
        internal Coroutine(IEnumerator enumerator)
        {
            _enumerator = enumerator;
            IsRunning = false;
            Finished = false;
            CanRestart = false;
        }

        public string Name => _enumerator.GetType().Name;
        public bool IsRunning { get; internal set; }
        public bool Finished { get; internal set; }

        /// <summary>
        /// this coroutine can restart when other coroutine is waiting
        /// </summary>
        public bool CanRestart { get; set; }

        internal CoroutineWaiter Waiter { get; set; }
        internal IEnumerator Enumerator => _enumerator;

        private IEnumerator _enumerator;
    }
}
