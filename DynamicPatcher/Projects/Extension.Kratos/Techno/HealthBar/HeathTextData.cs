using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using System.Threading.Tasks;
using Extension.INI;

namespace Extension.Ext
{
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
    public class HealthTextData : PrintTextData
    {
        public bool Hidden;
        public bool ShowEnemy;
        public bool ShowHover;
        public HealthTextStyle Style;
        public HealthTextStyle HoverStyle;
        public HealthTextAlign Align;

        static HealthTextData()
        {
            new HealthTextStyleParser().Register();
            new HealthTextAlignParser().Register();
        }

        public HealthTextData(HealthState healthState) : base()
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

        public HealthTextData Clone()
        {
            HealthTextData data = new HealthTextData(HealthState.Green);
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

        [Obsolete]
        private HealthTextStyle ReadStyle(string text, HealthTextStyle defStyle)
        {
            string t = text.Substring(0, 1).ToUpper();
            switch (t)
            {
                case "F":
                    return HealthTextStyle.FULL;
                case "S":
                    return HealthTextStyle.SHORT;
                case "P":
                    return HealthTextStyle.PERCENT;
            }
            return defStyle;
        }

        [Obsolete]
        public bool TryReadHealthText(INIReader reader, string section, string title)
        {
            bool isRead = false;


            bool hidden = false;
            if (reader.Read(section, title + "Hidden", ref hidden))
            {
                isRead = true;
                this.Hidden = hidden;
            }

            if (!Hidden)
            {

                isRead = TryReadPrintText(reader, section, title);

                bool enemy = false;
                if (reader.Read(section, title + "ShowEnemy", ref enemy))
                {
                    isRead = true;
                    this.ShowEnemy = enemy;
                }

                bool hover = false;
                if (reader.Read(section, title + "ShowHover", ref hover))
                {
                    isRead = true;
                    this.ShowHover = hover;
                }

                string s = null;
                if (reader.Read(section, title + "Style", ref s))
                {
                    isRead = true;
                    HealthTextStyle style = ReadStyle(s, HealthTextStyle.FULL);
                    this.Style = style;
                }

                string h = null;
                if (reader.Read(section, title + "HoverStyle", ref h))
                {
                    isRead = true;
                    HealthTextStyle style = ReadStyle(h, HealthTextStyle.SHORT);
                    this.HoverStyle = style;
                }

                string a = null;
                if (reader.Read(section, title + "Align", ref a))
                {
                    isRead = true;
                    string t = a.Substring(0, 1).ToUpper();
                    switch (t)
                    {
                        case "L":
                            this.Align = HealthTextAlign.LEFT;
                            break;
                        case "C":
                            this.Align = HealthTextAlign.CENTER;
                            break;
                        case "R":
                            this.Align = HealthTextAlign.RIGHT;
                            break;
                    }
                }
            }

            return isRead;
        }

    }

}