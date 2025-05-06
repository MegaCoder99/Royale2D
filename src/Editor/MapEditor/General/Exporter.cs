using Editor;
using Shared;
using System.Drawing;

namespace MapEditor;

public enum ExportOption
{
    NoTileset,
    NoImages,
    All
};

public class Exporter
{
    State state;

    Dictionary<int, int> tileIdToTileAboveId = new();
    public int maxImageSize = 1024;
    public MapWorkspace workspace => state.workspace;

    public MapWorkspace exportWorkspace;
    public FolderPath exportMapImageFolderPath;

    ExportOption exportOption;

    public Exporter(State state, string exportFolderPath, ExportOption exportOption)
    {
        this.state = state;
        this.exportOption = exportOption;
        exportWorkspace = new MapWorkspace(exportFolderPath);
        exportMapImageFolderPath = exportWorkspace.baseFolderPath.AppendFolder("images");
    }

    public bool Export()
    {
        bool exportTileset = exportOption == ExportOption.NoImages || exportOption == ExportOption.All;
        bool exportImages = exportOption == ExportOption.All;

        if (exportWorkspace.baseFolderPath.EqualTo(Config.main.workspacePath))
        {
            Prompt.ShowError("Your export path is the same as your workspace path. This would result in your workspace files being deleted!");
            return false;
        }

        Tileset clonedTileset = CloneTileSetForExport();

        List<MapSectionModel> clonedMapSections = GetMapSectionsForExport(clonedTileset);

        // We need to validate entrances/zones again because virtual sections might have messed up the entrance mappings if some entrances got pruned
        if (!new EntranceZoneData(clonedMapSections, clonedTileset.tileSize).Validate(true))
        {
            return false;
        }

        exportWorkspace.tilesetFolderPath.CreateIfNotExists();
        state.workspace.tileAnimationFilePath.CopyTo(exportWorkspace.tilesetFolderPath);
        state.workspace.tileClumpFilePath.CopyTo(exportWorkspace.tilesetFolderPath);
        state.workspace.minimapFolderPath.CopyTo(exportWorkspace.minimapFolderPath);

        exportWorkspace.mapSectionFolderPath.DeleteAndRecreate();
        if (exportImages)
        {
            exportMapImageFolderPath.CreateIfNotExists();
        }

        // Save the map sections to disk

        foreach (MapSectionModel mapSection in clonedMapSections)
        {
            exportWorkspace.SaveMapSection(mapSection);
            if (exportImages)
            {
                ExportMapSectionImages(mapSection, clonedTileset);
            }
        }

        // Save the tileset png to disk

        if (exportTileset)
        {
            // For some reason this causes bugs sometimes. Investigate if want to bring it back.
            // RemoveScratchOnlyTiles(clonedTileset);

            Dictionary<string, ExportedPixelRect> hashToExportedFrames = new();
            foreach (Tile tile in clonedTileset.GetAllTiles())
            {
                if (!hashToExportedFrames.ContainsKey(tile.hash))
                {
                    Drawer drawer = clonedTileset.GetDrawerFromTileHash(tile.hash);
                    hashToExportedFrames[tile.hash] = new ExportedPixelRect(drawer, tile.hash);
                }
            }

            ImagePacker imagePacker = new(exportWorkspace.tilesetFolderPath, "packaged", maxImageSize);
            imagePacker.PackExportImages(hashToExportedFrames.Values.ToList(), true, false);

            foreach (Tile tile in clonedTileset.GetAllTiles())
            {
                ExportedPixelRect exportedFrame = hashToExportedFrames[tile.hash];
                MyRect rect = exportedFrame.newRect!.Value;
                tile.imageTopLeftPos = new MyPoint(rect.x1, rect.y1);
                tile.imageFileName = imagePacker.GetExportImageFileName(exportedFrame.newSpritesheetNum);
            }

            foreach (Tile tile in clonedTileset.idToTile.Values)
            {
                // Game engine doesn't use tile hash and it's a big field. Clear out so it won't be in json export.
                tile.hash = "";
            }

            // Save the tileset to disk (must be done after packing the tileset image) 

            exportWorkspace.SaveTileset(clonedTileset.idToTile);
        }

        return true;
    }

    private void RemoveScratchOnlyTiles(Tileset clonedTileset)
    {
        HashSet<int> unusedTileIds = new(clonedTileset.idToTile.Keys);

        // Because we remove ones in use, we iterate through map sections, not scratch sections
        foreach (MapSection section in state.mapSectionsSC.mapSections)
        {
            foreach (MapSectionLayer layer in section.layers)
            {
                for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
                    {
                        // Remove ones in use, so what's left remaining are the orphans
                        unusedTileIds.Remove(layer.tileGrid[i, j]);
                    }
                }
            }
        }

        // Don't remove tiles referenced in other data models
        (HashSet<int> idsThatCanBeRemoved, _, _) = clonedTileset.GetRemovableTiles(unusedTileIds, state);

        foreach (int tileId in idsThatCanBeRemoved)
        {
            clonedTileset.RemoveTile(tileId);
        }
    }

    public Tileset CloneTileSetForExport()
    {
        List<Tile> allClonedTiles = [];
        foreach (Tile tile in state.tileset.GetAllTiles())
        {
            allClonedTiles.Add(JsonHelpers.DeepCloneJson(tile));
        }

        Tileset clonedTileset = new(allClonedTiles);

        foreach (Tile tile in allClonedTiles)
        {
            AddNewTilesForExport(tile, clonedTileset);
        }

        return clonedTileset;
    }

    private void AddNewTilesForExport(Tile baseTile, Tileset clonedTileset)
    {
        if (baseTile.zIndexMaskColor.IsSet())
        {
            Color color = Helpers.HexStringToColor(baseTile.zIndexMaskColor);
            Color[,] colorGrid = clonedTileset.GetColorsFromTileHash(baseTile.hash, true);
            for (int i = 0; i < clonedTileset.TS; i++)
            {
                for (int j = 0; j < clonedTileset.TS; j++)
                {
                    if (colorGrid[i, j] == color)
                    {
                        colorGrid[i, j] = Color.Transparent;
                    }
                }
            }
            string tileHash = clonedTileset.GetTileHashFromColors(colorGrid);

            Tile? tileAbove = clonedTileset.GetFirstTileByHash(tileHash);
            if (tileAbove == null)
            {
                tileAbove = clonedTileset.AddNewTile(tileHash);
            }

            baseTile.tileAboveId = tileAbove.id;
        }
        else if (baseTile.tileAboveIdIsSame)
        {
            baseTile.tileAboveId = baseTile.id;
        }

        if (baseTile.tileAboveId != null)
        {
            tileIdToTileAboveId[baseTile.id] = baseTile.tileAboveId.Value;
        }

        // Game engine doesn't care about these. Clear to default so doesn't show in JSON export
        baseTile.tileAboveId = null;
        baseTile.tileAboveIdIsSame = false;
        baseTile.zIndexMaskColor = null;
    }

    public List<MapSectionModel> GetMapSectionsForExport(Tileset clonedTileset)
    {
        List<MapSectionModel> clonedMapSections = [];
        foreach (MapSectionModel mapSection in state.mapSectionsSC.ToModel())
        {
            MapSectionModel newMapSection = JsonHelpers.DeepCloneJson(mapSection);

            List<ZoneModel> virtualSections = mapSection.zones.Where(z => z.zoneType == ZoneTypes.VirtualSection.name).ToList();

            if (virtualSections.Count == 0)
            {
                clonedMapSections.Add(newMapSection);
            }
            else
            {
                foreach (ZoneModel virtualSection in virtualSections)
                {
                    MapSectionModel realMapSectionFromVirtual = CreateFromVirtualSection(mapSection, virtualSection);
                    clonedMapSections.Add(realMapSectionFromVirtual);
                }
            }
        }

        foreach (var mapSection in clonedMapSections)
        {
            ProcessTileLayerAbstractions(mapSection, clonedTileset);
        }

        return clonedMapSections;
    }

    // All this advanced, convoluted yet time-saving "layer above" abstractions get mapped to separate layers on export. engine should not care about these abstractions.
    private void ProcessTileLayerAbstractions(MapSectionModel mapSection, Tileset clonedTileset)
    {
        for (int i = 0; i < mapSection.layers.Count; i++)
        {
            bool isLast = i + 1 >= mapSection.layers.Count;
            MapSectionLayerModel layerAbove;
            if (!isLast)
            {
                layerAbove = mapSection.layers[i + 1];
            }
            else
            {
                layerAbove = new(Helpers.Create2DArray(mapSection.rowCount, mapSection.colCount, Tile.TransparentTileId));
            }
            if (ProcessTileLayerAbstractionsHelper(mapSection.layers[i], layerAbove, clonedTileset) && isLast)
            {
                mapSection.layers.Add(layerAbove);
                break;
            }
        }
    }

    private bool ProcessTileLayerAbstractionsHelper(MapSectionLayerModel layer, MapSectionLayerModel layerAbove, Tileset clonedTileset)
    {
        bool found = false;
        for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
        {
            for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
            {
                Tile tile = clonedTileset.GetTileById(layer.tileGrid[i, j]);
                if (tileIdToTileAboveId.ContainsKey(tile.id))
                {
                    layerAbove.tileGrid[i, j] = tileIdToTileAboveId[tile.id];
                    found = true;
                }
            }
        }
        return found;
    }

    public void ExportMapSectionImages(MapSectionModel mapSection, Tileset clonedTileset)
    {
        int layerIndex = 0;
        foreach (MapSectionLayerModel layer in mapSection.layers)
        {
            BitmapDrawer drawer = clonedTileset.CreateDrawerAndDrawTileGrid(layer.tileGrid);

            BitmapDrawer[,] pieces = drawer.Split(maxImageSize, maxImageSize);
            for (int i = 0; i < pieces.GetLength(0); i++)
            {
                for (int j = 0; j < pieces.GetLength(1); j++)
                {
                    string fileName = workspace.GetMapSectionImageFileName(mapSection.name, layerIndex, i, j);
                    pieces[i, j].SaveBitmapToDisk(exportMapImageFolderPath.AppendFile(fileName).fullPath);
                    pieces[i, j].Dispose();
                }
            }

            layerIndex++;
            drawer.Dispose();
        }
    }

    public MapSectionModel CreateFromVirtualSection(MapSectionModel baseSection, ZoneModel virtualSection)
    {
        GridRect gridRect = virtualSection.gridRect!.Value;
        var bounds = new MyRect(gridRect.j1 * 8, gridRect.i1 * 8, (gridRect.j2 + 1) * 8, (gridRect.i2 + 1) * 8);

        List<MapSectionLayerModel> newLayers = [];
        foreach (MapSectionLayerModel layer in baseSection.layers)
        {
            int[,] newTileGrid = Helpers.Create2DArray(gridRect.i2 - gridRect.i1 + 1, gridRect.j2 - gridRect.j1 + 1, 0);
            MapSectionLayerModel newLayer = new(newTileGrid);

            for (int i = gridRect.i1; i <= gridRect.i2; i++) // Start from i1 to i2 inclusive
            {
                for (int j = gridRect.j1; j <= gridRect.j2; j++) // Start from j1 to j2 inclusive
                {
                    if (i < 0 || i >= layer.tileGrid.GetLength(0) || j < 0 || j >= layer.tileGrid.GetLength(1)) continue; // Check bounds for safety, though it might be unnecessary
                    newLayer.tileGrid[i - gridRect.i1, j - gridRect.j1] = layer.tileGrid[i, j];
                }
            }

            if (newLayer.tileGrid.Any(id => id != Tile.TransparentTileId))
            {
                newLayers.Add(newLayer);
            }
        }

        List<InstanceModel> newInstances = [];
        foreach (InstanceModel instance in baseSection.instances)
        {
            if (instance.pos.x >= bounds.x1 - 8 && instance.pos.x <= bounds.x2 + 8 && instance.pos.y >= bounds.y1 - 8 && instance.pos.y <= bounds.y2 + 8)
            {
                InstanceModel newInstance = instance with
                {
                    pos = new MyPoint(instance.pos.x - bounds.x1, instance.pos.y - bounds.y1)
                };
                newInstances.Add(newInstance);
            }
        }

        List<ZoneModel> newZones = [];
        foreach (ZoneModel zone in baseSection.zones)
        {
            if (zone.zoneType != ZoneTypes.VirtualSection.name)
            {
                newZones.Add(zone);
            }
        }

        return new MapSectionModel(
            baseSection.name + virtualSection.name,
            null,
            newLayers,
            newInstances,
            newZones,
            baseSection.defaultMusicName,
            baseSection.defaultEntranceDir,
            baseSection.defaultMaskColor,
            baseSection.startLayer
        );
    }
}
