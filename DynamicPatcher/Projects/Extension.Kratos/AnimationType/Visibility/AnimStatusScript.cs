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

    public partial class AnimStatusScript
    {
        private bool initInvisibleFlag = false;

        public void OnUpdate_Visibility()
        {
            // 断言第一次执行update时，所属已被设置
            if (!initInvisibleFlag)
            {
                // 初始化
                AnimVisibilityData data = Ini.GetConfig<AnimVisibilityData>(Ini.ArtDependency, section).Data;
                UpdateVisibility(data.Visibility);
            }
        }

        public void UpdateVisibility(Relation visibility)
        {
            pAnim.Ref.Invisible = GetInvisible(visibility);
            initInvisibleFlag = true;
        }

        private bool GetInvisible(Relation visibility)
        {
            // Logger.Log($"{Game.CurrentFrame} - {OwnerObject}[{OwnerObject.Ref.Type.Ref.Base.Base.ID}] get invisible visibility = {visibility}");
            if (!pAnim.Ref.Owner.IsNull)
            {
                Relation relation = pAnim.Ref.Owner.GetRelationWithPlayer();
                return !visibility.HasFlag(relation);
            }
            return false;
        }
    }
}
