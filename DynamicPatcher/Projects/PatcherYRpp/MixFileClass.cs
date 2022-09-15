using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MixHeaderData
    {
        public uint ID;
        public uint Offset;
        public uint Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MixFileClass
	{
        public struct GenericMixFiles
        {
            public Pointer<MixFileClass> RA2MD;
            public Pointer<MixFileClass> RA2;
            public Pointer<MixFileClass> LANGUAGE;
            public Pointer<MixFileClass> LANGMD;
            public Pointer<MixFileClass> THEATER_TEMPERAT;
            public Pointer<MixFileClass> THEATER_TEMPERATMD;
            public Pointer<MixFileClass> THEATER_TEM;
            public Pointer<MixFileClass> GENERIC;
            public Pointer<MixFileClass> GENERMD;
            public Pointer<MixFileClass> THEATER_ISOTEMP;
            public Pointer<MixFileClass> THEATER_ISOTEM;
            public Pointer<MixFileClass> ISOGEN;
            public Pointer<MixFileClass> ISOGENMD;
            public Pointer<MixFileClass> MOVIES02D;
            public Pointer<MixFileClass> UNKNOWN_1;
            public Pointer<MixFileClass> MAIN;
            public Pointer<MixFileClass> CONQMD;
            public Pointer<MixFileClass> CONQUER;
            public Pointer<MixFileClass> CAMEOMD;
            public Pointer<MixFileClass> CAMEO;
            public Pointer<MixFileClass> CACHEMD;
            public Pointer<MixFileClass> CACHE;
            public Pointer<MixFileClass> LOCALMD;
            public Pointer<MixFileClass> LOCAL;
            public Pointer<MixFileClass> NTRLMD;
            public Pointer<MixFileClass> NEUTRAL;
            public Pointer<MixFileClass> MAPSMD02D;
            public Pointer<MixFileClass> MAPS02D;
            public Pointer<MixFileClass> UNKNOWN_2;
            public Pointer<MixFileClass> UNKNOWN_3;
            public Pointer<MixFileClass> SIDEC02DMD;
            public Pointer<MixFileClass> SIDEC02D;
        };

        public static ref GenericList MIXes => ref new Pointer<GenericList>(0xABEFD8).Ref;
        public static ref DynamicVectorClass<Pointer<MixFileClass>> Array => ref new Pointer<DynamicVectorClass<Pointer<MixFileClass>>>(0x884D90).Ref;
        public static ref DynamicVectorClass<Pointer<MixFileClass>> Array_Alt => ref new Pointer<DynamicVectorClass<Pointer<MixFileClass>>>(0x884DC0).Ref;
        public static ref DynamicVectorClass<Pointer<MixFileClass>> Maps => ref new Pointer<DynamicVectorClass<Pointer<MixFileClass>>>(0x884DA8).Ref;
        public static ref DynamicVectorClass<Pointer<MixFileClass>> Movies => ref new Pointer<DynamicVectorClass<Pointer<MixFileClass>>>(0x884DE0).Ref;
        public static ref MixFileClass MULTIMD => ref new Pointer<MixFileClass>(0x884DD8).Ref;
        public static ref MixFileClass MULTI => ref new Pointer<MixFileClass>(0x884DDC).Ref;

        public static ref GenericMixFiles Generics => ref new Pointer<GenericMixFiles>(0x884DF8).Ref;

        public static unsafe void Bootstrap()
        {
            var func = (delegate* unmanaged[Thiscall]<void>)0x5301A0;
            func();
        }

        public static unsafe void Constructor(Pointer<MixFileClass> pThis, string fileName)
        {
            var func = (delegate* unmanaged[Thiscall]<ref MixFileClass, IntPtr, IntPtr, void>)0x5B3C20;
            func(ref pThis.Ref, new AnsiString(fileName), new IntPtr(0x886980));
        }
        public static unsafe void Destructor(Pointer<MixFileClass> pThis)
        {
            var func = (delegate* unmanaged[Thiscall]<ref MixFileClass, void>)Helpers.GetVirtualFunctionPointer(pThis, 0);
            func(ref pThis.Ref);
        }

        public AnsiStringPointer FileName;
        public Bool Blowfish;
        public Bool Encryption;
        public int CountFiles;
        public int FileSize;
        public int FileStartOffset;
        public Pointer<MixHeaderData> Headers;
        public int field_24;
    }
}
