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
using System.Runtime.InteropServices.ComTypes;

namespace Extension.Script
{


    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class InfDeployerScript : TransformScriptable
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

        public override bool OnAwake()
        {
            if (!data.Enable || !pTechno.CastToInfantry(out Pointer<InfantryClass> pInf) || !pInf.Ref.Type.Ref.Deployer)
            {
                return false;
            }
            return true;
        }

        public override void OnTransform(object sender, EventArgs args)
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
