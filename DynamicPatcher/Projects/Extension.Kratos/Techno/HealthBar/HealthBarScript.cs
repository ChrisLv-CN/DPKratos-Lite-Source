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

namespace Scripts
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    public class HealthBar : TechnoScriptable
    {
        // 全局设置
        private static HealthTextTypeControlData healthTextTypeControlData = new HealthTextTypeControlData();

        public HealthBar(TechnoExt owner) : base(owner) { }

        private HealthTextTypeData healthTextTypeData;


        public override void Awake()
        {
            if (!healthTextTypeControlData.Hidden)
            {
                switch (Owner.OwnerObject.Ref.BaseAbstract.WhatAmI())
                {
                    case AbstractType.Building:
                        // if (null == healthTextTypeData || RulesExt.Instance.GeneralHealthTextTypeControlDataHasChanged)
                        // {
                        //     healthTextTypeData = RulesExt.Instance.GeneralHealthTextTypeControlData.Building.Clone();
                        // }
                        // healthTextTypeData.TryReadHealthTextType(reader, section, "HealthText.");
                        // healthTextTypeData.TryReadHealthTextType(reader, section, "HealthText.Building.");
                        healthTextTypeData = healthTextTypeControlData.Building.Clone();
                        break;
                    case AbstractType.Infantry:
                        healthTextTypeData = healthTextTypeControlData.Infantry.Clone();
                        break;
                    case AbstractType.Unit:
                        healthTextTypeData = healthTextTypeControlData.Unit.Clone();
                        break;
                    case AbstractType.Aircraft:
                        healthTextTypeData = healthTextTypeControlData.Aircraft.Clone();
                        break;
                }
                // 读取私有设置
                if (null != healthTextTypeData)
                {
                    string section = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID;
                    ISectionReader reader = Ini.GetSection(Ini.GetDependency(INIConstant.RulesName), section);
                    healthTextTypeData.Read(reader);
                }
            }
        }

        public override void DrawHealthBar(int barLength, Pointer<Point2D> pPos, Pointer<RectangleStruct> pBound, bool isBuilding)
        {
            if (null != healthTextTypeData && !healthTextTypeData.Hidden)
            {
                PrintHealthText(barLength, pPos, pBound, isBuilding);
            }
        }

        private void PrintHealthText(int barLength, Pointer<Point2D> pPos, Pointer<RectangleStruct> pBound, bool isBuilding)
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            bool isSelected = pTechno.Ref.Base.IsSelected;
            // 根据血量状态获取设置
            HealthTextData data = healthTextTypeData.Green;
            HealthState healthState = pTechno.Ref.Base.GetHealthStatus();
            switch (healthState)
            {
                case HealthState.Yellow:
                    data = healthTextTypeData.Yellow;
                    break;
                case HealthState.Red:
                    data = healthTextTypeData.Red;
                    break;
            }
            // Logger.Log($"{Game.CurrentFrame} - Hidden = {data.Hidden}, ShowEnemy = {(!pTechno.Ref.Owner.Ref.PlayerControl && !data.ShowEnemy)}, ShowHover = {(!isSelected && !data.ShowHover)}");
            if (data.Hidden || (!pTechno.Ref.Owner.Ref.PlayerControl && !data.ShowEnemy) || (!isSelected && !data.ShowHover))
            {
                return;
            }
            // 调整锚点
            Point2D pos = pPos.Data;
            int xOffset = data.Offset.X; // 锚点向右的偏移值
            int yOffset = data.Offset.Y; // 锚点向下的偏移值
            int barWidth = barLength * 2; // 血条显示的个数，单位是半条，建筑是满条

            // Point2D fountSize = data.FontSize; // 使用shp则按照shp图案大小来偏移锚点
            HealthTextStyle style = isSelected ? data.Style : data.HoverStyle; ; // 数值的格式
            if (isBuilding)
            {
                // 算出建筑血条最左边格子的偏移
                CoordStruct dimension = pTechno.Ref.Type.Ref.Base.Dimension2();
                CoordStruct dimension2 = new CoordStruct(-dimension.X / 2, dimension.Y / 2, dimension.Z);
                Point2D pos2 = TacticalClass.Instance.Ref.CoordsToScreen(dimension2);
                // 修正锚点
                pos += pos2;
                pos.X = pos.X - 2 + xOffset;
                pos.Y = pos.Y - 2 + yOffset;
                barWidth = barLength * 4 + 6; // 建筑是满条，每个块是10像素宽，每个4像素绘制一个，头边距2，尾边距4
            }
            else
            {
                yOffset += pTechno.Ref.Type.Ref.PixelSelectionBracketDelta;
                pos.X += -barLength + 3 + xOffset;
                pos.Y += -28 + yOffset;
                if (barLength == 8)
                {
                    // 步兵血条 length = 8
                    pos.X += 1;
                }
                else
                {
                    // 载具血条 length = 17
                    pos.Y += -1;
                }
            }
            // 获得血量数据
            string text = null;
            int health = pTechno.Ref.Base.Health;
            switch (style)
            {
                case HealthTextStyle.FULL:
                    int strength = pTechno.Ref.Type.Ref.Base.Strength;
                    string s = isBuilding ? "|" : "/";
                    text = string.Format("{0}{1}{2}", health, s, strength);
                    break;
                case HealthTextStyle.PERCENT:
                    double per = pTechno.Ref.Base.GetHealthPercentage() * 100;
                    text = string.Format("{0}%", per);
                    break;
                default:
                    text = health.ToString();
                    break;
            }
            if (!string.IsNullOrEmpty(text))
            {
                // 修正锚点
                if (data.UseSHP)
                {
                    // 使用Shp显示数字，SHP锚点在图案中心
                    // 重新调整锚点位置，向上抬起一个半格字的高度
                    pos.Y = pos.Y - data.ImageSize.Y / 2;

                    // 按对齐方式再次调整锚点
                    if (data.Align != HealthTextAlign.LEFT)
                    {
                        int x = data.ImageSize.X % 2 == 0 ? data.ImageSize.X : data.ImageSize.X + 1;
                        int textWidth = text.ToCharArray().Length * x;
                        OffsetPosAlign(ref pos, textWidth, barWidth, data.Align, isBuilding, true);
                    }
                    else
                    {
                        if (isBuilding)
                        {
                            pos.X += data.ImageSize.X; // 右移一个字宽，美观
                        }
                    }
                }
                else
                {
                    // 使用文字显示数字，文字的锚点在左上角
                    // 重新调整锚点位置，向上抬起一个半格字的高度
                    pos.Y = pos.Y - PrintTextManager.FontSize.Y + 5; // 字是20格高，上4中9下7

                    // 按对齐方式再次调整锚点
                    if (data.Align != HealthTextAlign.LEFT)
                    {
                        RectangleStruct textRect = Drawing.GetTextDimensions(text, new Point2D(0, 0), 0, 2, 0);
                        int textWidth = textRect.Width;
                        OffsetPosAlign(ref pos, textWidth, barWidth, data.Align, isBuilding, false);
                    }
                    else
                    {
                        if (isBuilding)
                        {
                            pos.X += PrintTextManager.FontSize.X; // 右移一个字宽，美观
                        }
                        else
                        {
                            pos.X -= PrintTextManager.FontSize.X / 2; // 左移半个字宽，美观
                        }
                    }
                }
                PrintTextManager.Print(text, data, pos, pBound, Surface.Current, isBuilding);
            }

        }

        private void OffsetPosAlign(ref Point2D pos, int textWidth, int barWidth, HealthTextAlign align, bool isBuilding, bool useSHP)
        {
            // Logger.Log($"{Game.CurrentFrame} textWidth = {textWidth}, barWidth = {barWidth}, align = {align}, isBuilding = {isBuilding}");
            int offset = barWidth - textWidth;
            switch (align)
            {
                case HealthTextAlign.CENTER:
                    pos.X += offset / 2;
                    if (isBuilding)
                    {
                        pos.Y -= offset / 4;
                    }
                    break;
                case HealthTextAlign.RIGHT:
                    pos.X += offset;
                    if (!useSHP)
                    {
                        pos.X += PrintTextManager.FontSize.X / 2; // 右移半个字宽，补偿Margin
                    }
                    if (isBuilding)
                    {
                        pos.Y -= offset / 2;
                    }
                    break;
            }
        }

    }
}