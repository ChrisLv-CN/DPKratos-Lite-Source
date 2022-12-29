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
    public enum LongText
    {
        NONE = 0, HIT = 1, MISS = 2, CRIT = 3, GLANCING = 4, BLOCK = 5
    }

    [Serializable]
    public class PrintText
    {
        public string Text;
        public CoordStruct Location;
        public Point2D Offset;
        public int Duration;
        public TimerStruct LifeTimer;
        public PrintTextData Data;

        public PrintText(string text, CoordStruct location, Point2D offset, int duration, PrintTextData data)
        {
            this.Text = text;
            this.Location = location;
            this.Offset = offset;
            this.Duration = duration;
            this.LifeTimer.Start(duration);
            this.Data = data;
        }

        public bool CanPrint(out Point2D offset, out Point2D pos, out RectangleStruct bound)
        {
            offset = this.Offset;
            pos = default;
            bound = default;
            if (LifeTimer.InProgress() && !LocationOutOfViewOrHiddenInFog(out pos, out bound))
            {
                return true;
            }
            return false;
        }

        public bool LocationOutOfViewOrHiddenInFog(out Point2D pos, out RectangleStruct bound)
        {
            // 视野外
            pos = TacticalClass.Instance.Ref.CoordsToClient(Location);
            bound = Surface.Composite.Ref.GetRect();
            if (pos.X < 0 || pos.Y < 0 || pos.X > bound.Width || pos.Y > bound.Height - 32)
            {
                return true;
            }
            // 迷雾下
            if (MapClass.Instance.TryGetCellAt(Location, out Pointer<CellClass> pCell))
            {
                return !pCell.Ref.Flags.HasFlag(CellFlags.Revealed);
            }
            return false;
        }
    }

    [Serializable]
    public enum PrintTextAlign
    {
        LEFT = 0, CENTER = 1, RIGHT = 2
    }
    public class PrintTextAlignParser : KEnumParser<PrintTextAlign>
    {
        public override bool ParseInitials(string t, ref PrintTextAlign buffer)
        {
            switch (t)
            {
                case "L":
                    buffer = PrintTextAlign.LEFT;
                    return true;
                case "C":
                    buffer = PrintTextAlign.CENTER;
                    return true;
                case "R":
                    buffer = PrintTextAlign.RIGHT;
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public enum SHPDrawStyle
    {
        NUMBER = 0,
        TEXT = 1, // 固定帧
        PROGRESS = 2 // 进度条
    }

    [Serializable]
    public class PrintTextData
    {

        static PrintTextData()
        {
            new PrintTextAlignParser().Register();
        }

        public Point2D Offset;
        public Point2D ShadowOffset;
        public ColorStruct Color;
        public ColorStruct ShadowColor;
        public bool IsHouseColor;

        public PrintTextAlign Align;

        public bool UseSHP;
        public SHPDrawStyle SHPDrawStyle; // 使用哪个帧来渲染
        public string SHPFileName;
        public int ZeroFrameIndex;
        public int MaxFrameIndex;
        public Point2D ImageSize;
        public int Warp;

        public bool NoNumbers; // 不使用数字
        // long text
        public string HitSHP;
        public int HitIndex;
        public string MissSHP;
        public int MissIndex;
        public string CritSHP;
        public int CritIndex;
        public string GlancingSHP;
        public int GlancingIndex;
        public string BlockSHP;
        public int BlockIndex;

        public PrintTextData()
        {
            this.Offset = new Point2D(0, 0);
            this.ShadowOffset = new Point2D(1, 1);
            this.Color = new ColorStruct(252, 252, 252);
            this.ShadowColor = new ColorStruct(82, 85, 82);
            this.IsHouseColor = false;

            this.Align = PrintTextAlign.LEFT;

            this.UseSHP = false;
            this.SHPDrawStyle = SHPDrawStyle.NUMBER;
            this.SHPFileName = "pipsnum.shp";
            this.ZeroFrameIndex = 0;
            this.MaxFrameIndex = -1;
            this.ImageSize = new Point2D(5, 8);
            this.Warp = 1;

            this.NoNumbers = false;
            // long text
            this.HitSHP = "pipstext.shp";
            this.HitIndex = 0;
            this.MissSHP = "pipstext.shp";
            this.MissIndex = 2;
            this.CritSHP = "pipstext.shp";
            this.CritIndex = 3;
            this.GlancingSHP = "pipstext.shp";
            this.GlancingIndex = 4;
            this.BlockSHP = "pipstext.shp";
            this.BlockIndex = 5;
        }

        public PrintTextData CopyTo(PrintTextData data)
        {
            data.Offset = this.Offset;
            data.ShadowOffset = this.ShadowOffset;
            data.Color = this.Color;
            data.ShadowColor = this.ShadowColor;
            data.IsHouseColor = this.IsHouseColor;

            data.Align = this.Align;

            data.UseSHP = this.UseSHP;
            data.SHPDrawStyle = this.SHPDrawStyle;
            data.SHPFileName = this.SHPFileName;
            data.ZeroFrameIndex = this.ZeroFrameIndex;
            data.MaxFrameIndex = this.MaxFrameIndex;
            data.ImageSize = this.ImageSize;
            data.Warp = this.Warp;

            data.NoNumbers = this.NoNumbers;
            data.HitSHP = this.HitSHP;
            data.HitIndex = this.HitIndex;
            data.MissSHP = this.MissSHP;
            data.MissIndex = this.MissIndex;
            data.CritSHP = this.CritSHP;
            data.CritIndex = this.CritIndex;
            data.GlancingSHP = this.GlancingSHP;
            data.GlancingIndex = this.GlancingIndex;
            data.BlockSHP = this.BlockSHP;
            data.BlockIndex = this.BlockIndex;
            return data;
        }


        public virtual void Read(ISectionReader reader, string title)
        {
            this.Offset = reader.Get(title + "Offset", Offset);
            this.ShadowOffset = reader.Get(title + "ShadowOffset", ShadowOffset);
            this.Color = reader.Get(title + "Color", Color);
            this.ShadowColor = reader.Get(title + "ShadowColor", ShadowColor);
            this.IsHouseColor = reader.Get(title + "IsHouseColor", IsHouseColor);

            this.Align = reader.Get(title + "Align", Align);

            this.UseSHP = reader.Get(title + "UseSHP", UseSHP);
            this.SHPFileName = reader.Get(title + "SHP", SHPFileName);
            this.ZeroFrameIndex = reader.Get(title + "ZeroFrameIndex", ZeroFrameIndex);
            this.MaxFrameIndex = reader.Get(title + "MaxFrameIndex", MaxFrameIndex);
            this.ImageSize = reader.Get(title + "ImageSize", ImageSize);
            this.Warp = reader.Get(title + "Warp", Warp);

            this.NoNumbers = reader.Get(title + "NoNumbers", NoNumbers);
            // long text
            this.HitSHP = reader.Get(title + "HIT.SHP", HitSHP);
            this.HitIndex = reader.Get(title + "HIT.Index", HitIndex);
            this.MissSHP = reader.Get(title + "MISS.SHP", MissSHP);
            this.MissIndex = reader.Get(title + "MISS.Index", MissIndex);
            this.CritSHP = reader.Get(title + "CRIT.SHP", CritSHP);
            this.CritIndex = reader.Get(title + "CRIT.Index", CritIndex);
            this.GlancingSHP = reader.Get(title + "GLANCING.SHP", GlancingSHP);
            this.GlancingIndex = reader.Get(title + "GLANCING.Index", GlancingIndex);
            this.BlockSHP = reader.Get(title + "BLOCK.SHP", BlockSHP);
            this.BlockIndex = reader.Get(title + "BLOCK.Index", BlockIndex);
        }

    }

}
