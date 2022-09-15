using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 184)]
    public struct GameOptionsClass
    {
        private static IntPtr instance = new IntPtr(0xA8EB60);
        public static ref GameOptionsClass Instance => ref instance.Convert<GameOptionsClass>().Ref;

        public unsafe int GetAnimSpeed(int rate)
        {
            var func = (delegate* unmanaged[Thiscall]<ref GameOptionsClass, int>)0x5FB2E0;
            return func(ref this); 
        }

        public unsafe void LoadFromINI(int rate)
        {
            var func = (delegate* unmanaged[Thiscall]<ref GameOptionsClass, void>)0x5FA620;
            func(ref this);
        }

        [FieldOffset(0)] public int GameSpeed;
        [FieldOffset(4)] public int Difficulty;
        [FieldOffset(8)] public int CampDifficulty;
        [FieldOffset(12)] public int ScrollMethod;
        [FieldOffset(16)] public int ScrollRate;
        [FieldOffset(20)] public Bool AutoScroll;
        [FieldOffset(24)] public int DetailLevel;
        [FieldOffset(28)] public byte SidebarSide;
        [FieldOffset(29)] public Bool SidebarCameoText;
        [FieldOffset(30)] public Bool UnitActionLines;
        [FieldOffset(31)] public Bool ShowHidden;
        [FieldOffset(32)] public Bool Tooltips;
        [FieldOffset(36)] public int ScreenWidth;
        [FieldOffset(40)] public int ScreenHeight;
        [FieldOffset(44)] public int ShellWidth;
        [FieldOffset(48)] public int ShellHeight;
        [FieldOffset(52)] public Bool StretchMovies;
        [FieldOffset(53)] public Bool AllowHiResModes;
        [FieldOffset(54)] public Bool AllowVRAMSidebar;
        [FieldOffset(56)] public float SoundVolume;
        [FieldOffset(60)] public float VoiceVolume;
        [FieldOffset(64)] public float MovieVolume;
        [FieldOffset(68)] public Bool IsScoreRepeat;
        [FieldOffset(69)] public Bool InGameMusic;
        [FieldOffset(70)] public Bool IsScoreShuffle;
        [FieldOffset(72)] public short SoundLatency;
        [FieldOffset(74)] public short Socket;
        [FieldOffset(76)] public int LastUnlockedSovMovie;
        [FieldOffset(80)] public int LastUnlockedAllMovie;
        [FieldOffset(84)] public int NetCard;
        [FieldOffset(88)] public byte NetID_first;
        public AnsiStringPointer NetID => Pointer<byte>.AsPointer(ref NetID_first);
        [FieldOffset(120)] public int unknown_int_78;
        [FieldOffset(124)] public int unknown_int_7C;
        [FieldOffset(128)] public int unknown_int_80;
        [FieldOffset(132)] public int unknown_int_84;
        [FieldOffset(136)] public int unknown_int_88;
        [FieldOffset(140)] public int unknown_int_8C;
        [FieldOffset(144)] public int unknown_int_90;
        [FieldOffset(148)] public int unknown_int_94;
        // virtual key constants, each of them doubled
        // defaulting to VK_MENU, VK_CONTROL and VK_SHIFT
        [FieldOffset(152)] public int KeyForceMove1;
        [FieldOffset(156)] public int KeyForceMove2;
        [FieldOffset(160)] public int KeyForceFire1;
        [FieldOffset(164)] public int KeyForceFire2;
        [FieldOffset(168)] public int KeyForceSelect1;
        [FieldOffset(172)] public int KeyForceSelect2;
        [FieldOffset(176)] public int unknown_int_B0;
        [FieldOffset(180)] public int unknown_int_B4;
	}
}
