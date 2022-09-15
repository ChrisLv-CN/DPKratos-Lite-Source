
using PatcherYRpp;


namespace Extension.INI
{
    internal partial class YRParsers
    {
        public static partial void RegisterFindTypeParsers()
        {
            new BulletTypeClassParser().Register();
            new HouseTypeClassParser().Register();
            new SuperWeaponTypeClassParser().Register();
            new TechnoTypeClassParser().Register();
            new WeaponTypeClassParser().Register();
            new WarheadTypeClassParser().Register();
        }
    }
    
    public class BulletTypeClassParser : FindTypeParser<BulletTypeClass>
    {
        public BulletTypeClassParser() : base(BulletTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
    public class HouseTypeClassParser : FindTypeParser<HouseTypeClass>
    {
        public HouseTypeClassParser() : base(HouseTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
    public class SuperWeaponTypeClassParser : FindTypeParser<SuperWeaponTypeClass>
    {
        public SuperWeaponTypeClassParser() : base(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
    public class TechnoTypeClassParser : FindTypeParser<TechnoTypeClass>
    {
        public TechnoTypeClassParser() : base(TechnoTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
    public class WeaponTypeClassParser : FindTypeParser<WeaponTypeClass>
    {
        public WeaponTypeClassParser() : base(WeaponTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
    public class WarheadTypeClassParser : FindTypeParser<WarheadTypeClass>
    {
        public WarheadTypeClassParser() : base(WarheadTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
}
