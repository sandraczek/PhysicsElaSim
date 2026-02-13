
namespace PhysicsElaSim.physics
{
    readonly struct Collision(RigidBody a, RigidBody b, Vector2 n, Vector2 cp, float d)
    {
        public readonly RigidBody BodyA = a;
        public readonly RigidBody BodyB = b;
        public readonly Vector2 Normal = n;
        public readonly Vector2 ContactPoint = cp;
        public readonly float Depth = d;
    }
}