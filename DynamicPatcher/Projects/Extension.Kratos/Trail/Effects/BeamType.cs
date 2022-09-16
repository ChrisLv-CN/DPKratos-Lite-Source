using DynamicPatcher;
using PatcherYRpp;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Ext
{
    public partial class TrailType
    {
        public BeamType BeamType;

        private void InitBeamType()
        {
            BeamType = new BeamType(RadBeamType.RadBeam);
        }

        private void ReadBeamType(ISectionReader reader)
        {
            this.BeamType.BeamColor = reader.Get("Bean.Color", this.BeamType.BeamColor);
            this.BeamType.Period = reader.Get("Beam.Period", this.BeamType.Period);
            this.BeamType.Amplitude = reader.Get("Beam.Amplitude", this.BeamType.Amplitude);
        }
    }
}
