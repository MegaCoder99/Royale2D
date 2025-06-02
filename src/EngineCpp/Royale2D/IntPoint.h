#pragma once

#include "json.hpp"

using json = nlohmann::json;

struct IntPoint
{
	int x;
	int y;
	NLOHMANN_DEFINE_TYPE_INTRUSIVE(IntPoint, x, y)
};

