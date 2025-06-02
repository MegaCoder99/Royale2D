#include "Actor.h"

void Actor::render(sf::RenderWindow& window)
{
	sf::Sprite spriteToDraw(*currentFrame->texture);

	spriteToDraw.setPosition(sf::Vector2(pos.x + currentFrame->offset.x, pos.y + currentFrame->offset.y));
	
	IntRect frameRect = currentFrame->rect;

	if (sprite->alignment == "center")
	{
		spriteToDraw.setOrigin(sf::Vector2(
			(float)((frameRect.x2 - frameRect.x1) / 2),
			(float)((frameRect.y2 - frameRect.y1) / 2)
		));
	}

	sf::IntRect sfmlIntRect = sf::IntRect(
		sf::Vector2(frameRect.x1, frameRect.y1),
		sf::Vector2(frameRect.x2 - frameRect.x1, frameRect.y2 - frameRect.y1)
	);

	if (xDir == -1)
	{
		spriteToDraw.setScale(sf::Vector2f(-1, 1));
	}
	else
	{
		spriteToDraw.setScale(sf::Vector2f(1, 1));
	}

	spriteToDraw.setTextureRect(sfmlIntRect);

	window.draw(spriteToDraw);
}

void Actor::update(std::vector<std::vector<bool>> collisionGrid)
{
	frameNum++;
	if (frameNum > currentFrame->duration) {
		frameNum = 0;
		frameIndex++;
		if (frameIndex >= sprite->frames.size()) {
			frameIndex = 0;
		}
		currentFrame = &sprite->frames[frameIndex];
	}
}

void Actor::changeSprite(const std::string& spriteName)
{
	sprite = &assets.sprites.at(spriteName);
	frameNum = 0;
	frameIndex = 0;
	currentFrame = &sprite->frames[0];
}