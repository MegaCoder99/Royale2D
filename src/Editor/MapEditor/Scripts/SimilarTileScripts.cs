using Editor;
using Shared;
using System.Collections.Concurrent;
using System.Drawing;

namespace MapEditor;

public partial class State
{
    // IMPROVE saving doesn't seem useful. (Same for the competing "pr" script)
    // Don't pass in "s". The flow should be to generate a file visualizing the similar tiles, and then copy that in, then merge manually
    [Script("mt", "Merge similar tiles")]
    public void MergeSimilarTiles(string[] args)
    {
        bool saveChanges = false;
        if (args.Length == 1 && args[0] == "s")
        {
            saveChanges = true;
        }

        if (HasOrphanedTileIds())
        {
            Prompt.ShowMessage("There are unused tile ids. Please remove them first.");
            return;
        }

        Dictionary<Tile, int> tileToCount = GetTileToCount();

        int progress = 0;
        var uniqueSortedTiles = tileToCount.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        ConcurrentBag<(Tile, Tile)> similarTiles = new();
        object progressLock = new();

        int total = (uniqueSortedTiles.Count * uniqueSortedTiles.Count) / 2;
        Parallel.For(0, uniqueSortedTiles.Count, i =>
        {
            for (int j = i + 1; j < uniqueSortedTiles.Count; j++)
            {
                Tile tile1 = uniqueSortedTiles[i];
                Tile tile2 = uniqueSortedTiles[j];

                // Exact matches are generally tile variations and we don't want to de-dup those, so skip them
                if (tile1.hash == tile2.hash)
                {
                    continue;
                }

                if (tileset.IsSimilar(tile1, tile2, 1, 16))
                {
                    similarTiles.Add((tile1, tile2));
                }

                // Thread-safe progress tracking
                lock (progressLock)
                {
                    progress++;
                    if (progress % 10000 == 0)
                    {
                        double percentage = (progress / (double)total) * 100;
                        Console.WriteLine($"{progress}/{total} processed ({percentage.ToString("0")}%)");
                    }
                }
            }
        });

        var tileIdsToReplace = new Dictionary<int, int>();
        foreach ((Tile tile1, Tile tile2) in similarTiles)
        {
            Tile tileA = tile1;
            Tile tileB = tile2;
            if (tileToCount[tile2] > tileToCount[tile1])
            {
                tileA = tile2;
                tileB = tile1;
            }
            tileIdsToReplace[tileB.id] = tileA.id;
        }

        if (saveChanges)
        {
            ReplaceTileIds(tileIdsToReplace);
            ForceSaveAll();
            Prompt.ShowMessage("Done. Saved changes to disk");
        }
        else
        {
            // Draw to bitmap for visualization
            BitmapDrawer bitmapDrawer = new(150, similarTiles.Count * 16);
            int yPos = 0;

            foreach ((Tile tile1, Tile tile2) in similarTiles.OrderByDescending(st => tileToCount[st.Item1]))
            {
                int xPos = 0;

                Tile tileA = tile1;
                Tile tileB = tile2;
                if (tileToCount[tile2] > tileToCount[tile1])
                {
                    tileA = tile2;
                    tileB = tile1;
                }

                bitmapDrawer.DrawImage(tileset.GetDrawerFromTileHash(tileA.hash), xPos, yPos * 16);

                xPos += 16;
                bitmapDrawer.DrawImage(tileset.GetDrawerFromTileHash(tileB.hash), xPos, yPos * 16);

                xPos += 16;
                bitmapDrawer.DrawText(tileToCount[tileA] + ", " + tileToCount[tileB], xPos, -4 + ((yPos + 1) * 16), Color.Black, null, 16);

                yPos++;
            }
            bitmapDrawer.SaveBitmapToDisk(FolderPath.GetDesktopFilePath("similar_tiles.png").fullPath);
            
            Prompt.ShowMessage("Done. Wrote preview/visualization to \"similar_tiles.png\"");
        }
    }

    [Script("ra")]
    public void ExportRarelyUsedTiles(string[] args)
    {
        if (HasOrphanedTileIds())
        {
            Prompt.ShowMessage("There are unused tile ids. Please remove them first.");
            return;
        }

        Dictionary<Tile, int> tileToCount = [];
        foreach ((MapSection mapSection, SectionsSC sectionsSC) in GetAllSectionsWithSectionsSC())
        {
            foreach (MapSectionLayer layer in mapSection.layers)
            {
                for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
                    {
                        int tileId = layer.tileGrid[i, j];
                        if (tileId == 0) continue;
                        Tile tile = tileset.GetTileById(tileId);
                        if (!tileToCount.ContainsKey(tile))
                        {
                            tileToCount[tile] = 0;
                        }
                        tileToCount[tile]++;
                    }
                }
            }
        }

        var rarelyUsedTiles = tileToCount.OrderBy(kvp => kvp.Value).Where(kvp => kvp.Value < 16).Select(kvp => kvp.Key).ToList();

        int progress = 0;
        List<(Tile, Tile)> similarTiles = [];
        for (int i = 0; i < rarelyUsedTiles.Count; i++)
        {
            for (int j = i + 1; j < rarelyUsedTiles.Count; j++)
            {
                Tile tile1 = rarelyUsedTiles[i];
                Tile tile2 = rarelyUsedTiles[j];

                if (tileset.IsSimilar(tile1, tile2, 1, 16))
                {
                    similarTiles.Add((tile1, tile2));
                }

                progress++;
                if (progress % 10000 == 0)
                {
                    int total = (rarelyUsedTiles.Count() * rarelyUsedTiles.Count()) / 2;
                    double percentage = (progress / (double)total) * 100;
                    Console.WriteLine($"{progress}/{total} processed ({percentage.ToString("0")}%)");
                }
            }
        }

        // Draw to bitmap for visualization

        if (similarTiles.Count == 0)
        {
            Prompt.ShowMessage("No similar tiles found");
        }
        else
        {
            BitmapDrawer bitmapDrawer = new(150, similarTiles.Count * 16);
            int yPos = 0;
            foreach ((Tile tile1, Tile tile2) in similarTiles)
            {
                int xPos = 0;

                Tile tileA = tile1;
                Tile tileB = tile2;
                if (tileToCount[tile2] > tileToCount[tile1])
                {
                    tileA = tile2;
                    tileB = tile1;
                }

                bitmapDrawer.DrawImage(tileset.GetDrawerFromTileHash(tileA.hash), xPos, yPos * 16);

                xPos += 16;
                bitmapDrawer.DrawImage(tileset.GetDrawerFromTileHash(tileB.hash), xPos, yPos * 16);

                xPos += 16;
                bitmapDrawer.DrawText(tileToCount[tileA] + ", " + tileToCount[tileB], xPos, -4 + ((yPos + 1) * 16), Color.Black, null, 16);

                yPos++;
            }

            bitmapDrawer.SaveBitmapToDisk(FolderPath.GetDesktopFilePath("rare_similar_tiles.png").fullPath);

            Prompt.ShowMessage("Done");
        }
    }
}