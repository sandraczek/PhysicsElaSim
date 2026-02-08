#pragma once

#include "vector2.hpp"
#include "rigidbody.hpp"
#include <vector>

class World 
{
public:
    Vector2 gravity;
    std::vector<RigidBody> bodies;

    World(Vector2 gravity = Vector2{0, 9.81}): gravity(gravity), bodies() {}

    void update(float dt) {
        for (auto& body : bodies) {
            body.acceleration = Vector2{};
            body.acceleration += gravity;
            
            body.velocity += body.acceleration * dt;
            
            body.pos += body.velocity * dt;
            
            //resolve collisions

        }
    }    //😎
};