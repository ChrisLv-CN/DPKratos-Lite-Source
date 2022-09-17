using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.Ext
{
    enum InitState
    {
        Blank = 0x0, // CTOR'd
        Constanted = 0x1, // values that can be set without looking at Rules (i.e. country default loadscreen)
        Ruled = 0x2, // Rules has been loaded and props set (i.e. country powerplants taken from [General])
        Inited = 0x3, // values that need the object's state (i.e. is object a secretlab? -> load default boons)
        Completed = 0x4 // INI has been read and values set
    };

    public interface IExtension
    {
        IntPtr OwnerObject { get; }
    }

    [Serializable]
    public abstract class Extension<T> : IExtension, IReloadable/*, ISerializable*/, IDeserializationCallback
    {
        [NonSerialized]
        private Pointer<T> ownerObject;
        public Pointer<T> OwnerObject { get => ownerObject; set => ownerObject = value; }
        InitState Initialized;

        public Extension(Pointer<T> ownerObject)
        {
            OwnerObject = ownerObject;
            Initialized = InitState.Blank;
        }

        ~Extension() { }

        public bool Expired => OwnerObject.IsNull;

        IntPtr IExtension.OwnerObject => OwnerObject;

        //[SecurityPermission(SecurityAction.LinkDemand,
        //    Flags = SecurityPermissionFlag.SerializationFormatter)]
        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
        //protected Extension(SerializationInfo info, StreamingContext context) { }

        public virtual void OnDeserialization(object sender) { }

        //[OnSerializing]
        //protected void OnSerializing(StreamingContext context) { }

        //[OnSerialized]
        //protected void OnSerialized(StreamingContext context) { }

        //[OnDeserializing]
        //protected void OnDeserializing(StreamingContext context) { }

        //[OnDeserialized]
        //protected void OnDeserialized(StreamingContext context) { }

        public void EnsureConstanted()
        {
            if (Initialized < InitState.Constanted)
            {
                InitializeConstants();
                Initialized = InitState.Constanted;
            }
        }

        public void LoadFromINI(Pointer<CCINIClass> pINI)
        {
            if (pINI == Pointer<CCINIClass>.Zero)
            {
                return;
            }

            switch (Initialized)
            {
                case InitState.Blank:
                    EnsureConstanted();
                    goto case InitState.Constanted;
                case InitState.Constanted:
                    InitializeRuled();
                    Initialized = InitState.Ruled;
                    goto case InitState.Ruled;
                case InitState.Ruled:
                    Initialize();
                    Initialized = InitState.Inited;
                    goto case InitState.Inited;
                case InitState.Inited:
                case InitState.Completed:
                    if (pINI == CCINIClass.INI_Rules)
                    {
                        LoadFromRulesFile(pINI);
                    }
                    LoadFromINIFile(pINI);
                    this.PartialLoadINIConfig(pINI);
                    Initialized = InitState.Completed;
                    break;
            }
        }

        public void Expire()
        {
            OnExpire();
            
            OwnerObject = Pointer<T>.Zero;
        }

        // right after construction. only basic initialization tasks possible;
        // owner object is only partially constructed! do not use global state!
        protected virtual void InitializeConstants() { }

        protected virtual void InitializeRuled() { }

        // called before the first ini file is read
        protected virtual void Initialize() { }

        // for things that only logically work in rules - countries, sides, etc
        protected virtual void LoadFromRulesFile(Pointer<CCINIClass> pINI) { }

        // load any ini file: rules, game mode, scenario or map
        protected virtual void LoadFromINIFile(Pointer<CCINIClass> pINI) { }

        public virtual void LoadFromStream(IStream stream) { }
        public virtual void SaveToStream(IStream stream) { }

        public virtual void OnExpire() { }
    }


    public static class ExtensionHelper
    {
        public static bool IsNullOrExpired<T>(this Extension<T> ext)
        {
            if (ext == null)
                return true;
            return ext.Expired;
        }
    }
}
