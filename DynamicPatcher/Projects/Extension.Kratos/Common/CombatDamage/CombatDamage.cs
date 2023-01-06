using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Ext
{

    public class CombatDamage
    {
        private static IConfigWrapper<CombatDamageData> _data;
        public static CombatDamageData Data
        {
            get
            {
                if (null == _data)
                {
                    _data = Ini.GetConfig<CombatDamageData>(Ini.RulesDependency, RulesClass.SectionCombatDamage);
                }
                return _data.Data;
            }
        }

        public static void Reload(object sender, EventArgs e)
        {
            _data = null;
        }
    }
}
