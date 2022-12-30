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
            Point2D pos = location.ToClientPos();
            // 显示Duration和InitDelay
            bool checkDuration = false;
            bool checkInitDelay = false;
            if (checkDuration = Data.Duration.Mode != InfoMode.NONE || (checkInitDelay = Data.InitDelay.Mode != InfoMode.NONE))
            {
                if ((Data.Duration.ShowEnemy || isPlayerControl) && (!Data.Duration.OnlySelected || pOwner.Ref.IsSelected))
                {
                    int duration = -1;
                    int initDelay = -1;
                    List<AttachEffect> aes = aem.AttachEffects;
                    for (int i = aes.Count() - 1; i >= 0; i--)
                    {
                        AttachEffect temp = aes[i];
                        if (temp.AEData.Name == watch)
                        {
                            // 读取Duration
                            if (checkDuration && temp.TryGetDurationTimeLeft(out int durationLeft))
                            {
                                switch (Data.Duration.Sort)
                                {
                                    case SortType.MIN:
                                        if (durationLeft < duration)
                                        {
                                            duration = durationLeft;
                                        }
                                        break;
                                    case SortType.MAX:
                                        if (durationLeft > duration)
                                        {
                                            duration = durationLeft;
                                        }
                                        break;
                                    default:
                                        checkDuration = false;
                                        break;
                                }
                                if (duration < 0)
                                {
                                    duration = durationLeft;
                                }
                            }
                            // 读取InitDelay
                            if (checkInitDelay && temp.TryGetInitDelayTimeLeft(out int delayLeft))
                            {
                                switch (Data.InitDelay.Sort)
                                {
                                    case SortType.MIN:
                                        if (delayLeft < initDelay)
                                        {
                                            initDelay = delayLeft;
                                        }
                                        break;
                                    case SortType.MAX:
                                        if (delayLeft > initDelay)
                                        {
                                            initDelay = delayLeft;
                                        }
                                        break;
                                    default:
                                        checkInitDelay = false;
                                        break;
                                }
                                if (initDelay < 0)
                                {
                                    initDelay = delayLeft;
                                }
                            }
                            if (!checkDuration && !checkInitDelay)
                            {
                                break;
                            }
                        }
                    }
                    if (duration > -1)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 监视[{pOwner.Ref.Type.Ref.Base.ID}]{pOwner}上的AE[{watch}]剩余时间{number}");
                        // 显示Duration
                        PrintInfoText(duration, houseColor, pos, Data.Duration);
                    }
                    if (initDelay > -1)
                    {
                        // 显示InitDelay
                        PrintInfoText(duration, houseColor, pos, Data.InitDelay);
                    }
                }
            }
            // 显示Delay
            if (Data.Delay.Mode != InfoMode.NONE)
            {
                if ((Data.Duration.ShowEnemy || isPlayerControl) && (!Data.Duration.OnlySelected || pOwner.Ref.IsSelected))
                {
                    int delay = -1;
                    if (aem.DisableDelayTimers.ContainsKey(watch))
                    {
                        TimerStruct timer = aem.DisableDelayTimers[watch];
                        if (timer.InProgress())
                        {
                            delay = timer.GetTimeLeft();
                        }
                    }
                    if (delay > -1)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 监视[{pOwner.Ref.Type.Ref.Base.ID}]{pOwner}上的AE[{watch}]有{stacks}层");
                        // 显示delay
                        PrintInfoText(delay, houseColor, pos, Data.Delay);
                    }
                }
            }
            // 显示Stack
            if (Data.Stack.Mode != InfoMode.NONE)
            {
                if ((Data.Stack.ShowEnemy || isPlayerControl) && (!Data.Stack.OnlySelected || pOwner.Ref.IsSelected))
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
                        PrintInfoText(stacks, houseColor, pos, Data.Stack);
                    }
                }
            }
        }

        /// <summary>
        /// 显示数字
        /// </summary>
        /// <param name="number"></param>
        /// <param name="data"></param>
        private void PrintInfoText(int number, ColorStruct houseColor, Point2D location, InfoEntity data)
        {
            // 调整锚点
            Point2D pos = location;
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
