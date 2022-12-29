using System;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class AttachEffectData
    {
        public InfoData InfoData;

        private void ReadInfoData(IConfigReader reader)
        {
            InfoData data = new InfoData(reader);
            if (data.Enable)
            {
                this.InfoData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public enum InfoMode
    {
        NONE = 0, TEXT = 1, SHP = 2, IMAGE = 3
    }

    public class InfoModeParser : KEnumParser<InfoMode>
    {
        public override bool ParseInitials(string t, ref InfoMode buffer)
        {
            switch (t)
            {
                case "T":
                    buffer = InfoMode.TEXT;
                    return true;
                case "S":
                    buffer = InfoMode.SHP;
                    return true;
                case "I":
                    buffer = InfoMode.IMAGE;
                    return true;
                default:
                    buffer = InfoMode.NONE;
                    return true;
            }
        }
    }

    [Serializable]
    public class InfoEntity : PrintTextData
    {
        static InfoEntity()
        {
            new InfoModeParser().Register();
            new PrintTextAlignParser().Register();
        }

        public InfoMode Mode;
        public bool ShowEnemy;
        public bool OnlySelected;

        public InfoEntity() : base()
        {
            this.Mode = InfoMode.NONE;
            this.ShowEnemy = true;
            this.OnlySelected = false;

            this.Align = PrintTextAlign.CENTER;
            this.Color = new ColorStruct(0, 252, 0);
        }

        public override void Read(ISectionReader reader, string title)
        {
            base.Read(reader, title);
            this.Mode = reader.Get(title + "Mode", Mode);
            switch(Mode)
            {
                case InfoMode.SHP:
                    this.UseSHP = true;
                    break;
                case InfoMode.IMAGE:
                    this.UseSHP = true;
                    this.SHPDrawStyle = SHPDrawStyle.PROGRESS;
                    break;
            }
            this.ShowEnemy = reader.Get(title + "ShowEnemy", ShowEnemy);
            this.OnlySelected = reader.Get(title + "OnlySelected", OnlySelected);
        }
    }

    [Serializable]
    public class InfoData : EffectData
    {

        public const string TITLE = "Info.";

        public string Watch;

        public InfoEntity DurationInfo;
        public InfoEntity StackInfo;


        public InfoData()
        {
            this.Watch = null;
            this.DurationInfo = new InfoEntity();
            this.StackInfo = new InfoEntity();
        }

        public InfoData(IConfigReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Watch = reader.Get(TITLE + "Watch", this.Watch);

            this.DurationInfo.Read(reader, TITLE + "Duration.");
            this.StackInfo.Read(reader, TITLE + "Stack.");

            this.Enable = DurationInfo.Mode != InfoMode.NONE || StackInfo.Mode != InfoMode.NONE;
        }

    }


}
