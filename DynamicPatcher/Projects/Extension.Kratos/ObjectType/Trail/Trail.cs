using System;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class Trail : TrailData
    {
        private CoordStruct LastLocation;
        private bool canDraw;
        private int initialDelay;
        private TimerStruct delayTimer;

        private bool forceDraw;
        private DrivingState drivingState;

        private int laserColorIndex = 0;

        public Trail(IConfigWrapper<TrailType> type) : base(type)
        {
            this.LastLocation = default;
            if (Type.InitialDelay > 0)
            {
                this.canDraw = false;
                this.initialDelay = Type.InitialDelay;
            }
            else
            {
                this.canDraw = true;
                this.initialDelay = 0;
            }
            this.forceDraw = false;
            this.drivingState = DrivingState.Moving;
        }


        public void SetDrivingState(DrivingState state)
        {
            this.drivingState = state;
            if (state == DrivingState.Stop)
            {
                forceDraw = true;
            }
        }

        public void SetLastLocation(CoordStruct location)
        {
            this.LastLocation = location;
        }

        public void UpdateLastLocation(CoordStruct location)
        {
            int distance = Type.Distance;
            if (location.DistanceFrom(LastLocation) > distance || this.forceDraw)
            {
                this.LastLocation = location;
            }
        }

        public void ClearLastLocation()
        {
            this.LastLocation = default;
        }

        private bool CanDraw()
        {
            if (!canDraw)
            {
                if (initialDelay > 0)
                {
                    delayTimer.Start(initialDelay);
                    initialDelay = 0;
                }
                canDraw = delayTimer.Expired();
            }
            return canDraw;
        }

        private bool CheckVertical(CoordStruct sourcePos, CoordStruct targetPos)
        {
            return (Type.IgnoreVertical ? (Math.Abs(sourcePos.X - targetPos.X) > 32 || Math.Abs(sourcePos.Y - targetPos.Y) > 32) : true);
        }

        private bool IsOnLand(CoordStruct sourcePos)
        {
            if (null != OnLandTypes && OnLandTypes.Length > 0)
            {
                if (MapClass.Instance.TryGetCellAt(sourcePos, out Pointer<CellClass> pCell))
                {
                    LandType landType = pCell.Ref.LandType;

                    // Logger.Log("当前格子的地形类型{0}, 瓷砖类型{1}", landType, pCell.Ref.GetTileType());
                    if (OnLandTypes.Contains(landType))
                    {
                        if (null != OnTileTypes && OnTileTypes.Length > 0)
                        {
                            return OnTileTypes.Contains(pCell.Ref.GetTileType());
                        }
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        public void DrawTrail(Pointer<HouseClass> pHouse, CoordStruct currentPos)
        {
            // Logger.Log($"{Game.CurrentFrame} - 绘制尾巴 {sourcePos} {LastLocation}, {sourcePos.DistanceFrom(LastLocation)} > {Type.Distance}, CheckV {CheckVertical(sourcePos, LastLocation)}, IsOnLand {IsOnLand(sourcePos)}");
            if (default != currentPos)
            {
                if (default != LastLocation)
                {
                    CoordStruct behindPos = LastLocation;
                    int distance = Type.Distance;
                    if (currentPos.DistanceFrom(behindPos) > distance || this.forceDraw)
                    {
                        if ((CanDraw() && CheckVertical(currentPos, behindPos)) || this.forceDraw)
                        {
                            forceDraw = false;
                            if (IsOnLand(currentPos))
                            {
                                RealDrawTrail(currentPos, behindPos, pHouse);
                            }
                            drivingState = DrivingState.Moving;
                        }
                        LastLocation = currentPos;
                    }
                }
                else
                {
                    LastLocation = currentPos;
                }
            }
        }

        private void RealDrawTrail(CoordStruct currentPos, CoordStruct behindPos, Pointer<HouseClass> pHouse)
        {
            // Logger.Log("{0} - Draw the Tail {1}", Game.CurrentFrame, Type);
            switch (Type.Mode)
            {
                case TrailMode.LASER:
                    ColorStruct houseColor = default;
                    if (!pHouse.IsNull)
                    {
                        houseColor = pHouse.Ref.LaserColor;
                    }
                    // 修改渲染的颜色
                    if (null != Type.LaserType.ColorList && Type.LaserType.ColorList.Count() > 0)
                    {
                        ColorStruct[] colorList = Type.LaserType.ColorList;
                        int count = colorList.Count();
                        // 随机
                        laserColorIndex = Type.LaserType.ColorListRandom ? MathEx.Random.Next(count) : laserColorIndex;
                        ColorStruct color = colorList[laserColorIndex];
                        // Logger.Log($"{Game.CurrentFrame} 颜色列表有{count}种颜色，取第{laserColorIndex}个，{color}");
                        if (++laserColorIndex >= count)
                        {
                            laserColorIndex = 0;
                        }
                        Type.LaserType.InnerColor = color;
                        BulletEffectHelper.DrawLine(currentPos, behindPos, Type.LaserType);
                    }
                    else
                    {
                        BulletEffectHelper.DrawLine(currentPos, behindPos, Type.LaserType, Type.LaserType.IsHouseColor ? houseColor : default);
                    }
                    break;
                case TrailMode.ELECTIRIC:
                    BulletEffectHelper.DrawBolt(currentPos, behindPos, Type.BoltType);
                    break;
                case TrailMode.BEAM:
                    BulletEffectHelper.DrawBeam(currentPos, behindPos, Type.BeamType);
                    break;
                case TrailMode.PARTICLE:
                    BulletEffectHelper.DrawParticele(currentPos, behindPos, Type.ParticleSystem);
                    break;
                case TrailMode.ANIM:
                    DrawAnimTrail(currentPos, behindPos, pHouse);
                    break;
            }
        }

        public void DrawAnimTrail(CoordStruct currentPos, CoordStruct behindPos, Pointer<HouseClass> pHouse)
        {
            // Logger.Log("{0} - Draw the Anim Tail {1}", Game.CurrentFrame, Type);
            string[] animTypes = Type.WhileDrivingAnim;
            switch (drivingState)
            {
                case DrivingState.Start:
                    animTypes = Type.StartDrivingAnim;
                    break;
                case DrivingState.Stop:
                    animTypes = Type.StopDrivingAnim;
                    break;
            }
            string animType = null;
            // 随机或者按方向获取
            if (null != animTypes && animTypes.Any())
            {
                int facing = animTypes.Count();
                int index = 0;
                if (facing > 1)
                {
                    if (facing % 8 == 0)
                    {
                        CoordStruct tempCurrentPos = currentPos;
                        tempCurrentPos.Z = 0;
                        CoordStruct tempBehindPos = behindPos;
                        tempBehindPos.Z = 0;
                        DirStruct dir = FLHHelper.Point2Dir(tempBehindPos, tempCurrentPos);
                        index = dir.Dir2FrameIndex(facing);
                    }
                    else
                    {
                        index = MathEx.Random.Next(0, facing);
                    }
                }
                animType = animTypes[index];
            }
            if (!string.IsNullOrEmpty(animType))
            {
                Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(animType);
                if (!pAnimType.IsNull)
                {
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, currentPos);
                    pAnim.Ref.Owner = pHouse;
                }
            }
        }

    }

}