#pragma once

#include <unordered_map>
#include <string>
#include <vector>

#include "Sprite.h"
#include "Tile.h"
#include "MapSection.h"

class Assets
{
public:
	std::unordered_map<std::string, Sprite> sprites;
	std::unordered_map<std::string, sf::Texture> textureMap;
	std::vector<Tile> tileset;
	MapSection sampleMapSection;
	void load();
};

extern Assets assets;
extern std::string rootAssetPath;