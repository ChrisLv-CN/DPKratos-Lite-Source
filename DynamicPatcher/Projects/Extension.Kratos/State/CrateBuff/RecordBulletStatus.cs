using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class RecordBulletStatus
    {
        public int Health;
        public int Speed;
        public BulletVelocity Velocity;
        public bool CourseLocked;

        public RecordBulletStatus(int health, int speed, BulletVelocity velocity, bool courseLocked)
        {
            this.Health = health;
            this.Speed = speed;
            this.Velocity = velocity;
            this.CourseLocked = courseLocked;
        }

        public override string ToString()
        {
            return string.Format("{{\"Health\":{0}, \"Speed\":{1}, \"Velocity\":{2}, \"CourseLocked\":{3}}}", Health, Speed, Velocity, CourseLocked);
        }
    }

}
