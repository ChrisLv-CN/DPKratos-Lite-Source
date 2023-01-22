using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class AnimStatusScript
    {
        private IConfigWrapper<PaintballData> _paintballData;
        private PaintballData paintballData
        {
            get
            {
                if (null == _paintballData)
                {
                    _paintballData = Ini.GetConfig<PaintballData>(Ini.ArtDependency, section);
                }
                return _paintballData.Data;
            }
        }

        public unsafe void AnimClass_DrawSHP_Paintball(REGISTERS* R)
        {
            if (paintballData.Enable)
            {
                if (default != paintballData.Color)
                {
                    R->EBP = paintballData.GetColor();
                }
                if (1.0f != paintballData.BrightMultiplier)
                {
                    uint bright = R->Stack<uint>(0x38);
                    R->Stack<uint>(0x38, paintballData.GetBright(bright));
                }
            }
        }


    }
}
