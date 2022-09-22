using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
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

            m_GameObject.SetTransform(new YRTransform(this));
            m_GameObject.OnAwake += () =>
            {
                OnAwake(m_GameObject);
                // ensure to be invoked at least once
                CreateScriptable(null);
            };
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

        private static List<Script.Script> s_GlobalScripts = new();
        private static List<ScriptComponent> s_ScriptComponentBuffer = new();

        internal void CreateScriptable(IEnumerable<Script.Script> scripts)
        {
            if (m_ScriptsCreated)
                return;

            s_ScriptComponentBuffer.Clear();
            scripts = scripts == null ? s_GlobalScripts : s_GlobalScripts.Concat(scripts);

            foreach (var script in scripts)
            {
                try
                {
                    var scriptable = ScriptManager.CreateScriptable(script, this as TExt);
                    s_ScriptComponentBuffer.Add(scriptable);
                }
                catch (Exception e)
                {
                    Logger.LogError("unable to create scriptable {0}", script.ScriptableType.FullName);
                    Logger.PrintException(e);
                }
            }

            foreach (var scriptable in s_ScriptComponentBuffer)
            {
                m_GameObject.AddComponentNotAwake(scriptable);
            }

            foreach (var scriptable in s_ScriptComponentBuffer)
            {
                scriptable.EnsureAwaked();
            }

            m_ScriptsCreated = true;
        }

        public static void AddGlobalScript(Script.Script script)
        {
            var oldScript = s_GlobalScripts.Find(s => s.Name == script.Name);
            if (oldScript != null)
            {
                oldScript.ScriptableType = script.ScriptableType;
            }
            else
            {
                s_GlobalScripts.Add(script);
            }

            ScriptableHelpers.Sort(s_GlobalScripts);
        }

        private GameObject m_GameObject;
        private DecoratorComponent m_DecoratorComponent;
        private bool m_ScriptsCreated;
    }
}
