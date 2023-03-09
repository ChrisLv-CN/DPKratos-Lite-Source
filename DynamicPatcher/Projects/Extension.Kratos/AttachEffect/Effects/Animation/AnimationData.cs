using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class AttachEffectData
    {
        public AnimationData AnimationData;

        private void ReadAnimationData(IConfigReader reader)
        {
            AnimationData data = new AnimationData(reader);
            if (data.Enable)
            {
                this.AnimationData = data;
                this.Enable = true;
            }
        }
    }

    /// <summary>
    /// AE动画
    /// </summary>
    [Serializable]
    public class AnimationEntity
    {
        public string Type; // 动画类型
        public CoordStruct Offset; // 动画相对位置

        public CoordStruct StackOffset; // 堆叠偏移
        public int StackGroup; // 分组堆叠
        public CoordStruct StackGroupOffset; // 分组堆叠偏移

        public bool IsOnTurret; // 相对炮塔或者身体
        public bool IsOnWorld; // 相对世界

        public bool RemoveInCloak; // 隐形时移除
        public bool TranslucentInCloak; // 隐形时调整透明度为50
        public Relation Visibility; // 谁能看见持续动画

        public AnimationEntity()
        {
            this.Type = null;
            this.Offset = default;

            this.StackOffset = default;
            this.StackGroup = -1;
            this.StackGroupOffset = default;

            this.IsOnTurret = false;
            this.IsOnWorld = false;

            this.RemoveInCloak = true;
            this.TranslucentInCloak = false;
            this.Visibility = Relation.All;
        }

        public void Read(ISectionReader reader, string title, string anim)
        {
            this.Type = reader.Get(title + "Type", anim);
            this.Offset = reader.Get(title + "Offset", this.Offset);

            this.StackOffset = reader.Get(title + "StackOffset", this.StackOffset);
            this.StackGroup = reader.Get(title + "StackGroup", this.StackGroup);
            this.StackGroupOffset = reader.Get(title + "StackGroupOffset", this.StackGroupOffset);

            this.IsOnTurret = reader.Get(title + "IsOnTurret", this.IsOnTurret);
            this.IsOnWorld = reader.Get(title + "IsOnWorld", this.IsOnWorld);

            this.RemoveInCloak = reader.Get(title + "RemoveInCloak", this.RemoveInCloak);
            this.TranslucentInCloak = reader.Get(title + "TranslucentInCloak", this.TranslucentInCloak);
            this.Visibility = reader.Get(title + "Visibility", this.Visibility);
        }

    }


    [Serializable]
    public class AnimationData : EffectData
    {
        static AnimationData()
        {
            new RelationParser().Register();
        }

        public AnimationEntity IdleAnim; // 持续动画
        public AnimationEntity ActiveAnim; // 激活时播放的动画
        public AnimationEntity HitAnim; // 被击中时播放的动画
        public AnimationEntity DoneAnim; // 结束时播放的动画

        public AnimationData()
        {
            this.IdleAnim = null;
            this.ActiveAnim = null;
            this.HitAnim = null;
            this.DoneAnim = null;
        }

        public AnimationData(IConfigReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            string idle = reader.Get<string>("Animation", null);
            AnimationEntity idleAnim = new AnimationEntity();
            idleAnim.Read(reader, "Anim.", idle);
            idleAnim.Read(reader, "Anim.Idle.", idle);
            if (!idleAnim.Type.IsNullOrEmptyOrNone())
            {
                this.IdleAnim = idleAnim;
            }

            string active = reader.Get<string>("ActiveAnim", null);
            AnimationEntity activeAnim = new AnimationEntity();
            activeAnim.Read(reader, "Anim.Active.", active);
            if (!activeAnim.Type.IsNullOrEmptyOrNone())
            {
                this.ActiveAnim = activeAnim;
            }

            string hit = reader.Get<string>("HitAnim", null);
            AnimationEntity hitAnim = new AnimationEntity();
            hitAnim.Read(reader, "Anim.Hit.", active);
            if (!hitAnim.Type.IsNullOrEmptyOrNone())
            {
                this.HitAnim = hitAnim;
            }

            string done = reader.Get<string>("DoneAnim", null);
            AnimationEntity doneAnim = new AnimationEntity();
            doneAnim.Read(reader, "Anim.Done.", active);
            if (!doneAnim.Type.IsNullOrEmptyOrNone())
            {
                this.DoneAnim = doneAnim;
            }

            this.Enable = null != IdleAnim || null != ActiveAnim || null != HitAnim || null != DoneAnim;
        }

    }


}
