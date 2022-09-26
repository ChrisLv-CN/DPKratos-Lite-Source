using Extension.Ext;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Components
{
    [Serializable]
    internal class YRTransform : Transform
    {
        public YRTransform(IExtension extension)
        {
            m_Extension = extension;
        }

        
        public override Vector3 Location { get => Get<AbstractClass>().Ref.GetCoords().ToVector3(); set => Get<ObjectClass>().Ref.SetLocation(value.ToCoordStruct()); }
        public override Quaternion Rotation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Vector3 Scale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Pointer<T> Get<T>() => m_Extension.OwnerObject.Convert<T>();

        private IExtension m_Extension;
    }
}
