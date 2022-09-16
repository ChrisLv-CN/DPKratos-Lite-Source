using DynamicPatcher;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using Scripts;

namespace Extension.Ext
{
    public partial class WarheadTypeExt
    {
        // 全局设置
        private static DamageTextTypeControlData damageTextTypeControlData = new DamageTextTypeControlData();

        private DamageTextTypeData damageTextTypeData;
        public DamageTextTypeData DamageTextTypeData
        {
            get
            {
                if (null == damageTextTypeData)
                {
                    string section = OwnerObject.Ref.Base.ID;
                    int infDeath = Ini.Get(Ini.GetDependency(INIConstant.RulesName), section, "InfDeath", 0);
                    if (infDeath > 0 && infDeath <= 10)
                    {
                        damageTextTypeData = damageTextTypeControlData.Types[infDeath].Clone();
                    }
                    else
                    {
                        damageTextTypeData = damageTextTypeControlData.Types[0].Clone();
                    }
                    ISectionReader reader = Ini.GetSection(Ini.GetDependency(INIConstant.RulesName), section);
                    damageTextTypeData.Read(reader);
                }
                return damageTextTypeData;
            }
        }

    }
}
