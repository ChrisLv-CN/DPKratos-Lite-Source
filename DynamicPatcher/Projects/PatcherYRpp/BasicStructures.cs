﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [DebuggerDisplay("RGB={R}, {G}, {B}")]
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct ColorStruct : IEquatable<ColorStruct>
    {
        public static ColorStruct Empty = default;
        public static ColorStruct Red = new ColorStruct(252, 0, 0);
        public static ColorStruct Green = new ColorStruct(0, 252, 0);
        public static ColorStruct Blue = new ColorStruct(0, 0, 252);
        public static ColorStruct White = new ColorStruct(252, 252, 252);
        public static ColorStruct Black = new ColorStruct(3, 3, 3);

        public ColorStruct(int r, int g, int b)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
        }

        public byte R;
        public byte G;
        public byte B;

        public static bool operator ==(ColorStruct a, ColorStruct b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(ColorStruct a, ColorStruct b) => !(a == b);

        public bool Equals(ColorStruct other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.R == other.R && this.G == other.G && this.B == other.B;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ColorStruct)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{{\"R\":{0}, \"G\":{1}, \"B\":{2}}}", R, G, B);
        }
    }

    [DebuggerDisplay("XYZ={X}, {Y}, {Z}")]
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct CoordStruct : IEquatable<CoordStruct>
    {
        public static CoordStruct Empty = default;

        public CoordStruct(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public CoordStruct(float x, float y, float z)
        {
            X = (int)x;
            Y = (int)y;
            Z = (int)z;
        }

        public CoordStruct(double x, double y, double z)
        {
            X = (int)x;
            Y = (int)y;
            Z = (int)z;
        }

        public static CoordStruct operator -(CoordStruct a)
        {
            return new CoordStruct(-a.X, -a.Y, -a.Z);
        }
        public static CoordStruct operator +(CoordStruct a, CoordStruct b)
        {
            return new CoordStruct(
                 a.X + b.X,
                 a.Y + b.Y,
                 a.Z + b.Z);
        }
        public static CoordStruct operator -(CoordStruct a, CoordStruct b)
        {
            return new CoordStruct(
                 a.X - b.X,
                 a.Y - b.Y,
                 a.Z - b.Z);
        }
        public static CoordStruct operator *(CoordStruct a, double r)
        {
            return new CoordStruct(
                 (int)(a.X * r),
                 (int)(a.Y * r),
                 (int)(a.Z * r));
        }

        public static double operator *(CoordStruct a, CoordStruct b)
        {
            return a.X * b.X
                 + a.Y * b.Y
                 + a.Z * b.Z;
        }
        //magnitude
        public double Magnitude()
        {
            return Math.Sqrt(MagnitudeSquared());
        }
        //magnitude squared
        public double MagnitudeSquared()
        {
            return this * this;

        }

        public double DistanceFrom(CoordStruct other)
        {
            return (other - this).Magnitude();
        }


        public bool Equals(CoordStruct other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }


        public static bool operator ==(CoordStruct a, CoordStruct b)
        {
            return Equals(a, b);
        }
        public static bool operator !=(CoordStruct a, CoordStruct b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CoordStruct)obj);
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return string.Format("{{\"X\":{0}, \"Y\":{1}, \"Z\":{2}}}", X, Y, Z);
        }

        public int X;
        public int Y;
        public int Z;
    }

    [DebuggerDisplay("XYZ={X}, {Y}, {Z}")]
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct BulletVelocity : IEquatable<BulletVelocity>
    {
        public static BulletVelocity Empty = default;

        public BulletVelocity(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static BulletVelocity operator -(BulletVelocity a)
        {
            return new BulletVelocity(-a.X, -a.Y, -a.Z);
        }
        public static BulletVelocity operator +(BulletVelocity a, BulletVelocity b)
        {
            return new BulletVelocity(
                 a.X + b.X,
                 a.Y + b.Y,
                 a.Z + b.Z);
        }
        public static BulletVelocity operator -(BulletVelocity a, BulletVelocity b)
        {
            return new BulletVelocity(
                 a.X - b.X,
                 a.Y - b.Y,
                 a.Z - b.Z);
        }
        public static BulletVelocity operator *(BulletVelocity a, double r)
        {
            return new BulletVelocity(
                 (double)(a.X * r),
                 (double)(a.Y * r),
                 (double)(a.Z * r));
        }

        public static double operator *(BulletVelocity a, BulletVelocity b)
        {
            return a.X * b.X
                 + a.Y * b.Y
                 + a.Z * b.Z;
        }
        //magnitude
        public double Magnitude()
        {
            return Math.Sqrt(MagnitudeSquared());
        }
        //magnitude squared
        public double MagnitudeSquared()
        {
            return this * this;

        }

        public double DistanceFrom(BulletVelocity other)
        {
            return (other - this).Magnitude();
        }

        public bool Equals(BulletVelocity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        public static bool operator ==(BulletVelocity a, BulletVelocity b)
        {
            return Equals(a, b);
        }
        public static bool operator !=(BulletVelocity a, BulletVelocity b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BulletVelocity)obj);
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return string.Format("{{\"X\":{0}, \"Y\":{1}, \"Z\":{2}}}", X, Y, Z);
        }

        public double X;
        public double Y;
        public double Z;
    }

    [DebuggerDisplay("XY={X}, {Y}")]
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct CellStruct : IEquatable<CellStruct>
    {
        public static CellStruct Empty = default;

        public CellStruct(int x, int y)
        {
            X = (short)x;
            Y = (short)y;
        }

        public static CellStruct operator -(CellStruct a)
        {
            return new CellStruct(-a.X, -a.Y);
        }
        public static CellStruct operator +(CellStruct a, CellStruct b)
        {
            return new CellStruct(
                 a.X + b.X,
                 a.Y + b.Y);
        }
        public static CellStruct operator -(CellStruct a, CellStruct b)
        {
            return new CellStruct(
                 a.X - b.X,
                 a.Y - b.Y);
        }
        public static CellStruct operator *(CellStruct a, double r)
        {
            return new CellStruct(
                 (int)(a.X * r),
                 (int)(a.Y * r));
        }

        public static double operator *(CellStruct a, CellStruct b)
        {
            return a.X * b.X
                 + a.Y * b.Y;
        }
        //magnitude
        public double Magnitude()
        {
            return Math.Sqrt(MagnitudeSquared());
        }
        //magnitude squared
        public double MagnitudeSquared()
        {
            return this * this;

        }

        public double DistanceFrom(CellStruct other)
        {
            return (other - this).Magnitude();
        }

        public bool Equals(CellStruct other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.X == other.X && this.Y == other.Y;
        }

        public static bool operator ==(CellStruct a, CellStruct b)
        {
            return Equals(a, b);
        }
        public static bool operator !=(CellStruct a, CellStruct b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CellStruct)obj);
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return string.Format("{{\"X\":{0}, \"Y\":{1}}}", X, Y);
        }


        public short X;
        public short Y;
    }

    //Random number range
    [DebuggerDisplay("XYZ={X}, {Y}, {Z}")]
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct RandomStruct
    {
        public int Min, Max;
    };


    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct SingleVector3D
    {
        public SingleVector3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public SingleVector3D(double x, double y, double z)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
        }

        public static SingleVector3D operator -(SingleVector3D a)
        {
            return new SingleVector3D(-a.X, -a.Y, -a.Z);
        }
        public static SingleVector3D operator +(SingleVector3D a, SingleVector3D b)
        {
            return new SingleVector3D(
                 a.X + b.X,
                 a.Y + b.Y,
                 a.Z + b.Z);
        }
        public static SingleVector3D operator -(SingleVector3D a, SingleVector3D b)
        {
            return new SingleVector3D(
                 a.X - b.X,
                 a.Y - b.Y,
                 a.Z - b.Z);
        }
        public static SingleVector3D operator *(SingleVector3D a, double r)
        {
            return new SingleVector3D(
                 (float)(a.X * r),
                 (float)(a.Y * r),
                 (float)(a.Z * r));
        }

        public static double operator *(SingleVector3D a, SingleVector3D b)
        {
            return a.X * b.X
                 + a.Y * b.Y
                 + a.Z * b.Z;
        }
        //magnitude
        public double Magnitude()
        {
            return Math.Sqrt(MagnitudeSquared());
        }
        //magnitude squared
        public double MagnitudeSquared()
        {
            return this * this;
        }

        public double DistanceFrom(SingleVector3D other)
        {
            return (other - this).Magnitude();
        }

        public static bool operator ==(SingleVector3D a, SingleVector3D b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }
        public static bool operator !=(SingleVector3D a, SingleVector3D b) => !(a == b);

        public override bool Equals(object obj) => this == (SingleVector3D)obj;
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return string.Format("{{\"X\":{0}, \"Y\": {1}, \"Z\": {2}}}", X, Y, Z);
        }

        public float X;
        public float Y;
        public float Z;
    }

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct Quaternion_
    {
        public Quaternion_(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float X;
        public float Y;
        public float Z;
        public float W;
    };

    [DebuggerDisplay("XY={X}, {Y}")]
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct Point2D : IEquatable<Point2D>
    {
        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point2D operator -(Point2D a)
        {
            return new Point2D(-a.X, -a.Y);
        }
        public static Point2D operator +(Point2D a, Point2D b)
        {
            return new Point2D(
                 a.X + b.X,
                 a.Y + b.Y);
        }
        public static Point2D operator -(Point2D a, Point2D b)
        {
            return new Point2D(
                 a.X - b.X,
                 a.Y - b.Y);
        }
        public static Point2D operator *(Point2D a, double r)
        {
            return new Point2D(
                 (int)(a.X * r),
                 (int)(a.Y * r));
        }

        public static double operator *(Point2D a, Point2D b)
        {
            return a.X * b.X
                 + a.Y * b.Y;
        }
        //magnitude
        public double Magnitude()
        {
            return Math.Sqrt(MagnitudeSquared());
        }
        //magnitude squared
        public double MagnitudeSquared()
        {
            return this * this;
        }

        public double DistanceFrom(Point2D other)
        {
            return (other - this).Magnitude();
        }

        public bool Equals(Point2D other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.X == other.X && this.Y == other.Y;
        }

        public static bool operator ==(Point2D a, Point2D b)
        {
            return Equals(a, b);
        }
        public static bool operator !=(Point2D a, Point2D b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Point2D)obj);
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return string.Format("{{\"X\":{0}, \"Y\":{1}}}", X, Y);
        }

        public int X;
        public int Y;
    }

}

