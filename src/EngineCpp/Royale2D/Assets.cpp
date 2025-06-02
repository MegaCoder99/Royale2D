#include "Assets.h"
#include "json.hpp"

#include <iostream>
#include <vector>
#include <filesystem>
#include <fstream>

namespace fs = std::filesystem;
using json = nlohmann::json;

Assets assets;
std::string rootAssetPath = "./../../../assets";

void Assets::load()
{
    std::string spritesheetFolderPath = rootAssetPath + "/spritesheets";

    for (const auto& entry : fs::directory_iterator(spritesheetFolderPath)) {
        if (entry.is_regular_file() && entry.path().extension() == ".png") {
            std::string filename = entry.path().filename().string();

            sf::Texture texture;
            if (texture.loadFromFile(entry.path().string())) {
                textureMap[filename] = std::move(texture);
            }
            else {
                std::cerr << "Failed to load texture: " << entry.path() << std::endl;
            }
        }
    }

    std::string spriteFolderPath = rootAssetPath + "/sprites";

    for (const auto& entry : fs::directory_iterator(spriteFolderPath)) {
        if (entry.path().extension() == ".json") {
            std::ifstream inFile(entry.path());
            if (inFile) {
                try {
                    json j;
                    inFile >> j;
                    Sprite sprite = j.get<Sprite>();
                    
                    std::string fileNameWithoutExtension = entry.path().stem().string();
                    sprite.name = fileNameWithoutExtension;
                    sprite.init();

                    sprites[fileNameWithoutExtension] = std::move(sprite);
                }
                catch (const std::exception& e) {
                    std::cerr << "Failed to parse " << entry.path() << ": " << e.what() << "\n";
                }
            }
        }
    }
}