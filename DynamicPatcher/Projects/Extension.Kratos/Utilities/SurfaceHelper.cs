using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{
    public static class SurfaceHelper
    {
        public static void Crosshair(this Pointer<Surface> surface, CoordStruct sourcePos, int length, ColorStruct color, RectangleStruct bounds, bool dashed = true, bool blink = false)
        {
            CoordStruct p1 = sourcePos + new CoordStruct(length, 0, 0);
            CoordStruct p2 = sourcePos + new CoordStruct(-length, 0, 0);
            CoordStruct p3 = sourcePos + new CoordStruct(0, -length, 0);
            CoordStruct p4 = sourcePos + new CoordStruct(0, -length, 0);
            if (dashed)
            {
                surface.DrawDashedLine(p1, p2, color, bounds, blink);
                surface.DrawDashedLine(p3, p4, color, bounds, blink);
            }
            else
            {
                surface.DrawLine(p1, p2, color, bounds);
                surface.DrawLine(p3, p4, color, bounds);
            }
        }

        public static void Cell(this Pointer<Surface> surface, CoordStruct sourcePos, ColorStruct color, RectangleStruct bounds, bool dashed = true, bool blink = false, int length = 128)
        {
            CoordStruct p1 = sourcePos + new CoordStruct(length, length, 0);
            CoordStruct p2 = sourcePos + new CoordStruct(-length, length, 0);
            CoordStruct p3 = sourcePos + new CoordStruct(-length, -length, 0);
            CoordStruct p4 = sourcePos + new CoordStruct(length, -length, 0);
            if (dashed)
            {
                surface.DrawDashedLine(p1, p2, color, bounds);
                surface.DrawDashedLine(p2, p3, color, bounds);
                surface.DrawDashedLine(p3, p4, color, bounds);
                surface.DrawDashedLine(p4, p1, color, bounds);
            }
            else
            {
                surface.DrawLine(p1, p2, color, bounds);
                surface.DrawLine(p2, p3, color, bounds);
                surface.DrawLine(p3, p4, color, bounds);
                surface.DrawLine(p4, p1, color, bounds);
            }
        }

        public static unsafe bool DrawDashedLine(this Pointer<Surface> surface, CoordStruct sourcePos, CoordStruct targetPos, ColorStruct color, RectangleStruct bounds, bool blink = false)
        {
            return surface.DrawDashedLine(sourcePos.ToClientPos(), targetPos.ToClientPos(), color.RGB2DWORD(), bounds, blink);
        }

        public static unsafe bool DrawDashedLine(this Pointer<Surface> surface, Point2D point1, Point2D point2, ColorStruct color, RectangleStruct bounds, bool blink = false)
        {
            return surface.DrawDashedLine(point1, point2, color.RGB2DWORD(), bounds, blink);
        }

        /// <summary>
        /// 绘制虚线
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="dwColor"></param>
        /// <param name="blink"></param>
        /// <param name="heightOffset"></param>
        /// <returns></returns>
        public static unsafe bool DrawDashedLine(this Pointer<Surface> surface, Point2D point1, Point2D point2, int dwColor, RectangleStruct bound, bool blink = false)
        {
            if (default == bound)
            {
                bound = surface.Ref.GetRect();
            }
            Surface.ClipLine(ref point1, ref point2, bound);
            bool point1InRect = point1.X >= bound.X && point1.X <= bound.X + bound.Width && point1.Y >= bound.Y && point1.Y <= bound.Y + bound.Height;
            bool point2InRect = point2.X >= bound.X && point2.X <= bound.X + bound.Width && point2.Y >= bound.Y && point2.Y <= bound.Y + bound.Height;
            // point in rect then draw
            if (point1InRect && point2InRect)
            {
                int offset = 0;
                if (blink)
                {
                    offset = 7 * Game.CurrentFrame % 16;
                }
                return surface.Ref.DrawDashedLine(point1, point2, dwColor, offset);
            }
            return false;
        }

        public static unsafe bool DrawLine(this Pointer<Surface> surface, CoordStruct sourcePos, CoordStruct targetPos, ColorStruct color, RectangleStruct bound = default)
        {
            return surface.DrawLine(sourcePos.ToClientPos(), targetPos.ToClientPos(), color.RGB2DWORD(), bound);
        }

        public static unsafe bool DrawLine(this Pointer<Surface> surface, Point2D point1, Point2D point2, ColorStruct color, RectangleStruct bound = default)
        {
            return surface.DrawLine(point1, point2, color.RGB2DWORD(), bound);
        }

        /// <summary>
        /// 绘制实线
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="dwColor"></param>
        /// <param name="heightOffset"></param>
        /// <returns></returns>
        public static unsafe bool DrawLine(this Pointer<Surface> surface, Point2D point1, Point2D point2, int dwColor, RectangleStruct bound)
        {
            if (default == bound)
            {
                bound = surface.Ref.GetRect();
            }
            Surface.ClipLine(ref point1, ref point2, bound);
            bool point1InRect = point1.X >= bound.X && point1.X <= bound.X + bound.Width && point1.Y >= bound.Y && point1.Y <= bound.Y + bound.Height;
            bool point2InRect = point2.X >= bound.X && point2.X <= bound.X + bound.Width && point2.Y >= bound.Y && point2.Y <= bound.Y + bound.Height;
            // point in rect then draw
            if (point1InRect && point2InRect)
            {
                return surface.Ref.DrawLine(point1, point2, dwColor);
            }
            return false;
        }


    }

}