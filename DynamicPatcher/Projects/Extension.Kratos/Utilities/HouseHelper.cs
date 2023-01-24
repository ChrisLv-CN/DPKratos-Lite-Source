using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    [Flags]
    public enum Relation
    {
        NONE = 0x0, OWNER = 0x1, ALLIES = 0x2, ENEMIES = 0x4,

        Team = OWNER | ALLIES,
        NotAllies = OWNER | ENEMIES,
        NotOwner = ALLIES | ENEMIES,
        All = OWNER | ALLIES | ENEMIES
    }
    public class RelationParser : KEnumParser<Relation>
    {
        public override bool ParseInitials(string t, ref Relation buffer)
        {
            switch (t)
            {
                case "O":
                    buffer = Relation.OWNER;
                    return true;
                case "A":
                    buffer = Relation.Team;
                    return true;
                case "E":
                    buffer = Relation.ENEMIES;
                    return true;
            }
            return false;
        }
    }

    public static class HouseHelper
    {

        public static bool IsCivilian(this Pointer<HouseClass> pHouse)
        {
            return pHouse.IsNull || pHouse.Ref.Defeated || pHouse.Ref.Type.IsNull
                || pHouse.Ref.Type.Ref.MultiplayPassive;
            // || HouseClass.NEUTRAL == pHouse.Ref.Type.Ref.Base.ID // 自然也算平民吗？
            // || HouseClass.CIVILIAN == pHouse.Ref.Type.Ref.Base.ID
            // || HouseClass.SPECIAL == pHouse.Ref.Type.Ref.Base.ID; // 被狙掉驾驶员的阵营是Special
        }

        public static Relation GetRelationWithPlayer(this Pointer<HouseClass> pHouse)
        {
            return pHouse.GetRelation(HouseClass.Player);
        }

        public static Relation GetRelation(this Pointer<HouseClass> pHosue, Pointer<HouseClass> pTargetHouse)
        {
            if (pHosue == pTargetHouse)
            {
                return Relation.OWNER;
            }
            if (pHosue.Ref.IsAlliedWith(pTargetHouse))
            {
                return Relation.ALLIES;
            }
            return Relation.ENEMIES;
        }

        /// <summary>
        /// Ares 反击攻击友军的平民
        /// </summary>
        /// <param name="pHouse"></param>
        /// <returns></returns>
        public static bool AutoRepel(this Pointer<HouseClass> pHouse)
        {
            string key = "AutoRepel";
            if (pHouse.Ref.ControlledByHuman())
            {
                key = "PlayerAutoRepel";
            }
            return Ini.GetSection(Ini.RulesDependency, RulesClass.SectionCombatDamage).Get(key, false);
        }


        public static bool CanAffectHouse(this Pointer<HouseClass> pHouse, Pointer<HouseClass> pTargetHouse, bool owner, bool allied, bool enemies, bool civilian)
        {
            if (!pHouse.IsNull && !pTargetHouse.IsNull
                && ((pTargetHouse.IsCivilian() && !civilian)
                    || (pTargetHouse == pHouse ? !owner : (pTargetHouse.Ref.IsAlliedWith(pHouse) ? !allied : !enemies))
                ))
            {
                return false;
            }
            return true;
        }
    }

}