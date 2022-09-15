using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Extension.Components;
using Extension.Decorators;
using Extension.Script;
using PatcherYRpp;

namespace Extension.Ext
{
    [Serializable]
    [Obsolete()]
    public abstract class ECSInstanceExtension<TExt, TBase> : InstanceExtension<TExt, TBase> where TExt : Extension<TBase>
    {
        protected ECSInstanceExtension(Pointer<TBase> OwnerObject) : base(OwnerObject)
        {
        }


        public override void SaveToStream(IStream stream)
        {
            base.SaveToStream(stream);

        }

        public override void LoadFromStream(IStream stream)
        {
            base.LoadFromStream(stream);


        }

    }
}
