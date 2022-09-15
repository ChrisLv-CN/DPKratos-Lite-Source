using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatcherYRpp;

namespace Extension.Ext
{
    [Serializable]
    public abstract class InstanceExtension<TExt, TBase> : Extension<TBase> where TExt : Extension<TBase>
    {
        public static Container<TExt, TBase> ExtMap = new MapContainer<TExt, TBase>(typeof(TBase).Name);

        protected InstanceExtension(Pointer<TBase> OwnerObject) : base(OwnerObject)
        {

        }

        public ref TBase OwnerRef => ref OwnerObject.Ref;

    }
}
