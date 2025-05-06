using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace Shared;

public partial class SharedHelpers
{
    public static string GetVersion()
    {
        return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? "[Unable to retrieve version]";
        // If using informational version in future, use this instead/in addition
        //return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "[Unable to retrieve version]";
    }
    
    public static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').Replace("\\\\", "/");
    }

    public static byte[] SerializeToBinary<T>(T obj)
    {
        var serializer = new DataContractSerializer(typeof(T), new DataContractSerializerSettings
        {
            PreserveObjectReferences = true
        });

        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, obj);
            return stream.ToArray();
        }
    }

    public static T DeserializeFromBinary<T>(byte[] data)
    {
        var serializer = new DataContractSerializer(typeof(T), new DataContractSerializerSettings
        {
            PreserveObjectReferences = true
        });
        using (var stream = new MemoryStream(data))
        {
            return (T)serializer.ReadObject(stream)!;
        }
    }

    public static T DeepCloneBinary<T>(T obj)
    {
        var serializer = new DataContractSerializer(typeof(T), new DataContractSerializerSettings
        {
            PreserveObjectReferences = true
        });

        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, obj);
            stream.Position = 0;
            return (T)serializer.ReadObject(stream)!;
        }
    }

    public static T DeepCloneBinary<T>(T obj, out byte[] bytes)
    {
        var serializer = new DataContractSerializer(typeof(T), new DataContractSerializerSettings
        {
            PreserveObjectReferences = true
        });

        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, obj);
            bytes = stream.ToArray();
            stream.Position = 0;
            return (T)serializer.ReadObject(stream)!;
        }
    }
}
