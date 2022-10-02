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
    public class AnimVisibilityData : INIConfig
    {
        static AnimVisibilityData()
        {
            new RelationParser().Register();
        }

        public Relation Visibility;

        public AnimVisibilityData()
        {
            this.Visibility = Relation.All;
        }

        public override void Read(IConfigReader reader)
        {
            this.Visibility = reader.Get("Visibility", this.Visibility);
        }

    }


}
