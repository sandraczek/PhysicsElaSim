using System.ComponentModel.DataAnnotations;
using System;
namespace PhysicsElaSim.physics
{
    public readonly struct Vector2
    {
        public readonly float X;
        public readonly float Y;

        public Vector2(float x, float y){X = x; Y = y; }
        
        public static readonly Vector2 Zero = new (0f,0f); //😍

        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float d) => new(a.X * d, a.Y * d);
        public static Vector2 operator-(Vector2 v) => new(-v.X, -v.Y);
        public float Length() => (float)Math.Sqrt((double)X*X + (double)Y*Y);
        public float LengthSquared() => X*X + Y*Y;
        public Vector2 Normalized(){
            float len = Length();
            if(len == 0f) return Zero;
            return new(X/len,Y/len);
        }
        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X - a.Y * b.Y;    
        static public float Distance(Vector2 A, Vector2 B) => (A - B).Length();
        public bool Equals(Vector2 other) => X == other.X && Y == other.Y;
    }
}