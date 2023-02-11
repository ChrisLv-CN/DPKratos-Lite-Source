using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public enum ExpLevel
    {
        None = 0, Rookie = 1, Veteran = 2, Elite = 3
    }
    public class ExpLevelParser : KEnumParser<ExpLevel>
    {
        public override bool ParseInitials(string t, ref ExpLevel buffer)
        {
            switch (t)
            {
                case "R":
                    buffer = ExpLevel.Rookie;
                    // this.DefaultText = LongText.HIT; // 击中
                    return true;
                case "V":
                    buffer = ExpLevel.Veteran;
                    // this.DefaultText = LongText.GLANCING; // 偏斜
                    return true;
                case "E":
                    buffer = ExpLevel.Elite;
                    // this.DefaultText = LongText.BLOCK; // 格挡
                    return true;
                default:
                    buffer = ExpLevel.None;
                    // this.DefaultText = LongText.MISS; // 未命中
                    return true;
            }
        }
    }

    [Serializable]
    public class AresVersus
    {
        public double Versus = 1.0;

        public bool ForceFire = true;
        public bool Retaliate = true;
        public bool PassiveAcquire = true;

        public AresVersus(double versus, bool forceFire, bool retaliate, bool passiveAcquire)
        {
            this.Versus = versus;
            this.ForceFire = forceFire;
            this.Retaliate = retaliate;
            this.PassiveAcquire = passiveAcquire;
        }
    }


    [Serializable]
    public class WarheadTypeData : INIConfig
    {

        static WarheadTypeData()
        {
            new DamageReactionModeParser().Register();
            new ExpLevelParser().Register();
        }

        // 储存ares护甲的注册表，护甲的对应关系
        private static Dictionary<string, string> _aresArmorArray;
        public static Dictionary<string, string> AresArmorArray
        {
            get
            {
                if (null == _aresArmorArray)
                {
                    _aresArmorArray = new Dictionary<string, string>();
                    AnsiString section = "ArmorTypes";
                    if (Ini.HasSection(Ini.RulesDependency, section))
                    {
                        Pointer<CCINIClass> pINI = CCINIClass.INI_Rules;
                        int count = pINI.Ref.GetKeyCount(section);
                        if (count > 0)
                        {
                            ISectionReader reader = Ini.GetSection(Ini.RulesDependency, section);
                            Logger.Log($"Try to read Ares's [ArmorTypes], get {count} custom types.");
                            for (int i = 0; i < count; i++)
                            {
                                string keyName = pINI.Ref.GetKeyName(section, i);
                                string value = reader.Get<string>(keyName, null);
                                if (value.IsNullOrEmpty())
                                {
                                    Logger.LogWarning($"Try to read Ares's [ArmorTypes], ArmorType {keyName} is {value}");
                                    value = "0%";
                                }
                                // Logger.Log($"{keyName}={value}");
                                _aresArmorArray.Add(keyName, value);
                            }
                        }
                    }
                }
                return _aresArmorArray;
            }
        }

        // 读取完所有嵌套护甲对应的百分比默认值
        private static Dictionary<string, string> _aresArmorValueArray;
        public static Dictionary<string, string> AresArmorValueArray
        {
            get
            {
                if (null == _aresArmorValueArray)
                {
                    // 格式化所有的自定义护甲，包含嵌套护甲，获取实际的护甲信息，作为默认值
                    _aresArmorValueArray = new Dictionary<string, string>(AresArmorArray);
                    foreach (KeyValuePair<string, string> armor in AresArmorArray)
                    {
                        string key = armor.Key;
                        string val = GetArmorValue(key, _aresArmorValueArray);
                        _aresArmorValueArray[key] = val;
                    }
                    Logger.Log($"[ArmorTypes]");
                    int i = 11;
                    foreach (KeyValuePair<string, string> armor in AresArmorArray)
                    {
                        string key = armor.Key;
                        string value = armor.Value;
                        if (value.IndexOf("%") > -1 || IsDefaultArmor(value, out int index))
                        {
                            Logger.Log($"Armor {i} - {key} = {value}");
                        }
                        else
                        {
                            Logger.Log($"Armor {i} - {key} = {value} = {_aresArmorValueArray[key]}");
                        }
                        i++;
                    }
                }
                return _aresArmorValueArray;
            }
        }

        // YR
        public double[] Versus; // YRPP获取的比例全是1，只能自己维护
        // Ares
        public List<AresVersus> AresVersus; // Ares自定义护甲的参数

        public bool AffectsOwner;
        public bool AffectsAllies;
        public bool AffectsEnemies;
        public bool EffectsRequireDamage;
        public bool EffectsRequireVerses;
        public bool AllowZeroDamage;
        public string PreImpactAnim;

        // Kratos
        public bool AffectInAir;
        public bool AffectShooter;
        public bool AffectStand;

        public bool ClearTarget;
        public bool ClearDisguise;

        public Mission ForceMission;

        public int ExpCost;
        public ExpLevel ExpLevel;

        // 玩具弹头
        public bool IsToy;

        // 欠揍
        public bool Lueluelue;

        // 弹头传送标记
        public bool Teleporter;
        // 弹头捕获标记
        public bool Capturer;
        // 不触发复仇
        public bool IgnoreRevenge;
        // 不触发伤害响应
        public bool IgnoreDamageReaction;
        public DamageReactionMode[] IgnoreDamageReactionModes;
        // 替身不分摊伤害
        public bool IgnoreStandShareDamage;

        public WarheadTypeData()
        {
            this.Versus = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            // Ares
            this.AresVersus = null;
            this.AffectsOwner = true;
            this.AffectsAllies = true;
            this.AffectsEnemies = true;
            this.EffectsRequireDamage = false;
            this.EffectsRequireVerses = true;
            this.AllowZeroDamage = false;
            this.PreImpactAnim = null;

            // Kratos
            this.AffectInAir = true;
            this.AffectShooter = true;
            this.AffectStand = false;

            this.ClearTarget = false;
            this.ClearDisguise = false;

            this.ForceMission = Mission.None;

            this.ExpCost = 0;
            this.ExpLevel = ExpLevel.None;

            this.IsToy = false;

            this.Lueluelue = false;

            this.Teleporter = false;
            this.Capturer = false;
            this.IgnoreRevenge = false;
            this.IgnoreDamageReaction = false;
            this.IgnoreDamageReactionModes = null;
            this.IgnoreStandShareDamage = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.Versus = reader.GetPercentList("Verses", this.Versus);
            // Ares
            ReadAresVersus(reader);
            if (reader.TryGet("AffectsAllies", out bool affectsAllies))
            {
                this.AffectsAllies = affectsAllies;
                this.AffectsOwner = affectsAllies;
            }
            if (reader.TryGet("AffectsOwner", out bool affectsOwner))
            {
                this.AffectsOwner = affectsOwner;
            }
            this.AffectsEnemies = reader.Get("AffectsEnemies", this.AffectsEnemies);
            this.EffectsRequireDamage = reader.Get("EffectsRequireDamage", this.EffectsRequireDamage);
            this.EffectsRequireVerses = reader.Get("EffectsRequireVerses", this.EffectsRequireVerses);
            this.AllowZeroDamage = reader.Get("AllowZeroDamage", this.AllowZeroDamage);
            this.PreImpactAnim = reader.Get("PreImpactAnim", this.PreImpactAnim);

            // Kratos
            this.AffectInAir = reader.Get("AffectInAir", this.AffectInAir);
            this.AffectShooter = reader.Get("AffectShooter", this.AffectShooter);
            this.AffectStand = reader.Get("AffectStand", this.AffectStand);

            this.ClearTarget = reader.Get("ClearTarget", this.ClearTarget);
            this.ClearDisguise = reader.Get("ClearDisguise", this.ClearDisguise);

            this.ForceMission = reader.Get("ForceMission", this.ForceMission);

            this.ExpCost = reader.Get("ExpCost", this.ExpCost);
            this.ExpLevel = reader.Get("ExpLevel", this.ExpLevel);

            this.IsToy = reader.Get("IsToy", this.IsToy);

            this.Lueluelue = reader.Get("Lueluelue", this.Lueluelue);

            this.Teleporter = reader.Get("Teleporter", this.Teleporter);
            this.Capturer = reader.Get("Capturer", this.Capturer);
            this.IgnoreRevenge = reader.Get("IgnoreRevenge", this.IgnoreRevenge);
            this.IgnoreDamageReaction = reader.Get("IgnoreDamageReaction", this.IgnoreDamageReaction);
            this.IgnoreDamageReactionModes = reader.GetList("IgnoreDamageReaction.Modes", this.IgnoreDamageReactionModes);
            if (null != IgnoreDamageReactionModes && IgnoreDamageReactionModes.Any())
            {
                this.IgnoreDamageReaction = true;
            }
            this.IgnoreStandShareDamage = reader.Get("IgnoreStandShareDamage", this.IgnoreStandShareDamage);
        }

        private void ReadAresVersus(IConfigReader reader)
        {
            if (null != AresArmorValueArray && AresArmorValueArray.Any())
            {
                string title = "Versus.";
                foreach (KeyValuePair<string, string> armor in AresArmorValueArray)
                {
                    // 获得所有自定义护甲的信息
                    string name = armor.Key;
                    // 通过嵌套读取护甲的默认值
                    string value = armor.Value;
                    // 实际的比例
                    double defaultVersus = 1d;
                    if (!value.IsNullOrEmpty())
                    {
                        // 含百分号
                        if (value.IndexOf("%") > -1)
                        {
                            string temp = value.Substring(0, value.IndexOf("%"));
                            defaultVersus = Convert.ToDouble(temp) / 100;
                        }
                        else if (IsDefaultArmor(value, out int index) && index < 11)
                        {
                            defaultVersus = this.Versus[index];
                        }
                    }
                    // 读取弹头设置
                    double versus = defaultVersus;
                    bool forceFire = true;
                    bool retaliate = true;
                    bool passiveAcquire = true;
                    // 如果是嵌套护甲，如果没有明写，那么就需要去读嵌套的
                    Stack<string> armorNames = new Stack<string>();
                    GetArmorKeys(name, AresArmorArray, ref armorNames);
                    // Logger.Log($"{Game.CurrentFrame} 读取弹头 [{reader.Section}] 对护甲 [{name}]的比例，嵌套了 {armorNames.Count()} 层，逐层读取");
                    foreach (string armorName in armorNames)
                    {
                        string key = title + armorName;
                        // 从最上层开始读取
                        versus = reader.GetPercent(key, versus, true);
                        key = title + armorName + ".ForceFire";
                        forceFire = reader.Get(key, forceFire);
                        key = title + armorName + ".Retaliate";
                        retaliate = reader.Get(key, retaliate);
                        key = title + armorName + ".PassiveAcquire";
                        passiveAcquire = reader.Get(key, passiveAcquire);
                        // Logger.Log($"{Game.CurrentFrame} - 读取弹头 [{reader.Section}] 对护甲 [{armorName}]的比例 {key} = {versus}, forceFire = {forceFire}, retaliate = {retaliate}, passiveAcquire = {passiveAcquire}");
                    }
                    if (null == AresVersus)
                    {
                        AresVersus = new List<AresVersus>();
                    }
                    AresVersus.Add(new AresVersus(versus, forceFire, retaliate, passiveAcquire));
                }
            }
        }

        public static string GetArmorName(Armor armor)
        {
            string name = armor.ToString();
            if (!IsDefaultArmor(name, out int index))
            {
                int i = (int)armor;
                name = i.ToString();
                i -= 11;
                // Logger.Log($"{Game.CurrentFrame} 读取护甲 [{name}] 的名字，{i} = {AresArmorArray.Count()}");
                if (i >= 0 && i < AresArmorArray.Count())
                {
                    name = AresArmorArray.ElementAt(i).Key;
                }
            }
            return name;
        }

        private static bool IsDefaultArmor(string armor, out int index)
        {
            index = 0;
            if (Enum.TryParse<Armor>(armor, true, out Armor result) && Enum.IsDefined(typeof(Armor), result))
            {
                index = (int)result;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 迭代获取嵌套护甲的名字堆栈
        /// </summary>
        /// <param name="key"></param>
        /// <param name="array"></param>
        /// <param name="keys"></param>
        private static void GetArmorKeys(string key, Dictionary<string, string> array, ref Stack<string> keys)
        {
            if (array.ContainsKey(key))
            {
                keys.Push(key);
                // 查找是否嵌套
                string value = array[key];
                if (value.IndexOf("%") > -1 || IsDefaultArmor(value, out int index))
                {
                    return;
                }
                // 迭代查找
                if (value != key)
                {
                    GetArmorKeys(value, array, ref keys);
                }
            }
        }

        /// <summary>
        /// 迭代读取嵌套护甲最底层的数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        private static string GetArmorValue(string key, Dictionary<string, string> array)
        {
            if (array.ContainsKey(key))
            {
                string value = array[key];
                // 如果是百分比则返回百分比，如果是游戏默认护甲，则返回默认护甲
                if (value.IndexOf("%") > -1 || IsDefaultArmor(value, out int index))
                {
                    return value;
                }
                // 迭代查找
                if (value != key)
                {
                    return GetArmorValue(value, array);
                }
            }
            Logger.LogWarning($"Try to read Ares's [ArmorTypes] but type [{key}] value is wrong.");
            return "0%";
        }

        public double GetVersus(Armor armor, out bool forceFire, out bool retaliate, out bool passiveAcquire)
        {
            double versus = 1d;
            forceFire = true;
            retaliate = true;
            passiveAcquire = true;

            int index = (int)armor;
            if (index >= 0)
            {
                // Logger.Log($"{Game.CurrentFrame} 检查是否可攻击，护甲 {armor}，序号 {index}");
                if (index < 11)
                {
                    // 原始护甲
                    versus = Versus[index];
                    forceFire = versus > 0.0;
                    retaliate = versus > 0.1;
                    passiveAcquire = versus > 0.2;
                }
                else if (null != AresVersus && index < AresVersus.Count())
                {
                    index -= 11;
                    // 扩展护甲
                    AresVersus aresVersus = AresVersus[index];
                    versus = aresVersus.Versus;
                    forceFire = aresVersus.ForceFire;
                    retaliate = aresVersus.Retaliate;
                    passiveAcquire = aresVersus.PassiveAcquire;
                }
            }
            return versus;
        }

    }


}
