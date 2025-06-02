#include "Sprite.h"

void Sprite::init()
{
	for (int i = 0; i < frames.size(); i++)
	{
		frames[i].init();
	}
}