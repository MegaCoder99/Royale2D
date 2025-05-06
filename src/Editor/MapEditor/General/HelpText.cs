namespace MapEditor;

public class HelpText
{
    public const string EditTileDataModeHotkeys =
@"NumPad5 (or S) - FullTile Hitbox
NumPad0 (or D) - No Hitbox

NumPad7 - DiagTopLeft Hitbox
NumPad9 - DiagTopRight Hitbox
NumPad1 - DiagBotLeft Hitbox
NumPad3 - DiagBotRight Hitbox

= - Add Tile Variant (+r)
_ - Add Tile Variant (-r)";

    public const string TilePaintModeHotkeys =
@"C - Place Tool (From Copy)
X - Place Tool (From Cut)
Ctrl+C - Copy Tiles To Clipboard
Ctrl+X - Cut Tiles To Clipboard
Ctrl+V - Paste Tiles From Clipboard
Shift+V - Paste And Replace Tiles

D - Erase Selected Tiles
Arrows - Move Selected
H - Flip Selection Horizontal
T - Flip Selection Vertical
Esc - Unselect (In Select Tool)
Esc - To Select Tool (In Other Tools)

Q - Change To Layer Below
W - Enable All Layers
E - Change To Layer Above
Ctrl+Q - Move Tiles To Layer Below
Ctrl+E - Move Tiles To Layer Above";


    public const string EditEntityModeHotkeys =
@"Ctrl+C - Copy Instance
Ctrl+X - Cut Instance
Ctrl+V - Paste Instance At Mouse

Arrows - Move Instance
Arrows - Resize Zone (x1,y1)
Shift+Arrows - Resize Zone (x2,y2)
D - Delete Entity";

}
