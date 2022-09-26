using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class RockerPitch : TechnoScriptable
    {
        public RockerPitch(TechnoExt owner) : base(owner) { }

        public override void Awake()
        {
            if (!pTechno.Ref.IsVoxel())
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            Pointer<WeaponStruct> pWeapon = pTechno.Ref.GetWeapon(weaponIndex);
            if (!pWeapon.IsNull && !pWeapon.Ref.WeaponType.IsNull)
            {
                WeaponTypeData weaponTypeData = Ini.GetConfig<WeaponTypeData>(Ini.RulesDependency, pWeapon.Ref.WeaponType.Ref.Base.ID).Data;
                if (null != weaponTypeData && weaponTypeData.RockerPitch > 0)
                {
                    double halfPI = Math.PI / 2;
                    // 获取转角
                    double theta = 0;
                    if (pTechno.Ref.HasTurret())
                    {
                        double turretRad = pTechno.Ref.GetRealFacing().current().radians() - halfPI;
                        double bodyRad = pTechno.Ref.Facing.current().radians() - halfPI;
                        Matrix3DStruct matrix3D = new Matrix3DStruct(true);
                        matrix3D.RotateZ((float)turretRad);
                        matrix3D.RotateZ((float)-bodyRad);
                        theta = matrix3D.GetZRotation();
                    }
                    // 抬起的角度
                    double gamma = weaponTypeData.RockerPitch;
                    // 符号
                    int lrSide = 1;
                    int fbSide = 1;
                    if (theta < 0)
                    {
                        lrSide *= -1;
                    }
                    if (theta >= halfPI || theta <= -halfPI)
                    {
                        fbSide *= -1;
                    }
                    // 抬起的角度
                    double pitch = gamma;
                    double roll = 0.0;
                    if (theta != 0)
                    {
                        if (Math.Sin(halfPI - theta) == 0)
                        {
                            pitch = 0.0;
                            roll = gamma * lrSide;
                        }
                        else
                        {
                            // 以底盘朝向为y轴做相对三维坐标系
                            // 在三维坐标系中对于地面γ度，对x轴π/2-θ做一个长度为1线段 L
                            // 这条线段在地面投影的长度为
                            double l = Math.Cos(gamma);
                            // L在y轴上的投影长度为
                            double y = l / Math.Sin(halfPI - theta);
                            // L在x轴上的投影长度为
                            // double x = l / Math.Cos(halfPI - Math.Abs(theta));
                            // L在z轴上的投影长度为
                            double z = Math.Sin(gamma);
                            // L在yz面上的投影长度为
                            double lyz = Math.Sqrt(Math.Pow(y, 2) + Math.Pow(z, 2));
                            // L在xz面上的投影长度为
                            // double lxz = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(z, 2));

                            pitch = Math.Acos(Math.Abs(y) / lyz) * fbSide;
                            // roll = Math.Acos(x / lxz) * lrSide;
                            roll = (gamma - Math.Abs(pitch)) * lrSide;
                        }
                    }
                    pTechno.Ref.RockingForwardsPerFrame = -(float)pitch;
                    pTechno.Ref.RockingSidewaysPerFrame = (float)roll;
                }
            }
        }
    }
}
