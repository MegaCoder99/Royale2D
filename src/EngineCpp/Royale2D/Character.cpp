#include "Character.h"
#include <SFML/Window/Keyboard.hpp>
#include "Assets.h"

bool Character::collidedWith(std::vector<std::vector<bool>> collisionGrid)
{
	// Character's collision width and height assumed to be 16x16 pixels, or 2x2 tiles

	int yOff = 3;

	int startGridI = ((pos.y + yOff) / 8) - 1;
	if (startGridI < 0) startGridI = 0;
	if (startGridI > collisionGrid.size() - 1) startGridI = collisionGrid.size() - 1;

	int endGridI = startGridI + 2;
	if (endGridI < 0) endGridI = 0;
	if (endGridI > collisionGrid.size() - 1) endGridI = collisionGrid.size() - 1;

	int startGridJ = (pos.x / 8) - 1;
	if (startGridJ < 0) startGridJ = 0;
	if (startGridJ > collisionGrid[0].size() - 1) startGridJ = collisionGrid[0].size() - 1;

	int endGridJ = startGridJ + 2;
	if (endGridJ < 0) endGridJ = 0;
	if (endGridJ > collisionGrid[0].size() - 1) endGridJ = collisionGrid[0].size() - 1;

	if (collisionGrid[startGridI][startGridJ] == true || collisionGrid[startGridI][endGridJ] == true ||
		collisionGrid[endGridI][startGridJ] == true || collisionGrid[endGridI][endGridJ] == true)
	{
		return true;
	}
}

void Character::update(std::vector<std::vector<bool>> collisionGrid)
{
	Actor::update(collisionGrid);
	bool moved = false;

	int speed = 1;
	if (sf::Keyboard::isKeyPressed(sf::Keyboard::Key::LShift))
	{
		speed = 5;
	}

	Point origPos = pos;

	if (sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Down))
	{
		pos.y += speed;
		if (sprite != &assets.sprites["char_idle_down_move"])
		{
			changeSprite("char_idle_down_move");
		}
		xDir = 1;
		moved = true;
		while (collidedWith(collisionGrid))
		{
			pos.y--;
		}
	}
	else if (sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Up))
	{
		pos.y -= speed;
		if (sprite != &assets.sprites["char_idle_up_move"])
		{
			changeSprite("char_idle_up_move");
		}
		xDir = 1;
		moved = true;
		while (collidedWith(collisionGrid))
		{
			pos.y++;
		}
	}

	if (sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Left))
	{
		pos.x -= speed;
		if (!moved)
		{
			if (sprite != &assets.sprites["char_idle_right_move"])
			{
				changeSprite("char_idle_right_move");
			}
			xDir = -1;
			moved = true;
		}
		while (collidedWith(collisionGrid))
		{
			pos.x++;
		}
	}
	else if (sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Right))
	{
		pos.x += speed;
		if (!moved)
		{
			if (sprite != &assets.sprites["char_idle_right_move"])
			{
				changeSprite("char_idle_right_move");
			}
			xDir = 1;
			moved = true;
		}
		while (collidedWith(collisionGrid))
		{
			pos.x--;
		}
	}

	if (!moved)
	{
		if (sprite == &assets.sprites["char_idle_down_move"])
		{
			changeSprite("char_idle_down");
		}
		else if (sprite == &assets.sprites["char_idle_right_move"])
		{
			changeSprite("char_idle_right");
		}
		else if (sprite == &assets.sprites["char_idle_up_move"])
		{
			changeSprite("char_idle_up");
		}
	}
}