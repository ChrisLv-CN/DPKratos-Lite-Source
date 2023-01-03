using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{


    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class InfDeployerScript : TechnoScriptable
    {
        public InfDeployerScript(TechnoExt owner) : base(owner) { }

        private IConfigWrapper<DeployToTransformData> _data;
        private DeployToTransformData data
        {
            get
            {
                if (null == _data)
                {
                    _data = Ini.GetConfig<DeployToTransformData>(Ini.RulesDependency, section);
                }
                return _data.Data;
            }
        }

        public override void Awake()
        {
            if (!data.Enable || !pTechno.CastToInfantry(out Pointer<InfantryClass> pInf) || !pInf.Ref.Type.Ref.Deployer)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            EventSystem.Techno.AddTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public override void OnUnInit()
        {
            EventSystem.Techno.RemoveTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public void OnTransform(object sender, EventArgs args)
        {
            Pointer<TechnoClass> pTarget = ((TechnoTypeChangeEventArgs)args).pTechno;
            if (!pTarget.IsNull && pTarget == pTechno)
            {
                _data = null;
            }
        }

        public override void OnUpdate()
        {
            if (pTechno.CastToInfantry(out Pointer<InfantryClass> pInf) && pInf.Ref.SequenceAnim == SequenceAnimType.Deployed)
            {
                // 步兵部署完毕，变形
                pTechno.GetStatus().GiftBoxState.Enable(data);
            }
        }

    }
}
