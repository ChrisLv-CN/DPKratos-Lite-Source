using DynamicPatcher;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext
{
    /// <summary>
    /// Store TExt with fixed index in list
    /// </summary>
    /// <typeparam name="TExt"></typeparam>
    /// <typeparam name="TBase"></typeparam>
    public class FixedListContainer<TExt, TBase> : Container<TExt, TBase> where TExt : Extension<TBase>
    {
        List<TExt> m_Items;

        ExtensionFactory<TExt, TBase> m_Factory;

        Func<Pointer<TBase>, int> m_GetIdx;

        public FixedListContainer(string name, ExtensionFactory<TExt, TBase> factory = null, Func<Pointer<TBase>, int> getIdx = null) : base(name)
        {
            m_Items = new List<TExt>();
            m_Factory = factory ?? new LambdaExtensionFactory<TExt, TBase>();
            m_GetIdx = getIdx ?? (ptr => ptr.Cast<AbstractClass>().Ref.GetArrayIndex());
        }

        public override TExt Find(Pointer<TBase> key)
        {
            int idx = m_GetIdx(key);
            if (m_Items.Count > idx)
            {
                return m_Items[idx];
            }
            return null;
        }

        protected override TExt Allocate(Pointer<TBase> key)
        {
            TExt val = m_Factory.Create(key);
            SetItem(key, val);

            return val;
        }

        protected override void SetItem(Pointer<TBase> key, TExt ext)
        {
            int idx = m_GetIdx(key);
            if (m_Items.Count <= idx)
            {
                // fill null in gap
                int growth = idx - m_Items.Count + 1;
                for (int i = 0; i < growth; i++)
                {
                    m_Items.Add(null);
                }
            }

            m_Items[idx] = ext;
        }

        public override void RemoveItem(Pointer<TBase> key)
        {
            SetItem(key, null);
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
