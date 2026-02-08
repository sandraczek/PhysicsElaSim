#include <SFML/Graphics.hpp>
#include "../physics/world.hpp"

int main()
{
	sf::RenderWindow window( sf::VideoMode( { 800, 600 } ), "SFML works!" );

	World world;
	RigidBody circle{};
	circle.mass = 10.f;
	world.bodies.push_back(circle);

	sf::CircleShape shape( 10.f );
	shape.setFillColor( sf::Color::Green );

	float Time = 0.f;
	sf::Clock clock;
	clock.reset();
	while ( window.isOpen() )
	{
		float deltaTime = clock.restart().asSeconds();
		while ( const std::optional event = window.pollEvent() )
		{
			if ( event->is<sf::Event::Closed>() )
				window.close();
		}
		
		world.update(deltaTime);
		shape.setPosition({world.bodies[0].pos.x, world.bodies[0].pos.y});

		window.clear();
		window.draw( shape );
		window.display();
	}
}