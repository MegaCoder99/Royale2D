using Editor;
using Shared;
using System.Drawing;

namespace MapEditor;

public class EzdMapSection
{
    public MapSectionModel mapSection;
    public string name => mapSection.name;

    public List<EzdZone> zones = [];
    public List<EzdEntrance> entrances = [];

    public EzdMapSection(MapSectionModel mapSection)
    {
        this.mapSection = mapSection;
    }
}

public class EzdZone
{
    public ZoneModel zone;
    public string name => zone.name;
    public string type => zone.zoneType;

    public EzdMapSection section;
    public List<EzdEntrance> entrances = [];

    public EzdZone(ZoneModel zone, EzdMapSection section)
    {
        this.zone = zone;
        this.section = section;
    }
}

public class EzdEntrance
{
    public InstanceModel instance;
    public string name => instance.name;

    public EzdMapSection section;
    public List<EzdZone> zones = [];
    public EzdZone? virtualSection => zones.FirstOrDefault(z => z.type == ZoneTypes.VirtualSection.name);
    public List<EzdEntrance> linkedEntrances = [];
    public EzdEntrance? linkedEntrance => linkedEntrances.FirstOrDefault();

    public EzdEntrance(InstanceModel instance, EzdMapSection section)
    {
        this.instance = instance;
        this.section = section;
    }
}

// Responsible for managing and updating relational data among entrances and certain zone types.
// The data here is created on demand whenever it's needed and is NOT directly undo'able as it's all derived
// Design is to just create this on demand whenever we need to refresh the data with zone/entrance updates.
// This is WAY simpler than having to go through the dozens of places we can update instances/zones and try to update every little dictionary mapping on each place
// or create a zillion circular references in the data models or state components everywhere. Instead we SOC out these relationships/circular refs into this one file.
// It seems slow but usually takes < 10 ms to run on a decently powerful CPU and we only call it in specific one-off places or if a togglable flag is on
// Most of the complexity here is actually from the "IndoorMapping" zone type, a complex use case that may very well be specific to our project
public class EntranceZoneData
{
    int tileSize;

    List<EzdMapSection> sections;
    List<EzdEntrance> entrances => sections.SelectMany(s => s.entrances).ToList();
    List<EzdZone> zones => sections.SelectMany(s => s.zones).ToList();

    public EntranceZoneData(IEnumerable<MapSectionModel> mapSections, int tileSize)
    {
        sections = mapSections.SelectList(ms => new EzdMapSection(ms));
        this.tileSize = tileSize;

        // Setup map section to child relationships
        foreach (EzdMapSection section in sections)
        {
            foreach (InstanceModel instance in section.mapSection.instances)
            {
                if (instance.entranceData != null)
                {
                    section.entrances.Add(new EzdEntrance(instance, section));
                }
            }
            
            foreach (ZoneModel zone in section.mapSection.zones)
            {
                if (zone.zoneType == ZoneTypes.VirtualSection.name || zone.zoneType == ZoneTypes.IndoorMapping.name)
                {
                    section.zones.Add(new EzdZone(zone, section));
                }
            }
        }

        // Setup zone to child relationships
        foreach (EzdMapSection section in sections)
        {
            foreach (EzdZone zone in section.zones)
            {
                // Indoor mapping CSV override can accept a list of override entrance names that it should use as its child entrances.
                // This covers corner cases of overlapping indoor mapping/entrances/etc.
                if (zone.type == ZoneTypes.IndoorMapping.name && !string.IsNullOrEmpty(zone.zone.properties))
                {
                    // VALIDATE validate the CSV
                    string[] overrideEntranceNames = zone.zone.properties.Split(',');
                    foreach (EzdEntrance entrance in section.entrances)
                    {
                        if (overrideEntranceNames.Contains(entrance.name))
                        {
                            zone.entrances.Add(entrance);
                            entrance.zones.Add(zone);
                        }
                    }
                }
                // Otherwise, set the zone's entrances to all entrances it contains in its rect
                else
                {
                    foreach (EzdEntrance entrance in section.entrances)
                    {
                        if (zone.zone.GetRect(tileSize).Contains(entrance.instance.pos))
                        {
                            zone.entrances.Add(entrance);
                            entrance.zones.Add(zone);
                        }
                    }
                }
            }
        }

        // Setup entrance to entrance relationships
        Dictionary<string, List<EzdEntrance>> nameToEntrances = new();
        foreach (EzdEntrance entrance in entrances)
        {
            nameToEntrances[entrance.name] = nameToEntrances.GetOrCreate(entrance.name, []);
            nameToEntrances[entrance.name].Add(entrance);
        }
        foreach (EzdEntrance entrance in entrances)
        {
            entrance.linkedEntrances = new(nameToEntrances[entrance.name]); // Clone to prevent reusing same reference bug
            entrance.linkedEntrances.Remove(entrance);
        }
    }

    public bool Validate(bool fromExport)
    {
        foreach (EzdEntrance entrance in entrances)
        {
            if (entrance.linkedEntrances.Count == 0)
            {
                string exportMessage = fromExport ? "\n\nIf this is not happening on regular save and only on export, ensure that your entrances are within the bounds of their virtual sections, or they might have been removed in the export." : "";
                Prompt.ShowError($"Entrance '{entrance.name}' in section '{entrance.section.name}' is missing a matching entrance.{exportMessage}");
                return false;
            }
            else if (entrance.linkedEntrances.Count > 1)
            {
                string otherEntrances = string.Join("\n", entrance.linkedEntrances.Select(e => $"One in section '{e.section.name}'"));
                Prompt.ShowError($"Entrance '{entrance.name}' has more than two occurrances:\n\n{otherEntrances}");
                return false;
            }
        }

        foreach (EzdMapSection section in sections)
        {
            List<string> virtualSectionNames = section.zones.Where(z => z.zone.zoneType == ZoneTypes.VirtualSection.name).Select(z => z.name).ToList();

            List<string> duplicateNames = virtualSectionNames
                .GroupBy(name => name)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateNames.Count > 0)
            {
                string dupNames = string.Join("\n", duplicateNames);
                Prompt.ShowError($"Section '{section.name}' has multiple virtual sections with the same name:\n\n{dupNames}");
                return false;
            }
        }
        return true;
    }

    // Given an entrance name in a map section, "follow" that entrance to the map section + virtual section combination it leads to.
    public string? GetIndoorMappingNameFromEntrance(string currentMapSectionName, string entranceName)
    {
        if (string.IsNullOrEmpty(entranceName)) return null;

        foreach (EzdEntrance entrance in entrances)
        {
            if (entrance.name == entranceName && entrance.section.name != currentMapSectionName)
            {
                foreach (EzdZone zone in entrance.zones)
                {
                    if (zone.type == ZoneTypes.VirtualSection.name)
                    {
                        return entrance.section.name + zone.section.name;
                    }
                }
            }
        }

        // If we didn't find a virtual section, use the top level section name
        foreach (EzdEntrance entrance in entrances)
        {
            if (entrance.name == entranceName && entrance.section.name != currentMapSectionName)
            {
                return entrance.section.name;
            }
        }

        return null;
    }

    // In an indoor mapping zone, draws a visualization of the entrance locations it maps to in the linked entrances' virtual section
    // (or linked entrance's top-level map section if linked entrance is not in a virtual section).
    // This allows you to see if the entrances in the linked section align with the entrances in the map section with the indoor mapping.
    public void DrawIndoorMappingRelationalData(Drawer drawer, Color displayColor, string mapSectionName, ZoneModel indoorMappingZoneModel)
    {
        // Zones are not uniquely id'd by name
        EzdZone? indoorMappingZone = zones.FirstOrDefault(
            z => z.name == indoorMappingZoneModel.name && 
            z.section.name == mapSectionName && 
            z.zone.zoneType == indoorMappingZoneModel.zoneType &&
            z.zone.GetRect(8).EqualTo(indoorMappingZoneModel.GetRect(8)) // Tile size doesn't matter, we just want relative comparision
        );

        if (indoorMappingZone == null)
        {
            Helpers.AssertFailed("indoorMappingZone was null");
            return;
        }

        List<EzdEntrance> entrances;
        MyRect vsMappedToRect;
        EzdZone? virtualSectionMappedTo = indoorMappingZone.entrances.Count > 0 ? indoorMappingZone.entrances[0].linkedEntrance?.virtualSection : null;
        EzdMapSection? mapSectionMappedTo = indoorMappingZone.entrances.Count > 0 ? indoorMappingZone.entrances[0].linkedEntrance?.section : null;
        if (virtualSectionMappedTo != null)
        {
            entrances = virtualSectionMappedTo.entrances;
            vsMappedToRect = virtualSectionMappedTo.zone.GetRect(tileSize);
        }
        else if (mapSectionMappedTo != null)
        {
            entrances = mapSectionMappedTo.entrances;
            vsMappedToRect = new MyRect(0, 0, mapSectionMappedTo.mapSection.colCount * tileSize, mapSectionMappedTo.mapSection.rowCount * tileSize);
        }
        else
        {
            return;
        }

        MyRect mainSectionRect = indoorMappingZone.zone.GetRect(tileSize);
        foreach (EzdEntrance entrance in entrances)
        {
            float entrancePercentX = (entrance.instance.pos.x - vsMappedToRect.x1) / (float)vsMappedToRect.w;
            float entrancePercentY = (entrance.instance.pos.y - vsMappedToRect.y1) / (float)vsMappedToRect.h;

            float wRatio = (mainSectionRect.w / (float)vsMappedToRect.w);
            float hRatio = (mainSectionRect.h / (float)vsMappedToRect.h);

            float drawX = mainSectionRect.x1 + (entrancePercentX * mainSectionRect.w) - (8 * wRatio);
            float drawY = mainSectionRect.y1 + (entrancePercentY * mainSectionRect.h) - (8 * hRatio);
            float drawW = 16 * wRatio;
            float drawH = 16 * hRatio;

            MyRect drawRect = MyRect.CreateWH((int)drawX, (int)drawY, (int)drawW, (int)drawH);

            drawer.DrawRect(drawRect, null, displayColor, 1, offX: -0.5f, offY: -0.5f);
        }
    }
}
