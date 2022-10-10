using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class GiftBoxState : State<GiftBoxData>
    {
        public bool IsOpen;

        // 记录盒子的状态
        public bool IsSelected;
        public DirStruct BodyDir;
        public int Group;

        private bool isElite;
        private int delay;
        private TimerStruct delayTimer;

        public void Update(bool isElite)
        {
            if (this.isElite != isElite && IsActive() && delayTimer.Expired())
            {
                Reset();
            }
            this.isElite = isElite;
        }

        public void SaveStatue(Pointer<TechnoClass> pTechno)
        {
            // 保存记录
            this.IsSelected = pTechno.Ref.Base.IsSelected;
            this.Group = pTechno.Ref.Group;
            // 记录朝向
            if (pTechno.CastIf(AbstractType.Aircraft, out Pointer<AircraftClass> pPlane))
            {
                // 飞机朝向是TurretFacing
                this.BodyDir = pTechno.Ref.GetRealFacing().current();
            }
            else if (pTechno.CastToFoot(out Pointer<FootClass> pFoot))
            {
                ILocomotion loco = pFoot.Ref.Locomotor;
                if (loco.ToLocomotionClass().Ref.GetClassID() == LocomotionClass.Jumpjet)
                {
                    // JJ朝向是单独的Facing
                    Pointer<JumpjetLocomotionClass> pLoco = loco.ToLocomotionClass<JumpjetLocomotionClass>();
                    this.BodyDir = pLoco.Ref.LocomotionFacing.current();
                }
                else
                {
                    this.BodyDir = pTechno.Ref.Facing.current();
                }
            }
            else
            {
                this.BodyDir = pTechno.Ref.Facing.current();
            }

        }

        private GiftBoxEntity GetGiftBoxData()
        {
            if (isElite && null != Data.EliteData)
            {
                return Data.EliteData;
            }
            return Data.Data;
        }

        public override void OnEnable()
        {
            Reset();
        }

        public void Reset()
        {
            GiftBoxEntity data = GetGiftBoxData();
            this.IsOpen = false;
            this.delay = data.RandomDelay.GetRandomValue(data.Delay);
            if (this.delay > 0)
            {
                delayTimer.Start(this.delay);
            }
        }

        public bool CanOpen()
        {
            return IsActive() && !IsOpen && Timeup() && null != GetGiftBoxData();
        }

        private bool Timeup()
        {
            return this.delay <= 0 || delayTimer.Expired();
        }

        public List<string> GetGiftList()
        {
            GiftBoxEntity data = GetGiftBoxData();
            List<string> gifts = new List<string>();
            if (null != data)
            {
                int giftCount = data.Gifts.Length;
                int numsCount = null != data.Nums ? data.Nums.Length : 0;
                if (data.RandomType)
                {
                    int times = 1;
                    if (numsCount > 0)
                    {
                        times = 0;
                        foreach (int num in data.Nums)
                        {
                            times += num;
                        }
                    }
                    // int weightCount = null != Data.RandomWeights ? Data.RandomWeights.Count : 0;
                    // Dictionary<Point2D, int> targetPad = new Dictionary<Point2D, int>();
                    // int flag = 0;
                    // // 将所有的概率加起来，获得上游指标
                    // for (int index = 0; index < giftCount; index++)
                    // {
                    //     Point2D target = new Point2D();
                    //     target.X = flag;
                    //     int weight = 1;
                    //     if (weightCount > 0 && index < weightCount)
                    //     {
                    //         int w = Data.RandomWeights[index];
                    //         if (w > 0)
                    //         {
                    //             weight = w;
                    //         }
                    //     }
                    //     flag += weight;
                    //     target.Y = flag;
                    //     targetPad.Add(target, index);
                    // }
                    // 获取权重标靶
                    Dictionary<Point2D, int> targetPad = data.RandomWeights.MakeTargetPad(giftCount, out int maxValue);
                    // 算出随机值，确认位置，取得序号，选出单位
                    for (int i = 0; i < times; i++)
                    {
                        // 选出类型的序号
                        int index = targetPad.Hit(maxValue);
                        // // 产生标靶
                        // int p = MathEx.Random.Next(0, maxValue);
                        // // 检查命中
                        // foreach (var target in targetPad)
                        // {
                        //     Point2D tKey = target.Key;
                        //     if (p >= tKey.X && p < tKey.Y)
                        //     {
                        //         // 中
                        //         index = target.Value;
                        //         break;
                        //     }
                        // }
                        // 计算概率
                        if (data.Chances.Bingo(index))
                        {
                            gifts.Add(data.Gifts[index]);
                        }
                    }
                }
                else
                {
                    for (int index = 0; index < giftCount; index++)
                    {
                        string id = data.Gifts[index];
                        int times = 1;
                        if (numsCount > 0 && index < numsCount)
                        {
                            times = data.Nums[index];
                        }
                        for (int i = 0; i < times; i++)
                        {
                            // 计算概率
                            if (data.Chances.Bingo(index))
                            {
                                gifts.Add(id);
                            }
                        }
                    }
                }
            }
            return gifts;
        }
    }

}
