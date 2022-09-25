using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{


    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class DestroyAnimsScript : TechnoScriptable
    {
        public DestroyAnimsScript(TechnoExt owner) : base(owner) { }

        private DestroyAnimsData data => Ini.GetConfig<DestroyAnimsData>(Ini.RulesDependency, section).Data;

        public override void Awake()
        {
            
        }

        public unsafe bool PlayDestroyAnims()
        {
            // Logger.Log("Techno IsAlive={0} IsActive={1}, IsOnMap={2}", pTechno.Ref.Base.IsAlive, pTechno.Ref.Base.IsActive(), pTechno.Ref.Base.IsOnMap);
            string[] destroyAnims = data.DestroyAnims;
            if (null != destroyAnims && destroyAnims.Length > 0)
            {
                int facing = destroyAnims.Length;
                int index = 0;
                if (!data.DestroyAnimsRandom && facing % 8 == 0)
                {
                    // uint bits = (uint)Math.Round(Math.Sqrt(facing), MidpointRounding.AwayFromZero);
                    // double face = pTechno.Ref.GetRealFacing().target().GetValue(bits);
                    // double x = (face / (1 << (int)bits)) * facing;
                    // index = (int)Math.Round(x, MidpointRounding.AwayFromZero);
                    // Logger.Log("Index={0}/{1}, x={2}, bits={3}, face={4}, ", index, facing, x, bits, face);
                    index = ExHelper.Dir2FacingIndex(pTechno.Ref.Facing.current(), facing);
                    index = (int)(facing / 8) + index;
                    if (index >= facing)
                    {
                        index = 0;
                    }
                }
                else
                {
                    index = MathEx.Random.Next(0, destroyAnims.Length - 1);
                    // Logger.Log("随机选择摧毁动画{0}/{1}", index, facing);
                }
                string animID = destroyAnims[index];
                // Logger.Log("选择摧毁动画{0}/{1}[{2}], HouseClass={3}", index, facing, animID, pTechno.Ref.Owner.IsNull ? "Null" : pTechno.Ref.Owner.Ref.Type.Ref.Base.ID);
                Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(animID);
                if (!pAnimType.IsNull)
                {
                    // Logger.Log("AnimType AltPalette={0}, MakeInf={1}, Next={2}", pAnimType.Ref.AltPalette, pAnimType.Ref.MakeInfantry, !pAnimType.Ref.Next.IsNull);
                    pAnimType.Ref.AltPalette = true;
                    CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, location);
                    // Logger.Log("Anim Owner={0}, IsPlaying={1}, PaletteName={2}, TintColor={3}, HouseColorIndex={4}", !pAnim.Ref.Owner.IsNull, pAnim.Ref.IsPlaying, pAnim.Ref.PaletteName, pAnim.Ref.TintColor, pHouse.Ref.ColorSchemeIndex);
                    pAnim.Ref.Owner = pTechno.Ref.Owner;
                    return true;
                }
            }
            return false;
        }

    }
}