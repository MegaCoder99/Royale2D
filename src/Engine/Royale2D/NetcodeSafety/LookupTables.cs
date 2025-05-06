using Shared;

namespace Royale2D
{
    public class LookupTablesModel
    {
        public Dictionary<int, int> sin = new Dictionary<int, int>();
        public Dictionary<int, int> cos = new Dictionary<int, int>();
        public Dictionary<int, int> atan = new Dictionary<int, int>();
    }

    public class LookupTables
    {
        public static LookupTables main;
        public static FilePath path => Assets.assetPath.AppendFile("lookup_tables.json");

        public Dictionary<int, Fd> sin = new Dictionary<int, Fd>();
        public Dictionary<int, Fd> cos = new Dictionary<int, Fd>();
        public Dictionary<int, Fd> atan = new Dictionary<int, Fd>();

        public LookupTables()
        {
        }

        public static void Init()
        {
            var lookupTablesModel = JsonHelpers.DeserializeJsonFile<LookupTablesModel>(path);
            LookupTables.main = new LookupTables();

            // Map the values from the model to the lookup tables
            foreach (var kvp in lookupTablesModel.sin)
            {
                main.sin[kvp.Key] = Fd.FromInternalVal(kvp.Value);
            }
            foreach (var kvp in lookupTablesModel.cos)
            {
                main.cos[kvp.Key] = Fd.FromInternalVal(kvp.Value);
            }
            foreach (var kvp in lookupTablesModel.atan)
            {
                main.atan[kvp.Key] = Fd.FromInternalVal(kvp.Value);
            }
        }

        // Uncomment this section only if you need to regenerate the lookup tables for whatever reason
        /*
        public static void Generate()
        {
            var lookupTables = new LookupTables();
            lookupTables.Save();
        }

        public void Save()
        {
            for (int i = 0; i < 360; i++)
            {
                sin[i] = Fd.New(MyMath.SinD(i));
                cos[i] = Fd.New(MyMath.CosD(i));
            }

            for (int i = 0; i <= 20000; i++)
            {
                Fd fd = Fd.FromInternalVal(i);
                atan[i] = Fd.New(MyMath.ArcTanD(fd.floatVal));
            }

            JsonHelpers.SerializeToJsonFile(path, this);
        }
        */
    }
}
