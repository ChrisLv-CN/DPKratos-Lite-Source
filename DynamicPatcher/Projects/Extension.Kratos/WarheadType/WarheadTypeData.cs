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
        }

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
                            // Logger.Log($"Try to read Ares's [ArmorTypes], get {count} types.");
                            for (int i = 0; i < count; i++)
                            {
                                string keyName = pINI.Ref.GetKeyName(section, i);
                                string value = reader.Get<string>(keyName, null);
                                if (value.IsNullOrEmpty())
                                {
                                    Logger.LogWarning($"Try to read Ares's [ArmorTypes], ArmorType {keyName} is {value}");
                                    value = "0%";
                                }
                                _aresArmorArray.Add(keyName, value);
                            }
                        }
                        Dictionary<string, string> temp = new Dictionary<string, string>(_aresArmorArray);
                        // ????????????????????????????????????????????????????????????????????????????????????
                        foreach (KeyValuePair<string, string> armor in temp)
                        {
                            string key = armor.Key;
                            string val = GetArmorValue(key, _aresArmorArray);
                            _aresArmorArray[key] = val;
                        }
                        // int ii = 11;
                        // foreach (KeyValuePair<string, string> armor in _aresArmorArray)
                        // {
                        //     Logger.Log($"{Game.CurrentFrame} Armor {ii} - {armor.Key} = {armor.Value}");
                        //     ii++;
                        // }
                    }
                }
                return _aresArmorArray;
            }
        }

        // YR
        public double[] Versus; // YRPP?????????????????????1?????????????????????
        // Ares
        public List<AresVersus> AresVersus; // Ares????????????????????????

        public bool AffectsOwner;
        public bool AffectsAllies;
        public bool AffectsEnemies;
        public bool EffectsRequireDamage;
        public bool EffectsRequireVerses;
        public bool AllowZeroDamage;
        public string PreImpactAnim;

        // Kratos
        public bool AffectInAir;
        public bool AffectStand;

        public bool ClearTarget;

        public Mission ForceMission;

        // ????????????
        public bool IsToy;
        // ??????????????????
        public bool Teleporter;
        // ??????????????????
        public bool Capturer;
        // ???????????????
        public bool IgnoreRevenge;
        // ?????????????????????
        public bool IgnoreDamageReaction;
        public DamageReactionMode[] IgnoreDamageReactionModes;
        // ?????????????????????
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
            this.AffectStand = false;

            this.ClearTarget = false;

            this.ForceMission = Mission.None;

            this.IsToy = false;
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
            this.AffectsAllies = reader.Get("AffectsAllies", this.AffectsAllies);
            this.AffectsOwner = reader.Get("AffectsOwner", this.AffectsAllies);

            this.AffectsEnemies = reader.Get("AffectsEnemies", this.AffectsEnemies);
            this.EffectsRequireDamage = reader.Get("EffectsRequireDamage", this.EffectsRequireDamage);
            this.EffectsRequireVerses = reader.Get("EffectsRequireVerses", this.EffectsRequireVerses);
            this.AllowZeroDamage = reader.Get("AllowZeroDamage", this.AllowZeroDamage);
            this.PreImpactAnim = reader.Get("PreImpactAnim", this.PreImpactAnim);

            // Kratos
            this.AffectInAir = reader.Get("AffectInAir", this.AffectInAir);
            this.AffectStand = reader.Get("AffectStand", this.AffectStand);

            this.ClearTarget = reader.Get("ClearTarget", this.ClearTarget);

            this.ForceMission = reader.Get("ForceMission", this.ForceMission);

            this.IsToy = reader.Get("IsToy", this.IsToy);
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
            if (null != AresArmorArray && AresArmorArray.Any())
            {
                string title = "Versus.";
                foreach (KeyValuePair<string, string> armor in AresArmorArray)
                {
                    // ????????????????????????????????????
                    string name = armor.Key;
                    double defaultVersus = 1d;

                    string value = armor.Value;
                    if (!value.IsNullOrEmpty())
                    {
                        // ????????????
                        if (value.IndexOf("%") > -1)
                        {
                            string temp = value.Substring(0, value.IndexOf("%"));
                            defaultVersus = Convert.ToDouble(temp) / 100;
                        }
                        else if (isDefaultArmor(value, out int index) && index < 11)
                        {
                            defaultVersus = this.Versus[index];
                        }
                    }
                    // ??????????????????
                    string key = title + name;
                    double versus = reader.GetPercent(key, defaultVersus, true);
                    key = title + name + ".ForceFire";
                    bool forceFire = reader.Get(key, true);
                    key = title + name + ".Retaliate";
                    bool retaliate = reader.Get(key, true);
                    key = title + name + ".PassiveAcquire";
                    bool passiveAcquire = reader.Get(key, true);
                    if (null == AresVersus)
                    {
                        AresVersus = new List<AresVersus>();
                    }
                    AresVersus.Add(new AresVersus(versus, forceFire, retaliate, passiveAcquire));
                }
            }
        }

        private bool isDefaultArmor(string armor, out int index)
        {
            index = 0;
            if (Enum.TryParse<Armor>(armor, true, out Armor result))
            {
                index = (int)result;
                return true;
            }
            return false;
        }

        private static string GetArmorValue(string key, Dictionary<string, string> array)
        {
            if (array.ContainsKey(key))
            {
                string value = array[key];
                if (value.IndexOf("%") > -1 || Enum.TryParse<Armor>(value, true, out Armor armor))
                {
                    return value;
                }
                // ???????????????
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
                // Logger.Log($"{Game.CurrentFrame} ?????????????????????????????? {armor}????????? {index}");
                if (index < 11)
                {
                    // ????????????
                    versus = Versus[index];
                    forceFire = versus > 0.0;
                    retaliate = versus > 0.1;
                    passiveAcquire = versus > 0.2;
                }
                else if (null != AresVersus && index < AresVersus.Count())
                {
                    index -= 11;
                    // ????????????
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
