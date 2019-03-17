using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimFlix
{
    public class UserShowDef : ShowDef, IExposable
    {
        public string path = null;
        public List<string> televisionDefStrings = new List<string>();

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.defName, "defName");
            Scribe_Values.Look(ref this.path, "path");
            Scribe_Values.Look(ref this.label, "label");
            Scribe_Values.Look(ref this.description, "description");
            Scribe_Values.Look(ref this.secondsBetweenFrames, "secondsBetweenFrames");
            Scribe_Collections.Look(ref this.televisionDefStrings, "televisionDefStrings");
        }
    }
}
