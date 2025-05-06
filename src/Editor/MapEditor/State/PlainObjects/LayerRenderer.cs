using Editor;
using System.Drawing;

namespace MapEditor;

public class LayerRenderer
{
    public BitmapDrawer? layerContainerDrawer;

    public void Redraw(RedrawFlag redrawFlag, SectionsSC sectionsSC)
    {
        int TS = sectionsSC.TS;
        Tileset tileset = sectionsSC.tileset;

        int width = sectionsSC.selectedMapSection.firstLayer.tileGrid.GetLength(1) * TS;
        int height = sectionsSC.selectedMapSection.firstLayer.tileGrid.GetLength(0) * TS;

        if (layerContainerDrawer == null)
        {
            layerContainerDrawer = new BitmapDrawer(width, height);
            redrawFlag = RedrawFlag.All;
        }
        else if (layerContainerDrawer.width != width || layerContainerDrawer.height != height)
        {
            layerContainerDrawer.Resize(width, height);
            redrawFlag = RedrawFlag.All;
        }

        if (redrawFlag == RedrawFlag.Tooling)
        {
            return;
        }

        bool didRedraw = false;
        foreach (MapSectionLayer layer in sectionsSC.selectedMapSection.layers)
        {
            // If the layer bitmap is null (we lazy load), redraw it no matter the flag
            if (layer.drawer == null)
            {
                layer.drawer = tileset.CreateDrawerAndDrawTileGrid(layer.tileGrid);
                didRedraw = true;
                continue;
            }
            
            if (layer.drawer.width != width || layer.drawer.height != height)
            {
                layer.drawer.Resize(width, height);
                didRedraw = true;
            }
            
            if (redrawFlag == RedrawFlag.All)
            {
                tileset.RedrawEntireTileBitmap(layer.drawer, layer.tileGrid);
                didRedraw = true;
            }
            else if (redrawFlag == RedrawFlag.Diffs)
            {
                didRedraw = layer.RedrawTileBitmapDiff(sectionsSC) || didRedraw;
            }
        }

        if (didRedraw || redrawFlag == RedrawFlag.Container)
        {
            layerContainerDrawer.Clear(sectionsSC.magentaBgColor ? Color.Magenta : sectionsSC.canvas.backgroundColor);

            // Draw unselected layers below selected one, if the setting is on
            // If this behavior is undesirable can uncomment out the block further down and comment this one out
            for (int i = 0; i < sectionsSC.selectedMapSection.layers.Count; i++)
            {
                MapSectionLayer layer = sectionsSC.selectedMapSection.layers[i];
                if (!layer.isSelected && sectionsSC.showUnselectedLayers)
                {
                    layerContainerDrawer.DrawImage(layer.GetDrawer(tileset), 0, 0, alpha: 0.5f);
                }
            }

            for (int i = 0; i < sectionsSC.selectedMapSection.layers.Count; i++)
            {
                MapSectionLayer layer = sectionsSC.selectedMapSection.layers[i];
                if (layer.isSelected)
                {
                    layerContainerDrawer.DrawImage(layer.GetDrawer(tileset), 0, 0);
                }
                /*
                else if (sectionsSC.showUnselectedLayers)
                {
                    layerContainerDrawer.DrawImage(layer.GetDrawer(tileset), 0, 0, alpha: 0.5f);
                }
                */
            }
        }
    }
}
