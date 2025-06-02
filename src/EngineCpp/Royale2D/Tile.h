#pragma once

#include "IntPoint.h"
#include <string>

#include "json.hpp"

using json = nlohmann::json;

class Tile
{
public:
	int id;
	int hitboxMode;
	std::string imageFileName;
	IntPoint imageTopLeftPos;

	NLOHMANN_DEFINE_TYPE_INTRUSIVE(Tile, id, hitboxMode, imageFileName, imageTopLeftPos)
};

