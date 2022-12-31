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

        public static CombatDamageData Data = Ini.GetConfig<CombatDamageData>(Ini.RulesDependency, RulesClass.SectionCombatDamage).Data;

    }
}
