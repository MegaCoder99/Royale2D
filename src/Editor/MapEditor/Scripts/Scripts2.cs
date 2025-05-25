using Editor;

namespace MapEditor;

public partial class State
{
    [Script("mim", "Find mismatched indoor mappings")]
    public void Mim(string[] args)
    {
        var virtualSections = new HashSet<string>();
        // Get all virtual sections
        foreach (MapSection section in mapSectionsSC.mapSections)
        {
            foreach (Zone zone in section.zones)
            {
                if (zone.zoneTypeObj == ZoneTypes.VirtualSection)
                {
                    virtualSections.Add(section.name + zone.name);
                }
            }
            virtualSections.Add(section.name);
        }

        foreach (Zone zone in mapSectionsSC.selectedMapSection.zones)
        {
            if (zone.zoneTypeObj == ZoneTypes.IndoorMapping)
            {
                string indoorMapping = zone.name;
                if (!virtualSections.Contains(indoorMapping))
                {
                    Prompt.ShowMessage(indoorMapping);
                    mapSectionsSC.canvas.CenterScrollToPos(zone.gridRect!.j1 * 8, zone.gridRect!.i1 * 8);
                    break;
                }
            }
        }
    }

    [Script("stg", "Snap to grid coords")]
    public void SnapToZone(string[] args)
    {
        int iPos = int.Parse(args[0]);
        int jPos = int.Parse(args[1]);

        mapSectionsSC.canvas.CenterScrollToPos(jPos * 8, iPos * 8);
    }
}
