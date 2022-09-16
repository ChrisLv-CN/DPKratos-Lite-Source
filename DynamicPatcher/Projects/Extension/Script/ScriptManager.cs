using DynamicPatcher;
using Extension.Components;
using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    public partial class ScriptManager
    {
        // type name -> script
        static Dictionary<string, Script> Scripts = new Dictionary<string, Script>();

        public static bool TryGetScript(string scriptName, out Script script)
        {
            return Scripts.TryGetValue(scriptName, out script);
        }

        /// <summary>
        /// create script or get a exist script by script name
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public static Script GetScript(string scriptName)
        {
            if(scriptName == null)
                return null;

            if (TryGetScript(scriptName, out Script script))
            {
                return script;
            }
            else
            {
                Script newScript = new Script(scriptName);
                Assembly assembly = FindScriptAssembly(scriptName);

                if (assembly == null)
                {
                    Logger.LogError("[ScriptManager] could not find script: {0}", scriptName);
                    return null;
                }

                RefreshScript(newScript, assembly);

                if (newScript.ScriptableType.IsDefined(typeof(GlobalScriptableAttribute)))
                {
                    Logger.LogWarning("[ScriptManager] not allow to get global scriptable: {0}", scriptName);
                    return null;
                }

                RegisterScript(newScript);
                return newScript;
            }
        }
        /// <summary>
        /// get all scripts in cs file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<Script> GetScripts(string fileName)
        {
            List<Script> scripts = new List<Script>();

            var pair = Program.Patcher.FileAssembly.FirstOrDefault((pair) => pair.Key.EndsWith(fileName));
            Assembly assembly = pair.Value;

            if (assembly == null)
            {
                Logger.LogError("ScriptManager could not find scripts in file: {0}", fileName);
                return scripts;
            }

            Type[] types = FindScriptTypes(assembly);

            foreach (var type in types)
            {
                Script script = GetScript(type.Name);
                scripts.Add(script);
            }

            return scripts;
        }
        /// <summary>
        /// get scripts by script names or cs file names
        /// </summary>
        /// <param name="scriptList"></param>
        /// <returns></returns>
        public static List<Script> GetScripts(IEnumerable<string> scriptList)
        {
            List<Script> scripts = new List<Script>();

            foreach (string item in scriptList)
            {
                if (item.EndsWith(".cs"))
                {
                    scripts.AddRange(GetScripts(item));
                }
                else
                {
                    scripts.Add(GetScript(item));
                }
            }

            return scripts;
        }

        public static Type[] FindScriptTypes(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            if (types == null || types.Length == 0)
                return new Type[0];

            return types.Where(t => typeof(IScriptable).IsAssignableFrom(t)).ToArray();
        }

        public static Assembly FindScriptAssembly(string scriptName)
        {
            foreach (var pair in Program.Patcher.FileAssembly)
            {
                Assembly assembly = pair.Value;
                Type[] types = FindScriptTypes(assembly);
                foreach (Type type in types)
                {
                    if (IsScript(type, scriptName))
                    {
                        return assembly;
                    }
                }
            }

            // may find in AppDomain.CurrentDomain.GetAssemblies() later

            return null;
        }




        private static void RefreshScript(Script script, Assembly assembly)
        {
            Type[] types = FindScriptTypes(assembly);
            foreach (Type type in types)
            {
                if (IsScript(type, script.Name))
                {
                    script.ScriptableType = type;
                    break;
                }
            }
        }

        private static void RefreshScripts(Assembly assembly)
        {
            Type[] types = FindScriptTypes(assembly);

            foreach (Type type in types)
            {
                string scriptName = type.Name;
                if (TryGetScript(scriptName, out Script script))
                {
                    RefreshScript(script, assembly);
                }
                else
                {
                    script = GetScript(scriptName);
                }

                if (script != null)
                {
                    Logger.Log("refresh script: {0}", script.Name);
                }
            }

        }

        private static void RegisterScript(Script script)
        {
            Scripts[script.Name] = script;

            var aliases = script.ScriptableType.GetCustomAttribute<ScriptAliasAttribute>()?.Names ?? new string[0];
            foreach (string alias in aliases)
            {
                Scripts[alias] = script;
            }
        }

        private static bool IsScript(Type type, string scriptName)
        {
            if (type.Name == scriptName)
            {
                return true;
            }
            foreach (string alias in type.GetCustomAttribute<ScriptAliasAttribute>()?.Names ?? new string[0])
            {
                if (alias == scriptName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void InjectScript(Type type)
        {
            if (type.IsInterface || type.IsAbstract)
                return;

            Script script = new Script(type.FullName);
            script.ScriptableType = type;

            RegisterScript(script);

            Logger.Log("[ScriptManager] script {0} injected.", script.Name);
        }

        private static void InjectGlobalScript(Type type)
        {
            var attribute = type.GetCustomAttribute<GlobalScriptableAttribute>();
            if (attribute == null || attribute.Types == null || attribute.Types.Length == 0)
                return;

            Script script = new Script(type.FullName);
            script.ScriptableType = type;

            foreach (var scriptedType in attribute.Types)
            {
                var addGlobalScript = scriptedType.GetMethod("AddGlobalScript", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                addGlobalScript.Invoke(null, new object[] { script });
            }

            Logger.Log("[ScriptManager] global script {0} injected into {1}.", script.Name, string.Join(", ", attribute.Types.Select(t => t.FullName)));
        }

        private static void InjectAllScripts(Assembly assembly)
        {
            var types = FindScriptTypes(assembly);

            bool injectAll = !Program.Patcher.FileAssembly.ContainsValue(assembly);

            foreach (Type type in types)
            {
                if (type.IsDefined(typeof(GlobalScriptableAttribute)))
                {
                    InjectGlobalScript(type);
                }
                else if (injectAll)
                {
                    InjectScript(type);
                }
            }
        }

        private static void InjectAllScripts()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            assemblies = assemblies
                .Where(a => !a.IsDynamic)
                .Where(a => a.Location.StartsWith(GlobalVars.DynamicPatcherDirectory) || Program.Patcher.FileAssembly.ContainsValue(a))
                .ToArray();

            foreach (var assembly in assemblies)
            {
                InjectAllScripts(assembly);
            }

        }




        private static void Patcher_AssemblyRefresh(object sender, AssemblyRefreshEventArgs args)
        {
            Assembly assembly = args.RefreshedAssembly;

            ScriptCtors.Clear();
            RefreshScripts(assembly);
            InjectAllScripts(assembly);
        }

        static ScriptManager()
        {
            Program.Patcher.AssemblyRefresh += Patcher_AssemblyRefresh;

            InjectAllScripts();
        }

    }
}
