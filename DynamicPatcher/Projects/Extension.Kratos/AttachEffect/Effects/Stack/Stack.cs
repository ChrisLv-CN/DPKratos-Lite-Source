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
        public Stack Stack;

        private void InitStack()
        {
            this.Stack = AEData.StackData.CreateEffect<Stack>();
            RegisterEffect(Stack);
        }
    }


    [Serializable]
    public class Stack : Effect<StackData>
    {

        private int count;

        public override void OnUpdate(CoordStruct location, bool isDead)
        {
            if (!isDead)
            {
                Watch();
            }
        }

        public override void OnWarpUpdate(CoordStruct location, bool isDead)
        {
            if (!isDead)
            {
                Watch();
            }
        }

        private void Watch()
        {
            string watch = Data.Watch;
            int stacks = -1;
            // Logger.Log($"{Game.CurrentFrame} 检查AE {watch} 的层数");
            if (AE.AEManager.AEStacks.ContainsKey(watch) && (stacks = AE.AEManager.AEStacks[watch]) > 0)
            {
                // Logger.Log($"{Game.CurrentFrame} 检查AE {watch} 的层数 {stacks}");
                if (CanActive(stacks))
                {
                    count++;
                    // 触发
                    // 添加AE
                    if (Data.Attach)
                    {
                        AE.AEManager.Attach(Data.AttachEffects, Data.AttachChances, AE.pSource.Convert<ObjectClass>(), AE.pSourceHouse);
                    }
                    // 移除AE
                    if (Data.Remove)
                    {
                        AE.AEManager.Disable(Data.RemoveEffects);
                    }
                    // 移除被监视者
                    if (Data.RemoveAll)
                    {
                        AE.AEManager.Disable(new string[] { watch });
                    }
                }
            }
            if (Data.TriggeredTimes > 0 && count >= Data.TriggeredTimes)
            {
                // 触发次数够了，移除自身
                Disable(default);
            }
        }

        private bool CanActive(int stacks)
        {
            int level = Data.Level;
            if (level >= 0)
            {
                switch (Data.Condition)
                {
                    case Condition.EQ:
                        return stacks == level;
                    case Condition.NE:
                        return stacks != level;
                    case Condition.GT:
                        return stacks > level;
                    case Condition.LT:
                        return stacks < level;
                    case Condition.GE:
                        return stacks >= level;
                    case Condition.LE:
                        return stacks <= level;
                }
            }
            return true;
        }
    }
}
