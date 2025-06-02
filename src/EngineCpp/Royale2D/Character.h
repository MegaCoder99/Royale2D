#pragma once

#include "Actor.h"

class Character : public Actor
{
public:
	Character(Point pos) : Actor("char_idle_down", pos) { }
	void update(std::vector<std::vector<bool>> collisionGrid);
	bool collidedWith(std::vector<std::vector<bool>> collisionGrid);
};

