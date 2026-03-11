
namespace PhysicsElaSim.physics
{
    public class Softbody : Body
    {
        public struct Particle(Vector2 position, Vector2 previousPosition, float invMass)
        {
            public Vector2 Position = position;
            public Vector2 PreviousPosition = previousPosition;
            public float InvMass = invMass;
        }

        public struct Constraint(int particleId1, int particleId2, float restLength)
        {
            public int ParticleId1 = particleId1;
            public int ParticleId2 = particleId2;
            public float Length = restLength;
        }
        public Particle[] Particles;
        public Constraint[] Constraints;
        public float Stiffness;

        public Softbody()
        {
            Particles = [];
            Constraints = [];
        }

        public Softbody(Particle[] particles, Constraint[] constraints, float stiffness)
        {
            Particles = particles;
            Constraints = constraints;
            Stiffness = stiffness;
        }

        public void SolveConstraints()
        {
            foreach (var constraint in Constraints)
            {
                ref Particle p1 = ref Particles[constraint.ParticleId1];
                ref Particle p2 = ref Particles[constraint.ParticleId2];

                Vector2 delta = p2.Position - p1.Position;
                float currentLength = delta.Length();

                float diff = (currentLength - constraint.Length) / currentLength;
                float invMassSum = p1.InvMass + p2.InvMass;

                Vector2 correction = delta * diff * (1f / invMassSum) * Stiffness;

                p1.Position += correction * p1.InvMass;
                p2.Position += -correction * p2.InvMass;
            }
        }

        public void UpdatePosition(float dt, Vector2 gravity)
        {
            for (int i = 0; i < Particles.Length; i++)
            {
                if (Particles[i].InvMass == 0) continue; //static particle
                Vector2 velocity = Particles[i].Position - Particles[i].PreviousPosition;
                Vector2 posTemp = Particles[i].Position;
                Particles[i].Position += velocity + (gravity * dt * dt);
                Particles[i].PreviousPosition = posTemp;
            }
        }

        // public static Softbody CreateRectangularSoftbody(Vector2 pos, int width, int height, float spacing, float mass, float stiffness)
        // {
        //     Particle[] particles = new Particle[width * height];
        //     for 
        // }
    }
}