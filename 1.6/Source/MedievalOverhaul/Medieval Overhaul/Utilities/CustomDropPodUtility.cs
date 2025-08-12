﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MedievalOverhaul
{
    public static class CustomDropPodUtility
    {
        private static readonly List<List<Thing>> tempList = new List<List<Thing>>();

        public static void MakeDropPodAt(IntVec3 c, Map map, ActiveTransporterInfo info, Faction faction = null)
        {
            ActiveTransporter activeTransporter = (ActiveTransporter)ThingMaker.MakeThing(faction?.def.dropPodActive ?? ThingDefOf.ActiveDropPod);
            activeTransporter.Contents = info;
            SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, activeTransporter, c, map);
            foreach (Thing item in (IEnumerable<Thing>)activeTransporter.Contents.innerContainer)
            {
                if (item is Pawn pawn && pawn.IsWorldPawn())
                {
                    Find.WorldPawns.RemovePawn(pawn);
                    pawn.psychicEntropy?.SetInitialPsyfocusLevel();
                }
            }
        }

        public static void DropThingsNear(IntVec3 dropCenter, Map map, IEnumerable<Thing> things, int openDelay = 110, bool canInstaDropDuringInit = false, bool leaveSlag = false, bool canRoofPunch = true, bool forbid = true, bool allowFogged = true, Faction faction = null)
        {
            tempList.Clear();
            foreach (Thing thing in things)
            {
                List<Thing> list = new List<Thing>();
                list.Add(thing);
                tempList.Add(list);
            }
            DropThingGroupsNear(dropCenter, map, tempList, openDelay, canInstaDropDuringInit, leaveSlag, canRoofPunch, forbid, allowFogged, canTransfer: false, faction);
            tempList.Clear();
        }

        public static void DropThingGroupsNear(IntVec3 dropCenter, Map map, List<List<Thing>> thingsGroups, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool canRoofPunch = true, bool forbid = true, bool allowFogged = true, bool canTransfer = false, Faction faction = null)
        {
            foreach (List<Thing> thingsGroup in thingsGroups)
            {
                if (!DropCellFinder.TryFindDropSpotNear(dropCenter, map, out var result, allowFogged, canRoofPunch) && (canRoofPunch || !DropCellFinder.TryFindDropSpotNear(dropCenter, map, out result, allowFogged, canRoofPunch: true)))
                {
                    if (!dropCenter.IsValid)
                    {
                        continue;
                    }
                    string[] obj = new string[5]
                    {
                        "DropThingsNear failed to find a place to drop ",
                        thingsGroup.FirstOrDefault()?.ToString(),
                        " near ",
                        null,
                        null
                    };
                    IntVec3 intVec = dropCenter;
                    obj[3] = intVec.ToString();
                    obj[4] = ". Dropping on random square instead.";
                    Log.Warning(string.Concat(obj));
                    result = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(map) && !(c.GetRoof(map)?.isThickRoof ?? false), map);
                }
                if (forbid)
                {
                    for (int i = 0; i < thingsGroup.Count; i++)
                    {
                        thingsGroup[i].SetForbidden(value: true, warnOnFail: false);
                    }
                }
                if (instaDrop)
                {
                    foreach (Thing item in thingsGroup)
                    {
                        GenPlace.TryPlaceThing(item, result, map, ThingPlaceMode.Near);
                    }
                    continue;
                }
                ActiveTransporterInfo activeTransporterInfo = new ActiveTransporterInfo();
                foreach (Thing item2 in thingsGroup)
                {
                    activeTransporterInfo.innerContainer.TryAdd(item2);
                }
                activeTransporterInfo.openDelay = openDelay;
                activeTransporterInfo.leaveSlag = false;
                MakeDropPodAt(result, map, activeTransporterInfo, faction);
            }
        }
    }
}
