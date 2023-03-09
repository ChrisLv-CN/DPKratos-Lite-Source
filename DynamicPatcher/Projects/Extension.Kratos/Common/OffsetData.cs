using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class OffsetData : INIConfig
    {
        public CoordStruct Offset; // 偏移FLH

        public CoordStruct StackOffset; // 堆叠偏移
        public int StackGroup; // 分组堆叠
        public CoordStruct StackGroupOffset; // 分组堆叠偏移

        public bool IsOnTurret; // 相对炮塔或者身体
        public bool IsOnWorld; // 相对世界

        public int Direction; // 相对朝向，16分圆，[0-15]

        public OffsetData()
        {
            this.Offset = default;

            this.StackOffset = default;
            this.StackGroup = -1;
            this.StackGroupOffset = default;

            this.IsOnTurret = false;
            this.IsOnWorld = false;

            this.Direction = 0;
        }

        public override void Read(IConfigReader reader) { }

        public virtual void Read(ISectionReader reader, string title)
        {
            this.Offset = reader.Get(title + "Offset", this.Offset);

            this.StackOffset = reader.Get(title + "StackOffset", this.StackOffset);
            this.StackGroup = reader.Get(title + "StackGroup", this.StackGroup);
            this.StackGroupOffset = reader.Get(title + "StackGroupOffset", this.StackGroupOffset);

            this.IsOnTurret = reader.Get(title + "IsOnTurret", this.IsOnTurret);
            this.IsOnWorld = reader.Get(title + "IsOnWorld", this.IsOnWorld);

            this.Direction = reader.GetDir16(title + "Direction", this.Direction);
        }

    }

}
