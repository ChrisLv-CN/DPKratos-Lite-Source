using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct MPGameModeClass
    {
        private static IntPtr instance = new IntPtr(0xA8B23C);
        static public ref MPGameModeClass Instance { get => ref instance.Convert<Pointer<MPGameModeClass>>().Ref.Ref; }

        [FieldOffset(8)] public DynamicVectorClass<Pointer<MPTeam>> MPTeams;
        [FieldOffset(32)] public UniStringPointer CSFTitle;
        [FieldOffset(36)] public UniStringPointer CSFTooltip;
        [FieldOffset(40)] public int MPModeIndex;
        [FieldOffset(44)] public AnsiStringPointer INIFilename;
        [FieldOffset(48)] public AnsiStringPointer MapFilter;
        [FieldOffset(52)] public Bool AIAllowed;
        [FieldOffset(56)] public Pointer<CCINIClass> INI;
        [FieldOffset(60)] public Bool AlliesAllowed;
        [FieldOffset(61)] public Bool wolTourney;
        [FieldOffset(62)] public Bool wolClanTourney;
        [FieldOffset(63)] public Bool MustAlly;
    }
}
