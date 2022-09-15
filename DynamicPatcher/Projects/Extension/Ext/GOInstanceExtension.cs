using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Extension.Components;
using Extension.Decorators;
using Extension.Script;
using PatcherYRpp;

namespace Extension.Ext
{

    [Serializable]
    public abstract class GOInstanceExtension<TExt, TBase> : InstanceExtension<TExt, TBase> where TExt : Extension<TBase>
    {
        private static string s_GameObjectName = $"{typeof(TExt).Name}'s GameObject";

        protected GOInstanceExtension(Pointer<TBase> OwnerObject) : base(OwnerObject)
        {
            m_GameObject = new GameObject(s_GameObjectName);

            m_GameObject.OnAwake += () => OnAwake(m_GameObject);
        }

        public GameObject GameObject => m_GameObject.GetAwaked();
        public DecoratorComponent DecoratorComponent
        {
            get
            {
                if (m_DecoratorComponent == null)
                {
                    m_DecoratorComponent = new DecoratorComponent();
                    m_DecoratorComponent.AttachToComponent(m_GameObject);
                }

                return m_DecoratorComponent;
            }
        }

        public override void SaveToStream(IStream stream)
        {
            base.SaveToStream(stream);

            m_GameObject.Foreach(c => c.SaveToStream(stream));
        }

        public override void LoadFromStream(IStream stream)
        {
            base.LoadFromStream(stream);

            m_GameObject.Foreach(c => c.LoadFromStream(stream));
        }

        /// <summary>
        /// initialize values
        /// </summary>
        /// <param name="gameObject">unawaked GameObject</param>
        protected virtual void OnAwake(GameObject gameObject)
        {

        }

        public override void OnExpire()
        {
            GameObject.Destroy(m_GameObject);
        }

        private GameObject m_GameObject;
        private DecoratorComponent m_DecoratorComponent;
    }
}
