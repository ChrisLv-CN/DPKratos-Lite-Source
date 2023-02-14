using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Utilities;
using System.Runtime.InteropServices.ComTypes;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateBefore(typeof(TechnoStatusScript))]
    public class SupportSpawnsScript : TechnoScriptable
    {

        public SupportSpawnsScript(TechnoExt owner) : base(owner) { }

        private IConfigWrapper<SupportSpawnsData> _data;
        private SupportSpawnsData data
        {
            get
            {
                if (null == _data)
                {
                    Pointer<TechnoClass> pSpawnOwner = pTechno.Ref.SpawnOwner;
                    string spawnOwnerSection = pSpawnOwner.Ref.Type.Ref.Base.Base.ID;
                    _data = Ini.GetConfig<SupportSpawnsData>(Ini.RulesDependency, spawnOwnerSection);
                }
                return _data.Data;
            }
        }

        private bool active = false;

        private Dictionary<string, TimerStruct> weaponsROF = new Dictionary<string, TimerStruct>();
        private int flipY = 1;

        public override void Awake()
        {
            // I'm not a Spawn
            if (!pTechno.CastIf<AircraftClass>(AbstractType.Aircraft, out var pAircraft)
               || !pAircraft.Ref.Type.Ref.Base.Spawned)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            // SpwanOwner在这里拿不到，在put里检查
        }

        public override void LoadFromStream(IStream stream)
        {
            base.LoadFromStream(stream);
            active = false;
        }

        public override void OnPut(Pointer<CoordStruct> pLocation, ref DirType dirType)
        {
            if (pTechno.Ref.SpawnOwner.IsNull || !data.Enable)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            active = true;
            EventSystem.Techno.AddTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 启动子机支援武器 [{string.Join(", ", data.Weapons)}], Always = {data.AlwaysFire}");
        }

        public override void OnRemove()
        {
            active = false;
            EventSystem.Techno.RemoveTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public void OnTransform(object sender, EventArgs args)
        {
            Pointer<TechnoClass> pTarget = ((TechnoTypeChangeEventArgs)args).pTechno;
            if (!pTarget.IsNull && pTarget == pTechno)
            {
                _data = null;
            }
        }

        public override void OnUpdate()
        {
            if (active && !pTechno.IsDeadOrInvisible())
            {
                Pointer<TechnoClass> pSpawnOwner = pTechno.Ref.SpawnOwner;
                if (!pSpawnOwner.IsDeadOrInvisible())
                {
                    if (data.Enable && data.AlwaysFire)
                    {
                        SupportSpawnsFLHData flhData = pSpawnOwner.GetImageConfig<SupportSpawnsFLHData>();
                        FireSupportWeaponToSpawn(pSpawnOwner, data, flhData, true);
                    }
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            Pointer<TechnoClass> pSpawnOwner = pTechno.Ref.SpawnOwner;
            if (active && !pSpawnOwner.IsDeadOrInvisible())
            {
                if (data.Enable && !data.AlwaysFire)
                {
                    SupportSpawnsFLHData flhData = pSpawnOwner.GetImageConfig<SupportSpawnsFLHData>();
                    FireSupportWeaponToSpawn(pSpawnOwner, data, flhData, false);
                }
            }
        }

        private void FireSupportWeaponToSpawn(Pointer<TechnoClass> pSpawnOwner, SupportSpawnsData data, SupportSpawnsFLHData flhData, bool checkROF = false)
        {
            string[] weapons = data.Weapons;
            CoordStruct flh = flhData.SupportWeaponFLH;
            CoordStruct hitFLH = flhData.SupportWeaponHitFLH;
            if (pSpawnOwner.Ref.Veterancy.IsElite())
            {
                weapons = data.EliteWeapons;
                flh = flhData.EliteSupportWeaponFLH;
                hitFLH = flhData.EliteSupportWeaponHitFLH;
            }
            if (null != weapons && weapons.Any())
            {
                if (data.SwitchFLH)
                {
                    flh.Y = flh.Y * flipY;
                    flipY *= -1;
                }
                Pointer<ObjectClass> pShooter = pSpawnOwner.Convert<ObjectClass>();
                Pointer<TechnoClass> pAttacker = pSpawnOwner;
                Pointer<HouseClass> pAttackingHouse = pSpawnOwner.Ref.Owner;
                Pointer<AbstractClass> pTarget = pTechno.Convert<AbstractClass>();
                foreach (string weaponId in weapons)
                {
                    if (!weaponId.IsNullOrEmptyOrNone())
                    {
                        Pointer<WeaponTypeClass> pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponId);
                        if (!pWeapon.IsNull)
                        {
                            WeaponTypeData weaponTypeData = pWeapon.GetData();
                            // 目标筛选
                            if (!weaponTypeData.CanFireToTarget(pShooter, pAttacker, pTarget, pAttackingHouse, pWeapon))
                            {
                                // Logger.Log($"{Game.CurrentFrame} 支援武器[{weaponId}]{pWeapon}不能攻击子机[{section}]{pTechno}, 换下一个");
                                continue;
                            }
                            // 检查ROF
                            if (checkROF)
                            {
                                if (weaponsROF.TryGetValue(weaponId, out TimerStruct rofTimer))
                                {
                                    if (rofTimer.InProgress())
                                    {
                                        // Logger.Log($"{Game.CurrentFrame} 支援武器[{weaponId}]{pWeapon}冷却中，不能攻击子机[{section}]{pTechno}, 换下一个");
                                        continue;
                                    }
                                }
                            }
                            // Logger.Log($"{Game.CurrentFrame} [{pShooter.Ref.Type.Ref.Base.ID}]{pShooter}发射支援武器[{weaponId}]{pWeapon}攻击子机[{section}]{pTechno}, checkROF = {checkROF}");
                            // get source location
                            CoordStruct sourcePos = FLHHelper.GetFLHAbsoluteCoords(pSpawnOwner, flh, true);
                            // Logger.Log("Support Weapon FLH = {0}, hitFLH = {1}", flh, hitFLH);
                            // get target location
                            CoordStruct targetPos = FLHHelper.GetFLHAbsoluteCoords(pTechno, hitFLH, true);
                            // get bullet velocity
                            BulletVelocity bulletVelocity = WeaponHelper.GetBulletVelocity(sourcePos, targetPos);
                            // fire weapon
                            WeaponHelper.FireBulletTo(pShooter, pAttacker, pTarget, pAttackingHouse, pWeapon, sourcePos, targetPos, bulletVelocity);
                            if (checkROF)
                            {
                                int rof = pWeapon.Ref.ROF;
                                if (weaponsROF.TryGetValue(weaponId, out TimerStruct rofTimer))
                                {
                                    rofTimer.Start(rof);
                                    weaponsROF[weaponId] = rofTimer;
                                }
                                else
                                {
                                    weaponsROF.Add(weaponId, new TimerStruct(rof));
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
