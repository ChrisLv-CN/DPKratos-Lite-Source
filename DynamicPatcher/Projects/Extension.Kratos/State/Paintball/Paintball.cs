using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class AttachEffect
    {
        public Paintball Paintball;

        private void InitPaintball()
        {
            this.Paintball = AEData.PaintballData.CreateEffect<Paintball>();
            RegisterEffect(Paintball);
        }
    }


    [Serializable]
    public class Paintball : StateEffect<Paintball, PaintballData>
    {
        private PaintballData paintballData;

        public override State<PaintballData> GetState(TechnoStatusScript statusScript)
        {
            ColorStruct color = Data.IsHouseColor ? AE.pSourceHouse.Pointer.Ref.LaserColor : Data.Color;
            paintballData = new PaintballData();
            paintballData.Color = color;
            paintballData.BrightMultiplier = Data.BrightMultiplier;
            // Logger.Log($"{Game.CurrentFrame} 设置 染色 = {paintballData.Color}, 明暗 = {paintballData.BrightMultiplier}, Token = {Token}");
            return statusScript.PaintballState;
        }

        public override State<PaintballData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

        public override IStateData GetData()
        {
            return paintballData;
        }

    }
}
