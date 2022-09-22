using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
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



        public TTypeExt Type
        {
            get
            {
                if (type == null)
                {
                    type = CommonTypeExtension<TTypeExt, TTypeBase>.ExtMap.Find(OwnerObject.Ref.OwnType);
                }

                return type;
            }

            internal set => type = value;
        }
        public ref TTypeBase OwnerTypeRef => ref Type.OwnerRef;

        protected override void OnAwake(GameObject gameObject)
        {
            base.OnAwake(gameObject);

            CreateScriptable(Type.Scripts);
        }

        private TTypeExt type;
    }
}
