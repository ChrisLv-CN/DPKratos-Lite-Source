using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Coroutines
{
    [Serializable]
    public sealed class WaitUntil : CustomYieldInstruction
    {
        private Func<bool> _predicate;

        public override bool KeepWaiting => !_predicate();

        public WaitUntil(Func<bool> predicate)
        {
            _predicate = predicate;
        }
    }

    [Serializable]
    public sealed class WaitWhile : CustomYieldInstruction
    {
        private Func<bool> _predicate;

        public override bool KeepWaiting => _predicate();

        public WaitWhile(Func<bool> predicate)
        {
            _predicate = predicate;
        }
    }
}
