using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace RimFlix
{
    public class Dialog_SelectShow : Window
    {
        private readonly CompScreen screen;
        private Vector2 scrollPosition;
        private readonly float buttonHeight = 32f;
        private readonly float buttonMargin = 2f;

        public Dialog_SelectShow(CompScreen screen)
        {
            this.screen = screen;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.closeOnClickedOutside = true;
            this.absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(340f, 580f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect outRect = new Rect(inRect);
            outRect.yMin += 20f;
            outRect.yMax -= 40f;
            outRect.xMax -= 16f;
            float viewHeight = (this.buttonHeight + this.buttonMargin) * this.screen.Shows.Count + 80f;
            float viewWidth = viewHeight > outRect.height ? outRect.width - 32f : outRect.width - 16f;
            Rect viewRect = new Rect(0f, 0f, viewWidth, viewHeight);
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            try
            {
                float y = 0;
                foreach (ShowDef show in this.screen.Shows)
                {
                    Rect rect = new Rect(16f, y, viewRect.width, this.buttonHeight);
                    TooltipHandler.TipRegion(rect, show.description);
                    if (Widgets.ButtonText(rect, show.label, true, false, true))
                    {
                        this.screen.ChangeShow(show);
                        SoundDefOf.Click.PlayOneShotOnCamera(null);
                        this.Close(true);
                    }
                    y += (this.buttonHeight + this.buttonMargin);
                }
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }

    }
}