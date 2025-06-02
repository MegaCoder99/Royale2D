#pragma once

#include <string>
#include <vector>
#include "json.hpp"

using json = nlohmann::json;

class MapSectionLayer
{
public:
	std::string tileGrid;

	NLOHMANN_DEFINE_TYPE_INTRUSIVE(MapSectionLayer, tileGrid)
};

class MapSection
{
public:
	std::vector<MapSectionLayer> layers;

	NLOHMANN_DEFINE_TYPE_INTRUSIVE(MapSection, layers)
};

