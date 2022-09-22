using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Coroutines;

namespace Extension.Components
{
    public interface IGameObject
    {
        Coroutine StartCoroutine(IEnumerator enumerator);
        void StopCoroutine(IEnumerator enumerator);
        void StopCoroutine(Coroutine coroutine);

        Component GetComponent(Predicate<Component> predicate);
        Component GetComponent(int id);
        Component GetComponent(Type type);
        TComponent GetComponent<TComponent>() where TComponent : Component;

        /// <summary>
        /// get components that match predicate in direct children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Component[] GetComponents(Func<Component, bool> predicate);
        Component[] GetComponents();
        Component[] GetComponents(Type type);
        TComponent[] GetComponents<TComponent>() where TComponent : Component;

        
        /// <summary>
        /// get component that match predicate in all children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Component GetComponentInChildren(Predicate<Component> predicate);
        Component GetComponentInChildren(int id);
        Component GetComponentInChildren(Type type);
        TComponent GetComponentInChildren<TComponent>() where TComponent : Component;

        /// <summary>
        /// get components that match predicate in all children
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Component[] GetComponentsInChildren(Func<Component, bool> predicate);
        Component[] GetComponentsInChildren();
        Component[] GetComponentsInChildren(Type type);
        TComponent[] GetComponentsInChildren<TComponent>() where TComponent : Component;


        /// <summary>
        /// get component that match predicate in direct children or parents
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Component GetComponentInParent(Predicate<Component> predicate);
        Component GetComponentInParent(int id);
        Component GetComponentInParent(Type type);
        TComponent GetComponentInParent<TComponent>() where TComponent : Component;

        /// <summary>
        ///  get components that match predicate in direct children or parents
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Component[] GetComponentsInParent(Func<Component, bool> predicate);
        Component[] GetComponentsInParent();
        Component[] GetComponentsInParent(Type type);
        TComponent[] GetComponentsInParent<TComponent>() where TComponent : Component;
        
        void AddComponent(Component component);
        /// <summary>
        /// attach component to the child of GameObject
        /// </summary>
        /// <param name="component"></param>
        /// <param name="attached">component to be attached</param>
        void AddComponentEx(Component component, Component attached);
        void RemoveComponent(Component component);
    }

    /// <summary>
    /// although GameObject derived from Component, don't use it as Component now.
    /// </summary>
    [Serializable]
    public sealed class GameObject : Component, IGameObject
    {
        internal GameObject(string name) : base(0)
        {
            Name = name;

            _unstartedComponents = new List<Component>();
            _unstartedComponents.Add(this);

            _coroutineSystem = new CoroutineSystem();
        }

        public override Transform Transform => _transform;

        internal event Action OnAwake;

        public override void Awake()
        {
            base.Awake();

            OnAwake?.Invoke();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (_unstartedComponents.Count > 0)
            {
                Component.ForeachComponents(_unstartedComponents, c => c.EnsureStarted());
                _unstartedComponents.Clear();
            }

            _coroutineSystem.Update();
        }

        /// <summary>
        /// return myself and ensure awaked
        /// </summary>
        /// <returns></returns>
        public GameObject GetAwaked()
        {
            EnsureAwaked();

            return this;
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return _coroutineSystem.StartCoroutine(enumerator);
        }

        public void StopCoroutine(IEnumerator enumerator)
        {
            _coroutineSystem.StopCoroutine(enumerator);
        }
        public void StopCoroutine(Coroutine coroutine)
        {
            _coroutineSystem.StopCoroutine(coroutine);
        }

        public new void AddComponent(Component component)
        {
            AddComponentEx(component, this);
        }

        public void AddComponentEx(Component component, Component attached)
        {
            if (component.Parent != attached)
            {
                component.AttachToComponent(attached);
            }
            else
            {
                // AddComponentEx will called again by AttachToComponent and enter this branch

                component.EnsureAwaked();
                _unstartedComponents.Add(component);
            }
        }

        public new void RemoveComponent(Component component)
        {
            component.DetachFromParent();
        }

        public static void Destroy(GameObject gameObject)
        {
            Component.Destroy(gameObject);
        }

        internal void AddComponentNotAwake(Component component)
        {
            base.AddComponent(component);
            _unstartedComponents.Add(component);
        }

        internal void SetTransform(Transform transform)
        {
            _transform = transform;
        }

        private List<Component> _unstartedComponents;
        private CoroutineSystem _coroutineSystem;
        private Transform _transform;
    }
}
