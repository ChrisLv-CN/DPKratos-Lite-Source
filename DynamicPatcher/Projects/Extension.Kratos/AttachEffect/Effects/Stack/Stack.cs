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
                if (Data.Level < 0 || stacks > Data.Level)
                {
                    count++;
                    // 触发
                    AE.AEManager.Attach(Data.AttachEffects, AE.pSource.Convert<ObjectClass>(), AE.pSourceHouse);
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
    }
}
