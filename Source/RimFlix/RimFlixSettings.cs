using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace RimFlix
{
    public class RimFlixSettings : ModSettings
    {
        public string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public string lastPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public bool PlayAlways = true;
        public float PowerConsumptionOn = 100f;
        public float PowerConsumptionOff = 100f;
        public float SecondsBetweenShows = 60f;
        public DrawType DrawType = DrawType.Stretch;
        public List<UserShowDef> UserShows = new List<UserShowDef>();
        public HashSet<string> DisabledShows = new HashSet<string>();

        public static Vector2 TubeScaleDefault = new Vector2(0.5162f, 0.4200f);
        public static Vector2 FlatScaleDefault = new Vector2(0.8700f, 0.7179f);
        public static Vector2 MegaScaleDefault = new Vector2(0.9414f, 0.8017f);
        public static Vector2 TubeScale = RimFlixSettings.TubeScaleDefault;
        public static Vector2 FlatScale = RimFlixSettings.FlatScaleDefault;
        public static Vector2 MegaScale = RimFlixSettings.MegaScaleDefault;

        public static Vector2 TubeOffsetDefault = new Vector2(-0.0897f, 0.1172f);
        public static Vector2 FlatOffsetDefault = new Vector2(0.0f, -0.0346f);
        public static Vector2 MegaOffsetDefault = new Vector2(0.0f, -0.0207f);
        public static Vector2 TubeOffset = RimFlixSettings.TubeOffsetDefault;
        public static Vector2 FlatOffset = RimFlixSettings.FlatOffsetDefault;
        public static Vector2 MegaOffset = RimFlixSettings.MegaOffsetDefault;

        public float TubeScaleX = RimFlixSettings.TubeScaleDefault.x;
        public float TubeScaleY = RimFlixSettings.TubeScaleDefault.y;
        public float FlatScaleX = RimFlixSettings.FlatScaleDefault.x;
        public float FlatScaleY = RimFlixSettings.FlatScaleDefault.y;
        public float MegaScaleX = RimFlixSettings.MegaScaleDefault.x;
        public float MegaScaleY = RimFlixSettings.MegaScaleDefault.y;

        public float TubeOffsetX = RimFlixSettings.TubeOffsetDefault.x;
        public float TubeOffsetY = RimFlixSettings.TubeOffsetDefault.y;
        public float FlatOffsetX = RimFlixSettings.FlatOffsetDefault.x;
        public float FlatOffsetY = RimFlixSettings.FlatOffsetDefault.y;
        public float MegaOffsetX = RimFlixSettings.MegaOffsetDefault.x;
        public float MegaOffsetY = RimFlixSettings.MegaOffsetDefault.y;

        public static double screenUpdateTime = 0;
        public static double showUpdateTime = 0;

        public static DateTime RimFlixEpoch = new DateTime(2019, 03, 10, 0, 0, 0, DateTimeKind.Utc);
        public static double TotalSeconds
        {
            get
            {
                return (DateTime.UtcNow - RimFlixSettings.RimFlixEpoch).TotalSeconds;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.PlayAlways, "playAlways", true);
            Scribe_Values.Look(ref this.PowerConsumptionOn, "powerConsumptionOn", 100f);
            Scribe_Values.Look(ref this.PowerConsumptionOff, "powerConsumptionOff", 100f);
            Scribe_Values.Look(ref this.SecondsBetweenShows, "secondsBetweenShows", 60f);
            Scribe_Values.Look(ref this.DrawType, "drawType", DrawType.Stretch);
            Scribe_Values.Look(ref this.lastPath, "lastPath");
            Scribe_Collections.Look(ref this.DisabledShows, "disabledShows");
            Scribe_Collections.Look(ref this.UserShows, "userShows", LookMode.Deep);
            this.UserShows = this.UserShows ?? new List<UserShowDef>();
            this.DisabledShows = this.DisabledShows ?? new HashSet<string>();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.Look(ref this.TubeScaleX, "tubeScaleX", RimFlixSettings.TubeScaleDefault.x);
                Scribe_Values.Look(ref this.TubeScaleY, "tubeScaleY", RimFlixSettings.TubeScaleDefault.y);
                Scribe_Values.Look(ref this.FlatScaleX, "flatScaleX", RimFlixSettings.FlatScaleDefault.x);
                Scribe_Values.Look(ref this.FlatScaleY, "flatScaleY", RimFlixSettings.FlatScaleDefault.y);
                Scribe_Values.Look(ref this.MegaScaleX, "megaScaleX", RimFlixSettings.MegaScaleDefault.x);
                Scribe_Values.Look(ref this.MegaScaleY, "megaScaleY", RimFlixSettings.MegaScaleDefault.y);
                RimFlixSettings.TubeScale = new Vector2(this.TubeScaleX, this.TubeScaleY);
                RimFlixSettings.FlatScale = new Vector2(this.FlatScaleX, this.FlatScaleY);
                RimFlixSettings.MegaScale = new Vector2(this.MegaScaleX, this.MegaScaleY);

                Scribe_Values.Look(ref this.TubeOffsetX, "tubeOffsetX", RimFlixSettings.TubeOffsetDefault.x);
                Scribe_Values.Look(ref this.TubeOffsetY, "tubeOffsetY", RimFlixSettings.TubeOffsetDefault.y);
                Scribe_Values.Look(ref this.FlatOffsetX, "flatOffsetX", RimFlixSettings.FlatOffsetDefault.x);
                Scribe_Values.Look(ref this.FlatOffsetY, "flatOffsetY", RimFlixSettings.FlatOffsetDefault.y);
                Scribe_Values.Look(ref this.MegaOffsetX, "megaOffsetX", RimFlixSettings.MegaOffsetDefault.x);
                Scribe_Values.Look(ref this.MegaOffsetY, "megaOffsetY", RimFlixSettings.MegaOffsetDefault.y);
                RimFlixSettings.TubeOffset = new Vector2(this.TubeOffsetX, this.TubeOffsetY);
                RimFlixSettings.FlatOffset = new Vector2(this.FlatOffsetX, this.FlatOffsetY);
                RimFlixSettings.MegaOffset = new Vector2(this.MegaOffsetX, this.MegaOffsetY);
            }
            else
            {
                this.TubeScaleX = TubeScale.x;
                this.TubeScaleY = TubeScale.y;
                this.FlatScaleX = FlatScale.x;
                this.FlatScaleY = FlatScale.y;
                this.MegaScaleX = MegaScale.x;
                this.MegaScaleY = MegaScale.y;
                Scribe_Values.Look(ref this.TubeScaleX, "tubeScaleX", RimFlixSettings.TubeScaleDefault.x);
                Scribe_Values.Look(ref this.TubeScaleY, "tubeScaleY", RimFlixSettings.TubeScaleDefault.y);
                Scribe_Values.Look(ref this.FlatScaleX, "flatScaleX", RimFlixSettings.FlatScaleDefault.x);
                Scribe_Values.Look(ref this.FlatScaleY, "flatScaleY", RimFlixSettings.FlatScaleDefault.y);
                Scribe_Values.Look(ref this.MegaScaleX, "megaScaleX", RimFlixSettings.MegaScaleDefault.x);
                Scribe_Values.Look(ref this.MegaScaleY, "megaScaleY", RimFlixSettings.MegaScaleDefault.y);

                this.TubeOffsetX = TubeOffset.x;
                this.TubeOffsetY = TubeOffset.y;
                this.FlatOffsetX = FlatOffset.x;
                this.FlatOffsetY = FlatOffset.y;
                this.MegaOffsetX = MegaOffset.x;
                this.MegaOffsetY = MegaOffset.y;
                Scribe_Values.Look(ref this.TubeOffsetX, "tubeOffsetX", RimFlixSettings.TubeOffsetDefault.x);
                Scribe_Values.Look(ref this.TubeOffsetY, "tubeOffsetY", RimFlixSettings.TubeOffsetDefault.y);
                Scribe_Values.Look(ref this.FlatOffsetX, "flatOffsetX", RimFlixSettings.FlatOffsetDefault.x);
                Scribe_Values.Look(ref this.FlatOffsetY, "flatOffsetY", RimFlixSettings.FlatOffsetDefault.y);
                Scribe_Values.Look(ref this.MegaOffsetX, "megaOffsetX", RimFlixSettings.MegaOffsetDefault.x);
                Scribe_Values.Look(ref this.MegaOffsetY, "megaOffsetY", RimFlixSettings.MegaOffsetDefault.y);
            }

            base.ExposeData();
        }
        
    }

}
