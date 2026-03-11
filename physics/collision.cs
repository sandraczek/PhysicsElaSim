
namespace PhysicsElaSim.physics
{
    readonly struct Collision(RigidBody a, RigidBody b, Vector2 normal, Vector2[] cps, float[] depths)
    {
        public readonly RigidBody BodyA = a;
        public readonly RigidBody BodyB = b;
        public readonly Vector2 Normal = normal;
        public readonly Vector2[] ContactPoints = cps;
        public readonly float[] Depths = depths;
    }
}