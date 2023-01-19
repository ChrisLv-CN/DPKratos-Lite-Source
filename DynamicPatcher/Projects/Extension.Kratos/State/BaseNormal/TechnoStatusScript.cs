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

    public partial class TechnoStatusScript
    {
        public static List<TechnoExt> BaseUnitArray = new List<TechnoExt>();
        public static List<TechnoExt> BaseStandArray = new List<TechnoExt>();

        private IConfigWrapper<BaseNormalData> _baseNormalData;
        public BaseNormalData BaseNormalData
        {
            get
            {
                if (null == _baseNormalData)
                {
                    _baseNormalData = Ini.GetConfig<BaseNormalData>(Ini.RulesDependency, section);
                }
                return _baseNormalData.Data;
            }
        }

        public static void Clear_BaseNormal()
        {
            BaseUnitArray.Clear();
            BaseStandArray.Clear();
        }

        public void OnPut_BaseNormal(Pointer<CoordStruct> pCoord, DirType dirType)
        {
            if (!isBuilding && BaseNormalData.BaseNormal)
            {
                if (AmIStand())
                {
                    BaseStandArray.Add(Owner);
                }
                else
                {
                    BaseUnitArray.Add(Owner);
                }
            }
        }

        public void OnReceiveDamageDestroy_BaseNormal()
        {
            // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pTechno} 被炸死, VirtualUnit = {VirtualUnit}, MyMasterIsAnim = {MyMasterIsAnim}");
            OnRemove_BaseNormal();
        }

        public void OnRemove_BaseNormal()
        {
            // Logger.Log($"{Game.CurrentFrame}, [{section}]{pTechno} remove on the map");
            BaseUnitArray.Remove(Owner);
            BaseStandArray.Remove(Owner);
        }

    }
}
