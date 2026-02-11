using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhysicsElaSim.physics;
using S = SFML.System;

namespace PhysicsElaSim.src
{
    public static class MathP
    {
        public static S.Vector2f ToSF(Vector2 Physics_Vector)
        {
            return new(Physics_Vector.X,Physics_Vector.Y);
        }
        public static Vector2 ToP(S.Vector2f SFML_Vector)
        {
            return new(SFML_Vector.X,SFML_Vector.Y);
        }
        public static Vector2 ToP(S.Vector2i SFML_Vector)
        {
            return new(SFML_Vector.X,SFML_Vector.Y);
        }
    }
}