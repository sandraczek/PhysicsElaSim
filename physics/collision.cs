
namespace PhysicsElaSim.physics
{
    readonly struct Collision
    {
        public readonly RigidBody BodyA;
        public readonly RigidBody BodyB;
        public readonly Vector2 Normal;
        public readonly float Depth;

        public Collision(RigidBody a,RigidBody b,Vector2 n,float d)
        {
            BodyA = a;
            BodyB = b;
            Normal = n;
            Depth = d;
        }
    }
}