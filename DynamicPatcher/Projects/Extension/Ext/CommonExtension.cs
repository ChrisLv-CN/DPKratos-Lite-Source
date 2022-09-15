using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Extension.Components;
using Extension.Decorators;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.Ext
{
    /// <summary>
    /// Type extension with scripts
    /// </summary>
    /// <typeparam name="TExt"></typeparam>
    /// <typeparam name="TBase"></typeparam>
    [Serializable]
    public abstract class CommonTypeExtension<TExt, TBase> : TypeExtension<TExt, TBase>, IHaveScript where TExt : Extension<TBase>
    {
        protected CommonTypeExtension(Pointer<TBase> OwnerObject) : base(OwnerObject)
        {

        }


        public List<Script.Script> Scripts => _scripts;

        protected override void LoadFromINI(INIReader reader)
        {
            string section = OwnerObject.Convert<AbstractTypeClass>().Ref.ID;

            reader.Read(section, "Scripts", ref _scripts);
        }

        private List<Script.Script> _scripts;
    }

    [Serializable]
    public abstract class CommonInstanceExtension<TExt, TBase, TTypeExt, TTypeBase> : GOInstanceExtension<TExt, TBase>
        where TExt : Extension<TBase>
        where TBase : IOwnAbstractType<TTypeBase>
        where TTypeExt : CommonTypeExtension<TTypeExt, TTypeBase>
    {

        protected CommonInstanceExtension(Pointer<TBase> OwnerObject) : base(OwnerObject)
        {
        }



        public TTypeExt Type { get; internal set; }
        public ref TTypeBase OwnerTypeRef => ref Type.OwnerRef;

        protected override void OnAwake(GameObject gameObject)
        {
            base.OnAwake(gameObject);

            Type = CommonTypeExtension<TTypeExt, TTypeBase>.ExtMap.Find(OwnerObject.Ref.OwnType);

            if (Type.Scripts != null)
            {
                foreach (var script in Type.Scripts)
                {
                    ScriptManager.CreateScriptableTo(gameObject, script, this as TExt);
                }
            }
            foreach (var script in s_GlobalScripts)
            {
                ScriptManager.CreateScriptableTo(gameObject, script, this as TExt);
            }
        }



        private static List<Script.Script> s_GlobalScripts = new();

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
        }
    }
}
