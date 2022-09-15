
using System;
using System.Collections.Generic;
using System.Linq;

namespace PatcherYRpp
{
	internal static class CompileTimeCheck
	{
		private static void Check<T>() where T : unmanaged { }

		private static void CheckAll() 
		{
			Check<AbstractClass>();
			Check<AbstractTypeClass>();
			Check<AircraftClass>();
			Check<AircraftTypeClass>();
			Check<AnimClass>();
			Check<AnimTypeClass>();
			Check<BounceClass>();
			Check<BuildingClass>();
			Check<BuildingTypeClass>();
			Check<BulletClass>();
			Check<BulletTypeClass>();
			Check<CaptureManagerClass>();
			Check<CCFileClass>();
			Check<CCINIClass>();
			Check<CellClass>();
			Check<ConvertClass>();
			Check<FactoryClass>();
			Check<FlyLocomotionClass>();
			Check<FootClass>();
			Check<GameOptionsClass>();
			Check<HouseClass>();
			Check<HouseTypeClass>();
			Check<InfantryClass>();
			Check<InfantryTypeClass>();
			Check<LaserDrawClass>();
			Check<LocomotionClass>();
			Check<MapClass>();
			Check<MissionClass>();
			Check<MixFileClass>();
			Check<MPGameModeClass>();
			Check<ObjectClass>();
			Check<ObjectTypeClass>();
			Check<OverlayTypeClass>();
			Check<ParticleSystemTypeClass>();
			Check<RadioClass>();
			Check<RulesClass>();
			Check<ScenarioClass>();
			Check<SessionClass>();
			Check<SideClass>();
			Check<SuperClass>();
			Check<SuperWeaponTypeClass>();
			Check<SwizzleManagerClass>();
			Check<TacticalClass>();
			Check<TagClass>();
			Check<TechnoClass>();
			Check<TechnoTypeClass>();
			Check<TerrainClass>();
			Check<TerrainTypeClass>();
			Check<UnitClass>();
			Check<UnitTypeClass>();
			Check<WarheadTypeClass>();
			Check<WeaponTypeClass>();
			Check<Surface>();
			Check<DynamicVectorClass<int>>();
			Check<Pointer<int>>();
		}
	}
}