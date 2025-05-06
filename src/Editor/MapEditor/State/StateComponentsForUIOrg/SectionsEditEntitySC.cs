using Editor;
using Shared;
using System.Drawing;
using System.Windows.Input;

namespace MapEditor;

// Contains state component code related to the "Edit Entities" editor mode.
// This mode is used for CRUD on non-tile "entities" (instances, zones, etc).
// Stuff here will NOT be applicable to the scratch canvas.
public partial class SectionsSC
{
    static Instance? lastCopiedInstance;
    public Zone? selectedZone => selectedMapSection.selectedZone;
    public Instance? selectedInstance => selectedMapSection.selectedInstance;

    public void AddEditEntityModeHotkeys(HotkeyManager hotkeyManager)
    {
        hotkeyManager.AddHotkeys([
            new HotkeyConfig(Key.A, ToggleAddInstanceMode),
            new HotkeyConfig(Key.S, AddZoneFromSelectionCommit),
            new HotkeyConfig(Key.D, DeleteSelectionCommit),
            new HotkeyConfig(Key.Escape, () => canvas.ChangeToDefaultTool()),
            new HotkeyConfig(Key.C, HotkeyModifier.Control, () => CopyInstance(false)),
            new HotkeyConfig(Key.X, HotkeyModifier.Control, () => CopyInstance(true)),
            new HotkeyConfig(Key.V, HotkeyModifier.Control, () => PasteInstance()),
        ], MapEditorMode.EditEntity);
    }

    public void PlaceInstanceCommit(int placeX, int placeY)
    {
        DirtyRedraw(() =>
        {
            EntranceData? entranceData = null;
            if (state.selectedInstanceType == InstanceTypes.Entrance.name)
            {
                string defaultDir = EntranceData.Down;
                if (!string.IsNullOrEmpty(selectedMapSection.defaultEntranceDir))
                {
                    defaultDir = selectedMapSection.defaultEntranceDir;
                }
                entranceData = new EntranceData(defaultDir, "", "");
            }

            var instance = Instance.New(context, state.selectedInstanceType, "", new MyPoint(placeX, placeY), state.selectedInstanceType, 0, entranceData);
            selectedMapSection.instances.Add(instance);
            selectedMapSection.selectedInstance = instance;
            state.showEntities = true;
        });
    }

    public void ToggleAddInstanceMode()
    {
        if (canvas.tool is not PlaceInstanceTool)
        {
            ChangeTool(new PlaceInstanceTool(DrawPreviewInstance()));
        }
    }

    public void AddZoneFromSelectionCommit()
    {
        if (!state.canAddZoneToSelection) return;

        DirtyRedraw(() =>
        {
            (int minI, int maxI, int minJ, int maxJ) = GetMinMaxSelectionIJs();

            var zoneRect = new GridRect(minI, minJ, maxI, maxJ);
            string zoneName = state.selectedCreateZoneType.ToString();
            if (state.selectedCreateZoneType == ZoneTypes.VirtualSection.name)
            {
                var zoneIds = new List<int>();
                foreach (Zone z in selectedMapSection.zones)
                {
                    if (z.zoneType == ZoneTypes.VirtualSection.name && int.TryParse(z.name, out int id))
                    {
                        zoneIds.Add(id);
                    }
                }
                zoneName = GetNextId(zoneIds).ToString();
            }
            else if (state.selectedCreateZoneType == ZoneTypes.IndoorMapping.name)
            {
                string firstContainedEntranceId = "";
                foreach (Instance instance in selectedMapSection.instances)
                {
                    if (instance is Entrance entrance && zoneRect.GetRect(TS).Contains(entrance.pos))
                    {
                        firstContainedEntranceId = entrance.GetEntranceId();
                        break;
                    }
                }
                string? newZoneName = GetEntranceZoneData().GetIndoorMappingNameFromEntrance(selectedMapSection.name, firstContainedEntranceId);
                zoneName = newZoneName ?? "IndoorMapping";
            }

            var zone = Zone.New(context, zoneName, state.selectedCreateZoneType, zoneRect, TS);
            selectedMapSection.zones.Add(zone);
            selectedMapSection.selectedZone = zone;
            selectedTileCoords.Clear();
            state.showEntities = true;
        });
    }

    private int GetNextId(List<int> ids)
    {
        ids = ids.Distinct().ToList();
        ids.Sort();
        for (int i = 0; i < ids.Count - 1; i++)
        {
            if (ids[i] + 1 != ids[i + 1])
            {
                return ids[i] + 1;
            }
        }
        if (ids.Count > 0) return ids[ids.Count - 1] + 1;
        return 1;
    }

    public bool MoveSelectedEntityCommit(int incX, int incY)
    {
        if (selectedZone == null && selectedInstance == null)
        {
            return false;
        }

        DirtyRedraw(() =>
        {
            if (selectedZone != null)
            {
                if (Helpers.ShiftHeld())
                {
                    selectedZone.Inc(0, 0, incX, incY);
                }
                else if (Helpers.ControlHeld())
                {
                    selectedZone.Inc(incX, incY, incX, incY);
                }
                else
                {
                    selectedZone.Inc(incX, incY, 0, 0);
                }
            }
            else if (selectedInstance != null)
            {
                if (Helpers.ShiftHeld())
                {
                    selectedInstance.pos.x += incX * (TS / 2);
                    selectedInstance.pos.y += incY * (TS / 2);
                }
                else
                {
                    selectedInstance.pos.x += incX * TS;
                    selectedInstance.pos.y += incY * TS;
                }
            }
        });

        return true;
    }

    public void CopyInstance(bool cut)
    {
        if (selectedInstance != null)
        {
            lastCopiedInstance = selectedInstance;
            if (cut)
            {
                DeleteSelectedInstanceCommit();
            }
        }
    }

    public void PasteInstance()
    {
        if (lastCopiedInstance != null)
        {
            DirtyRedraw(() =>
            {
                Instance newInstance = Instance.New(context, lastCopiedInstance.ToModel());
                newInstance.pos.x = (mouseXInt / TS) * TS;
                newInstance.pos.y = (mouseYInt / TS) * TS;
                selectedMapSection.instances.Add(newInstance);
                selectedMapSection.selectedInstance = newInstance;
            });
        }
    }

    public void DeleteSelectionCommit()
    {
        if (selectedInstance != null)
        {
            DeleteSelectedInstanceCommit();
        }
        else if (selectedZone != null)
        {
            DeleteSelectedZoneCommit();
        }
    }

    public void DeleteSelectedInstanceCommit()
    {
        if (selectedInstance == null) return;
        DirtyRedraw(() =>
        {
            selectedMapSection.instances.Remove(selectedInstance);
            selectedMapSection.selectedInstance = null;
        });
    }

    public void DeleteSelectedZoneCommit()
    {
        if (selectedZone == null) return;
        DirtyRedraw(() =>
        {
            selectedMapSection.zones.Remove(selectedZone);
            selectedMapSection.selectedZone = null;
        });
    }

    public void SelectInstanceOrZoneCommit()
    {
        if (!state.showEntities) return;

        RedrawWithUndo(() =>
        {
            (Instance? selectedInstance, Zone? selectedZone) = GetSelections();
            if (selectedInstance != null && selectedMapSection.selectedInstance != selectedInstance)
            {
                selectedMapSection.selectedInstance = selectedInstance;
            }
            else if (selectedZone != null && selectedMapSection.selectedZone != selectedZone)
            {
                selectedMapSection.selectedZone = selectedZone;
            }
            else
            {
                selectedMapSection.selectedInstance = null;
                selectedMapSection.selectedZone = null;
            }
        });
    }

    private (Instance?, Zone?) GetSelections()
    {
        Instance? bestInstance = null;
        foreach (Instance instance in selectedMapSection.instances)
        {
            MyRect rect = instance.GetPositionalRect();
            //var dragRect = new Rect(this.dragLeftX, this.dragTopY, this.dragRightX, this.dragBotY);
            MyRect dragRect = new MyRect(mouseXInt, mouseYInt, mouseXInt + 1, mouseYInt + 1);
            if (rect.Overlaps(dragRect))
            {
                bestInstance = instance;
                break;
            }
        }

        if (bestInstance != null)
        {
            return (bestInstance, null);
        }

        int smallestArea = int.MaxValue;
        Zone? bestZone = null;
        foreach (Zone zone in selectedMapSection.zones)
        {
            MyRect rect = zone.GetRect(TS);
            //var dragRect = new Rect(this.dragLeftX, this.dragTopY, this.dragRightX, this.dragBotY);
            var dragRect = new MyRect(mouseXInt, mouseYInt, mouseXInt + 1, mouseYInt + 1);
            if (rect.Overlaps(dragRect))
            {
                int area = rect.area;
                if (area < smallestArea)
                {
                    bestZone = zone;
                    smallestArea = area;
                }
            }
        }

        return (null, bestZone);
    }

    public void DrawInstancesAndZones(Drawer drawer)
    {
        foreach (Zone zone in selectedMapSection.zones)
        {
            if (!RectInBounds(zone.GetRect(TS))) continue;
            zone.Draw(drawer, TS);
        }

        foreach (Instance instance in selectedMapSection.instances)
        {
            var rect = instance.GetPositionalRect();
            if (!RectInBounds(rect)) continue;
            instance.Draw(drawer);
        }

        if (state.drawIndoorMappingData)
        {
            // This seems very slow to call on every draw. But it's behind a togglable setting, and usually takes < 10 ms on a decently powerful CPU
            EntranceZoneData entranceZoneData = GetEntranceZoneData();
            foreach (Zone zone in selectedMapSection.zones)
            {
                if (!RectInBounds(zone.GetRect(TS)) || zone.zoneType != ZoneTypes.IndoorMapping.name) continue;
                entranceZoneData.DrawIndoorMappingRelationalData(drawer, zone.zoneTypeObj.displayColor, selectedMapSection.name, zone.ToModel());
            }
        }

        if (selectedInstance != null)
        {
            drawer.DrawRect(selectedInstance.GetPositionalRect(), null, Color.Yellow, 2.0f / zoom);
        }

        if (selectedZone != null)
        {
            MyRect rect = selectedZone.GetRect(TS);
            MyRect rect2 = new MyRect(rect.x1 - 2, rect.y1 - 2, rect.x2 + 2, rect.y2 + 2);
            drawer.DrawRect(rect2, null, Color.FromArgb(128, 0, 255, 0), 2.0f / zoom, offX: -0.5f, offY: -0.5f);
        }
    }

    public Drawer DrawPreviewInstance()
    {
        Drawer iconDrawer = InstanceTypes.GetInstanceTypeObj(state.selectedInstanceType).drawer;
        var drawer = new BitmapDrawer(iconDrawer.width, iconDrawer.height);
        drawer.DrawImage(iconDrawer, 0, 0, alpha: 0.5f);
        return drawer;
    }
}
