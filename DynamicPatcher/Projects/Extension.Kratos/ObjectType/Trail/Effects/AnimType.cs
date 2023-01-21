using DynamicPatcher;
using Extension.INI;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.Ext
{

    public partial class TrailType
    {
        public string[] StartDrivingAnim;
        public string[] WhileDrivingAnim;
        public string[] StopDrivingAnim;

        private void InitAnimType()
        {
            StartDrivingAnim = null;
            WhileDrivingAnim = null;
            StopDrivingAnim = null;
        }

        private void ReadAnimType(ISectionReader reader)
        {
            this.StartDrivingAnim = reader.GetList("Anim.Start", StartDrivingAnim);
            this.WhileDrivingAnim = reader.GetList("Anim.While", WhileDrivingAnim);
            this.StopDrivingAnim = reader.GetList("Anim.Stop", StopDrivingAnim);
        }
    }


}