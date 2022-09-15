using Extension.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;

namespace Extension.Components
{
    [Serializable]
    public abstract class Component : IReloadable
    {
        public const int NO_ID = -1;

        protected Component()
        {
            ID = NO_ID;
            _transform = new Transform();
        }

        protected Component(bool withoutTransform = false)
        {
            ID = NO_ID;
            _transform = withoutTransform ? null : new Transform();
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
        
        [Obsolete("don't use before finish")]
        public Transform Transform { get; }

        public Component GetRoot()
        {
            if (Parent == null)
                return this;

            return Parent.GetRoot();
        }

        public void AttachToComponent(Component component)
        {
            if (_parent == component)
                return;

            DetachFromParent();

            _parent = component;
            component.AddComponent(this);
            GameObject?.AddComponentEx(this, component);
        }

        public void DetachFromParent()
        {
            Parent?.RemoveComponent(this);
            _parent = null;
        }


        /// <summary>
        /// get component that match predicate in direct children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Component GetComponent(Predicate<Component> predicate)
        {
            return _children.Find(predicate);
        }

        public Component GetComponent(int id)
        {
            return GetComponent(c => c.ID == id);
        }

        public Component GetComponent(Type type)
        {
            return GetComponent(c => type.IsAssignableFrom(c.GetType()));
        }

        public TComponent GetComponent<TComponent>() where TComponent : Component
        {
            return GetComponent(typeof(TComponent)) as TComponent;
        }

        /// <summary>
        /// get components that match predicate in direct children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Component[] GetComponents(Func<Component, bool> predicate)
        {
            return _children.Where(predicate).ToArray();
        }

        public Component[] GetComponents()
        {
            return GetComponents(_ => true);
        }

        public Component[] GetComponents(Type type)
        {
            return GetComponents(c => type.IsAssignableFrom(c.GetType())).ToArray();
        }
        public TComponent[] GetComponents<TComponent>() where TComponent : Component
        {
            return GetComponents(typeof(TComponent)).Cast<TComponent>().ToArray();
        }




        /// <summary>
        /// get component that match predicate in all children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Component GetComponentInChildren(Predicate<Component> predicate)
        {
            // find first level
            Component component = _children.Find(predicate);

            if (component != null)
            {
                return component;
            }

            foreach (var child in _children)
            {
                component = child.GetComponentInChildren(predicate);
                if (component != null)
                    break;
            }

            return component;
        }

        public Component GetComponentInChildren(int id)
        {
            return GetComponentInChildren(c => c.ID == id);
        }

        public Component GetComponentInChildren(Type type)
        {
            return GetComponentInChildren(type.IsInstanceOfType);
        }

        public TComponent GetComponentInChildren<TComponent>() where TComponent : Component
        {
            return GetComponentInChildren(typeof(TComponent)) as TComponent;
        }

        /// <summary>
        /// get components that match predicate in all children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Component[] GetComponentsInChildren(Func<Component, bool> predicate)
        {
            List<Component> components = _children.Where(predicate).ToList();

            foreach (var child in _children)
            {
                components.AddRange(child.GetComponentsInChildren(predicate));
            }

            return components.ToArray();
        }

        public Component[] GetComponentsInChildren()
        {
            return GetComponentsInChildren(_ => true);
        }

        public Component[] GetComponentsInChildren(Type type)
        {
            return GetComponentsInChildren(type.IsInstanceOfType).ToArray();
        }
        public TComponent[] GetComponentsInChildren<TComponent>() where TComponent : Component
        {
            return GetComponentsInChildren(typeof(TComponent)).Cast<TComponent>().ToArray();
        }


        /// <summary>
        /// get component that match predicate in direct children or parents
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Component GetComponentInParent(Predicate<Component> predicate)
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

        public Component GetComponentInParent(int id)
        {
            return GetComponentInParent(c => c.ID == id);
        }

        public Component GetComponentInParent(Type type)
        {
            return GetComponentInParent(type.IsInstanceOfType);
        }

        public TComponent GetComponentInParent<TComponent>() where TComponent : Component
        {
            return GetComponentInParent(typeof(TComponent)) as TComponent;
        }

        /// <summary>
        ///  get components that match predicate in direct children or parents
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Component[] GetComponentsInParent(Func<Component, bool> predicate)
        {
            List<Component> components = _children.Where(predicate).ToList();

            components.AddRange(_parent.GetComponentsInParent(predicate));

            return components.ToArray();
        }

        public Component[] GetComponentsInParent()
        {
            return GetComponentsInParent(_ => true);
        }

        public Component[] GetComponentsInParent(Type type)
        {
            return GetComponentsInParent(type.IsInstanceOfType).ToArray();
        }
        public TComponent[] GetComponentsInParent<TComponent>() where TComponent : Component
        {
            return GetComponentsInParent(typeof(TComponent)).Cast<TComponent>().ToArray();
        }



        protected virtual void AddComponent(Component component)
        {
            _children.Add(component);
        }
        protected virtual void RemoveComponent(Component component)
        {
            _children.Remove(component);
        }



        /// <summary>
        /// Awake is called when an enabled instance is being created.
        /// </summary>
        public virtual void Awake()
        {
        }
        /// <summary>
        /// OnStart called on the frame
        /// </summary>
        public virtual void Start()
        {
        }
        public virtual void OnUpdate()
        {
        }
        public virtual void OnLateUpdate()
        {
        }
        public virtual void OnRender()
        {

        }
        public virtual void OnDestroy()
        {
        }


        public virtual void SaveToStream(IStream stream)
        {
        }

        public virtual void LoadFromStream(IStream stream)
        {
        }

        [OnSerializing]
        protected void OnSerializing(StreamingContext context)
        {

        }

        [OnSerialized]
        protected void OnSerialized(StreamingContext context)
        {

        }

        [OnDeserializing]
        protected void OnDeserializing(StreamingContext context)
        {

        }

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
                Awake();
                ForeachChild(c => c.EnsureAwaked());
                _awaked = true;
            }
        }

        public void EnsureStarted()
        {
            if (!_started)
            {
                Start();
                ForeachChild(c => c.EnsureStarted());
                _started = true;
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
        }

        protected Transform _transform;
        [NonSerialized] // set back in OnDeserialized
        protected Component _parent = null;
        protected List<Component> _children = new List<Component>();

        private bool _awaked = false;
        private bool _started = false;
    }
}
