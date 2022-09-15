using DynamicPatcher;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext
{
    public interface IContainer
    {
        public IExtension Find(IntPtr key);
    }


    public abstract class Container<TExt, TBase> : IContainer where TExt : Extension<TBase>
        {
        protected Container(string name)
        {
            Name = name;

            m_SavingObject = Pointer<TBase>.Zero;
            m_SavingStream = null;
        }

        public string Name;

        private Pointer<TBase> m_SavingObject;
        private IStream m_SavingStream;

        public TExt FindOrAllocate(Pointer<TBase> key)
        {
            if (key.IsNull)
            {
                Logger.Log("CTOR of {0} attempted for a NULL pointer! WTF!\n", Name);
                return null;
            }

            TExt val = Find(key);
            if (val == null)
            {
                val = Allocate(key);

                val.EnsureConstanted();
            }
            return val;
        }

        public void Remove(Pointer<TBase> key)
        {
            TExt val = Find(key);
            val?.Expire();

            RemoveItem(key);
        }

        public abstract TExt Find(Pointer<TBase> key);
        protected abstract TExt Allocate(Pointer<TBase> key);

        IExtension IContainer.Find(IntPtr key)
        {
            return this.Find(key);
        }

        protected abstract void SetItem(Pointer<TBase> key, TExt ext);
        public abstract void RemoveItem(Pointer<TBase> key);

        public abstract void Clear();

        public void LoadFromINI(Pointer<TBase> key, Pointer<CCINIClass> pINI)
        {
            TExt val = Find(key);
            if (val != null)
            {
                val.LoadFromINI(pINI);
            }
        }

        public void PrepareStream(Pointer<TBase> key, IStream pStm)
        {
            //Logger.Log("[PrepareStream] Next is {0:X} of type '{1}'\n", (int)key, Name);

            m_SavingObject = key;
            m_SavingStream = pStm;
        }

        public void SaveStatic()
        {
            if (m_SavingObject.IsNull == false && m_SavingStream != null)
            {
                //Logger.Log("[SaveStatic] Saving object {0:X} as '{1}'\n", (int)m_SavingObject, Name);

                if (!Save(m_SavingObject, m_SavingStream))
                {
                    Logger.Log("[SaveStatic] Saving failed!\n");
                }
            }
            else
            {
                Logger.Log("[SaveStatic] Object or Stream not set for '{0}': {1:X}, {2}\n", Name, (int)m_SavingObject, m_SavingStream);
            }

            m_SavingObject = Pointer<TBase>.Zero;
            m_SavingStream = null;
        }

        public void LoadStatic()
        {
            if (m_SavingObject.IsNull == false && m_SavingStream != null)
            {
                //Logger.Log("[LoadStatic] Loading object {0:X} as '{1}'\n", (int)m_SavingObject, Name);

                if (!Load(m_SavingObject, m_SavingStream))
                {
                    Logger.Log("[LoadStatic] Loading failed!\n");
                }
            }
            else
            {
                Logger.Log("[LoadStatic] Object or Stream not set for '{0}': {1:X}, {2}\n", Name, (int)m_SavingObject, m_SavingStream);
            }

            m_SavingObject = Pointer<TBase>.Zero;
            m_SavingStream = null;
        }

        // specialize this method to do type-specific stuff
        private bool Save(Pointer<TBase> key, IStream pStm)
        {
            return SaveKey(key, pStm) != null;
        }

        // specialize this method to do type-specific stuff
        private bool Load(Pointer<TBase> key, IStream pStm)
        {
            return LoadKey(key, pStm) != null;
        }

        private TExt SaveKey(Pointer<TBase> key, IStream pStm)
        {
            if (key.IsNull)
            {
                return null;
            }
            TExt val = Find(key);

            if (val != null)
            {
                pStm.WriteObject(val);

                val.SaveToStream(pStm);
                val.PartialSaveToStream(pStm);
            }

            return val;
        }

        private TExt LoadKey(Pointer<TBase> key, IStream pStm)
        {
            if (key.IsNull)
            {
                Logger.Log("Load attempted for a NULL pointer! WTF!\n");
                return null;
            }
            //TExt val = FindOrAllocate(key);

            pStm.ReadObject(out TExt val);

            val.OwnerObject = key;
            val.EnsureConstanted();

            SetItem(key, val);

            val.LoadFromStream(pStm);
            val.PartialLoadFromStream(pStm);

            return val;
        }
    }
}
