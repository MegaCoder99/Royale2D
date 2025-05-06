using Editor;
using Shared;
using System.Windows;
using System.Windows.Controls;

namespace MapEditor;

public class StartupFactory : BaseStartupFactory
{
    public override string displayName => "map";
    public override MapWorkspace NewWorkspace(string workspacePath) => new MapWorkspace(workspacePath);
    public override MainWindow NewMainWindow() => new MainWindow();
#if DEBUG
    public override string defaultWorkspacePath => "./../../../sample_map_workspace";
#else
    public override string defaultWorkspacePath => "./sample_map_workspace";
#endif

    public override void InitMainWindow(Window window, IWorkspace workspace)
    {
        // These two "as" casts may seem compile-time unsafe and look awkward, but making BaseStartupFactory have generic types
        // leads to generic type propagation hell and unreadable code. A clear example of compile time safety taken too far.
        (window as MainWindow)!.Init((workspace as MapWorkspace)!, EditorContext.GetContext());
    }

    public StartupFactory(Application application) : base(application)
    {
    }

    public override Page GetNewWorkspacePage(string workspacePath)
    {
        MapWorkspace workspace = NewWorkspace(workspacePath);
        return new InitialImportPage((tileSize, mapImportFolderPath, scratchImportFolderPath) =>
        {
            Tileset tileset = new(tileSize);

            List<MapSectionModel> mapSectionsToCreate;
            if (!string.IsNullOrEmpty(mapImportFolderPath))
            {
                mapSectionsToCreate = ImportMapSectionHelper(mapImportFolderPath, false, tileset);
            }
            else
            {
                mapSectionsToCreate = [MapSectionModel.New("map_section_1", false, 100, 100)];
            }

            List<MapSectionModel> scratchSectionsToCreate;
            if (!string.IsNullOrEmpty(scratchImportFolderPath))
            {
                scratchSectionsToCreate = ImportMapSectionHelper(scratchImportFolderPath, true, tileset);
            }
            else
            {
                scratchSectionsToCreate = [MapSectionModel.New("scratch_section_1", true, 100, 100)];
            }

            workspace.CreateFoldersAndFiles();
            foreach (MapSectionModel mapSection in mapSectionsToCreate)
            {
                workspace.SaveMapSection(mapSection);
            }
            foreach (MapSectionModel scratchSection in scratchSectionsToCreate)
            {
                workspace.SaveMapSection(scratchSection);
            }
            workspace.SaveTileset(tileset.idToTile);

            CreateAndShowEditorWindow(workspace);
        });
    }

    private static List<MapSectionModel> ImportMapSectionHelper(string sectionImportFolderRawPath, bool isScratch, Tileset tileset)
    {
        List<MapSectionModel> mapSections = [];
        FolderPath sectionImportFolderPath = new(sectionImportFolderRawPath);
        foreach (FilePath pngFilePath in sectionImportFolderPath.GetFiles(true, "png"))
        {
            ImageImporter mii = new ImageImporter().Import(pngFilePath, tileset);
            MapSectionLayerModel initialLayer = new(mii.importGrid);
            string name = pngFilePath.GetRelativeFilePath(sectionImportFolderPath).fullPathNoExt;
            var mapSection = MapSectionModel.New(name, isScratch, initialLayer);
            mapSections.Add(mapSection);
        }
        return mapSections;
    }
}
