#include "Level.h"
#include "Character.h"
#include "Assets.h"
#include "Helpers.h"

#include <unordered_map>
#include <fstream>
#include <filesystem>
#include <iostream>
namespace fs = std::filesystem;

Level::Level()
{
    std::string levelImagesFolderPath = rootAssetPath + "/maps/sample_map/images";
    for (const auto& entry : fs::directory_iterator(levelImagesFolderPath)) {
        if (entry.is_regular_file() && entry.path().extension() == ".png") {
            std::string filename = entry.path().filename().string();

            sf::Texture texture;
            if (texture.loadFromFile(entry.path().string())) {
                assets.textureMap[filename] = std::move(texture);
            }
            else {
                std::cerr << "Failed to load texture: " << entry.path() << std::endl;
            }
        }
    }

    backgrounds.push_back(&assets.textureMap["main.000.png"]);
    backgrounds.push_back(&assets.textureMap["main.100.png"]);
    backgrounds.push_back(&assets.textureMap["main.200.png"]);

    std::string tilesetPath = rootAssetPath + "/maps/sample_map/tileset/tileset.json";
    std::ifstream file(tilesetPath);
    if (!file) throw std::runtime_error("Failed to open tileset.json");
    json j;
    file >> j;
    auto tileset = j.get<std::unordered_map<std::string, Tile>>();
    
    std::string mapSectionsPath = rootAssetPath + "/maps/sample_map/map_sections/main.json";
    std::ifstream file2(mapSectionsPath);
    if (!file2) throw std::runtime_error("Failed to open main.json");
    json j2;
    file2 >> j2;
    auto sampleMapSection = j2.get<MapSection>();

    // For now only use the middle layer for tile grid/collision since that has all the collision hitboxes
    parseTileGridString(sampleMapSection.layers[1].tileGrid, tileset);

    view.setSize(sf::Vector2f(256, 224));
    pos = Point(128, 112);
    width = backgrounds[0]->getSize().x;
    height = backgrounds[0]->getSize().y;
}

void Level::parseTileGridString(const std::string& tileGridStr, const std::unordered_map<std::string, Tile>& tileset)
{
    std::vector<uint8_t> bytes = decodeBase64(tileGridStr);

    if (bytes.size() < 4) throw std::runtime_error("Invalid base64 input: too short");

    int numCols = (bytes[0] & 0xFF) | ((bytes[1] & 0xFF) << 8);
    int numRows = (bytes[2] & 0xFF) | ((bytes[3] & 0xFF) << 8);

    tileCollisionGrid = std::vector<std::vector<bool>>(numRows, std::vector<bool>(numCols));

    size_t byteIndex = 4;

    for (int i = 0; i < numRows; ++i) 
    {
        for (int j = 0; j < numCols; ++j) 
        {
            if (byteIndex + 1 >= bytes.size()) throw std::runtime_error("Unexpected end of data");
            int value = (bytes[byteIndex++] & 0xFF) | ((bytes[byteIndex++] & 0xFF) << 8);
            auto tile = tileset.at(std::to_string(value));
            tileCollisionGrid[i][j] = tile.hitboxMode > 0;
        }
    }
}

void Level::addActor(const std::shared_ptr<Actor>& actor)
{
    actors.push_back(actor);
}

void Level::update()
{
    for (auto& actor : actors)
    {
        actor->update(tileCollisionGrid);
        if (std::shared_ptr<Character> character = std::dynamic_pointer_cast<Character>(actor))
        {
            if (character->pos.x > 128 && character->pos.x < width - 128)
            {
                pos.x = character->pos.x;
            }

            if (character->pos.y > 112 && character->pos.y < height - 112)
            {
                pos.y = character->pos.y;
            }
        }
    }

    view.setCenter(sf::Vector2f(pos.x, pos.y));
}

void Level::render(sf::RenderWindow& window)
{
    window.setView(view);

    for (int i = 0; i < backgrounds.size(); i++)
    {
        sf::Texture texture = *backgrounds[i];
        sf::Sprite sprite(texture);
        sprite.setPosition(sf::Vector2f(0, 0));
        window.draw(sprite);

        // For now assume all actors are in layer 1
        if (i == 1)
        {
            for (auto& actor : actors)
            {
                actor->render(window);
            }
        }
    }
}