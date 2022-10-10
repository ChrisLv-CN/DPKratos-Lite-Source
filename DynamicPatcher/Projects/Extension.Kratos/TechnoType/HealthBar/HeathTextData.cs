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
    public enum HealthTextStyle
    {
        AUTO = 0, FULL = 1, SHORT = 2, PERCENT = 3
    }
    public class HealthTextStyleParser : KEnumParser<HealthTextStyle>
    {
        public override bool ParseInitials(string t, ref HealthTextStyle buffer)
        {
            switch (t)
            {
                case "F":
                    buffer = HealthTextStyle.FULL;
                    return true;
                case "S":
                    buffer = HealthTextStyle.SHORT;
                    return true;
                case "P":
                    buffer = HealthTextStyle.PERCENT;
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public enum HealthTextAlign
    {
        LEFT = 0, CENTER = 1, RIGHT = 2
    }
    public class HealthTextAlignParser : KEnumParser<HealthTextAlign>
    {
        public override bool ParseInitials(string t, ref HealthTextAlign buffer)
        {
            switch (t)
            {
                case "L":
                    buffer = HealthTextAlign.LEFT;
                    return true;
                case "C":
                    buffer = HealthTextAlign.CENTER;
                    return true;
                case "R":
                    buffer = HealthTextAlign.RIGHT;
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public class HealthText : PrintTextData
    {
        public bool Hidden;
        public bool ShowEnemy;
        public bool ShowHover;
        public HealthTextStyle Style;
        public HealthTextStyle HoverStyle;
        public HealthTextAlign Align;

        static HealthText()
        {
            new HealthTextStyleParser().Register();
            new HealthTextAlignParser().Register();
        }

        public HealthText(HealthState healthState) : base()
        {
            this.Hidden = false;
            this.ShowEnemy = false;
            this.ShowHover = false;
            this.Style = HealthTextStyle.FULL;
            this.HoverStyle = HealthTextStyle.SHORT;
            this.Align = HealthTextAlign.LEFT;

            this.SHPFileName = "pipsnum.shp";
            this.ImageSize = new Point2D(5, 8);
            switch (healthState)
            {
                case HealthState.Green:
                    this.Color = new ColorStruct(0, 252, 0);
                    this.ZeroFrameIndex = 0;
                    break;
                case HealthState.Yellow:
                    this.Color = new ColorStruct(252, 212, 0);
                    this.ZeroFrameIndex = 15;
                    break;
                case HealthState.Red:
                    this.Color = new ColorStruct(252, 0, 0);
                    this.ZeroFrameIndex = 30;
                    break;
            }
        }

        public HealthText Clone()
        {
            HealthText data = new HealthText(HealthState.Green);
            CopyTo(data);
            data.Hidden = this.Hidden;
            data.ShowEnemy = this.ShowEnemy;
            data.ShowHover = this.ShowHover;
            data.Style = this.Style;
            data.HoverStyle = this.HoverStyle;
            data.Align = this.Align;
            return data;
        }

        public override void Read(ISectionReader reader, string title)
        {
            base.Read(reader, title);
            this.Hidden = reader.Get(title + "Hidden", Hidden);
            this.ShowEnemy = reader.Get(title + "ShowEnemy", ShowEnemy);
            this.ShowHover = reader.Get(title + "ShowHover", ShowHover);
            this.Style = reader.Get(title + "Style", Style);
            this.HoverStyle = reader.Get(title + "HoverStyle", HoverStyle);
            this.Align = reader.Get(title + "Align", Align);
        }

    }

    [Serializable]
    public class HealthTextData
    {

        public const string TITLE = "HealthText.";

        public bool Hidden;

        public HealthText Green;
        public HealthText Yellow;
        public HealthText Red;

        public HealthTextData() { }

        public HealthTextData(AbstractType type)
        {
            this.Hidden = false;
            this.Green = new HealthText(HealthState.Green);
            this.Yellow = new HealthText(HealthState.Yellow);
            this.Red = new HealthText(HealthState.Red);
            switch (type)
            {
                case AbstractType.Building:
                    Green.Style = HealthTextStyle.FULL;
                    Yellow.Style = HealthTextStyle.FULL;
                    Red.Style = HealthTextStyle.FULL;
                    break;
                case AbstractType.Infantry:
                    Green.Style = HealthTextStyle.SHORT;
                    Yellow.Style = HealthTextStyle.SHORT;
                    Red.Style = HealthTextStyle.SHORT;
                    break;
                case AbstractType.Unit:
                    Green.Style = HealthTextStyle.FULL;
                    Yellow.Style = HealthTextStyle.FULL;
                    Red.Style = HealthTextStyle.FULL;
                    break;
                case AbstractType.Aircraft:
                    Green.Style = HealthTextStyle.FULL;
                    Yellow.Style = HealthTextStyle.FULL;
                    Red.Style = HealthTextStyle.FULL;
                    break;
            }
        }

        public HealthTextData Clone()
        {
            HealthTextData data = new HealthTextData();
            data.Hidden = this.Hidden;
            data.Green = this.Green.Clone();
            data.Yellow = this.Yellow.Clone();
            data.Red = this.Red.Clone();
            return data;
        }

        public void Read(ISectionReader reader, string title = TITLE)
        {
            this.Hidden = reader.Get(title + "Hidden", Hidden);
            // Phobos
            this.Hidden = reader.Get("HealthBar.Hide", Hidden);

            this.Green.Read(reader, title);
            this.Yellow.Read(reader, title);
            this.Red.Read(reader, title);

            this.Green.Read(reader, title + "Green.");
            this.Yellow.Read(reader, title + "Yellow.");
            this.Red.Read(reader, title + "Red.");
        }

    }
}
