#pragma once

#include "Sprite.h"
#include "Point.h"
#include "Assets.h"
#include <SFML/Graphics.hpp>

class Actor
{
public:
	Sprite* sprite = nullptr;
	Frame* currentFrame = nullptr;
	Point pos;
	int frameNum = 0;
	int frameIndex = 0;
	int xDir = 1;

	Actor(const std::string& spriteName, Point pos) : 
		sprite(&assets.sprites.at(spriteName)), pos(pos)
	{
		currentFrame = &sprite->frames[0];
	}

	void render(sf::RenderWindow& window);
	virtual void update(std::vector<std::vector<bool>> collisionGrid);
	void changeSprite(const std::string& spriteName);
};

