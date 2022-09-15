
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Decorators;
using Extension.Utilities;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Extension;
using Extension.INI;
using PatcherYRpp.FileFormats;

namespace Miscellaneous
{
    public class DynamicLoadINI
    {
#if REALTIME_INI
        // the list of ini to ignore appling changes
        static string[] ignoreList = new[]
        {
            "ra2md.ini",
            // reshader
            "reshade.ini",
            "DefaultPreset.ini"

        }.Select(i => i.ToLower().Replace(@"\", "/")).ToArray();

        static string[] watchList = new[]
        {
            "*.ini",
            "*.map",
            "*.yrm",

        }.Select(i => i.ToLower().Replace(@"\", "/")).ToArray();

        static string FindMainINI(string name)
        {
            Func<Pointer<CCINIClass>, bool> IsMainINI = (Pointer<CCINIClass> pINI) => 
            {
	            string section = "#include";
	            int length = pINI.Ref.GetKeyCount(section);
                INIReader reader = new INIFileReader(pINI);
                for (int i = 0; i < length; i++)
                {
		            string key = pINI.Ref.GetKeyName(section, i);
                    string sub_name = null;
                    if (reader.Read(section, key, ref sub_name))
                    {
                        //Logger.Log("sub_name {0}", sub_name);
                        if (sub_name.ToLower().Contains(name.ToLower()))
                        //if (string.Compare(sub_name, name, true) == 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            };

            List<string> mainINIs = new() {
                INIConstant.RulesName, INIConstant.ArtName, INIConstant.AiName,
                INIConstant.GameModeName, INIConstant.MapName,
            };
            
            foreach (var iniName in mainINIs)
            {
                using var ini = CreateAndReadINI(iniName);
                if (string.Compare(iniName, name, true) == 0 || IsMainINI(ini))
                {
                    return iniName;
                }
            }

            return name;
        }

        private static YRClassHandle<CCINIClass> CreateAndReadINI(string iniName)
        {
            var hINI = new YRClassHandle<CCINIClass>();
            using var hFile = new YRClassHandle<CCFileClass>(iniName);
            Logger.Log("reloading {0}.", iniName);
            hINI.Ref.ReadCCFile(hFile);
            return hINI;
        }

        private static IEnumerable<Pointer<T>> ShowProgress<T>(IEnumerable<Pointer<T>> enumerable, int length)
        {
            Logger.Log("processing {0} items...", length);
            var random = new Random();
            int curIdx = 0;
            foreach (var item in enumerable)
            {
                yield return item;
                curIdx++;
                if (length <= 10 || curIdx % (length / random.Next(5, 10)) == 0)
                {
                    Logger.Log("{0:00.0%} | {1}/{2}", curIdx / (float)length, curIdx, length);
                }
            }

            Logger.Log("{0} items processed.", length);
        }
        private static IEnumerable<Pointer<T>> ShowProgress<T>(YRPP.GLOBAL_DVC_ARRAY<T> dvc)
        {
            return ShowProgress(dvc, dvc.Array.Count);
        }

        private static void ReloadRulesAndArt()
        {
            using var hRulesINI = CreateAndReadINI(INIConstant.RulesName);
            using var hGameModeINI = CreateAndReadINI(INIConstant.GameModeName);
            using var hMapINI = CreateAndReadINI(INIConstant.MapName);

            using var hArtINI = CreateAndReadINI(INIConstant.ArtName);
            using var memory = new MemoryHandle(Marshal.SizeOf<CCINIClass>());

            Logger.Log("storing art data.");
            Helpers.Copy(CCINIClass.INI_Art, (IntPtr)memory.Memory, memory.Size);
            Logger.Log("writing new art data.");
            Helpers.Copy(hArtINI.Pointer, CCINIClass.INI_Art, memory.Size);


            Logger.Log("reloading RulesClass");
            void ReadRules(Pointer<CCINIClass> ini)
            {
                //RulesClass.Instance.Ref.Read_File(hRulesINI);

                Logger.Log("reloading Colors");
                RulesClass.Instance.Ref.Read_Colors(ini);

                Logger.Log("reloading ColorAdd -- skipped");
                //RulesClass.Instance.Ref.Read_ColorAdd(ini);

                Logger.Log("reloading Countries");
                RulesClass.Instance.Ref.Read_Countries(ini);

                Logger.Log("reloading Sides");
                RulesClass.Instance.Ref.Read_Sides(ini);

                Logger.Log("reloading OverlayTypes");
                RulesClass.Instance.Ref.Read_OverlayTypes(ini);

                Logger.Log("reloading SuperWeaponTypes");
                RulesClass.Instance.Ref.Read_SuperWeaponTypes(ini);

                Logger.Log("reloading Warheads");
                RulesClass.Instance.Ref.Read_Warheads(ini);

                Logger.Log("reloading SmudgeTypes");
                RulesClass.Instance.Ref.Read_SmudgeTypes(ini);

                Logger.Log("reloading TerrainTypes");
                RulesClass.Instance.Ref.Read_TerrainTypes(ini);

                Logger.Log("reloading BuildingTypes");
                RulesClass.Instance.Ref.Read_BuildingTypes(ini);

                Logger.Log("reloading VehicleTypes");
                RulesClass.Instance.Ref.Read_VehicleTypes(ini);

                Logger.Log("reloading AircraftTypes");
                RulesClass.Instance.Ref.Read_AircraftTypes(ini);

                Logger.Log("reloading InfantryTypes");
                RulesClass.Instance.Ref.Read_InfantryTypes(ini);

                Logger.Log("reloading Animations");
                RulesClass.Instance.Ref.Read_Animations(ini);

                Logger.Log("reloading VoxelAnims");
                RulesClass.Instance.Ref.Read_VoxelAnims(ini);

                Logger.Log("reloading Particles");
                RulesClass.Instance.Ref.Read_Particles(ini);

                Logger.Log("reloading ParticleSystems");
                RulesClass.Instance.Ref.Read_ParticleSystems(ini);

                Logger.Log("reloading JumpjetControls");
                RulesClass.Instance.Ref.Read_JumpjetControls(ini);

                Logger.Log("reloading MultiplayerDialogSettings");
                RulesClass.Instance.Ref.Read_MultiplayerDialogSettings(ini);

                Logger.Log("reloading AI");
                RulesClass.Instance.Ref.Read_AI(ini);

                Logger.Log("reloading Powerups");
                RulesClass.Instance.Ref.Read_Powerups(ini);

                Logger.Log("reloading LandCharacteristics");
                RulesClass.Instance.Ref.Read_LandCharacteristics(ini);

                Logger.Log("reloading IQ");
                RulesClass.Instance.Ref.Read_IQ(ini);

                Logger.Log("reloading General");
                RulesClass.Instance.Ref.Read_General(ini);

                Logger.Log("reloading Types -- skipped");
                //RulesClass.Instance.Ref.Read_Types(ini);

                Logger.Log("reloading Difficulties");
                RulesClass.Instance.Ref.Read_Difficulties(ini);

                Logger.Log("reloading CrateRules");
                RulesClass.Instance.Ref.Read_CrateRules(ini);

                Logger.Log("reloading CombatDamage");
                RulesClass.Instance.Ref.Read_CombatDamage(ini);

                Logger.Log("reloading Radiation");
                RulesClass.Instance.Ref.Read_Radiation(ini);

                Logger.Log("reloading ElevationModel");
                RulesClass.Instance.Ref.Read_ElevationModel(ini);

                Logger.Log("reloading WallModel");
                RulesClass.Instance.Ref.Read_WallModel(ini);

                Logger.Log("reloading AudioVisual");
                RulesClass.Instance.Ref.Read_AudioVisual(ini);

                Logger.Log("reloading SpecialWeapons");
                RulesClass.Instance.Ref.Read_SpecialWeapons(ini);

                Logger.Log("reloading Tiberiums -- skipped");
                //RulesClass.Instance.Ref.Read_Tiberiums(ini);

                Logger.Log("reloading AdvancedCommandBar");
                RulesClass.Instance.Ref.Read_AdvancedCommandBar(ini, isMultiplayer: SessionClass.Instance.GameMode != GameMode.Campaign && SessionClass.Instance.GameMode != GameMode.Skirmish);
            }
            Logger.Log("----------------------------------------");
            Logger.Log("reloading RulesClass from {0}", INIConstant.RulesName);
            ReadRules(hRulesINI);
            Logger.Log("----------------------------------------");
            Logger.Log("reloading RulesClass from {0}", INIConstant.GameModeName);
            ReadRules(hGameModeINI);
            Logger.Log("----------------------------------------");
            Logger.Log("reloading RulesClass from {0}", INIConstant.MapName);
            ReadRules(hMapINI);

            Logger.Log("----------------------------------------");
            Logger.Log("clearing TechnoType buffers...");
            foreach (Pointer<TechnoTypeClass> pItem in ShowProgress(TechnoTypeClass.ABSTRACTTYPE_ARRAY))
            {
                // may memory leak
                //YRMemory.Delete(pItem.Ref.Cameo);
                //YRMemory.Delete(pItem.Ref.AltCameo);

                pItem.Ref.Cameo = Pointer<SHPStruct>.Zero;
                pItem.Ref.AltCameo = Pointer<SHPStruct>.Zero;
            }

            Logger.Log("----------------------------------------");
            Logger.Log("reloading Types.");
            foreach (Pointer<AbstractTypeClass> pItem in ShowProgress(AbstractTypeClass.ABSTRACTTYPE_ARRAY))
            {
                switch (pItem.Ref.Base.WhatAmI())
                {
                    case AbstractType.AnimType:
                    case AbstractType.AircraftType:
                    case AbstractType.BuildingType:
                    case AbstractType.BulletType:
                    case AbstractType.Campaign:
                    case AbstractType.HouseType:
                    case AbstractType.InfantryType:
                    case AbstractType.IsotileType:
                    case AbstractType.OverlayType:
                    case AbstractType.ParticleType:
                    case AbstractType.ParticleSystemType:
                    case AbstractType.Side:
                    case AbstractType.SmudgeType:
                    case AbstractType.SuperWeaponType:
                    case AbstractType.TagType:
                    case AbstractType.Tiberium:
                    case AbstractType.TriggerType:
                    case AbstractType.UnitType:
                    case AbstractType.VoxelAnimType:
                    case AbstractType.WeaponType:
                    case AbstractType.WarheadType:
                        //Logger.Log("{0} {1} is reloading.", pItem.Ref.Base.WhatAmI(), pItem.Ref.ID);
                        pItem.Ref.LoadFromINI(hRulesINI);
                        pItem.Ref.LoadFromINI(hGameModeINI);
                        pItem.Ref.LoadFromINI(hMapINI);
                        break;
                    case AbstractType.TerrainType: // which will get crash
                        Logger.Log("{0} {1} is skipped.", pItem.Ref.Base.WhatAmI(), pItem.Ref.ID);
                        break;
                }
            }

            Logger.Log("----------------------------------------");
            Logger.Log("reloading MissionControlClass.");
            var enumerator = new PointerEnumerator<MissionControlClass>(MissionControlClass.Array(), new Pointer<MissionControlClass>(0xA8E7A8));
            foreach (Pointer<MissionControlClass> pItem in ShowProgress(enumerator, enumerator.Count))
            {
                pItem.Ref.LoadFromINI(hRulesINI);
                pItem.Ref.LoadFromINI(hGameModeINI);
                pItem.Ref.LoadFromINI(hMapINI);
            }

            Logger.Log("----------------------------------------");
            Logger.Log("recheck tech tree.");
            foreach (Pointer<HouseClass> pHouse in ShowProgress(HouseClass.Array, HouseClass.Array.Count))
            {
                pHouse.Ref.RecheckTechTree = true;
            }

            Logger.Log("");
            Logger.Log("writing old art data back.");
            Helpers.Copy((IntPtr)memory.Memory, CCINIClass.INI_Art, memory.Size);
        }

        private static void ReloadAi()
        {
            using var hAiINI = CreateAndReadINI(INIConstant.AiName);
            using var hMapINI = CreateAndReadINI(INIConstant.MapName);

            Logger.Log("reloading Types.");
            foreach (Pointer<AbstractTypeClass> pItem in ShowProgress(AbstractTypeClass.ABSTRACTTYPE_ARRAY))
            {
                switch (pItem.Ref.Base.WhatAmI())
                {
                    case AbstractType.AITriggerType:
                    case AbstractType.ScriptType:
                    case AbstractType.TaskForce:
                    case AbstractType.TeamType:
                        pItem.Ref.LoadFromINI(hAiINI);
                        pItem.Ref.LoadFromINI(hMapINI);
                        break;
                }
            }
        }

        private static void OnINIChange(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath;
            string relPath = path.Replace(GlobalVars.RootDirectory, "").Replace(@"\", "/");

            if (ignoreList.Contains(relPath.ToLower()))
            {
                Logger.Log("ignore file: {0}", relPath);
                return;
            }

            Logger.Log("");
            Logger.LogWarning("experimental feature REALTIME_INI is working.");
            Logger.Log("detected file {0}: {1}", e.ChangeType, path);

            try
            {
                string iniName = FindMainINI(relPath);

                Logger.Log("stop game and wait for editor to release the handle of file");
                StopGame();
                Thread.Sleep(TimeSpan.FromSeconds(1.0));

                if (iniName.Equals(INIConstant.MapName, StringComparison.OrdinalIgnoreCase))
                {
                    ReloadRulesAndArt();
                    ReloadAi();
                }
                else if (iniName.Equals(INIConstant.AiName, StringComparison.OrdinalIgnoreCase))
                {
                    ReloadAi();
                }
                else if (iniName.Equals(INIConstant.ArtName, StringComparison.OrdinalIgnoreCase)
                    || iniName.Equals(INIConstant.RulesName, StringComparison.OrdinalIgnoreCase)
                    || iniName.Equals(INIConstant.GameModeName, StringComparison.OrdinalIgnoreCase))
                {
                    ReloadRulesAndArt();
                }
                else
                {
                    Logger.LogError("{0} unsupported by realtime ini", iniName);
                }

                Logger.Log("----------------------------------------");
                RefreshINIComponent();
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
            finally
            {
                ResumeGame();
            }

            Logger.Log("{0} reloaded.", path);
            Logger.LogWarning("new changes are only be applied to current game.");
            Logger.LogWarning("you may get crash in next game.");
        }

        static public void RefreshINIComponent()
        {
            Ini.ClearBuffer();

        }

        private static int oldSpeed = 1;
        private static void StopGame()
        {
            oldSpeed = GameOptionsClass.Instance.GameSpeed;
            GameOptionsClass.Instance.GameSpeed = 200;
        }
        private static void ResumeGame()
        {
            Pointer<int> curGameSpeed = new Pointer<int>(0x887350);
            curGameSpeed.Ref = 0;
            GameOptionsClass.Instance.GameSpeed = oldSpeed;
        }

        private static List<CodeWatcher> watchers = new();

        [Hook(HookType.WriteBytesHook, Address = 0x7E03E8, Size = 1)]
        static public unsafe byte[] Watch()
        {
            string watchDir = AppDomain.CurrentDomain.BaseDirectory;

            Logger.LogWithColor("realtime ini feature will be activated a bit later.", ConsoleColor.Magenta);
            Task.Delay(TimeSpan.FromSeconds(2.5)).ContinueWith(_ =>
            {
                // show active message
                Logger.LogWarning("************************************************************************");
                Logger.LogWarning("watching configure files ({1}) at directory {0}", watchDir, string.Join(", ", watchList));
                Logger.LogWarning("realtime ini feature only support rules, art, ai, game mode ini, map now");
                Logger.LogWarning("all other ini will be ignored");
                Logger.LogWarning("this is a experimental feature that is not perfect");
                Logger.LogWarning("remove preprocessor_symbols->REALTIME_INI if unwanted.");
                Logger.LogWarning("************************************************************************");

                foreach (var filter in watchList)
                {
                    var watcher = new CodeWatcher(AppDomain.CurrentDomain.BaseDirectory, filter);

                    watcher.OnCodeChanged += OnINIChange;
                    watcher.StartWatchPath();

                    watchers.Add(watcher);
                }
                
                EnsureOneInstance();
            });

            return new byte[] { 0 };
        }

        private static void EnsureOneInstance()
        {
            const string idFile = "DynamicLoadINI.id";
            string myId = Guid.NewGuid().ToString();
            File.WriteAllText(idFile, myId);

            string curId;
            do
            {
                Thread.Sleep(1000);
                curId = File.ReadAllText(idFile);

            } while (curId == myId);

            Logger.Log("new INI watcher detected, stop old INI watcher.");
            watchers.ForEach(w => w.Stop());
            watchers.Clear();
        }
#endif
    }
}