using Extension.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using System.Runtime.CompilerServices;

namespace Extension.Components
{
    [Serializable]
    public abstract partial class Component : IReloadable
    {
        public const int NO_ID = -1;

        protected Component()
        {
            ID = NO_ID;

            m_TraitQuery = new ComponentTraitQuery(this);
        }

        protected Component(int id) : this()
        {
            ID = id;
        }

        [Obsolete("don't use")]
        public int ID { get; set; }

        public string Name { get; set; }
        public string Tag { get; set; }
        
        public Component Parent => _parent;
        public Component Root => GetRoot();
        public GameObject GameObject => Root as GameObject;

        [Obsolete("don't use")]
        public virtual Transform Transform => GameObject.Transform;

        public Component GetRoot()
        {
            if (_parent == null)
                return this;

            return _parent.GetRoot();
        }

        public void AttachToComponent(Component component)
        {
            if (_parent == component)
                return;

            DetachFromParent();

            component.AddComponent(this);
            GameObject?.AddComponentEx(this, component);
        }

        public void DetachFromParent()
        {
            _parent?.RemoveComponent(this);
        }


        /// <summary>
        /// get component that match predicate in direct children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component GetComponent(Predicate<Component> predicate);

        public partial Component GetComponent(int id);

        public partial Component GetComponent(Type type);

        public partial TComponent GetComponent<TComponent>() where TComponent : Component;

        /// <summary>
        /// get components that match predicate in direct children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component[] GetComponents(Predicate<Component> predicate);
        public partial Component[] GetComponents();
        public partial Component[] GetComponents(Type type);
        public partial TComponent[] GetComponents<TComponent>() where TComponent : Component;

        /// <summary>
        /// get component that match predicate in all children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component GetComponentInChildren(Predicate<Component> predicate);
        public partial Component GetComponentInChildren(int id);
        public partial Component GetComponentInChildren(Type type);
        public partial TComponent GetComponentInChildren<TComponent>() where TComponent : Component;

        /// <summary>
        /// get components that match predicate in all children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component[] GetComponentsInChildren(Predicate<Component> predicate);
        public partial Component[] GetComponentsInChildren();
        public partial Component[] GetComponentsInChildren(Type type);
        public partial TComponent[] GetComponentsInChildren<TComponent>() where TComponent : Component;

        /// <summary>
        /// get component that match predicate in direct children or parents
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component GetComponentInParent(Predicate<Component> predicate);
        public partial Component GetComponentInParent(int id);
        public partial Component GetComponentInParent(Type type);
        public partial TComponent GetComponentInParent<TComponent>() where TComponent : Component;

        /// <summary>
        ///  get components that match predicate in direct children or parents
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component[] GetComponentsInParent(Predicate<Component> predicate);
        public partial Component[] GetComponentsInParent();
        public partial Component[] GetComponentsInParent(Type type);
        public partial TComponent[] GetComponentsInParent<TComponent>() where TComponent : Component;



        protected void AddComponent(Component component)
        {
            component._parent = this;
            _children.Add(component);

            m_TraitQuery.SetDirtyFlag(self: true);
            m_TraitQuery.SetDirtyFlag(component.GetType());
        }
        protected void RemoveComponent(Component component)
        {
            _children.Remove(component);
            component._parent = null;

            m_TraitQuery.SetDirtyFlag(self: true);
            m_TraitQuery.SetDirtyFlag(component.GetType());
        }



        /// <summary>
        /// Awake is called when an enabled instance is being created.
        /// </summary>
        public virtual void Awake() { }
        /// <summary>
        /// OnStart called on the frame
        /// </summary>
        public virtual void Start() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnWarpUpdate() { }
        public virtual void OnRender() { }
        public virtual void OnRenderEnd() { }
        public virtual void OnDestroy() { }


        public virtual void SaveToStream(IStream stream) { }
        public virtual void LoadFromStream(IStream stream) { }

        [OnSerializing]
        protected void OnSerializing(StreamingContext context) { }
        [OnSerialized]
        protected void OnSerialized(StreamingContext context) { }
        [OnDeserializing]
        protected void OnDeserializing(StreamingContext context) { }
        [OnDeserialized]
        protected void OnDeserialized(StreamingContext context)
        {
            void SetParent(Component component)
            {
                foreach (var child in component._children)
                {
                    child._parent = component;
                    SetParent(child);
                }
            }

            SetParent(this);
            m_TraitQuery = new ComponentTraitQuery(this);
        }

        /// <summary>
        /// execute action for each components in root (include itself)
        /// </summary>
        /// <param name="action"></param>
        public void Foreach(Action<Component> action)
        {
            ForeachComponents(this, action);
        }
        public void ForeachChild(Action<Component> action)
        {
            ForeachComponents(GetComponentsInChildren(), action);

            // slower
            //ForeachComponents(_children, action);
            //int length = _children.Count;
            //for (int i = 0; i < length; i++)
            //{
            //    var child = _children[i];
            //    child.ForeachChild(action);
            //}
        }

        public static void ForeachComponents(IEnumerable<Component> components, Action<Component> action)
        {
            foreach (var component in components)
            {
                try
                {
                    action(component);
                }
                catch (Exception e)
                {
                    Logger.PrintException(e);
                }
            }
        }
        /// <summary>
        /// execute action for each components in root (include root)
        /// </summary>
        /// <param name="root">the root component</param>
        /// <param name="action">the action to executed</param>
        public static void ForeachComponents(Component root, Action<Component> action)
        {
            try
            {
                action(root);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            root.ForeachChild(action);
        }

        /// <summary>
        /// destroy the component and call OnDestroy
        /// </summary>
        /// <param name="component"></param>
        public static void Destroy(Component component)
        {
            component.Destroy();
            component.DetachFromParent();
        }

        public void EnsureAwaked()
        {
            if (!_awaked)
            {
                _awaked = true;
                Awake();
                ForeachChild(c => c.EnsureAwaked());
            }
        }

        public void EnsureStarted()
        {
            if (!_started)
            {
                _started = true;
                Start();
                ForeachChild(c => c.EnsureStarted());
            }
        }

        private void Destroy()
        {
            foreach (Component child in _children)
            {
                child.Destroy();
            }

            try
            {
                OnDestroy();
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }

            _children.Clear();
            m_TraitQuery.Clear();
        }

        [NonSerialized] // set back in OnDeserialized
        internal Component _parent = null;
        private List<Component> _children = new List<Component>();

        private bool _awaked = false;
        private bool _started = false;
    }

    public partial class Component
    {
        private static List<Component> s_Buffer = new(10);
        private List<Component> GetBuffer()
        {
            s_Buffer.Clear();
            return s_Buffer;
        }
        private static List<Component[]> s_ArrayBuffer = Enumerable.Range(0, 32).Select(size => new Component[size]).ToList();
        private Component[] FastToArray(List<Component> list)
        {
            int size = list.Count;
            if (s_ArrayBuffer.Count > size)
            {
                var array = s_ArrayBuffer[size];
                list.CopyTo(array);
                return array;
            }
            return list.ToArray();
        }
        private TComponent[] FastToArray<TComponent>(Component[] array) where TComponent : Component
        {
            //return array as TComponent[]; // slower
            return Array.ConvertAll(array, c => c as TComponent);
        }

        private static Predicate<Component> NO_PREDICATION = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void Filter(List<Component> src, List<Component> dst, Predicate<Component> predicate)
        {
            int length = src.Count;

            if (predicate == null)
            {
                for (int i = 0; i < length; i++)
                {
                    var child = src[i];
                    dst.Add(child);
                }
                return;
            }

            for (int i = 0; i < length; i++)
            {
                var child = src[i];
                if (predicate(child))
                {
                    dst.Add(child);
                }
            }
        }

        public partial Component GetComponent(Predicate<Component> predicate)
        {
            return _children.Find(predicate);
        }

        public partial Component GetComponent(int id)
        {
            return GetComponent(c => c.ID == id);
        }

        public partial Component GetComponent(Type type)
        {
            return m_TraitQuery.QueryComponents(type).FirstOrDefault();
            //return GetComponent(c => type.IsAssignableFrom(c.GetType()));
        }

        public partial TComponent GetComponent<TComponent>() where TComponent : Component
        {
            return m_TraitQuery.QueryComponents<TComponent>().FirstOrDefault();
            //return GetComponent(typeof(TComponent)) as TComponent;
        }

        /// <summary>
        /// get component that match predicate in all children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component GetComponentInChildren(Predicate<Component> predicate)
        {
            // find first level
            Component component = _children.Find(predicate);

            if (component != null)
            {
                return component;
            }

            int length = _children.Count;
            for (int i = 0; i < length; i++)
            {
                var child = _children[i];
                component = child.GetComponentInChildren(predicate);
                if (component != null)
                    break;
            }

            return component;
        }

        public partial Component GetComponentInChildren(int id)
        {
            return GetComponentInChildren(c => c.ID == id);
        }

        public partial Component GetComponentInChildren(Type type)
        {
            return GetComponentInChildren(type.IsInstanceOfType);
        }

        public partial TComponent GetComponentInChildren<TComponent>() where TComponent : Component
        {
            return GetComponentInChildren(typeof(TComponent)) as TComponent;
        }

        /// <summary>
        /// get component that match predicate in direct children or parents
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component GetComponentInParent(Predicate<Component> predicate)
        {
            // find first level
            Component component = _children.Find(predicate);

            if (component != null)
            {
                return component;
            }

            component = _parent.GetComponentInParent(predicate);

            return component;
        }

        public partial Component GetComponentInParent(int id)
        {
            return GetComponentInParent(c => c.ID == id);
        }

        public partial Component GetComponentInParent(Type type)
        {
            return GetComponentInParent(type.IsInstanceOfType);
        }

        public partial TComponent GetComponentInParent<TComponent>() where TComponent : Component
        {
            return GetComponentInParent(typeof(TComponent)) as TComponent;
        }

        /// <summary>
        /// get components that match predicate in direct children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component[] GetComponents(Predicate<Component> predicate)
        {
            var buffer = GetBuffer();

            Filter(_children, buffer, predicate);

            return FastToArray(buffer);
        }

        public partial Component[] GetComponents()
        {
            return m_TraitQuery.QueryComponents();
            //return GetComponents(NO_PREDICATION);
        }

        public partial Component[] GetComponents(Type type)
        {
            return m_TraitQuery.QueryComponents(type);
            //return GetComponents(c => type.IsAssignableFrom(c.GetType()));
        }
        public partial TComponent[] GetComponents<TComponent>() where TComponent : Component
        {
            return m_TraitQuery.QueryComponents<TComponent>();
            //return FastToArray<TComponent>(GetComponents(typeof(TComponent)));
        }

        /// <summary>
        /// get components that match predicate in all children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component[] GetComponentsInChildren(Predicate<Component> predicate)
        {
            var buffer = GetBuffer();

            void GetComponentsInChildren(Component component)
            {
                var children = component._children;
                int length = children.Count;

                Filter(children, buffer, predicate);

                for (int i = 0; i < length; i++)
                {
                    var child = children[i];
                    GetComponentsInChildren(child);
                }
            }

            GetComponentsInChildren(this);

            return FastToArray(buffer);
        }

        public partial Component[] GetComponentsInChildren()
        {
            return m_TraitQuery.QueryComponentsInChildren();
            //return GetComponentsInChildren(NO_PREDICATION);
        }

        public partial Component[] GetComponentsInChildren(Type type)
        {
            return GetComponentsInChildren(type.IsInstanceOfType);
        }
        public partial TComponent[] GetComponentsInChildren<TComponent>() where TComponent : Component
        {
            return FastToArray<TComponent>(GetComponentsInChildren(typeof(TComponent)));
        }

        /// <summary>
        ///  get components that match predicate in direct children or parents
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public partial Component[] GetComponentsInParent(Predicate<Component> predicate)
        {
            var buffer = GetBuffer();

            void GetComponentsInParent(Component component)
            {
                var children = component._children;
                int length = children.Count;

                Filter(children, buffer, predicate);

                if (component._parent != null)
                {
                    GetComponentsInParent(component._parent);
                }
            }

            GetComponentsInParent(this);

            return FastToArray(buffer);
        }

        public partial Component[] GetComponentsInParent()
        {
            return GetComponentsInParent(NO_PREDICATION);
        }

        public partial Component[] GetComponentsInParent(Type type)
        {
            return GetComponentsInParent(type.IsInstanceOfType);
        }
        public partial TComponent[] GetComponentsInParent<TComponent>() where TComponent : Component
        {
            return FastToArray<TComponent>(GetComponentsInParent(typeof(TComponent)));
        }

        [NonSerialized]
        internal ComponentTraitQuery m_TraitQuery;
    }
}
