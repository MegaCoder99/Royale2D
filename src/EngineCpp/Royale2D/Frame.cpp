#include "frame.h"
#include "Assets.h"

void Frame::init()
{
	texture = &assets.textureMap[spritesheetName];
}