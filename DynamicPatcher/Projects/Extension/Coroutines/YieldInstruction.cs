using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Coroutines
{
    [Serializable]
    public class YieldInstruction
    {
    }

    [Serializable]
    public abstract class CustomYieldInstruction : IEnumerator
    {
        public abstract bool KeepWaiting { get; }

        public object Current => null;

        public bool MoveNext()
        {
            return KeepWaiting;
        }

        public void Reset()
        {
        }
    }
}
