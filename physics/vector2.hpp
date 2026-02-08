#pragma once

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