using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Components
{
    internal class ComponentTraitQuery
    {
        public ComponentTraitQuery(Component component)
        {
            m_Component = component;
        }

        public Component[] QueryComponents()
        {
            if (m_Components == null)
            {
                Component[] components = m_Component.GetComponents((Predicate<Component>)null);
                m_Components = components.ToArray();
            }

            return m_Components;
        }

        public Component[] QueryComponentsInChildren()
        {
            if (m_ComponentsInChildren == null)
            {
                Component[] components = m_Component.GetComponentsInChildren((Predicate<Component>)null);
                m_ComponentsInChildren = components.ToArray();
            }

            return m_ComponentsInChildren;
        }

        public Component[] QueryComponents(Type type)
        {
            if (!m_ComponentsDict.TryGetValue(type, out Component[] components))
            {
                components = m_Component.GetComponents(c => type.IsAssignableFrom(c.GetType()));
                components = components.ToArray();
                m_ComponentsDict[type] = components;
            }

            return components;
        }

        public TComponent[] QueryComponents<TComponent>() where TComponent : Component
        {
            Type type = typeof(TComponent);

            if (!m_TComponentsDict.TryGetValue(type, out Array components))
            {
                components = Array.ConvertAll(QueryComponents(type), c => c as TComponent);
                m_TComponentsDict[type] = components;
            }

            return (TComponent[])components;
        }

        public void SetDirtyFlag(bool self = false, bool inChildren = false)
        {
            if (inChildren)
            {
                m_ComponentsInChildren = null;
            }
            if (self)
            {
                Clear();
            }

            m_Component.Parent?.m_TraitQuery.SetDirtyFlag(inChildren: true);
        }

        public void SetDirtyFlag(Type type)
        {
            m_ComponentsDict.Remove(type);
            m_TComponentsDict.Remove(type);

            m_Component.Parent?.m_TraitQuery.SetDirtyFlag(type);
        }

        public void Clear()
        {
            m_Components = null;
            m_ComponentsInChildren = null;

            m_ComponentsDict.Clear();
            m_TComponentsDict.Clear();
        }

        private Component m_Component;

        // buffered query result

        private Component[] m_Components;
        private Component[] m_ComponentsInChildren;

        private Dictionary<Type, Component[]> m_ComponentsDict = new();
        private Dictionary<Type, Array> m_TComponentsDict = new();
    }
}
