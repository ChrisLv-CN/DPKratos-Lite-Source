using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{


    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class UnitDeployerScript : TechnoScriptable
    {
        public UnitDeployerScript(TechnoExt owner) : base(owner) { }

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
            if (!data.Enable || !pTechno.CastToUnit(out Pointer<UnitClass> pUint) || !pUint.Ref.Type.Ref.IsSimpleDeployer)
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        // Hook触发
        public unsafe void UnitDeployToTransform()
        {
            if (data.Enable && pTechno.Convert<UnitClass>().Ref.Deployed)
            {
                // 载具部署完毕，变形
                pTechno.GetStatus().GiftBoxState.Enable(data);
            }
        }

    }
}