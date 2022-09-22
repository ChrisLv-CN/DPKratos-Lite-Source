using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Components
{
    [Serializable]
    public abstract class Transform : Component
    {
        public Transform() : base()
        {
        }

        public abstract Vector3 Location { get; set; }
        public abstract Quaternion Rotation { get; set; }
        public abstract Vector3 Scale { get; set; }

        public Transform GetParent()
        {
            return Parent.Parent.Transform;
        }

        public new Transform GetRoot()
        {
            return base.GetRoot().Transform;
        }
    }
}

