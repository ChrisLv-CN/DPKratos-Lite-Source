using DynamicPatcher;
using PatcherYRpp;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Ext
{
    public partial class TrailType
    {
        public BoltType BoltType;

        private void InitElectricType()
        {
            BoltType = new BoltType(false);
        }

        private void ReadElectricType(ISectionReader reader)
        {
            this.BoltType.IsAlternateColor = reader.Get("Electric.IsAlternateColor", this.BoltType.IsAlternateColor);

            this.BoltType.Color1 = reader.Get("Bolt.Color1", this.BoltType.Color1);
            this.BoltType.Color2 = reader.Get("Bolt.Color2", this.BoltType.Color2);
            this.BoltType.Color3 = reader.Get("Bolt.Color3", this.BoltType.Color3);

            this.BoltType.Disable1 = reader.Get("Bolt.Disable1", this.BoltType.Disable1);
            this.BoltType.Disable2 = reader.Get("Bolt.Disable2", this.BoltType.Disable2);
            this.BoltType.Disable3 = reader.Get("Bolt.Disable3", this.BoltType.Disable3);
        }
    }
}
