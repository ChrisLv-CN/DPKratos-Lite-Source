using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Coroutines
{
    [Serializable]
    public sealed class WaitForFrames : YieldInstruction
    {
        public WaitForFrames(int frames)
        {
            if (frames < 0)
            {
                throw new InvalidOperationException("you can not wait for negative frames.");
            }
            Frames = (uint)frames;
        }
        public WaitForFrames(uint frames)
        {
            Frames = frames;
        }

        public uint Frames { get; }
    }
}
