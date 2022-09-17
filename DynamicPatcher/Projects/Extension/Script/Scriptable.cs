using DynamicPatcher;
using Extension.Ext;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlobalScriptableAttribute : Attribute
    {
        public GlobalScriptableAttribute(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class UpdateBeforeAttribute : Attribute
    {
        public UpdateBeforeAttribute(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class UpdateAfterAttribute : Attribute
    {
        public UpdateAfterAttribute(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; }
    }

    public interface IScriptable : IReloadable
    {
    }

    [Serializable]
    public abstract class Scriptable<T> : ScriptComponent, IScriptable
    {
        public T Owner { get; protected set; }

        protected Scriptable(T owner) : base()
        {
            Owner = owner;
        }
    }

    internal static class ScriptableHelpers
    {
        class ScriptDepenency
        {
            public ScriptDepenency(Script script)
            {
                Script = script;

                UpdateAfter = (script.ScriptableType.GetCustomAttribute<UpdateAfterAttribute>()?.Types ?? new Type[0]).ToList();
                UpdateBefore = (script.ScriptableType.GetCustomAttribute<UpdateBeforeAttribute>()?.Types ?? new Type[0]).ToList();
            }

            public Script Script;
            public List<Type> UpdateAfter;
            public List<Type> UpdateBefore;

            public Type Type => Script.ScriptableType;
            public List<ScriptDepenency> Links = new();
            public int Indegree;
        }

        private static List<ScriptDepenency> BuildGraph(List<Script> scripts)
        {
            var dict = scripts.Select(s => new ScriptDepenency(s)).ToDictionary(d => d.Type);

            // convert UpdateAfter and UpdateBefore to single relation
            foreach (var pair in dict)
            {
                var dep = pair.Value;
                dep.Links.AddRange(dep.UpdateBefore.Where(t => dict.ContainsKey(t)).Select(t => dict[t]));

                foreach (var t in dep.UpdateAfter.Where(t => dict.ContainsKey(t)))
                {
                    dict[t].Links.Add(dep);
                }
            }

            foreach (var pair in dict)
            {
                var dep = pair.Value;
                dep.Links.ForEach(d => d.Indegree++);
            }

            return dict.Values.ToList();
        }

        private static void TopoSort(List<ScriptDepenency> graph)
        {
            var tmp = new List<ScriptDepenency>(graph);
            graph.Clear();

            while (tmp.Count > 0)
            {
                int count = tmp.Count;
                tmp.RemoveAll(d =>
                {
                    if (d.Indegree == 0)
                    {
                        d.Links.ForEach(d => d.Indegree--);
                        graph.Add(d);
                        return true;
                    }

                    return false;
                });

                if (count == tmp.Count)
                {
                    Logger.LogError("Circle dependent chain detected: {0}", string.Join(", ", tmp.Select(d => d.Type.FullName)));
                    graph.AddRange(tmp);
                    return;
                }
            }
        }

        public static void Sort(List<Script> scripts)
        {
            var graph = BuildGraph(scripts);
            TopoSort(graph);

            scripts.Clear();
            scripts.AddRange(graph.Select(d => d.Script));

            Check(scripts);
        }

        public static void Check(List<Script> scripts)
        {
            var items = scripts.Select(s => new ScriptDepenency(s)).ToList();

            foreach (var item in items)
            {
                if (item.UpdateBefore.Count + item.UpdateAfter.Count == 0)
                    continue;

                int itemIdx = scripts.IndexOf(item.Script);
                for (int i = 0; i < scripts.Count; i++)
                {
                    var cur = scripts[i];
                    // test if item before cur
                    if (item.UpdateAfter.Contains(cur.ScriptableType) && i > itemIdx)
                    {
                        Logger.LogError("Wrong Order: {0} is before {1}!", item.Script.ScriptableType.Name, cur.ScriptableType.Name);
                    }
                    // test if item after cur
                    if (item.UpdateBefore.Contains(cur.ScriptableType) && i < itemIdx)
                    {
                        Logger.LogError("Wrong Order: {0} is after {1}!", item.Script.ScriptableType.Name, cur.ScriptableType.Name);
                    }

                    foreach (var t in item.UpdateAfter.Concat(item.UpdateBefore))
                    {
                        if (!t.IsDefined(typeof(GlobalScriptableAttribute)))
                        {
                            Logger.LogError("{0} has no relation to {1} undecorated with GlobalScriptable", item.Type.FullName, t.FullName);
                        }
                    }
                }
            }
        }
    }
}
