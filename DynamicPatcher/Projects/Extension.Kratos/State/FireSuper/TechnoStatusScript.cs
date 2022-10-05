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

        public State<FireSuperData> FireSuperState = new State<FireSuperData>();


        public void OnPut_FireSuper()
        {
            // 初始化状态机
            FireSuperData data = Ini.GetConfig<FireSuperData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                FireSuperState.Enable(data);
            }
        }

        public void OnFire_FireSuper(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (FireSuperState.IsActive() && null != FireSuperState.Data)
            {
                FireSuperEntity data = FireSuperState.Data.Data;
                if (pTechno.Ref.Veterancy.IsElite())
                {
                    data = FireSuperState.Data.EliteData;
                }
                if (null != data && (data.WeaponIndex < 0 || data.WeaponIndex == weaponIndex))
                {
                    Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
                    // 检查平民
                    if (!FireSuperState.Data.DeactiveWhenCivilian || !pHouse.IsCivilian())
                    {
                        // Logger.Log($"{Game.CurrentFrame} - 发射超武 检查平民 = {FireSuperState.Data.DeactiveWhenCivilian}, 我是 {pHouse.Ref.Type.Ref.Base.ID} 平民 = {pHouse.IsCivilian()}");
                        CoordStruct targetPos = data.ToTarget ? pTarget.Ref.GetCoords() : pTechno.Ref.Base.Base.GetCoords();
                        FireSuperManager.Launch(pHouse, targetPos, data);
                    }
                }
            }
        }

    }
}
