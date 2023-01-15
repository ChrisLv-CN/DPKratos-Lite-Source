using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.EventSystems;
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
        public override void OnEnable()
        {
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
        }

        public override void OnDisable(CoordStruct location)
        {
            EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
        }

        public override void LoadFromStream(IStream stream)
        {
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
        }

        public void OnGScreenRender(object sender, EventArgs args)
        {
            if (((GScreenEventArgs)args).IsLateRender)
            {
                AttachEffectScript aem = AE.AEManager;
                Pointer<HouseClass> pHouse = AE.pSourceHouse;
                ColorStruct houseColor = !pHouse.IsNull ? pHouse.Ref.LaserColor : default;
                bool isPlayerControl = !pHouse.IsNull && pHouse.Ref.PlayerControl;
                bool isSelected = pOwner.Ref.IsSelected;
                Point2D pos = pOwner.Ref.Base.GetCoords().ToClientPos();

                // 需要遍历读取具体的AE状态的信息的部分统一读取
                // 显示Duration和InitDelay
                bool checkDuration = Data.Duration.Mode != InfoMode.NONE && !Data.Duration.Watch.IsNullOrEmptyOrNone() && (Data.Duration.ShowEnemy || isPlayerControl) && (!Data.Duration.OnlySelected || isSelected);
                bool checkInitDelay = Data.InitDelay.Mode != InfoMode.NONE && !Data.InitDelay.Watch.IsNullOrEmptyOrNone() && (Data.InitDelay.ShowEnemy || isPlayerControl) && (!Data.InitDelay.OnlySelected || isSelected);
                if (checkDuration || checkInitDelay)
                {
                    // 循环遍历AE
                    int duration = -1;
                    int initDelay = -1;
                    List<AttachEffect> aes = aem.AttachEffects;
                    for (int i = aes.Count() - 1; i >= 0; i--)
                    {
                        AttachEffect temp = aes[i];
                        string aeName = temp.AEData.Name;
                        // 读取Duration
                        if (checkDuration && aeName == Data.Duration.Watch && temp.TryGetDurationTimeLeft(out int durationLeft))
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
                                    // 找到目标后，停止检查
                                    checkDuration = false;
                                    break;
                            }
                            if (duration < 0)
                            {
                                duration = durationLeft;
                            }
                        }
                        // 读取InitDelay
                        if (checkInitDelay && aeName == Data.InitDelay.Watch && temp.TryGetInitDelayTimeLeft(out int delayLeft))
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
                                    // 找到目标后，停止检查
                                    checkInitDelay = false;
                                    break;
                            }
                            if (initDelay < 0)
                            {
                                initDelay = delayLeft;
                            }
                        }
                        // 都不需要继续查找，跳出AE遍历
                        if (!checkDuration && !checkInitDelay)
                        {
                            break;
                        }
                    }
                    // 显示查找到的信息
                    if (duration > -1)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 监视[{pOwner.Ref.Type.Ref.Base.ID}]{pOwner}上的AE[{watch}]剩余时间{number}");
                        // 显示Duration
                        PrintInfoNumber(duration, houseColor, pos, Data.Duration);
                    }
                    if (initDelay > -1)
                    {
                        // 显示InitDelay
                        PrintInfoNumber(duration, houseColor, pos, Data.InitDelay);
                    }
                }

                // 不需要遍历AE清单，可以直接从AE管理器获得的信息
                // 显示Delay
                if (Data.Delay.Mode != InfoMode.NONE && !Data.Delay.Watch.IsNullOrEmptyOrNone() && (Data.Delay.ShowEnemy || isPlayerControl) && (!Data.Delay.OnlySelected || isSelected))
                {
                    int delay = -1;
                    if (aem.DisableDelayTimers.ContainsKey(Data.Delay.Watch))
                    {
                        TimerStruct timer = aem.DisableDelayTimers[Data.Delay.Watch];
                        if (timer.InProgress())
                        {
                            delay = timer.GetTimeLeft();
                        }
                    }
                    if (delay > -1)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 监视[{pOwner.Ref.Type.Ref.Base.ID}]{pOwner}上的AE[{watch}]有{stacks}层");
                        // 显示delay
                        PrintInfoNumber(delay, houseColor, pos, Data.Delay);
                    }
                }
                // 显示Stack
                if (Data.Stack.Mode != InfoMode.NONE && !Data.Stack.Watch.IsNullOrEmptyOrNone() && (Data.Stack.ShowEnemy || isPlayerControl) && (!Data.Stack.OnlySelected || isSelected))
                {
                    int stacks = -1;
                    if (aem.AEStacks.ContainsKey(Data.Stack.Watch))
                    {
                        stacks = AE.AEManager.AEStacks[Data.Stack.Watch];
                    }
                    if (stacks > -1)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 监视[{pOwner.Ref.Type.Ref.Base.ID}]{pOwner}上的AE[{watch}]有{stacks}层");
                        // 显示Stacks
                        PrintInfoNumber(stacks, houseColor, pos, Data.Stack);
                    }
                }

                // 显示附着对象的信息
                // 显示Mission
                if (Data.Mission.Mode == InfoMode.TEXT && (Data.Mission.ShowEnemy || isPlayerControl) && (!Data.Mission.OnlySelected || isSelected))
                {
                    Mission mission = pOwner.Ref.GetCurrentMission();
                    PrintInfoText(mission.ToString(), houseColor, pos, Data.Mission);
                }
                // 显示目标连线
                if (Data.Target.Mode != InfoMode.NONE && (Data.Target.ShowEnemy || isPlayerControl) && (!Data.Target.OnlySelected || isSelected))
                {
                    Pointer<AbstractClass> pTarget = IntPtr.Zero;
                    if (AE.AEManager.IsBullet)
                    {
                        pTarget = pOwner.Convert<BulletClass>().Ref.Target;
                    }
                    else
                    {
                        pTarget = pOwner.Convert<TechnoClass>().Ref.Target;
                    }
                    if (!pTarget.IsNull)
                    {
                        CoordStruct sourcePos = pOwner.Ref.Base.GetCoords();
                        CoordStruct targetPos = pTarget.Ref.GetCoords();
                        RectangleStruct bounds = Surface.Current.Ref.GetRect();
                        bounds.Height -= 34;
                        Surface.Current.DrawDashedLine(sourcePos.ToClientPos(), targetPos.ToClientPos(), Data.Target.Color, bounds, true);
                    }
                }
            }
        }

        /// <summary>
        /// 显示数字
        /// </summary>
        /// <param name="number"></param>
        /// <param name="data"></param>
        private void PrintInfoNumber(int number, ColorStruct houseColor, Point2D location, InfoEntity data)
        {
            // 调整锚点
            Point2D pos = location;
            pos.X += data.Offset.X; // 锚点向右的偏移值
            pos.Y += data.Offset.Y; // 锚点向下的偏移值

            // 修正锚点
            if (!data.UseSHP)
            {
                // 根据对其方式修正锚点
                OffsetAlign(ref pos, number.ToString(), data);
            }
            PrintTextManager.Print(number, houseColor, data, pos);
        }

        private void PrintInfoText(string text, ColorStruct houseColor, Point2D location, InfoEntity data)
        {
            // 调整锚点
            Point2D pos = location;
            pos.X += data.Offset.X; // 锚点向右的偏移值
            pos.Y += data.Offset.Y; // 锚点向下的偏移值
            // 根据对其方式修正锚点
            OffsetAlign(ref pos, text, data);
            // 显示
            PrintTextManager.PrintOnlyText(text, houseColor, data, pos);
        }

        private void OffsetAlign(ref Point2D pos, string text, InfoEntity data)
        {
            // 使用文字显示数字，文字的锚点在左上角
            // 重新调整锚点位置，向上抬起半个字的高度
            pos.Y = pos.Y - PrintTextManager.FontSize.Y / 2; // 字是20格高，上4中9下7
            // 按照文字对齐方式修正X的偏移值
            RectangleStruct textRect = Drawing.GetTextDimensions(text, new Point2D(0, 0), 0, 2, 0);
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

    }
}
