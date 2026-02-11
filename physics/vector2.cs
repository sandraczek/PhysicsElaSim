using System.ComponentModel.DataAnnotations;
using System;
namespace PhysicsElaSim.physics
{
    public readonly struct Vector2(float x, float y): IEquatable<Vector2>
    {
        public readonly float X = x;
        public readonly float Y = y;

        public static readonly Vector2 Zero = new (0f,0f); //😍
        public static readonly Vector2 Right = new (1f,0f);
        public static readonly Vector2 Up = new (0f,-1f);
        public static readonly Vector2 Left = new (-1f,0f);
        public static readonly Vector2 Down = new (0f,1f);
        

        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float d) => new(a.X * d, a.Y * d);
        public static Vector2 operator-(Vector2 v) => new(-v.X, -v.Y);
        public static bool operator ==(Vector2 a, Vector2 b) => a.Equals(b);
        public static bool operator !=(Vector2 a, Vector2 b) => !a.Equals(b);
        
        public float Length() => (float)Math.Sqrt((double)X*X + (double)Y*Y);
        public float LengthSquared() => X*X + Y*Y;
        public Vector2 Normalized(){
            float len = Length();
            if(len == 0f) return Zero;
            return new(X/len,Y/len);
        }
        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;    
        static public float Distance(Vector2 A, Vector2 B) => (A - B).Length();
        public bool Equals(Vector2 other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj)
        {
            return obj is Vector2 other && Equals(other);
        }
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString()
        {
            return "[" + X.ToString() + "," + Y.ToString() + "]";
        }
    }
}