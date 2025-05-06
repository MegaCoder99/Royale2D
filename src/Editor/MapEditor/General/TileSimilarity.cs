using Editor;
using System.Drawing;

namespace MapEditor;

public class TileSimilarity
{
    int tileSize;

    public TileSimilarity(int tileSize)
    {
        this.tileSize = tileSize;
    }

    public bool AllSameColor(Color[,] tile)
    {
        Color firstColor = tile[0, 0];
        for (int x = 0; x < tileSize; x++)
        {
            for (int y = 0; y < tileSize; y++)
            {
                if (tile[x, y] != firstColor) return false;
            }
        }
        return true;
    }

    public float ComputeTileSimilarityByPattern(Color[,] tileA, Color[,] tileB, int colorDiffThreshold = 16)
    {
        int width = tileA.GetLength(0);
        int height = tileA.GetLength(1);

        if (AllSameColor(tileB) || AllSameColor(tileA)) return 0;

        float score = 0;

        Dictionary<Color, Color> colorToMapping = [];
        Dictionary<Color, Color> mappingToColor = [];

        Dictionary<(Color, Color), int> pairToColorDiff = [];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color colorA = tileA[x, y];
                Color colorB = tileB[x, y];

                pairToColorDiff[(colorA, colorB)] = Helpers.GetColorDifference(colorA, colorB);

                if (!colorToMapping.ContainsKey(colorA))
                {
                    score++;
                    if (mappingToColor.ContainsKey(colorB))
                    {
                        score--;
                    }
                    colorToMapping[colorA] = colorB;
                    mappingToColor[colorB] = colorA;
                }
                else
                {
                    if (colorToMapping[colorA] == colorB)
                    {
                        score++;
                    }
                    else
                    {
                        colorToMapping[colorA] = colorB;
                        score--;
                    }
                }
            }
        }

        float colorDiff = 0;
        float maxColorDiff = float.MinValue;
        foreach (var kvp in pairToColorDiff)
        {
            colorDiff += kvp.Value;
            if (colorDiff > maxColorDiff)
            {
                maxColorDiff = colorDiff;
            }
        }

        float avgColorDiff = colorDiff / pairToColorDiff.Count;

        if (maxColorDiff > (colorDiffThreshold * 2) || avgColorDiff > colorDiffThreshold) score = 0;

        return score / (width * height);
    }
}
