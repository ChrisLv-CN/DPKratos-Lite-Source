using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using System.Threading.Tasks;
using Extension.INI;

namespace Scripts
{

    [Serializable]
    public class DamageTextCache
    {
        public int StartFrame;
        public int Value;

        public DamageTextCache(int value)
        {
            this.StartFrame = Game.CurrentFrame;
            this.Value = value;
        }

        public void Add(int value)
        {
            this.Value += value;
        }
    }

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    public class DamageText : TechnoScriptable
    {

        public DamageText(TechnoExt owner) : base(owner) { }

        public bool SkipDamageText = false;
        Dictionary<DamageTextData, DamageTextCache> DamageCache = new Dictionary<DamageTextData, DamageTextCache>();
        Dictionary<DamageTextData, DamageTextCache> RepairCache = new Dictionary<DamageTextData, DamageTextCache>();

        public override void Awake()
        {
            
        }

        public override void OnUpdate()
        {
            CoordStruct location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            int frame = Game.CurrentFrame;
            for (int i = DamageCache.Count() - 1; i >= 0; i--)
            {
                var d = DamageCache.ElementAt(i);
                if (frame - d.Value.StartFrame >= d.Key.Rate)
                {
                    string text = "-" + d.Value.Value;
                    OrderDamageText(text, location, d.Key);
                    DamageCache.Remove(d.Key);
                }
            }
            for (int j = RepairCache.Count() - 1; j >= 0; j--)
            {
                var r = RepairCache.ElementAt(j);
                if (frame - r.Value.StartFrame >= r.Key.Rate)
                {
                    string text = "+" + r.Value.Value;
                    OrderDamageText(text, location, r.Key);
                    RepairCache.Remove(r.Key);
                }
            }
        }

        public override void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState)
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            WarheadTypeExt whExt = WarheadTypeExt.ExtMap.Find(pWH);
            if (SkipDrawDamageText(whExt))
            {
                return;
            }
            else
            {
                SkipDamageText = false;
            }
            string text = null;
            DamageTextData data = null;
            int damage = pRealDamage.Data;
            int damageValue = 0;
            int repairValue = 0;
            if (damage > 0)
            {
                data = whExt.DamageTextTypeData.Damage;
                if (!data.Hidden)
                {
                    damageValue += damage;
                    text = "-" + damage;
                }
            }
            else if (damage < 0)
            {
                data = whExt.DamageTextTypeData.Repair;
                if (!data.Hidden)
                {
                    repairValue += -damage;
                    text = "+" + -damage;
                }
            }
            if (null == data || data.Hidden)
            {
                return;
            }
            if (!string.IsNullOrEmpty(text))
            {
                if (data.Detail || damageState == DamageState.NowDead)
                {
                    // 直接下单
                    CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                    OrderDamageText(text, location, data);
                }
                else
                {
                    // 写入缓存
                    if (damageValue > 0)
                    {
                        if (DamageCache.ContainsKey(data))
                        {
                            DamageCache[data].Add(damageValue);
                        }
                        else
                        {
                            DamageCache.Add(data, new DamageTextCache(damageValue));
                        }
                    }
                    else if (repairValue > 0)
                    {
                        if (RepairCache.ContainsKey(data))
                        {
                            RepairCache[data].Add(repairValue);
                        }
                        else
                        {
                            RepairCache.Add(data, new DamageTextCache(repairValue));
                        }
                    }
                }
            }
        }

        private bool SkipDrawDamageText(WarheadTypeExt whExt)
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            return SkipDamageText || pTechno.IsInvisible() || pTechno.IsCloaked() || null == whExt || null == whExt.DamageTextTypeData || whExt.DamageTextTypeData.Hidden;
        }

        private void OrderDamageText(string text, CoordStruct location, DamageTextData data)
        {
            int x = MathEx.Random.Next(data.XOffset.X, data.XOffset.Y);
            int y = MathEx.Random.Next(data.YOffset.X, data.YOffset.Y) - 15; // 离地高度
            Point2D offset = new Point2D(x, y);
            // 横向锚点修正
            int length = text.Length / 2;
            if (data.UseSHP)
            {
                offset.X -= data.ImageSize.X * length;
            }
            else
            {
                offset.X -= PrintTextManager.FontSize.X * length;
            }
            PrintTextManager.RollingText(text, location, offset, data.RollSpeed, data.Duration, data);
        }

    }
}