using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
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

        public static void PlayExpandDebirs(DynamicVectorClass<Pointer<VoxelAnimTypeClass>> types, DynamicVectorClass<int> nums, int times, CoordStruct location, Pointer<HouseClass> pHouse, Pointer<TechnoClass> pCreater)
        {
            int numsCount = nums.Count;
            int max = 0;
            Dictionary<Pointer<VoxelAnimTypeClass>, int> debirTypes = new Dictionary<Pointer<VoxelAnimTypeClass>, int>();
            for (int i = 0; i < types.Count; i++)
            {
                if (i < numsCount)
                {
                    int num = nums[i];
                    max += num;
                    debirTypes.Add(types[i], num);
                }
                else
                {
                    break;
                }
            }
            // 刷碎片
            if (max <= times)
            {
                // Logger.Log($"{Game.CurrentFrame} 刷vxl碎片 {times} - {max} = {times - max}");
                // 刷出所有的vxl碎片
                foreach (KeyValuePair<Pointer<VoxelAnimTypeClass>, int> kv in debirTypes)
                {
                    Pointer<VoxelAnimTypeClass> pAnimType = kv.Key;
                    for (int i = 0; i < kv.Value; i++)
                    {
                        Pointer<VoxelAnimClass> pAnim = YRMemory.Create<VoxelAnimClass>(pAnimType, location, pHouse);
                    }
                }
                // 剩余的从shp碎片中随机
                int lastTimes = times - max;
                if (lastTimes > 0)
                {
                    DynamicVectorClass<Pointer<AnimTypeClass>> debirs = RulesClass.Instance.Ref.MetallicDebris;
                    int count = debirs.Count;
                    if (count > 0)
                    {
                        for (int i = 0; i < lastTimes; i++)
                        {
                            int index = MathEx.Random.Next(count);
                            Pointer<AnimTypeClass> pAnimType = debirs[index];
                            Pointer<AnimClass> pNewAnim = YRMemory.Create<AnimClass>(pAnimType, location);
                            pNewAnim.Ref.Owner = pHouse;
                            pNewAnim.SetCreater(pCreater);
                        }
                    }
                }
            }
            else
            {
                // Logger.Log($"{Game.CurrentFrame} 按权重随机刷vxl碎片 {times} - {max} = {times - max}");
                // 按照权重随机
                Dictionary<int, int> marks = new Dictionary<int, int>();
                // 权重标靶
                Dictionary<Point2D, int> targetPad = debirTypes.Values.ToArray().MakeTargetPad(debirTypes.Count(), out int maxValue);
                for (int i = 0; i < times; i++)
                {
                    bool spawn = false;
                    Pointer<VoxelAnimTypeClass> pAnimType = IntPtr.Zero;
                    int index = targetPad.Hit(maxValue);
                    if (!marks.ContainsKey(index))
                    {
                        spawn = true;
                        marks.Add(index, 1);
                        pAnimType = debirTypes.ElementAt(index).Key;
                    }
                    else
                    {
                        int count = marks[index];
                        KeyValuePair<Pointer<VoxelAnimTypeClass>, int> kv = debirTypes.ElementAt(index);
                        if (count < kv.Value)
                        {
                            spawn = true;
                            marks[index]++;
                            pAnimType = kv.Key;
                        }
                        else
                        {
                            i--;
                        }
                    }
                    if (spawn)
                    {
                        Pointer<VoxelAnimClass> pAnim = YRMemory.Create<VoxelAnimClass>(pAnimType, location, pHouse);
                    }
                }
            }
        }

    }


}
