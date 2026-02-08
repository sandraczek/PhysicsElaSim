#pragma once

#include "vector2.hpp"
#include "shape.hpp"

class RigidBody
{
public:
    Vector2 pos;
    Vector2 velocity;
    Vector2 acceleration;
    float mass;//🤦‍♂️
    Shape shape;

    RigidBody(Vector2 pos = Vector2::zero(), Vector2 velocity = {0.f, 0.f}, Vector2 acceleration = Vector2{}, float mass = 0.f, Shape shape = Shape{})
        : pos(pos), velocity(velocity), acceleration(acceleration), mass(mass), shape(shape) {}
};