using DynamicPatcher;
using Extension.INI;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.Ext
{

    public partial class TrailType
    {
        public string StartDrivingAnim;
        public string WhileDrivingAnim;
        public string StopDrivingAnim;

        private void InitAnimType()
        {
            StartDrivingAnim = null;
            WhileDrivingAnim = null;
            StopDrivingAnim = null;
        }

        private void ReadAnimType(ISectionReader reader)
        {
            this.StartDrivingAnim = reader.Get<string>("Anim.Start", StartDrivingAnim).NotNONE();
            this.WhileDrivingAnim = reader.Get<string>("Anim.While", WhileDrivingAnim).NotNONE();
            this.StopDrivingAnim = reader.Get<string>("Anim.Stop", StopDrivingAnim).NotNONE();
        }
    }


}