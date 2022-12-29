using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class AttachEffect
    {
        public Info Info;

        private void InitInfo()
        {
            this.Info = AEData.InfoData.CreateEffect<Info>();
            RegisterEffect(Info);
        }
    }


    [Serializable]
    public class Info : Effect<InfoData>
    {

        public override void OnRenderEnd(CoordStruct location)
        {
            string watch = Data.Watch;
            AttachEffectScript aem = AE.AEManager;
            Pointer<HouseClass> pHouse = IntPtr.Zero;
            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                pHouse = pTechno.Ref.Owner;
            }
            else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                pHouse = pBullet.GetSourceHouse();
            }
            ColorStruct houseColor = !pHouse.IsNull ? pHouse.Ref.LaserColor : default;
            bool isPlayerControl = !pHouse.IsNull && pHouse.Ref.PlayerControl;
            // 显示Duration
            if (Data.DurationInfo.Mode != InfoMode.NONE)
            {
                if ((Data.DurationInfo.ShowEnemy || isPlayerControl) && (!Data.DurationInfo.OnlySelected || pOwner.Ref.IsSelected))
                {
                    int number = -1;
                    List<AttachEffect> aes = aem.AttachEffects;
                    bool breakout = false;
                    for (int i = aes.Count() - 1; i >= 0; i--)
                    {
                        AttachEffect temp = aes[i];
                        if (temp.AEData.Name == watch)
                        {
                            int num = temp.GetTimeLeft();
                            // Logger.Log($"{Game.CurrentFrame} 监视AE[{watch}]，剩余时间{num}，排序{Data.DurationInfo.Sort}");
                            switch (Data.DurationInfo.Sort)
                            {
                                case SortType.MIN:
                                    if (num < number)
                                    {
                                        number = num;
                                    }
                                    break;
                                case SortType.MAX:
                                    if (num > number)
                                    {
                                        number = num;
                                    }
                                    break;
                                default:
                                    number = num;
                                    breakout = true;
                                    break;
                            }
                            if (breakout)
                            {
                                break;
                            }
                            if (number < 0)
                            {
                                number = num;
                            }
                        }
                    }
                    if (number > -1)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 监视[{pOwner.Ref.Type.Ref.Base.ID}]{pOwner}上的AE[{watch}]剩余时间{number}");
                        // 显示Duration
                        PrintInfoText(number, houseColor, location, Data.DurationInfo);
                    }
                }
            }
            // 显示Stack
            if (Data.StackInfo.Mode != InfoMode.NONE)
            {
                if ((Data.StackInfo.ShowEnemy || isPlayerControl) && (!Data.StackInfo.OnlySelected || pOwner.Ref.IsSelected))
                {
                    int stacks = -1;
                    if (aem.AEStacks.ContainsKey(watch))
                    {
                        stacks = AE.AEManager.AEStacks[watch];
                    }
                    if (stacks > -1)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 监视[{pOwner.Ref.Type.Ref.Base.ID}]{pOwner}上的AE[{watch}]有{stacks}层");
                        // 显示Stacks
                        PrintInfoText(stacks, houseColor, location, Data.StackInfo);
                    }
                }
            }
        }

        /// <summary>
        /// 显示数字
        /// </summary>
        /// <param name="number"></param>
        /// <param name="data"></param>
        private void PrintInfoText(int number, ColorStruct houseColor, CoordStruct location, InfoEntity data)
        {
            // 调整锚点
            Point2D pos = location.ToClientPos();
            pos.X += data.Offset.X; // 锚点向右的偏移值
            pos.Y += data.Offset.Y; // 锚点向下的偏移值

            // 修正锚点
            if (!data.UseSHP)
            {
                // 使用文字显示数字，文字的锚点在左上角
                // 重新调整锚点位置，向上抬起半个字的高度
                pos.Y = pos.Y - PrintTextManager.FontSize.Y / 2; // 字是20格高，上4中9下7
                // 按照文字对齐方式修正X的偏移值
                RectangleStruct textRect = Drawing.GetTextDimensions(number.ToString(), new Point2D(0, 0), 0, 2, 0);
                int width = textRect.Width;
                switch (data.Align)
                {
                    case PrintTextAlign.CENTER:
                        pos.X -= width / 2;
                        break;
                    case PrintTextAlign.RIGHT:
                        pos.X -= width;
                        break;
                }
            }
            PrintTextManager.Print(number, houseColor, data, pos);
        }

    }
}
