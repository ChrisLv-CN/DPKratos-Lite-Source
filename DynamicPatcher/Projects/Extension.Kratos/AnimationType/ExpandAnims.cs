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
    public class ExpandAnims
    {
        public static void PlayExpandAnims(ExpandAnimsData data, CoordStruct location, Pointer<HouseClass> pHouse = default)
        {
            if (null != data.Anims && data.Anims.Any())
            {
                List<string> anims = new List<string>();
                int animCount = data.Anims.Length;
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
                    // 获取权重标靶
                    Dictionary<Point2D, int> targetPad = data.RandomWeights.MakeTargetPad(animCount, out int maxValue);
                    // 算出随机值，确认位置，取得序号，选出单位
                    for (int i = 0; i < times; i++)
                    {
                        // 选出类型的序号
                        int index = targetPad.Hit(maxValue);
                        // 计算概率
                        if (data.Chances.Bingo(index))
                        {
                            anims.Add(data.Anims[index]);
                        }
                    }
                }
                else
                {
                    // 不随机
                    for (int index = 0; index < animCount; index++)
                    {
                        string id = data.Anims[index];
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
                                anims.Add(id);
                            }
                        }
                    }
                }
                // 开始召唤
                foreach (string animType in anims)
                {
                    if (!animType.IsNullOrEmptyOrNone())
                    {
                        Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(animType);
                        if (!pAnimType.IsNull)
                        {
                            // 位置偏移
                            CoordStruct offset = data.GetOffset();
                            Pointer<AnimClass> pNewAnim = YRMemory.Create<AnimClass>(pAnimType, location + offset);
                            pNewAnim.Ref.Owner = pHouse;
                        }
                    }
                }
            }
        }

    }


}
