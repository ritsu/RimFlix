using System.Collections.Generic;
using Verse;

namespace RimFlix
{
    public class ShowDef : Def
    {
        public float secondsBetweenFrames = 1;
        public SoundDef sound = null;
        public List<GraphicData> frames = new List<GraphicData>();
        public List<ThingDef> televisionDefs = new List<ThingDef>();

        public string SortName = "";
        public bool disabled = false;
        public bool deleted = false;
    }
}
