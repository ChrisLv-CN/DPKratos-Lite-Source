using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.EventSystems
{
    public class SaveGameEvent : EventBase
    {
        public override string Name => "SaveGame";
        public override string Description => "Raised when saving game";
    }
    public class LoadGameEvent : EventBase
    {
        public override string Name => "SaveGame";
        public override string Description => "Raised when saving game";
    }

    public class SaveLoadEventArgs : EventArgs
    {
        public SaveLoadEventArgs(string fileName, bool isStart)
        {
            FileName = fileName;
            IsStart = isStart;
        }
        public SaveLoadEventArgs(IStream stream, bool isStart)
        {
            Stream = stream;
            IsStartInStream = isStart;
        }

        public string FileName { get; }
        public IStream Stream { get; }
        public bool InStream => Stream != null;
        public bool IsStart { get; }
        public bool IsEnd => !IsStart && !InStream;
        public bool IsStartInStream { get; }
        public bool IsEndInStream => !IsStartInStream && InStream;
    }

    public class SaveGameEventArgs : SaveLoadEventArgs
    {
        public SaveGameEventArgs(string fileName, bool isStart) : base(fileName, isStart)
        {
        }
        public SaveGameEventArgs(IStream stream, bool isStart) : base(stream, isStart)
        {
        }
    }

    public class LoadGameEventArgs : SaveLoadEventArgs
    {
        public LoadGameEventArgs(string fileName, bool isStart) : base(fileName, isStart)
        {
        }
        public LoadGameEventArgs(IStream stream, bool isStart) : base(stream, isStart)
        {
        }
    }

    public class SaveGameEventSystem : EventSystem
    {
        public SaveGameEventSystem()
        {
            SaveGameEvent = new SaveGameEvent();
            LoadGameEvent = new LoadGameEvent();
        }

        public SaveGameEvent SaveGameEvent { get; }
        public LoadGameEvent LoadGameEvent { get; }

    }
}
