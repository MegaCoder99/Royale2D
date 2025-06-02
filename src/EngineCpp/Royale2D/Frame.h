#pragma once
#include <string>
#include <vector>
#include <optional>
#include <SFML/Graphics/Texture.hpp>

#include "IntPoint.h"
#include "IntRect.h"
#include "json.hpp"

using json = nlohmann::json;

class Frame
{
public:
    IntRect rect;
    int duration;
    IntPoint offset;
    std::string spritesheetName;
    
    sf::Texture* texture;
    void init();

    NLOHMANN_DEFINE_TYPE_INTRUSIVE(Frame, rect, duration, offset, spritesheetName)
};
