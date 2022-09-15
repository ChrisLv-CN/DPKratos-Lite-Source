using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatcherYRpp;

namespace Extension.Coroutines
{
    [Serializable]
    public abstract class CoroutineWaiter
    {
        public abstract bool CanRun { get; }
    }

    [Serializable]
    sealed class NullCoroutineWaiter : CoroutineWaiter
    {
        public override bool CanRun => true;
    }

    [Serializable]
    sealed class FramesCoroutineWaiter : CoroutineWaiter
    {
        public FramesCoroutineWaiter(WaitForFrames waitForFrames)
        {
            _waitForFrames = waitForFrames;
            _startFrame = Game.CurrentFrame;
        }

        public override bool CanRun => _waitForFrames.Frames + _startFrame <= Game.CurrentFrame;

        private WaitForFrames _waitForFrames;
        private int _startFrame;
    }

    [Serializable]
    class CoroutineCoroutineWaiter : CoroutineWaiter
    {
        public CoroutineCoroutineWaiter(Coroutine coroutine, CoroutineSystem coroutineSystem)
        {
            _coroutine = coroutine;
            _coroutineSystem = coroutineSystem;
            if (!coroutine.IsRunning)
            {
                _coroutineSystem.StartCoroutine(coroutine);
            }
        }

        public override bool CanRun
        {
            get
            {
                if (_coroutine.Finished)
                {
                    return true;
                }

                if (_coroutine.CanRestart)
                {
                    if (!_coroutine.IsRunning)
                    {
                        _coroutineSystem.StartCoroutine(_coroutine);
                    }
                }

                return _coroutine.Finished;
            }
        }

        private Coroutine _coroutine;
        private CoroutineSystem _coroutineSystem;
    }

    [Serializable]
    class EnumeratorCoroutineWaiter : CoroutineWaiter
    {
        public EnumeratorCoroutineWaiter(IEnumerator enumerator, CoroutineSystem coroutineSystem)
        {
            _coroutine = coroutineSystem.StartCoroutine(enumerator);
        }

        public override bool CanRun => _coroutine.Finished;

        private Coroutine _coroutine;
    }

    [Serializable]
    class AsyncResultCoroutineWaiter : CoroutineWaiter
    {
        public AsyncResultCoroutineWaiter(IAsyncResult asyncResult)
        {
            _asyncResult = asyncResult;
        }

        public override bool CanRun => _asyncResult == null || _asyncResult.IsCompleted;

        [NonSerialized]
        private IAsyncResult _asyncResult;
    }

    [Serializable]
    class CustomCoroutineWaiter : CoroutineWaiter
    {
        public CustomCoroutineWaiter(CustomYieldInstruction custom)
        {
            _custom = custom;
        }

        public override bool CanRun => !_custom.KeepWaiting;

        private CustomYieldInstruction _custom;
    }

}
