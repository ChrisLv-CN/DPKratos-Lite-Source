using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Miscellaneous
{
    [RunClassConstructorFirst]
    public static class DynamicScript
    {
        private static void Refresh(Assembly assembly)
        {
            Type[] types = ScriptManager.FindScriptTypes(assembly);

            foreach (var type in types)
            {
                if (!ScriptManager.TryGetScript(type.Name, out _))
                {
                    return;
                }
            }

            foreach (Type type in types)
            {

                // [warning!] unsafe change to scriptable
                unsafe
                {
                    // refresh modified scripts only
                    void RefreshScriptComponents<TExt, TBase>(GOInstanceExtension<TExt, TBase> ext) where TExt : Extension<TBase>
                    {
                        ScriptComponent[] components = ext.GameObject.GetComponentsInChildren(c => c.GetType().Name == type.Name).Cast<ScriptComponent>().ToArray();
                        if (components.Length > 0)
                        {
                            foreach (var component in components)
                            {
                                var root = component.Parent;
                                var script = component.Script;

                                component.DetachFromParent();

                                ScriptManager.CreateScriptableTo(root, script, ext as TExt);
                            }
                        }
                    }

                    void Refresh<TExt, TBase>(Container<TExt, TBase> container, ref DynamicVectorClass<Pointer<TBase>> dvc) where TExt : GOInstanceExtension<TExt, TBase>
                    {
                        Logger.Log("refreshing {0}'s {1}...", typeof(TExt).Name, type.Name);
                        foreach (var pItem in dvc)
                        {
                            var ext = container.Find(pItem);
                            RefreshScriptComponents(ext);
                        }
                    }

                    Refresh(TechnoExt.ExtMap, ref TechnoClass.Array);
                    Refresh(BulletExt.ExtMap, ref BulletClass.Array);
#if USE_ANIM_EXT
                    Refresh(AnimExt.ExtMap, ref AnimClass.Array);
#endif
#if USE_CELL_EXT
                    Refresh(CellExt.ExtMap, ref MapClass.Instance.Cells);
#endif
                }
            }
        }


        private static void Patcher_AssemblyRefresh(object sender, AssemblyRefreshEventArgs args)
        {
            Assembly assembly = args.RefreshedAssembly;

            Refresh(assembly);
        }

        static void ScenarioStartEventHandler(object sender, EventArgs e)
        {
            EventSystem.General.RemovePermanentHandler(EventSystem.General.ScenarioStartEvent, ScenarioStartEventHandler);
            Task.Delay(1000).ContinueWith(_ =>
            {
                Program.Patcher.AssemblyRefresh += Patcher_AssemblyRefresh;
            });
        }

        static DynamicScript()
        {
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, ScenarioStartEventHandler);
        }
    }
}
