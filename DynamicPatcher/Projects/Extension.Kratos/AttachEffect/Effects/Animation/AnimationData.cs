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

        private void ReadAnimationData(ISectionReader reader)
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
    public class AnimationData : EffectData
    {
        static AnimationData()
        {
            new RelationParser().Register();
        }

        public string IdleAnim; // 持续动画
        public string ActiveAnim; // 激活时播放的动画
        public string HitAnim; // 被击中时播放的动画
        public string DoneAnim; // 结束时播放的动画

        public bool RemoveInCloak; // 隐形时移除
        public bool TranslucentInCloak; // 隐形时调整透明度为50
        public Relation Visibility; // 谁能看见持续动画

        public AnimationData()
        {
            this.IdleAnim = null;
            this.ActiveAnim = null;
            this.HitAnim = null;
            this.DoneAnim = null;

            this.RemoveInCloak = true;
            this.TranslucentInCloak = false;
            this.Visibility = Relation.All;
        }

        public AnimationData(ISectionReader reader) : this()
        {
            Read(reader);
        }

        public void Read(ISectionReader reader)
        {
            this.IdleAnim = reader.Get("Animation", this.IdleAnim);
            this.ActiveAnim = reader.Get("ActiveAnim", this.ActiveAnim);
            this.HitAnim = reader.Get("HitAnim", this.HitAnim);
            this.DoneAnim = reader.Get("DoneAnim", this.DoneAnim);

            this.Enable = !IdleAnim.IsNullOrEmptyOrNone() || !ActiveAnim.IsNullOrEmptyOrNone() || !HitAnim.IsNullOrEmptyOrNone() || !DoneAnim.IsNullOrEmptyOrNone();

            this.RemoveInCloak = reader.Get("Anim.RemoveInCloak", this.RemoveInCloak);
            this.TranslucentInCloak = reader.Get("Anim.TranslucentInCloak", this.TranslucentInCloak);

            this.Visibility = reader.Get("Anim.Visibility", this.Visibility);
        }

    }


}
