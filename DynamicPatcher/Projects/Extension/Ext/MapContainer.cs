using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using PatcherYRpp;

namespace Extension.Ext
{
    public class MapContainer<TExt, TBase> : Container<TExt, TBase> where TExt : Extension<TBase>
    {
        Dictionary<Pointer<TBase>, TExt> m_Items;

        ExtensionFactory<TExt, TBase> m_Factory;

        public MapContainer(string name) : this(name, new LambdaExtensionFactory<TExt, TBase>())
        {
        }

        public MapContainer(string name, ExtensionFactory<TExt, TBase> factory) : base(name)
        {
            m_Items = new Dictionary<Pointer<TBase>, TExt>();
            m_Factory = factory;
        }

        public override TExt Find(Pointer<TBase> key)
        {
            if (m_Items.TryGetValue(key, out TExt ext))
            {
                return ext;
            }
            return null;
        }

        protected override TExt Allocate(Pointer<TBase> key)
        {
            TExt val = m_Factory.Create(key);
            m_Items.Add(key, val);

            return val;
        }

        protected override void SetItem(Pointer<TBase> key, TExt ext)
        {
            m_Items[key] = ext;
        }

        public override void RemoveItem(Pointer<TBase> key)
        {
            m_Items.Remove(key);
        }

        public override void Clear()
        {
            if (m_Items.Count > 0)
            {
                Logger.Log("Cleared {0} items from {1}.\n", m_Items.Count, Name);
                m_Items.Clear();
            }
        }

    }

}
