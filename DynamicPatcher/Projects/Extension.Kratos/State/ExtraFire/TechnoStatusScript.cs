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

    public partial class TechnoStatusScript
    {
        public State<ExtraFireData> ExtraFireState = new State<ExtraFireData>();

        private Dictionary<string, TimerStruct> extraFireROF = new Dictionary<string, TimerStruct>();

        public void InitState_ExtraFire()
        {
            // 初始化状态机
            ExtraFireData data = Ini.GetConfig<ExtraFireData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                ExtraFireState.Enable(data);
            }
        }

        public void OnFire_ExtraFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (ExtraFireState.IsActive() && null != ExtraFireState.Data)
            {
                ExtraFireData fireData = ExtraFireState.Data;
                ExtraFireFLHData flhData = null;
                // in Transport
                Pointer<TechnoClass> pTransporter = pTechno.Ref.Transporter;
                if (!pTransporter.IsNull)
                {
                    flhData = pTransporter.GetImageConfig<ExtraFireFLHData>();
                }
                else
                {
                    flhData = pTechno.GetImageConfig<ExtraFireFLHData>();
                }

                ExtraFireEntity data = fireData.Data;
                ExtraFireFLH flh = null != flhData ? flhData.Data : null;

                if (pTechno.Ref.Veterancy.IsElite())
                {
                    data = fireData.EliteData;
                    flh = null != flhData ? flhData.EliteData : null;
                }

                string[] weapons = null;
                CoordStruct customFLH = default;
                // 检查WeaponX
                if (pTechno.Ref.Type.Ref.WeaponCount > 0)
                {
                    if (null != data.WeaponX)
                    {
                        data.WeaponX.TryGetValue(weaponIndex, out weapons);
                    }
                    if (null != flh && null != flh.WeaponXFLH)
                    {
                        flh.WeaponXFLH.TryGetValue(weaponIndex, out customFLH);
                    }

                }
                else if (weaponIndex == 0)
                {
                    weapons = data.Primary;
                    if (null != flh)
                    {
                        customFLH = flh.PrimaryFLH;
                    }
                }
                else if (weaponIndex == 1)
                {
                    weapons = data.Secondary;
                    if (null != flh)
                    {
                        customFLH = flh.SecondaryFLH;
                    }
                }

                // 发射武器
                if (null != weapons)
                {
                    double rofMult = pTechno.GetROFMult(); // ExHelper.GetROFMult(OwnerObject);
                    // 循环武器清单并发射
                    foreach (string weaponId in weapons)
                    {
                        // 进行ROF检查
                        Pointer<WeaponTypeClass> pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponId);
                        if (!pWeapon.IsNull)
                        {
                            WeaponTypeData weaponTypeData = Ini.GetConfig<WeaponTypeData>(Ini.RulesDependency, weaponId).Data;

                            bool canFire = !weaponTypeData.UseROF;
                            if (!canFire)
                            {
                                // 进行ROF检查
                                // 本次发射的rof
                                int rof = (int)(pWeapon.Ref.ROF * rofMult);
                                if (extraFireROF.TryGetValue(weaponId, out TimerStruct rofTimer))
                                {
                                    if (rofTimer.Expired())
                                    {
                                        canFire = true;
                                        rofTimer.Start(rof);
                                        extraFireROF[weaponId] = rofTimer;
                                    }
                                }
                                else
                                {
                                    canFire = true;
                                    extraFireROF.Add(weaponId, new TimerStruct(rof));
                                }
                            }
                            if (canFire)
                            {
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 发射额外武器 {weaponId}");
                                // 检查搭乘状态
                                if (!pTransporter.IsNull ? pWeapon.Ref.FireInTransport : !weaponTypeData.OnlyFireInTransport)
                                {
                                    if (weaponTypeData.UseAlternateFLH && !pTransporter.IsNull)
                                    {
                                        // 获取载具的FLH
                                        // 获取开或者位于载具的序号，再获取载具的开火坐标，先进后出，序号随着乘客数量增长
                                        int index = pTransporter.Ref.Passengers.IndexOf(pTechno.Convert<FootClass>());
                                        // 有效序号1 - 5, 对应FLH 6 - 10
                                        if (index < 6)
                                        {
                                            customFLH = pTransporter.Ref.Type.Ref.TurretWeaponFLH[index + 5];
                                        }
                                    }
                                    Pointer<TechnoClass> pShooter = pTechno.WhoIsShooter();
                                    AttachFireScript attachFire = pShooter.FindOrAllocate<AttachFireScript>();
                                    if (null != attachFire)
                                    {
                                        if (weaponTypeData.Feedback)
                                        {
                                            // 调转枪口干自己
                                            attachFire.FireCustomWeapon(IntPtr.Zero, pTechno.Convert<AbstractClass>(), pTechno.Ref.Owner, weaponId, customFLH, rofMult);
                                        }
                                        else
                                        {
                                            attachFire.FireCustomWeapon(pTechno, pTarget, pTechno.Ref.Owner, weaponId, customFLH, rofMult);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }

    }
}
