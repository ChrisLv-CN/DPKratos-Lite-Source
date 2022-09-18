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
    public class BulletDamageData
    {
        public int Damage; // 伤害
        public bool Eliminate; // 一击必杀
        public bool Harmless; // 和平处置

        public BulletDamageData(int damage)
        {
            this.Damage = damage;
            this.Eliminate = true;
            this.Harmless = false;
        }

        public override string ToString()
        {
            return $"{{\"Damage\":{Damage}, \"Eliminate\":{Eliminate}, \"Harmless\":{Harmless}}}";
        }

    }
}