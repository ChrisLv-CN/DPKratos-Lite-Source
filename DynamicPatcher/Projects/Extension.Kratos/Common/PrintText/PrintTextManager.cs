using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public static class PrintTextManager
    {

        private static Point2D fontSize = default;
        public static Point2D FontSize
        {
            get
            {
                if (default == fontSize)
                {
                    string temp = "0123456789+-*/%";
                    RectangleStruct fontRect = Drawing.GetTextDimensions(temp, default, 0, 0, 0);
                    int x = fontRect.Width / 15;
                    fontSize.X = x % 2 == 0 ? x : x + 1;
                    fontSize.Y = fontRect.Height;
                    // Logger.Log($"{Game.CurrentFrame}, FontSize = {fontSize}");
                }
                return fontSize;
            }
        }

        private static Queue<RollingText> rollingTextQueue = new Queue<RollingText>();

        public static void Clear(object sender, EventArgs args)
        {
            rollingTextQueue.Clear();
        }

        public static void RollingText(string text, CoordStruct location, Point2D offset, int rollSpeed, int duration, PrintTextData data)
        {
            RollingText rollingText = new RollingText(text, location, offset, rollSpeed, duration, data);
            PrintTextManager.rollingTextQueue.Enqueue(rollingText);
        }

        public static void PrintText(object sender, EventArgs args)
        {
            // 打印滚动文字
            for (int i = 0; i < rollingTextQueue.Count; i++)
            {
                RollingText rollingText = rollingTextQueue.Dequeue();
                // 检查存活然后渲染
                if (rollingText.CanPrint(out Point2D offset, out Point2D pos, out RectangleStruct bound))
                {
                    // 获得锚点位置
                    Point2D pos2 = pos + offset;
                    Print(rollingText.Text, default, rollingText.Data, pos2, Pointer<RectangleStruct>.AsPointer(ref bound), Surface.Current, false);
                    rollingTextQueue.Enqueue(rollingText);
                }
            }
        }

        public static void Print(int number, ColorStruct houseColor, PrintTextData data, Point2D pos, bool isBuilding = false)
        {
            Pointer<Surface> pSurface = Surface.Current;
            RectangleStruct rect = pSurface.Ref.GetRect();
            if (data.UseSHP && data.SHPDrawStyle == SHPDrawStyle.PROGRESS)
            {
                string file = data.SHPFileName;
                int idx = data.ZeroFrameIndex + number / data.Wrap;
                if (data.MaxFrameIndex >= 0)
                {
                    idx = Math.Min(data.MaxFrameIndex, idx);
                }
                if (!file.IsNullOrEmptyOrNone() && FileSystem.TyrLoadSHPFile(file, out Pointer<SHPStruct> pCustomSHP))
                {
                    // Logger.Log($"{Game.CurrentFrame} - {data.ZeroFrameIndex} + {number} / {data.Wrap} 使用自定义SHP {file}, {idx}帧, 位置{pos}");
                    // 显示对应的帧
                    pSurface.Ref.DrawSHP(FileSystem.PALETTE_PAL, pCustomSHP, idx, pos, rect.GetThisPointer());
                }
            }
            else
            {
                // Logger.Log($"{Game.CurrentFrame} 渲染数字 {number}, 位置{pos}");
                Print(number.ToString(), houseColor, data, pos, rect.GetThisPointer(), pSurface, isBuilding);
            }
        }

        public static void Print(string text, ColorStruct houseColor, PrintTextData data, Point2D pos, Pointer<RectangleStruct> pBound, Pointer<Surface> pSurface, bool isBuilding)
        {

            bool noNumbers = data.NoNumbers || data.SHPDrawStyle != SHPDrawStyle.NUMBER;
            LongText longText = LongText.NONE;
            if (Enum.IsDefined(typeof(LongText), text.ToUpper()))
            {
                // Logger.Log($"{Game.CurrentFrame} LongText.IsDefined({text})");
                longText = (LongText)Enum.Parse(typeof(LongText), text, true);
                noNumbers = true;
            }

            // 渲染
            if (data.UseSHP)
            {
                // 使用Shp显示数字
                int zeroFrameIndex = data.ZeroFrameIndex; // shp时的起始帧序号
                Point2D imageSize = data.ImageSize; // shp时的图案大小
                // 获取字体横向位移值，即图像宽度，同时计算阶梯高度偏移
                int x = imageSize.X % 2 == 0 ? imageSize.X : imageSize.X + 1;
                int y = isBuilding ? x / 2 : 0;

                if (noNumbers)
                {
                    // 使用长字符不使用数字
                    string file = null;
                    int idx = 0;
                    if (data.SHPDrawStyle == SHPDrawStyle.TEXT)
                    {
                        file = data.SHPFileName;
                        idx = data.ZeroFrameIndex;
                    }
                    else
                    {
                        switch (longText)
                        {
                            case LongText.HIT:
                                file = data.HitSHP;
                                idx = data.HitIndex;
                                break;
                            case LongText.MISS:
                                file = data.MissSHP;
                                idx = data.MissIndex;
                                break;
                            case LongText.CRIT:
                                file = data.CritSHP;
                                idx = data.CritIndex;
                                break;
                            case LongText.GLANCING:
                                file = data.GlancingSHP;
                                idx = data.GlancingIndex;
                                break;
                            case LongText.BLOCK:
                                file = data.BlockSHP;
                                idx = data.BlockIndex;
                                break;
                            default:
                                return;
                        }
                    }
                    if (!file.IsNullOrEmptyOrNone() && FileSystem.TyrLoadSHPFile(file, out Pointer<SHPStruct> pCustomSHP))
                    {
                        // Logger.Log($"{Game.CurrentFrame} - 使用自定义SHP {file}, {idx}帧, 位置{pos}");
                        // 显示对应的帧
                        pSurface.Ref.DrawSHP(FileSystem.PALETTE_PAL, pCustomSHP, idx, pos, pBound);
                    }
                }
                else
                {
                    // 拆成单个字符
                    char[] t = text.ToCharArray();
                    foreach (char c in t)
                    {
                        int frameIndex = zeroFrameIndex;
                        int frameOffset = 0;
                        // 找到数字或者字符对应的图像帧
                        switch (c)
                        {
                            case '0':
                                frameOffset = 0;
                                break;
                            case '1':
                                frameOffset = 1;
                                break;
                            case '2':
                                frameOffset = 2;
                                break;
                            case '3':
                                frameOffset = 3;
                                break;
                            case '4':
                                frameOffset = 4;
                                break;
                            case '5':
                                frameOffset = 5;
                                break;
                            case '6':
                                frameOffset = 6;
                                break;
                            case '7':
                                frameOffset = 7;
                                break;
                            case '8':
                                frameOffset = 8;
                                break;
                            case '9':
                                frameOffset = 9;
                                break;
                            case '+':
                                frameOffset = 10;
                                break;
                            case '-':
                                frameOffset = 11;
                                break;
                            case '*':
                                frameOffset = 12;
                                break;
                            case '/':
                            case '|':
                                frameOffset = 13;
                                break;
                            case '%':
                                frameOffset = 14;
                                break;
                        }
                        // Logger.Log("{0} - frameIdx = {1}, frameOffset = {2}", Game.CurrentFrame, frameIndex, frameOffset);
                        // 找到对应的帧序号
                        frameIndex += frameOffset;
                        if (FileSystem.TyrLoadSHPFile(data.SHPFileName, out Pointer<SHPStruct> pCustomSHP))
                        {
                            // Logger.Log($"{Game.CurrentFrame} - 使用SHP渲染{text} {data.SHPFileName}, {frameIndex}帧, 位置{pos}");
                            // 显示对应的帧
                            pSurface.Ref.DrawSHP(FileSystem.PALETTE_PAL, pCustomSHP, frameIndex, pos, pBound);
                        }
                        // 调整下一个字符锚点
                        pos.X += x;
                        pos.Y -= y;
                    }
                }
            }
            else
            {
                if (noNumbers && longText == LongText.NONE)
                {
                    return;
                }
                // 使用文字显示数字
                ColorStruct textColor = data.Color; // 文字时渲染颜色
                if (data.IsHouseColor && default != houseColor)
                {
                    textColor = houseColor;
                }
                int x = FontSize.X;
                int y = isBuilding ? FontSize.X / 2 : 0;
                // 拆成单个字符
                char[] t = text.ToCharArray();
                foreach (char c in t)
                {
                    // 画阴影
                    if (default != data.ShadowOffset)
                    {
                        Point2D shadow = pos + data.ShadowOffset;
                        pSurface.Ref.DrawText(c.ToString(), pBound, Pointer<Point2D>.AsPointer(ref shadow), data.ShadowColor);
                    }
                    pSurface.Ref.DrawText(c.ToString(), pBound, Pointer<Point2D>.AsPointer(ref pos), textColor);
                    // 获取字体横向位移值，即图像宽度，同时计算阶梯高度偏移
                    pos.X += x;
                    pos.Y -= y;
                }
            }
        }
    }
}