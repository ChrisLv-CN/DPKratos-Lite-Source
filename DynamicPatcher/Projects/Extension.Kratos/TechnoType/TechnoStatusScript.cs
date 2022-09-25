using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    public class TechnoStatusScript : TechnoScriptable
    {
        public TechnoStatusScript(TechnoExt owner) : base(owner) { }

        public SwizzleablePointer<TechnoClass> MyMaster = new SwizzleablePointer<TechnoClass>(IntPtr.Zero);

        public bool DisableVoxelCache;
        public float VoxelShadowScaleInAir;

        public bool DisableSelectable;
        public bool DisableSelectVoice;

        // 自毁
        public DestroySelfState DestroySelfState = new DestroySelfState();

        public override void Awake()
        {
            this.VoxelShadowScaleInAir = Ini.GetSection(Ini.RulesDependency, RulesExt.SectionAudioVisual).Get("VoxelShadowScaleInAir", 2f);
            // 初始化状态机
            DestroySelfData destroySelfData = Ini.GetConfig<DestroySelfData>(Ini.RulesDependency, section).Data;
            if (destroySelfData.Delay >= 0)
            {
                DestroySelfState.Enable(destroySelfData);
            }
        }

        public override void OnUpdate()
        {
            if (DestroySelfState.AmIDead())
            {
                // 啊我死了
                if (DestroySelfState.Data.Peaceful)
                {
                    pTechno.Ref.Base.Remove();
                    pTechno.Ref.Base.UnInit();
                }
                else
                {
                    if (pTechno.TryGetComponent<DamageTextScript>(out DamageTextScript damageText))
                    {
                        damageText.SkipDamageText = true;
                    }
                    pTechno.Ref.Base.TakeDamage(pTechno.Ref.Base.Health + 1, pTechno.Ref.Type.Ref.Crewed);
                    // pTechno.Ref.Base.Destroy();
                }
                return;
            }
        }

        public override void OnSelect(ref bool selectable)
        {
            selectable = !DisableSelectable;
        }
    }
}