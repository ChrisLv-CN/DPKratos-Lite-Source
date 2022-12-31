using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Components;
using PatcherYRpp;

namespace Extension.Ext
{
#if !USE_ANIM_EXT
    [Obsolete("AnimExt is disable because performance consideration. Define symbol 'USE_ANIM_EXT' to use it.", true)]
#endif
    public partial class AnimExt
    {
        public Component Status;
    }
}
