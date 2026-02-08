#pragma once

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