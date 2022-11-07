using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;

namespace Extension.Utilities
{

    [Serializable]
    public class RadialFireHelper
    {

        private int burst;

        private double dirRad = 0;
        private double spliteRad = 0;
        private int delta = 0;
        private float deltaZ = 0;

        public RadialFireHelper(Pointer<TechnoClass> pTechno, int burst, int splitAngle)
        {
            this.burst = burst;

            DirStruct dir = pTechno.Ref.Facing.target();
            if (pTechno.Ref.HasTurret())
            {
                dir = pTechno.Ref.GetTurretFacing().target();
            }

            InitData(dir, splitAngle);
        }

        public RadialFireHelper(DirStruct dir, int burst, int splitAngle)
        {
            this.burst = burst;

            InitData(dir, splitAngle);
        }

        private void InitData(DirStruct dir, int splitAngle)
        {
            // degrees = MathEx.Rad2Deg(dir.radians()) - 180 + (splitAngle * 0.5);
            dirRad = dir.radians();
            spliteRad = MathEx.Deg2Rad(splitAngle * 0.5); // Deg2Rad是逆时针
            // Logger.Log($"{Game.CurrentFrame} 扇形分布 degrees = {degrees}  dir = {MathEx.Rad2Deg(dir.radians())}, splitAngle = {splitAngle}");
            delta = splitAngle / (burst + 1);
            deltaZ = 1f / (burst / 2f + 1);
        }

        public BulletVelocity GetBulletVelocity(int index, bool radialZ)
        {
            int z = 0;
            float temp = burst / 2f;
            if (radialZ)
            {
                if (index - temp < 0)
                {
                    z = index;
                }
                else
                {
                    z = Math.Abs(index - burst + 1);
                }
            }
            int angle = delta * (index + 1);
            // Logger.Log($"{Game.CurrentFrame} {index} - Burst = {burst}, DirRad = {dirRad}, Delta = {delta}, DeltaZ = {deltaZ}, Angle = {angle}, Z = {z}");
            // double radians = FLHHelper.DirNormalized(angle + 90, 360).radians(); // 0的方位在右下角，Matrix3D的0方位在右上角，所以需要+90度
            double radians = MathEx.Deg2Rad(angle); // 逆时针
            Matrix3DStruct matrix3D = new Matrix3DStruct(true);
            matrix3D.RotateZ((float)dirRad); // 转到单位朝向
            matrix3D.RotateZ((float)spliteRad); // 逆时针转到发射角
            matrix3D.RotateZ(-(float)radians); // 顺时针发射
            matrix3D.Translate(256, 0, 0);
            SingleVector3D offset = Game.MatrixMultiply(matrix3D);
            return new BulletVelocity(offset.X, -offset.Y, deltaZ * z);
        }

    }
}

