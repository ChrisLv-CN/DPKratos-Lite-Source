using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Ext
{

    public class AudioVisual
    {
        private static IConfigWrapper<AudioVisualData> _data;
        public static AudioVisualData Data
        {
            get
            {
                if (null == _data)
                {
                    _data = Ini.GetConfig<AudioVisualData>(Ini.RulesDependency, RulesClass.SectionAudioVisual);
                }
                return _data.Data;
            }
        }

        public static void Reload(object sender, EventArgs e)
        {
            _data = null;
        }
    }
}
