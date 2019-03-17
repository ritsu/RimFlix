using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace RimFlix
{
    // Defines a screen space on a TV
    public class CompScreen : ThingComp
    {
        // RimFlix settings
        private RimFlixSettings settings;
        private double screenUpdateTime;
        private double showUpdateTime;

        // Current show info
        private int showIndex = 0;
        private int showTicks = 0;
        private int frameIndex = 0;
        private int frameTicks = 0;

        // Current frame graphic
        private bool frameDirty = true;
        private Graphic frameGraphic;
        private Graphic FrameGraphic
        {
            get
            {
                if ((this.Show?.frames?.Count ?? 0) == 0)
                {
                    return null;
                }
                if (this.frameDirty || this.screenUpdateTime < RimFlixSettings.screenUpdateTime)
                {
                    Graphic graphic = this.Show.frames[this.frameIndex % this.Show.frames.Count].Graphic;
                    Vector2 frameSize = GetSize(graphic);
                    this.frameGraphic = graphic.GetCopy(frameSize);
                    this.screenUpdateTime = RimFlixSettings.screenUpdateTime;
                    this.frameDirty = false;
                }
                return this.frameGraphic;
            }
        }

        // Available shows for this television
        private List<ShowDef> shows;
        public List<ShowDef> Shows
        {
            get
            {
                if (this.showUpdateTime < RimFlixSettings.showUpdateTime)
                {
                    this.shows = (from show in DefDatabase<ShowDef>.AllDefs
                                  where show.televisionDefs.Contains(this.parent.def) && !show.deleted && !show.disabled
                                  select show).ToList();
                    this.showUpdateTime = RimFlixSettings.showUpdateTime;
                    this.frameDirty = true;
                    ResolveShowDefName();
                }
                return this.shows;
            }
        }

        private string showDefName;
        private ShowDef show;
        private ShowDef Show
        {
            get
            {
                if ((this.Shows?.Count ?? 0) == 0)
                {
                    return null;
                }
                this.show = this.Shows[this.showIndex % this.Shows.Count];
                this.showDefName = this.show.defName;
                return this.show;
            }
        }

        // Gizmo toggle and Scribe ref
        public bool AllowPawn = true;

        // Power consumption tweaks
        private CompPowerTrader compPowerTrader;
        private float powerOutputOn;
        private float powerOutputOff;

        // Flag indicating whether or not TV is being watched, set by JobDriver on each tick
        public int SleepTimer { get; set; }

        public CompProperties_Screen Props => (CompProperties_Screen)props;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            this.settings = LoadedModManager.GetMod<RimFlixMod>().GetSettings<RimFlixSettings>();
            this.compPowerTrader = this.parent.GetComp<CompPowerTrader>();
            this.powerOutputOn = -1f * this.compPowerTrader.Props.basePowerConsumption * this.settings.PowerConsumptionOn / 100f;
            this.powerOutputOff = -1f * this.compPowerTrader.Props.basePowerConsumption * this.settings.PowerConsumptionOff / 100f;
            this.screenUpdateTime = 0;
            this.showUpdateTime = 0;
        }

        private void ResolveShow()
        {
            if (this.show == null)
            {
                return;
            }
            int i = this.shows.IndexOf(this.show);
            if (i >= 0)
            {
                this.showIndex = i;
            }
        }

        // Use Show.defName instead of Show in case user deletes show midgame
        private void ResolveShowDefName()
        {
            if (this.showDefName == null)
            {
                return;
            }
            int i = this.shows.FindIndex(s => s.defName.Equals(this.showDefName));
            if (i >= 0)
            {
                this.showIndex = i;
            }
        }


        private Vector2 GetSize(Graphic frame)
        {
            Vector2 screenScale = new Vector2();
            if (this.parent.def == ThingDef.Named("TubeTelevision"))
            {
                screenScale = RimFlixSettings.TubeScale;
            }
            else if (this.parent.def == ThingDef.Named("FlatscreenTelevision"))
            {
                screenScale = RimFlixSettings.FlatScale;
            }
            else if (this.parent.def == ThingDef.Named("MegascreenTelevision"))
            {
                screenScale = RimFlixSettings.MegaScale;
            }
            Vector2 screenSize = Vector2.Scale(screenScale, this.parent.Graphic.drawSize);
            Vector2 frameSize = new Vector2(frame.MatSingle.mainTexture.width, frame.MatSingle.mainTexture.height);
            bool isWide = (frameSize.x / screenSize.x > frameSize.y / screenSize.y);

            // Stretch: resize image to fill frame, ignoring aspect ratio
            if (this.settings.DrawType == DrawType.Stretch)
            {
                return screenSize;
            }
            // Fit: resize image to fit within frame while maintaining aspect ratio
            if (this.settings.DrawType == DrawType.Fit)
            {
                return isWide
                    ? new Vector2(screenSize.x, screenSize.x * frameSize.y / frameSize.x)
                    : new Vector2(screenSize.y * frameSize.x / frameSize.y, screenSize.y);
            }
            // Fill: resize image to fill frame while maintaining aspect ratio (can be larger than parent)
            if (this.settings.DrawType == DrawType.Fill)
            {
                return isWide
                    ? new Vector2(screenSize.y * frameSize.x / frameSize.y, screenSize.y)
                    : new Vector2(screenSize.x, frameSize.y / frameSize.x * screenSize.x);
            }
            return screenSize;
        }

        private Vector3 GetOffset(ThingDef def)
        {
            // Altitude layers are 0.046875f
            // For more info refer to `Verse.Altitudes` and `Verse.SectionLayer`
            float y = 0.0234375f;
            if (def == ThingDef.Named("TubeTelevision"))
            {
                return new Vector3(RimFlixSettings.TubeOffset.x, y, -1f * RimFlixSettings.TubeOffset.y);
            }
            if (def == ThingDef.Named("FlatscreenTelevision"))
            {
                return new Vector3(RimFlixSettings.FlatOffset.x, y, -1f * RimFlixSettings.FlatOffset.y);
            }
            if (def == ThingDef.Named("MegascreenTelevision"))
            {
                return new Vector3(RimFlixSettings.MegaOffset.x, y, -1f * RimFlixSettings.MegaOffset.y);
            }
            return new Vector3(0, 0, 0);
        }

        private bool IsPlaying()
        {
            // Not facing south
            if (this.parent.Rotation != Rot4.South)
            {
                return false;
            }
            // No pawn watching, and PlayAlways is false
            if (this.SleepTimer == 0 && !this.settings.PlayAlways)
            {
                return false;
            }
            // No shows available, or show has no frames
            if ((this.Show?.frames?.Count ?? 0) == 0)
            {
                return false;
            }
            // Not powered
            if (!this.compPowerTrader.PowerOn)
            {
                return false;
            }
            return true;
        }

        public void ChangeShow(int i)
        {
            if (i >= 0 && i < this.Shows.Count)
            {
                this.showIndex = i;
                this.frameIndex = this.showTicks = this.frameTicks = 0;
                this.frameDirty = true;
            }
        }

        public void ChangeShow(ShowDef s)
        {
            this.ChangeShow(this.Shows.IndexOf(s));
        }

        // Process show and frame ticks
        // Should only be called when tv is playing (show exists and has frames)
        private void RunShow()
        {
            if (this.SleepTimer > 0 && this.AllowPawn && ++this.showTicks > this.settings.SecondsBetweenShows.SecondsToTicks())
            {
                // Pawn changed show
                this.showIndex = (this.showIndex + 1) % this.Shows.Count;
                this.frameIndex = this.showTicks = this.frameTicks = 0;
                this.frameDirty = true;
            }
            else if (++this.frameTicks > this.Show.secondsBetweenFrames.SecondsToTicks())
            {
                // Frame change in current show
                this.frameIndex = (this.frameIndex + 1) % this.Show.frames.Count;
                this.frameTicks = 0;
                this.frameDirty = true;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();

            if (IsPlaying())
            {
                Vector3 drawPos = this.parent.DrawPos + GetOffset(this.parent.def);
                this.FrameGraphic.Draw(drawPos, Rot4.North, this.parent, 0f);
            }
            if (!Find.TickManager.Paused)
            {
                this.SleepTimer = this.SleepTimer > 0 ? this.SleepTimer - 1 : 0;
            }
        }

        public override void CompTick()
        {
            if (IsPlaying())
            {
                RunShow();
                this.compPowerTrader.PowerOutput = this.powerOutputOn;
            }
            else
            {
                this.compPowerTrader.PowerOutput = this.powerOutputOff;
            }
            base.CompTick();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo c in base.CompGetGizmosExtra())
            {
                yield return c;
            }

            if ((this.Shows.Count > 0 && this.parent.Faction == Faction.OfPlayer) || Prefs.DevMode)
            {
                yield return new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.Misc6,
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/AllowPawn", true),
                    defaultLabel = "RimFlix_AllowPawnLabel".Translate(),
                    defaultDesc = "RimFlix_AllowPawnDesc".Translate(),
                    isActive = () => AllowPawn,
                    toggleAction = delegate {
                        this.AllowPawn = !this.AllowPawn;
                    }
                };
                yield return new Command_Action
                {
                    hotKey = KeyBindingDefOf.Misc7,
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/SelectShow", true),
                    defaultLabel = "RimFlix_SelectShowLabel".Translate(),
                    defaultDesc = "RimFlix_SelectShowDesc".Translate(),
                    action = delegate ()
                    {
                        Find.WindowStack.Add(new Dialog_SelectShow(this));
                    }
                };
                yield return new Command_Action
                {
                    hotKey = KeyBindingDefOf.Misc8,
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/NextShow", true),
                    activateSound = SoundDefOf.Click,
                    defaultLabel = "RimFlix_NextShowLabel".Translate(),
                    defaultDesc = "RimFlix_NextShowDesc".Translate(),
                    action = delegate {
                        ChangeShow((this.showIndex + 1) % this.Shows.Count);
                    }
                };
            }

            //yield break;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            //Scribe_Defs.Look(ref this.show, "RimFlix_Show");
            Scribe_Values.Look(ref this.showDefName, "RimFlix_ShowDefName");
            Scribe_Values.Look(ref this.frameIndex, "RimFlix_FrameIndex", 0);
            Scribe_Values.Look(ref this.showTicks, "RimFlix_ShowTicks", 0);
            Scribe_Values.Look(ref this.frameTicks, "RimFlix_FrameTicks", 0);
            Scribe_Values.Look(ref this.AllowPawn, "RimFlix_AllowPawn", true);
        }
    }
}
