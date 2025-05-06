namespace Shared;

public partial class SharedHelpers
{
    public static List<List<T>> Create2DList<T>(int rows, int cols, T defaultValue)
    {
        var list = new List<List<T>>();
        for (int i = 0; i < rows; i++)
        {
            list.Add(new List<T>());
            for (int j = 0; j < cols; j++)
            {
                list.Last().Add(defaultValue);
            }
        }
        return list;
    }

    public static T[,] Create2DArray<T>(int rows, int cols, T defaultValue)
    {
        var array = new T[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array[i, j] = defaultValue;
            }
        }
        return array;
    }

    public static List<List<T>> ConvertArrayToList<T>(T[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        var list = new List<List<T>>();

        for (int i = 0; i < rows; i++)
        {
            var rowList = new List<T>();
            for (int j = 0; j < cols; j++)
            {
                rowList.Add(array[i, j]);
            }
            list.Add(rowList);
        }

        return list;
    }

    public static T[,] ConvertListToArray<T>(List<List<T>> list)
    {
        if (list == null || list.Count == 0)
            throw new ArgumentException("The list of lists cannot be null or empty.");

        int rows = list.Count;
        int cols = list[0].Count;
        var array = new T[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            if (list[i].Count != cols)
                throw new ArgumentException("All rows in the list of lists must have the same number of columns.");

            for (int j = 0; j < cols; j++)
            {
                array[i, j] = list[i][j];
            }
        }

        return array;
    }

    public static List<List<T>> CloneListOfLists<T>(List<List<T>> tileGrid)
    {
        var newTileGrid = new List<List<T>>();
        foreach (var row in tileGrid)
        {
            newTileGrid.Add(new List<T>(row));
        }
        return newTileGrid;
    }
}

public static partial class Extensions
{
    public static bool InRange<T>(this List<T> list, int index)
    {
        return index >= 0 && index < list.Count;
    }

    public static bool InRange<T>(this List<List<T>> list, int i, int j)
    {
        return i >= 0 && i < list.Count && j >= 0 && j < list[i].Count;
    }

    public static bool InRange<T>(this T[,] grid, int i, int j)
    {
        return i >= 0 && i < grid.GetLength(0) && j >= 0 && j < grid.GetLength(1);
    }

    public static List<T> ToList<T>(this T[,] grid)
    {
        List<T> result = [];
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                result.Add(grid[i, j]);
            }
        }
        return result;
    }

    public static bool Any<T>(this T[,] grid, Func<T, bool> predicate)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (predicate(grid[i, j])) return true;
            }
        }
        return false;
    }

    public static bool All<T>(this T[,] grid, Func<T, bool> predicate)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (!predicate(grid[i, j])) return false;
            }
        }
        return true;
    }

    public static void FlipHorizontal<T>(this T[,] grid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols / 2; j++)
            {
                T temp = grid[i, j];
                grid[i, j] = grid[i, cols - j - 1];
                grid[i, cols - j - 1] = temp;
            }
        }
    }

    public static void FlipVertical<T>(this T[,] grid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        for (int i = 0; i < rows / 2; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                T temp = grid[i, j];
                grid[i, j] = grid[rows - i - 1, j];
                grid[rows - i - 1, j] = temp;
            }
        }
    }

    /// <summary>
    /// Calculates how much each cell in the 2D array would have shifted as a result of a resize, relative to its coordinates in the old grid.
    /// </summary>
    /// <param name="grid">2D array of generic type T</param>
    /// <param name="newRows">New row count</param>
    /// <param name="newCols">New column count</param>
    /// <param name="resizeOriginRow">
    /// Can be -1, 0 or 1. -1 indicates our resize should add/remove rows above. 1 indicates our resize should add/remove rows below.
    /// 0 indicates our resize should try to add/remove an equal amount of rows above and below
    /// </param>
    /// <param name="resizeOriginCol">
    /// Can be -1, 0 or 1. -1 indicates our resize should add/remove columns to the left. 1 indicates our resize should add/remove columns to the right.
    /// 0 indicates our resize should try to add/remove an equal amount of colums to the left and right
    /// </param>
    /// <returns>
    /// rowShift indicates how many rows each cell had to shift when being resized from the old grid to the new grid. Can be negative or positive.
    /// colShift indicates how many columns each cell had to shift when being resized from the old grid to the new grid. Can be negative or positive.
    /// </returns>
    public static (int rowShift, int colShift) GetResizeShift<T>(this T[,] grid, int newRows, int newCols, int resizeOriginRow, int resizeOriginCol)
    {
        int oldRows = grid.GetLength(0);
        int oldCols = grid.GetLength(1);

        int rowShift = 0;
        int colShift = 0;

        if (resizeOriginRow == -1) // Adding/removing rows above
        {
            rowShift = newRows - oldRows;
        }
        else if (resizeOriginRow == 0) // Adding/removing rows equally above and below
        {
            rowShift = (newRows - oldRows) / 2;
        }
        else if (resizeOriginRow == 1) // Adding/removing rows below
        {
            rowShift = 0;
        }

        if (resizeOriginCol == -1) // Adding/removing columns to the left
        {
            colShift = newCols - oldCols;
        }
        else if (resizeOriginCol == 0) // Adding/removing columns equally left and right
        {
            colShift = (newCols - oldCols) / 2;
        }
        else if (resizeOriginCol == 1) // Adding/removing columns to the right
        {
            colShift = 0;
        }

        return (rowShift, colShift);
    }

    /// <summary>
    /// Resizes a 2D array.
    /// </summary>
    /// <param name="grid">2D array of generic type T</param>
    /// <param name="newRows">New row count</param>
    /// <param name="newCols">New column count</param>
    /// <param name="resizeOriginRow">
    /// Can be -1, 0 or 1. -1 indicates our resize should add/remove rows above. 1 indicates our resize should add/remove rows below.
    /// 0 indicates our resize should try to add/remove an equal amount of rows above and below
    /// </param>
    /// <param name="resizeOriginCol">
    /// Can be -1, 0 or 1. -1 indicates our resize should add/remove columns to the left. 1 indicates our resize should add/remove columns to the right.
    /// 0 indicates our resize should try to add/remove an equal amount of colums to the left and right
    /// </param>
    /// <returns>
    /// The resized array.
    /// </returns>
    public static T[,] ResizeGrid<T>(this T[,] grid, int newRows, int newCols, int resizeOriginRow, int resizeOriginCol)
    {
        int oldRows = grid.GetLength(0);
        int oldCols = grid.GetLength(1);

        T[,] newGrid = new T[newRows, newCols];

        (int rowShift, int colShift) = grid.GetResizeShift(newRows, newCols, resizeOriginRow, resizeOriginCol);

        for (int newRow = 0; newRow < newRows; newRow++)
        {
            for (int newCol = 0; newCol < newCols; newCol++)
            {
                int oldRow = newRow - rowShift;
                int oldCol = newCol - colShift;

                if (oldRow >= 0 && oldRow < oldRows && oldCol >= 0 && oldCol < oldCols)
                {
                    newGrid[newRow, newCol] = grid[oldRow, oldCol];
                }
            }
        }

        return newGrid;
    }
}