namespace Royale2D
{
    public class EntranceSystem
    {
        public List<Entrance> entrances;

        public EntranceSystem(Map map)
        {
            // Create the entrances
            entrances = new List<Entrance>();
            foreach (MapSection section in map.sections)
            {
                foreach (Instance instance in section.instances)
                {
                    Entrance? entrance = instance.CreateEntrance(section.name);
                    if (entrance != null)
                    {
                        entrances.Add(entrance);
                    }
                }
            }

            // Link the entrances
            for (int i = 0; i < entrances.Count; i++)
            {
                for (int j = i + 1; j < entrances.Count; j++)
                {
                    Entrance myEntrance = entrances[i];
                    Entrance otherEntrance = entrances[j];
                    
                    if (myEntrance.entranceId == otherEntrance.entranceId)
                    {
                        if (myEntrance.sectionName == otherEntrance.sectionName)
                        {
                            throw new Exception("Duplicate entrance id " + myEntrance.entranceId + " found in map section " + myEntrance.sectionName);
                        }
                        else if (myEntrance.linkedEntrance != null || otherEntrance.linkedEntrance != null)
                        {
                            throw new Exception("First check for entrances jutting into other map sections. Entrance id " + myEntrance.entranceId + " used more than twice. Last use found in " + myEntrance.sectionName + ", position " + myEntrance.pos.x + "," + myEntrance.pos.y);
                        }
                        else
                        {
                            myEntrance.linkedEntrance = otherEntrance;
                            otherEntrance.linkedEntrance = myEntrance;
                        }
                    }
                }
            }

            foreach (Entrance entrance in entrances)
            {
                // if (entrance.linkedEntrance == null) throw new Exception("Entrance id " + entrance.entranceId + " in " + entrance.sectionName + " does not have a matching pair");
            }
        }

        public Entrance GetEntranceBySectionAndId(string sectionName, string entranceId)
        {
            foreach (Entrance entrance in entrances)
            {
                if (entrance.sectionName == sectionName && entrance.entranceId == entranceId)
                {
                    return entrance;
                }
            }
            throw new Exception("Entrance not found: " + sectionName + " " + entranceId);
        }

        // PERF optimize
        public Entrance? GetCollidingEntrance(Character character)
        {
            IntRect? charColliderRect = character.colliderComponent.GetWallColliders().FirstOrDefault()?.GetActorWorldShape(character) as IntRect;
            if (charColliderRect != null)
            {
                foreach (Entrance entrance in entrances)
                {
                    if (entrance.sectionName == character.section.name && entrance.layerIndex == character.layerIndex && charColliderRect.Overlaps(entrance.worldRect))
                    {
                        return entrance;
                    }
                }
            }
            return null;
        }
    }
}
