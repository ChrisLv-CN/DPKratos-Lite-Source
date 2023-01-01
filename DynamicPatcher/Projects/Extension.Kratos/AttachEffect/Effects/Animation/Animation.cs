using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class AttachEffect
    {
        public Animation Animation;

        private void InitAnimation()
        {
            this.Animation = AEData.AnimationData.CreateEffect<Animation>();
            RegisterEffect(Animation);
        }
    }


    [Serializable]
    public class Animation : Effect<AnimationData>
    {
        private SwizzleablePointer<AnimClass> pAnim;
        private BlitterFlags animFlags;

        private bool OnwerIsDead;
        private bool OnwerIsCloakable;

        public Animation() : base()
        {
            this.pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
            this.OnwerIsDead = false;
        }

        public override void OnEnable()
        {
            // 激活动画
            // Logger.Log("效果激活，播放激活动画{0}", Data.ActiveAnim);
            if (!Data.ActiveAnim.IsNullOrEmptyOrNone())
            {
                Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.ActiveAnim);
                if (!pAnimType.IsNull)
                {
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, pOwner.Ref.Base.GetCoords());
                    pAnim.Ref.SetOwnerObject(pOwner);
                    pAnim.SetAnimOwner(pOwner);
                }
            }
            // 持续动画
            CreateIdleAnim();
        }

        private void CreateIdleAnim()
        {
            if (!pAnim.IsNull)
            {
                // Logger.Log($"{Game.CurrentFrame} AE[{AEData.Name}]持续动画[{Data.IdleAnim}]已存在，清除再重新创建");
                KillIdleAnim();
            }
            if (string.IsNullOrEmpty(Data.IdleAnim) || pOwner.IsInvisible() || (Data.RemoveInCloak && OnwerIsCloakable))
            {
                return;
            }
            // 创建动画
            // Logger.Log($"{Game.CurrentFrame} AE[{AEData.Name}]创建持续动画[{Data.IdleAnim}]");
            Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.IdleAnim);
            if (!pAnimType.IsNull)
            {
                Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, pOwner.Ref.Base.GetCoords());
                // Logger.Log($"{Game.CurrentFrame} AE[{AEData.Name}]成功创建持续动画[{Data.IdleAnim}], 指针 {pAnim}");
                // pAnim.Ref.SetOwnerObject(pObject); // 当单位隐形时，附着在单位上的动画会被游戏注销
                // Logger.Log(" - 将动画{0}赋予对象", Data.IdleAnim);
                pAnim.Ref.Loops = 0xFF;
                // Logger.Log(" - 设置动画{0}的剩余迭代次数为{1}", Data.IdleAnim, 0xFF);
                pAnim.SetAnimOwner(pOwner);
                pAnim.Show(Data.Visibility);
                this.pAnim.Pointer = pAnim;
                this.animFlags = pAnim.Ref.AnimFlags;
                // Logger.Log(" - 缓存动画{0}的实例对象指针", Data.IdleAnim);
            }
        }

        private void KillIdleAnim()
        {
            if (!pAnim.IsNull)
            {
                // 不将动画附着于单位上，动画就不会自行注销，需要手动注销
                pAnim.Ref.TimeToDie = true;
                pAnim.Ref.Base.UnInit(); // 包含了SetOwnerObject(0) 0x4255B0
                // Logger.Log("{0} - 已销毁动画{1}实例", Game.CurrentFrame, Data.IdleAnim);
                pAnim.Pointer = IntPtr.Zero;
                // Logger.Log("{0} - 成功移除持续动画{1}", Game.CurrentFrame, Data.IdleAnim);
            }
        }

        public override void OnDisable(CoordStruct location)
        {
            // Logger.Log($"{Game.CurrentFrame} AE[{AEData.Name}]效果结束，移除持续动画[{Data.IdleAnim}]");
            KillIdleAnim();
            // Logger.Log($"{Game.CurrentFrame} AE[{AEData.Name}]效果结束，播放结束动画[{Data.DoneAnim}]");
            // 结束动画
            if (!string.IsNullOrEmpty(Data.DoneAnim))
            {
                Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.DoneAnim);
                if (!pAnimType.IsNull)
                {
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, location);
                    // pAnim.Ref.SetOwnerObject(pObject);
                }
            }
        }

        public override void OnPut(Pointer<CoordStruct> pCoord, DirType dirType)
        {
            // Logger.Log("单位显现，创建持续动画{0}", Data.IdleAnim);
            CreateIdleAnim();
        }

        public override void OnGScreenRender(CoordStruct location)
        {
            // Logger.Log($"{Game.CurrentFrame} AE[{AEData.Name}]持续动画 {pAnim.Pointer}[{Data.IdleAnim}], 透明度 {pAnim.Ref.UnderTemporal}, 类型透明度 {pAnim.Ref.Data.Ref.Translucency}");
            UpdateLocation(location);
        }

        public override void OnUpdate(CoordStruct location, bool isDead)
        {
            this.OnwerIsDead = isDead;
            if (!isDead && pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                // Logger.Log($"{Game.CurrentFrame} AE[{AEData.Name}]附着单位 {pOwner} [{pOwner.Ref.Data.Ref.Base.ID}] 隐形 = {pTechno.Ref.Cloakable}，隐形状态 {pTechno.Ref.CloakStates}");
                switch (pTechno.Ref.CloakStates)
                {
                    case CloakStates.UnCloaked:
                        // 显形状态
                        if (OnwerIsCloakable)
                        {
                            OnwerIsCloakable = false;
                            if (Data.RemoveInCloak)
                            {
                                CreateIdleAnim();
                            }
                            else if (Data.TranslucentInCloak)
                            {
                                // 恢复不透明
                                pAnim.Ref.AnimFlags = animFlags;
                            }
                        }
                        break;
                    default:
                        // 进入隐形或处于正在隐\显形
                        if (!OnwerIsCloakable)
                        {
                            OnwerIsCloakable = true;
                            if (Data.RemoveInCloak)
                            {
                                KillIdleAnim();
                            }
                            else if (Data.TranslucentInCloak)
                            {
                                // 半透明
                                pAnim.Ref.AnimFlags |= BlitterFlags.TransLucent50;
                            }
                        }
                        break;
                }
            }
        }

        private void UpdateLocation(CoordStruct location)
        {
            if (!pAnim.IsNull)
            {
                // 没有附着在单位上，需要手动同步动画的位置
                pAnim.Ref.Base.SetLocation(location);
            }
        }

        public override void OnRemove()
        {
            // Logger.Log("{0} 单位{1}隐藏，移除持续动画{2}", Game.CurrentFrame, pObject, Data.IdleAnim);
            KillIdleAnim();
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            // Logger.Log("播放受击动画{0}", Data.HitAnim);
            // 受击动画
            if (!string.IsNullOrEmpty(Data.HitAnim))
            {
                Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.HitAnim);
                if (!pAnimType.IsNull)
                {
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, pOwner.Ref.Base.GetCoords());
                    pAnim.Ref.SetOwnerObject(pOwner);
                    if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno) && !pTechno.Ref.Owner.IsNull)
                    {
                        pAnim.Ref.Owner = pTechno.Ref.Owner;
                    }
                }
            }
        }

        public override void OnReceiveDamageDestroy()
        {
            // 单位被杀死时，附着动画会自动remove，0x4A9770
            // Logger.Log("{0} 单位{1}被炸死，不移除持续动画{2}", Game.CurrentFrame, pOwner, Data.IdleAnim);
            // this.Disable(pOwner.Ref.Location);
            this.OnwerIsDead = true;
        }

    }
}
