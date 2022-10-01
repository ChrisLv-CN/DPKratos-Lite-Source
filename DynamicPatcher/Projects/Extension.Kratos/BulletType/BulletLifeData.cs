using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class BulletLifeData
    {
        public bool Interceptable; // 可被伤害
        public int Strength; // 自定义血量

        public int Health; // 血量
        public bool IsDetonate; // 已损毁
        public bool IsHarmless; // 和平处置
        public bool SkipAE; // 爆炸不赋予AE


        public BulletLifeData(int health)
        {
            this.Interceptable = false;
            this.Strength = -1;

            this.Health = health;
            this.IsDetonate = false;
            this.IsHarmless = false;
            this.SkipAE = false;
        }

        public void Read(ISectionReader reader)
        {
            this.Interceptable = reader.Get("Interceptable", this.Interceptable);
            this.Strength = reader.Get("Strength", this.Strength);
            if (Strength > 0)
            {
                this.Health = this.Strength;
            }
        }

        /// <summary>
        /// 直接摧毁
        /// </summary>
        /// <param name="harmless"></param>
        /// <param name="skipAE"></param>
        public void Detonate(bool harmless, bool skipAE = false)
        {
            this.Health = -1;
            this.IsDetonate = true;
            this.IsHarmless = harmless;
            this.SkipAE = skipAE;
        }

        /// <summary>
        /// 减去伤害
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="harmless"></param>
        /// <param name="skipAE"></param>
        public void TakeDamage(int damage, bool harmless, bool skipAE = false)
        {
            this.Health -= damage;
            this.IsDetonate = this.Health <= 0;
            this.IsHarmless = harmless;
            if (IsDetonate)
            {
                this.SkipAE = skipAE;
            }
        }

        public override string ToString()
        {
            return $"{{\"Interceptable\":{Interceptable}, \"Health\":{Health}, \"IsDetonate\":{IsDetonate}, \"IsHarmless\":{IsHarmless}, \"SkipAE\":{SkipAE}}}";
        }
    }

}