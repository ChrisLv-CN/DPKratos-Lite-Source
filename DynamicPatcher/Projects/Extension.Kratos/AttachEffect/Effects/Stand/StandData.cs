using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{
    public class LayerParser : KEnumParser<Layer>
    {
        public override bool ParseInitials(string t, ref Layer buffer)
        {
            switch (t)
            {
                case "U":
                    buffer = Layer.Underground;
                    return true;
                case "S":
                    buffer = Layer.Surface;
                    return true;
                case "G":
                    buffer = Layer.Ground;
                    return true;
                case "A":
                    buffer = Layer.Air;
                    return true;
                case "T":
                    buffer = Layer.Top;
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public enum StandTargeting
    {
        BOTH = 0, LAND = 1, AIR = 2
    }

    public class StandTargetingParser : KEnumParser<StandTargeting>
    {
        public override bool ParseInitials(string t, ref StandTargeting buffer)
        {
            switch (t)
            {
                case "L":
                    buffer = StandTargeting.LAND;
                    return true;
                case "A":
                    buffer = StandTargeting.AIR;
                    return true;
                default:
                    buffer = StandTargeting.BOTH;
                    return true;
            }
        }
    }


    public partial class AttachEffectData
    {
        public StandData StandData;

        private void ReadStandData(ISectionReader reader)
        {
            StandData data = new StandData(reader);
            if (data.Enable)
            {
                this.StandData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class StandData : EffectData
    {
        static StandData()
        {
            new LayerParser().Register();
            new StandTargetingParser().Register();
        }

        public const string TITLE = "Stand.";

        public string Type; // ????????????
        public CoordStruct Offset; // ??????????????????
        // public Point2D OffsetRandomF; // ??????F
        // public Point2D OffsetRandomL; // ??????L
        // public Point2D OffsetRandomH; // ??????H
        public int Direction; // ???????????????16?????????[0-15]
        public bool LockDirection; // ???????????????????????????????????????
        public bool FreeDirection; // ?????????????????????
        public bool IsOnTurret; // ????????????????????????
        public bool IsOnWorld; // ????????????

        public Layer DrawLayer; // ????????????
        public int ZOffset; // ZAdjust?????????
        public bool SameTilter; // ????????????
        public bool SameMoving; // ??????????????????
        public bool StickOnFloor; // ?????????????????????????????????

        public bool SameHouse; // ??????????????????
        public bool SameTarget; // ?????????????????????
        public bool SameLoseTarget; // ????????????????????????????????????
        public bool SameAmmo; // ????????????????????????
        public bool UseMasterAmmo; // ?????????????????????
        public bool SamePassengers; // ????????????????????????

        public StandTargeting Targeting; // ????????????????????????
        public bool ForceAttackMaster; // ???????????????????????????
        public bool MobileFire; // ????????????

        public bool Immune; // ??????
        public double DamageFromMaster; // ??????JOJO?????????
        public double DamageToMaster; // ???????????????JOJO
        public bool AllowShareRepair; // ???????????????????????????

        public bool Explodes; // ???????????????
        public bool ExplodesWithMaster; // ?????????????????????????????????
        public bool ExplodesWithRocket; // ????????????????????????
        public bool RemoveAtSinking; // ???????????????

        public bool PromoteFromMaster; // ??????????????????
        public bool PromoteFromSpawnOwner; // ???????????????????????????
        public double ExperienceToMaster; // ???????????????
        public bool ExperienceToSpawnOwner; // ???????????????????????????

        public bool SelectToMaster; // ????????????????????????????????????

        public bool VirtualUnit; // ?????????

        public bool IsVirtualTurret; // ????????????

        public bool IsTrain; // ????????????
        public bool CabinHead; // ??????????????????
        public int CabinGroup; // ????????????

        public StandData()
        {
            this.Type = null;
            this.Offset = default;
            // this.OffsetRandomF = default;
            // this.OffsetRandomL = default;
            // this.OffsetRandomH = default;
            this.Direction = 0;
            this.LockDirection = false;
            this.FreeDirection = false;
            this.IsOnTurret = false;
            this.IsOnWorld = false;

            this.DrawLayer = Layer.None;
            this.ZOffset = 14;
            this.SameTilter = true;
            this.SameMoving = false;
            this.StickOnFloor = true;

            this.SameHouse = true;
            this.SameTarget = true;
            this.SameLoseTarget = false;
            this.SameAmmo = false;
            this.UseMasterAmmo = false;
            this.SamePassengers = false;

            this.Targeting = StandTargeting.BOTH;
            this.ForceAttackMaster = false;
            this.MobileFire = true;

            this.Immune = true;
            this.DamageFromMaster = 0.0;
            this.DamageToMaster = 0.0;
            this.AllowShareRepair = false;

            this.Explodes = false;
            this.ExplodesWithMaster = false;
            this.ExplodesWithRocket = true;
            this.RemoveAtSinking = false;

            this.PromoteFromMaster = false;
            this.PromoteFromSpawnOwner = false;
            this.ExperienceToMaster = 0.0;
            this.ExperienceToSpawnOwner = false;

            this.SelectToMaster = false;

            this.VirtualUnit = true;

            this.IsVirtualTurret = true;

            this.IsTrain = false;
            this.CabinHead = false;
            this.CabinGroup = -1;
        }

        public StandData(ISectionReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            this.Read(reader);
        }

        public void Read(ISectionReader reader)
        {
            this.Type = reader.Get(TITLE + "Type", this.Type);

            if (this.Enable = !Type.IsNullOrEmptyOrNone())
            {
                this.Offset = reader.Get(TITLE + "Offset", this.Offset);
                // this.OffsetRandomF = reader.Get(TITLE + "OffsetRandomF", this.OffsetRandomF);
                // this.OffsetRandomL = reader.Get(TITLE + "OffsetRandomL", this.OffsetRandomL);
                // this.OffsetRandomH = reader.Get(TITLE + "OffsetRandomH", this.OffsetRandomH);
                this.Direction = reader.GetDir16(TITLE + "Direction", this.Direction);
                this.LockDirection = reader.Get(TITLE + "LockDirection", this.LockDirection);
                this.FreeDirection = reader.Get(TITLE + "FreeDirection", this.FreeDirection);
                this.IsOnTurret = reader.Get(TITLE + "IsOnTurret", this.IsOnTurret);
                this.IsOnWorld = reader.Get(TITLE + "IsOnWorld", this.IsOnWorld);

                this.DrawLayer = reader.Get(TITLE + "DrawLayer", this.DrawLayer);
                this.ZOffset = reader.Get(TITLE + "ZOffset", this.ZOffset);
                this.SameTilter = reader.Get(TITLE + "SameTilter", this.SameTilter);
                this.SameMoving = reader.Get(TITLE + "SameMoving", this.SameMoving);
                this.StickOnFloor = reader.Get(TITLE + "StickOnFloor", this.StickOnFloor);

                this.SameHouse = reader.Get(TITLE + "SameHouse", this.SameHouse);
                this.SameTarget = reader.Get(TITLE + "SameTarget", this.SameTarget);
                this.SameLoseTarget = reader.Get(TITLE + "SameLoseTarget", this.SameLoseTarget);
                this.SameAmmo = reader.Get(TITLE + "SameAmmo", this.SameAmmo);
                if (SameAmmo)
                {
                    this.UseMasterAmmo = true;
                }
                this.UseMasterAmmo = reader.Get(TITLE + "UseMasterAmmo", this.UseMasterAmmo);
                this.SamePassengers = reader.Get(TITLE + "SamePassengers", this.SamePassengers);

                this.Targeting = reader.Get(TITLE + "Targeting", this.Targeting);
                this.ForceAttackMaster = reader.Get(TITLE + "ForceAttackMaster", this.ForceAttackMaster);
                this.MobileFire = reader.Get(TITLE + "MobileFire", this.MobileFire);

                this.Immune = reader.Get(TITLE + "Immune", this.Immune);
                this.DamageFromMaster = reader.GetPercent(TITLE + "DamageFromMaster", this.DamageFromMaster);
                this.DamageToMaster = reader.GetPercent(TITLE + "DamageToMaster", this.DamageToMaster);
                this.AllowShareRepair = reader.Get(TITLE + "AllowShareRepair", this.AllowShareRepair);

                this.Explodes = reader.Get(TITLE + "Explodes", this.Explodes);
                this.ExplodesWithMaster = reader.Get(TITLE + "ExplodesWithMaster", this.ExplodesWithMaster);
                this.ExplodesWithRocket = reader.Get(TITLE + "ExplodesWithRocket", this.ExplodesWithRocket);
                this.RemoveAtSinking = reader.Get(TITLE + "RemoveAtSinking", this.RemoveAtSinking);

                this.PromoteFromMaster = reader.Get(TITLE + "PromoteFromMaster", this.PromoteFromMaster);
                this.PromoteFromSpawnOwner = reader.Get(TITLE + "PromoteFromSpawnOwner", this.PromoteFromSpawnOwner);
                this.ExperienceToMaster = reader.GetPercent(TITLE + "ExperienceToMaster", this.ExperienceToMaster);
                this.ExperienceToSpawnOwner = reader.Get(TITLE + "ExperienceToSpawnOwner", this.ExperienceToSpawnOwner);

                this.SelectToMaster = reader.Get(TITLE + "SelectToMaster", this.SelectToMaster);

                this.VirtualUnit = reader.Get(TITLE + "VirtualUnit", this.VirtualUnit);

                this.IsVirtualTurret = reader.Get(TITLE + "IsVirtualTurret", this.IsVirtualTurret);

                this.IsTrain = reader.Get(TITLE + "IsTrain", this.IsTrain);
                this.CabinHead = reader.Get(TITLE + "CabinHead", this.CabinHead);
                this.CabinGroup = reader.Get(TITLE + "CabinGroup", this.CabinGroup);
            }
        }

        // public CoordStruct GetOffset()
        // {
        //     CoordStruct offset = this.Offset;
        //     if (default != OffsetRandomF)
        //     {

        //     }
        //     return offset;
        // }

    }


}
