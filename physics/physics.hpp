#include <vector>
#include <iostream>
#include <cmath>

class Vector2
{
public:
    float x;
    float y;

    Vector2(float x = 0.f, float y = 0.f): x(x), y(y) {}
    
    Vector2& operator+= (const Vector2& other) {
        x += other.x;
        y += other.y;
        return *this;
    }
    Vector2& operator-= (const Vector2& other) {
        x -= other.x;
        y -= other.y;
        return *this;
    }
    
    friend Vector2 operator+ (Vector2 left,const Vector2& right) {
        left+= right;
        return left;
    }
    friend Vector2 operator- (Vector2 left,const Vector2& right) {
        left-= right;
        return left;
    }

    static inline Vector2 zero() {return Vector2{0.f, 0.f};}
    
    
    Vector2 operator* (const float scalar) {
        return Vector2(x * scalar, y  * scalar);
    }
    Vector2 operator/ (const float scalar) {
        if(scalar == 0.f) throw "Division by Zero";
        return Vector2(x / scalar, y  / scalar);
    }

    Vector2 normalized(){
        float len = sqrt(x*x + y*y);
        if(len == 0.f) return Vector2::zero();
        return Vector2{x/len, y/len};
    }
};

class Shape {};

class Circle : public Shape 
{
public:
    float radius;
};

class Rectangle : public Shape
{
public:
    float width;
    float height;
};

class Body
{
public:
    Vector2 pos;
    Shape shape;
    // Quaternion rotation;
    
    Body(Vector2 pos = Vector2{}, Shape shape) : pos(pos), shape(shape) {}
};
class RigidBody : public Body
{
public:
    Vector2 velocity;
    Vector2 acceleration;
    float mass;//🤦‍♂️

    RigidBody(Vector2 pos = Vector2::zero(), Shape shape, Vector2 velocity = {0.f, 0.f}, Vector2 acceleration = Vector2{}, float mass = 0.f)
        : Body(pos, shape), velocity(velocity), acceleration(acceleration), mass(mass) {}
};

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
            std::cerr << "new pos: " << body.pos.x << ' ' << body.pos.y << '\n';
        }
    }    //😎
};

/*
Rigidbody (pos, velocity, )
 -> Shape (circle, rect)
*/