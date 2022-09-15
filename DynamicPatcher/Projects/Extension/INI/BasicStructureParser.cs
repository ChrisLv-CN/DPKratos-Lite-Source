using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    public abstract class BasicStructureParser<T> : IParserRegister, IParser<T>
    {
        public abstract bool Parse(string val, ref T buffer);

        public void Register()
        {
            Parsers.AddParser<T>(this);
        }
        public void Unregister()
        {
            Parsers.RemoveParser<T>();
        }
    }

    public class ColorStructParser : BasicStructureParser<ColorStruct>
    {
        public override bool Parse(string val, ref ColorStruct buffer)
        {
            var tmp = new byte[3];
            if (Parsers.GetParser<byte>().ParseArray(val, ref tmp))
            {
                buffer.R = tmp[0];
                buffer.G = tmp[1];
                buffer.B = tmp[2];
                return true;
            }

            return false;
        }
    }
    public class CoordStructParser : BasicStructureParser<CoordStruct>
    {
        public override bool Parse(string val, ref CoordStruct buffer)
        {
            var tmp = new int[3];
            if (Parsers.GetParser<int>().ParseArray(val, ref tmp))
            {
                buffer.X = tmp[0];
                buffer.Y = tmp[1];
                buffer.Z = tmp[2];
                return true;
            }

            return false;
        }
    }
    public class CellStructParser : BasicStructureParser<CellStruct>
    {
        public override bool Parse(string val, ref CellStruct buffer)
        {
            var tmp = new short[2];
            if (Parsers.GetParser<short>().ParseArray(val, ref tmp))
            {
                buffer.X = tmp[0];
                buffer.Y = tmp[1];
                return true;
            }

            return false;
        }
    }
    public class Point2DParser : BasicStructureParser<Point2D>
    {
        public override bool Parse(string val, ref Point2D buffer)
        {
            var tmp = new int[2];
            if (Parsers.GetParser<int>().ParseArray(val, ref tmp))
            {
                buffer.X = tmp[0];
                buffer.Y = tmp[1];
                return true;
            }

            return false;
        }
    }
    public class RectangleStructParser : BasicStructureParser<RectangleStruct>
    {
        public override bool Parse(string val, ref RectangleStruct buffer)
        {
            var tmp = new int[4];
            if (Parsers.GetParser<int>().ParseArray(val, ref tmp))
            {
                buffer.X = tmp[0];
                buffer.Y = tmp[1];
                buffer.Width = tmp[2];
                buffer.Height = tmp[3];
                return true;
            }

            return false;
        }
    }
    public class TintStructParser : BasicStructureParser<TintStruct>
    {
        public override bool Parse(string val, ref TintStruct buffer)
        {
            var tmp = new int[3];
            if (Parsers.GetParser<int>().ParseArray(val, ref tmp))
            {
                buffer.Red = tmp[0];
                buffer.Green = tmp[1];
                buffer.Blue = tmp[2];
                return true;
            }

            return false;
        }
    }

    internal partial class YRParsers
    {
        public static partial void RegisterBasicStructureParsers()
        {
            new ColorStructParser().Register();
            new CoordStructParser().Register();
            new CellStructParser().Register();
            new Point2DParser().Register();
            new RectangleStructParser().Register();
            new TintStructParser().Register();
        }
    }
}
