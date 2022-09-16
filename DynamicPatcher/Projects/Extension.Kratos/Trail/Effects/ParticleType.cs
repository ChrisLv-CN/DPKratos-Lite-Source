using DynamicPatcher;
using PatcherYRpp;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class TrailType
    {
        public string ParticleSystem;

        private void InitParticleType()
        {
            ParticleSystem = null;
        }

        private void ReadParticleType(ISectionReader reader)
        {
            this.ParticleSystem = reader.Get("ParticleSystem", this.ParticleSystem).NotNONE();
        }
    }
}
