#include <SFML/Graphics.hpp>
#include "Assets.h"
#include "Level.h"
#include "Character.h"

int main()
{
    unsigned int windowScale = 4;
    sf::RenderWindow window(sf::VideoMode({ 256 * windowScale, 224 * windowScale }), "Royale 2D");

    assets.load();

    Level level;
    auto player = std::make_shared<Character>(Point(500, 500));
	level.addActor(player);
    window.setFramerateLimit(60);

    while (window.isOpen())
    {
        while (const std::optional event = window.pollEvent())
        {
            if (event->is<sf::Event::Closed>())
            {
                window.close();
            }
        }

        window.clear();
        
        level.render(window);
        level.update();

        window.display();
    }
}