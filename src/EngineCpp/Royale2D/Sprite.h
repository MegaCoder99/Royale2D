#pragma once
#include <string>
#include <vector>
#include <SFML/Graphics/Texture.hpp>

#include "Frame.h"
#include "json.hpp"

using json = nlohmann::json;

class Sprite
{
public:
    std::vector<Frame> frames;
    int loopStartFrame = 0;

    std::string alignment = "center";
    std::string wrapMode = "once";

    std::string name = "";

    void init();

    NLOHMANN_DEFINE_TYPE_INTRUSIVE(Sprite, frames, loopStartFrame, alignment, wrapMode)
};