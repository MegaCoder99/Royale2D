#pragma once
#include <vector>
#include <memory>
#include <SFML/Graphics.hpp>
#include "Actor.h"
#include "Point.h"
#include "Tile.h"

class Level
{
public:
    std::vector<sf::Texture*> backgrounds;
    std::vector<std::shared_ptr<Actor>> actors;
    std::vector<std::vector<bool>> tileCollisionGrid;
    int width;
    int height;

    sf::View view;
    Point pos = Point(0, 0);

    Level();

    void addActor(const std::shared_ptr<Actor>& actor);
    void parseTileGridString(const std::string& tileGridStr, const std::unordered_map<std::string, Tile>& tileset);
    void update();
    void render(sf::RenderWindow& window);
};

