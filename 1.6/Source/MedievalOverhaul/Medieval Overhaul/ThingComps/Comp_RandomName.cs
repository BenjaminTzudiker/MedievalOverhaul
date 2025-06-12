﻿using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace MedievalOverhaul
{
    public class CompProperties_RandomName : CompProperties
    {
        public List<RulePackDef> nameMakers;
        public CompProperties_RandomName()
        {
            compClass = typeof(Comp_RandomName);
        }
    }

    public class Comp_RandomName : ThingComp
    {
        public CompProperties_RandomName Props => base.props as CompProperties_RandomName;

        public string randomName;
        public override string TransformLabel(string label)
        {
            if (randomName.NullOrEmpty())
            {
                var nameMaker = Props.nameMakers.RandomElement();
                var request = new GrammarRequest();
                request.Includes.Add(nameMaker);
                randomName = GrammarResolver.Resolve(nameMaker.RulesPlusIncludes.RandomElement().keyword, request);
            }
            return randomName;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref randomName, "randomName");
        }
    }
}
