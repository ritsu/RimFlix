using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimFlix
{
    class Dialog_AdjustScreen : Window
    {
        private float inRectWidth = 650;
        private float inRectHeight = 450;
        private float headerHeight = 40;
        private float texDim;

        private Texture tubeTex;
        private Texture flatTex;
        private Texture megaTex;

        private Vector3 tubeVec;
        private Vector3 flatVec;
        private Vector3 megaVec;

        public Dialog_AdjustScreen()
        {
            this.doCloseX = true;
            this.doCloseButton = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;

            this.tubeTex = ThingDef.Named("TubeTelevision").graphic.MatSouth.mainTexture;
            this.flatTex = ThingDef.Named("FlatscreenTelevision").graphic.MatSouth.mainTexture;
            this.megaTex = ThingDef.Named("MegascreenTelevision").graphic.MatSouth.mainTexture;

            this.tubeVec = ThingDef.Named("TubeTelevision").graphicData.drawSize;
            this.flatVec = ThingDef.Named("FlatscreenTelevision").graphicData.drawSize;
            this.megaVec = ThingDef.Named("MegascreenTelevision").graphicData.drawSize;

            // Get fluid dim (Listing column padding is 17 pixels)
            float maxVecX = Math.Max(tubeVec.x, Math.Max(flatVec.x, megaVec.x));
            this.texDim = (this.inRectWidth - 34f) / 3f / maxVecX;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(inRectWidth + 36f, inRectHeight + 36f);
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            RimFlixSettings.screenUpdateTime = RimFlixSettings.TotalSeconds;
            base.Close(doCloseSound);
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Avoid Close button overlap
            inRect.yMax -= 50;

            // Header title
            Text.Font = GameFont.Medium;
            Rect headerRect = inRect.TopPartPixels(headerHeight);
            Widgets.Label(inRect, "RimFlix_AdjustSreenTitle".Translate());

            // Use inRect for main Listing so screen overlay does not get cut off by header when moved up
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperCenter;
            Listing_Standard list = new Listing_Standard() { ColumnWidth = (inRect.width - 34) / 3 };
            list.Begin(inRect);

            list.Gap(headerRect.height);
            list.Label($"{ThingDef.Named("TubeTelevision").LabelCap}");
            {
                // Tube tv and frame
                Rect outRect = list.GetRect(this.texDim);
                Vector2 tvSize = this.tubeVec * this.texDim;
                Vector2 frameSize = Vector2.Scale(tvSize, RimFlixSettings.TubeScale);
                Rect tvRect = new Rect(Vector2.zero, tvSize);
                Rect frameRect = new Rect(Vector2.zero, frameSize);
                tvRect.center = outRect.center;
                frameRect.center = outRect.center + RimFlixSettings.TubeOffset * this.texDim;
                Widgets.DrawTextureFitted(tvRect, this.tubeTex, 1f, this.tubeVec, new Rect(0f, 0f, 1f, 1f), 0f, null);
                Widgets.DrawBoxSolid(frameRect, new Color(1f, 1f, 1f, 0.3f));
                Widgets.DrawBox(frameRect);
            }
            list.Gap(8f);
            list.Label($"{"RimFlix_XScale".Translate()}: {Math.Round(RimFlixSettings.TubeScale.x, 3):F3}");
            RimFlixSettings.TubeScale.x = list.Slider(RimFlixSettings.TubeScale.x, 0.1f, 1.0f);
            list.Gap(4f);
            list.Label($"{"RimFlix_YScale".Translate()}: {Math.Round(RimFlixSettings.TubeScale.y, 3):F3}");
            RimFlixSettings.TubeScale.y = list.Slider(RimFlixSettings.TubeScale.y, 0.1f, 1.0f);
            list.Gap(4f);
            list.Label($"{"RimFlix_XOffset".Translate()}: {Math.Round(RimFlixSettings.TubeOffset.x, 3):F3}");
            RimFlixSettings.TubeOffset.x = list.Slider(RimFlixSettings.TubeOffset.x, -1.0f, 1.0f);
            list.Gap(4f);
            list.Label($"{"RimFlix_YOffset".Translate()}: {Math.Round(RimFlixSettings.TubeOffset.y, 3):F3}");
            RimFlixSettings.TubeOffset.y = list.Slider(RimFlixSettings.TubeOffset.y, -1.0f, 1.0f);
            list.Gap(4f);
            if (list.ButtonText("RimFlix_DefaultScreen".Translate()))
            {
                RimFlixSettings.TubeScale = RimFlixSettings.TubeScaleDefault;
                RimFlixSettings.TubeOffset = RimFlixSettings.TubeOffsetDefault;
            }
            list.NewColumn();

            list.Gap(headerRect.height);
            list.Label($"{ThingDef.Named("FlatscreenTelevision").LabelCap}");
            {
                // Fatscreen tv and frame
                Rect outRect = list.GetRect(this.texDim);
                Vector2 tvSize = this.flatVec * this.texDim;
                Vector2 frameSize = Vector2.Scale(tvSize, RimFlixSettings.FlatScale);
                Rect tvRect = new Rect(Vector2.zero, tvSize);
                Rect frameRect = new Rect(Vector2.zero, frameSize);
                tvRect.center = outRect.center;
                frameRect.center = outRect.center + RimFlixSettings.FlatOffset * this.texDim;
                Widgets.DrawTextureFitted(tvRect, this.flatTex, 1f, this.flatVec, new Rect(0f, 0f, 1f, 1f), 0f, null);
                Widgets.DrawBoxSolid(frameRect, new Color(1f, 1f, 1f, 0.3f));
                Widgets.DrawBox(frameRect);
            }
            list.Gap(8f);
            list.Label($"{"RimFlix_XScale".Translate()}: {Math.Round(RimFlixSettings.FlatScale.x, 3):F3}");
            RimFlixSettings.FlatScale.x = list.Slider(RimFlixSettings.FlatScale.x, 0.1f, 1.0f);
            list.Gap(4f);
            list.Label($"{"RimFlix_YScale".Translate()}: {Math.Round(RimFlixSettings.FlatScale.y, 3):F3}");
            RimFlixSettings.FlatScale.y = list.Slider(RimFlixSettings.FlatScale.y, 0.1f, 1.0f);
            list.Gap(4f);
            list.Label($"{"RimFlix_XOffset".Translate()}: {Math.Round(RimFlixSettings.FlatOffset.x, 3):F3}");
            RimFlixSettings.FlatOffset.x = list.Slider(RimFlixSettings.FlatOffset.x, -1.0f, 1.0f);
            list.Gap(4f);
            list.Label($"{"RimFlix_YOffset".Translate()}: {Math.Round(RimFlixSettings.FlatOffset.y, 3):F3}");
            RimFlixSettings.FlatOffset.y = list.Slider(RimFlixSettings.FlatOffset.y, -1.0f, 1.0f);
            list.Gap(4f);
            if (list.ButtonText("RimFlix_DefaultScreen".Translate()))
            {
                RimFlixSettings.FlatScale = RimFlixSettings.FlatScaleDefault;
                RimFlixSettings.FlatOffset = RimFlixSettings.FlatOffsetDefault;
            }
            list.NewColumn();

            list.Gap(headerRect.height);
            list.Label($"{ThingDef.Named("MegascreenTelevision").LabelCap}");
            {
                // Fatscreen tv and frame
                Rect outRect = list.GetRect(this.texDim);
                Vector2 tvSize = this.megaVec * this.texDim;
                //Vector2 frameSize = Vector2.Scale(tvSize, RimFlixSettings.megaScale);
                Vector2 frameSize = Vector2.Scale(tvSize, RimFlixSettings.MegaScale);
                Rect tvRect = new Rect(Vector2.zero, tvSize);
                Rect frameRect = new Rect(Vector2.zero, frameSize);
                tvRect.center = outRect.center;
                frameRect.center = outRect.center + RimFlixSettings.MegaOffset * this.texDim;
                Widgets.DrawTextureFitted(tvRect, this.megaTex, 1f, this.megaVec, new Rect(0f, 0f, 1f, 1f), 0f, null);
                Widgets.DrawBoxSolid(frameRect, new Color(1f, 1f, 1f, 0.3f));
                Widgets.DrawBox(frameRect);
            }
            list.Gap(8f);
            list.Label($"{"RimFlix_XScale".Translate()}: {Math.Round(RimFlixSettings.MegaScale.x, 3):F3}");
            RimFlixSettings.MegaScale.x = list.Slider(RimFlixSettings.MegaScale.x, 0.1f, 1.0f);
            list.Gap(4f);
            list.Label($"{"RimFlix_YScale".Translate()}: {Math.Round(RimFlixSettings.MegaScale.y, 3):F3}");
            RimFlixSettings.MegaScale.y = list.Slider(RimFlixSettings.MegaScale.y, 0.1f, 1.0f);
            list.Gap(4f);
            list.Label($"{"RimFlix_XOffset".Translate()}: {Math.Round(RimFlixSettings.MegaOffset.x, 3):F3}");
            RimFlixSettings.MegaOffset.x = list.Slider(RimFlixSettings.MegaOffset.x, -1.0f, 1.0f);
            list.Gap(4f);
            list.Label($"{"RimFlix_YOffset".Translate()}: {Math.Round(RimFlixSettings.MegaOffset.y, 3):F3}");
            RimFlixSettings.MegaOffset.y = list.Slider(RimFlixSettings.MegaOffset.y, -1.0f, 1.0f);
            list.Gap(4f);
            if (list.ButtonText("RimFlix_DefaultScreen".Translate()))
            {
                RimFlixSettings.MegaScale = RimFlixSettings.MegaScaleDefault;
                RimFlixSettings.MegaOffset = RimFlixSettings.MegaOffsetDefault;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            list.End();
        }
    }
}

