using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct MPTeam
    {

        [FieldOffset(4)] public UniStringPointer Title;
        [FieldOffset(8)] public int Index;
    }
}
