#pragma once

#include "json.hpp"

using json = nlohmann::json;

struct IntRect
{
	int x1;
	int y1;
	int x2;
	int y2;
	NLOHMANN_DEFINE_TYPE_INTRUSIVE(IntRect, x1, y1, x2, y2)
};

