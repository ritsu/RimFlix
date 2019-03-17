using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace RimFlix
{
    public class RimFlixMod : Mod
    {
        private RimFlixSettings settings;
        private List<ShowDef> shows = new List<ShowDef>();
        private List<ShowDef> Shows
        {
            get
            {
                if (this.ShowUpdateTime < RimFlixSettings.showUpdateTime)
                {
                    this.shows = DefDatabase<ShowDef>.AllDefs.Where(s => !s.deleted).ToList();
                    foreach (ShowDef show in this.shows)
                    {
                        if (show.modContentPack == null)
                        {
                            show.SortName = $"{"RimFlix_UserShowLabel".Translate()} : {show.label}";
                        }
                        else
                        {
                            show.SortName = $"{show.modContentPack.Name} : {show.label}";
                        }
                        show.disabled = this.settings.DisabledShows == null ? false : this.settings.DisabledShows.Contains(show.defName);
                    }
                    this.shows = GetSortedShows(false);
                    this.ShowUpdateTime = RimFlixSettings.showUpdateTime;
                    this.showCountsDirty = true;
                }
                return shows;
            }
        }
        public double ShowUpdateTime = 0;

        private Dictionary<string, int> showCounts = new Dictionary<string, int>()
        {
            { "TubeTelevision", 0 },
            { "FlatscreenTelevision", 0 },
            { "MegascreenTelevision", 0 }
        };
        private Dictionary<string, int> ShowCounts
        {
            get
            {
                if (this.showCountsDirty)
                {
                    foreach (string key in this.showCounts.Keys.ToList())
                    {
                        this.showCounts[key] = this.Shows.Count(s => s.televisionDefs.Contains(ThingDef.Named(key)));
                    }
                    this.showCountsDirty = false;
                }
                return this.showCounts;
            }
        }
        private bool showCountsDirty = true;

        private List<FloatMenuOption> drawTypeMenu;
        public List<FloatMenuOption> DrawTypeMenu
        {
            get
            {
                if (this.drawTypeMenu == null)
                {
                    this.drawTypeMenu = new List<FloatMenuOption>
                    {
                        new FloatMenuOption("RimFlix_DrawTypeStretch".Translate(), delegate {
                            this.settings.DrawType = DrawType.Stretch;
                            RimFlixSettings.screenUpdateTime = RimFlixSettings.TotalSeconds;
                        }),
                        new FloatMenuOption("RimFlix_DrawTypeFit".Translate(), delegate {
                            this.settings.DrawType = DrawType.Fit;
                            RimFlixSettings.screenUpdateTime = RimFlixSettings.TotalSeconds;
                        }),
                        /*
                        new FloatMenuOption("RimFlix_DrawTypeFill".Translate(), delegate {
                            this.settings.DrawType = DrawType.Fill;
                            RimFlixSettings.lastUpdateTick = GenTicks.TicksAbs;
                        })
                        */
                    };
                }
                return this.drawTypeMenu;
            }
        }
        private string[] drawTypeNames;
        public string[] DrawTypeNames
        {
            get
            {
                if (this.drawTypeNames == null)
                {
                    this.drawTypeNames = new string[]
                    {
                        "RimFlix_DrawTypeStretch".Translate(),
                        "RimFlix_DrawTypeFit".Translate(),
                        "RimFlix_DrawTypeFill".Translate()
                    };
                }
                return this.drawTypeNames;
            }
        }

        // We need to delay loading textures until game has fully loaded
        private Texture tubeTex;
        private Texture TubeTex
        {
            get
            {
                if (tubeTex == null)
                {
                    tubeTex = ThingDef.Named("TubeTelevision").graphic.MatSouth.mainTexture;
                }
                return tubeTex;
            }
        }
        private Texture frameTex;
        private Texture FlatTex
        {
            get
            {
                if (frameTex == null)
                {
                    frameTex = ThingDef.Named("FlatscreenTelevision").graphic.MatSouth.mainTexture;
                }
                return frameTex;
            }
        }
        private Texture megaTex;
        private Texture MegaTex
        {
            get
            {
                if (megaTex == null)
                {
                    megaTex = ThingDef.Named("MegascreenTelevision").graphic.MatSouth.mainTexture;
                }
                return megaTex;
            }
        }

        private Texture disabledTex;
        private Texture DisabledTex
        {
            get
            {
                if (disabledTex == null)
                {
                    disabledTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.067f, 0.079f, 0.091f, 0.5f));
                }
                return disabledTex;
            }
        }
        private Color disabledTextColor = new Color(0.616f, 0.443f, 0.451f);
        private Color disabledLineColor = new Color(0.616f, 0.443f, 0.451f, 0.3f);

        private Texture2D adjustTex;
        private Texture2D AdjustTex
        {
            get
            {
                if (adjustTex == null)
                {
                    adjustTex = ContentFinder<Texture2D>.Get("UI/Buttons/OpenSpecificTab");
                }
                return adjustTex;
            }
        }

        private Vector2 scrollPos = Vector2.zero;
        private float scrollBarWidth = 16f;

        private SortType sortType = SortType.None;
        private bool[] sortAsc = new bool[Enum.GetNames(typeof(SortType)).Length];

        public RimFlixMod(ModContentPack content) : base(content)
        {
            this.settings = base.GetSettings<RimFlixSettings>();
        }

        private void DoOptions(Rect rect)
        {
            // 500 x 150
            float labelWidth = 330f;
            float inputWidth = 100f;
            float unitWidth = 70f;
            float padding = 6f;

            rect.x += padding;
            Listing_Standard list = new Listing_Standard(GameFont.Small);
            list.Begin(rect);
            list.Gap(padding);
            {
                Rect lineRect = list.GetRect(Text.LineHeight);
                Widgets.DrawHighlightIfMouseover(lineRect);
                TooltipHandler.TipRegion(lineRect, "RimFlix_PlayAlwaysTooltip".Translate());

                Rect checkRect = lineRect.LeftPartPixels(labelWidth + inputWidth);
                Widgets.CheckboxLabeled(checkRect, "RimFlix_PlayAlwaysLabel".Translate(), ref this.settings.PlayAlways, false, null, null, false);
                list.Gap(padding);
            }
            {
                string buffer = this.settings.SecondsBetweenShows.ToString();
                Rect lineRect = list.GetRect(Text.LineHeight);
                Widgets.DrawHighlightIfMouseover(lineRect);
                TooltipHandler.TipRegion(lineRect, "RimFlix_SecondsBetweenShowsTooltip".Translate());

                Rect labelRect = lineRect.LeftPartPixels(labelWidth);
                Rect tmpRect = lineRect.RightPartPixels(lineRect.width - labelRect.width);
                Rect inputRect = tmpRect.LeftPartPixels(inputWidth);
                Rect unitRect = tmpRect.RightPartPixels(unitWidth);
                Widgets.Label(labelRect, "RimFlix_SecondsBetweenShowsLabel".Translate());
                Widgets.TextFieldNumeric(inputRect, ref this.settings.SecondsBetweenShows, ref buffer, 1, 10000);
                Widgets.Label(unitRect, " " + "RimFlix_SecondsBetweenShowsUnits".Translate());
                list.Gap(padding);
            }
            {
                string buffer = this.settings.PowerConsumptionOn.ToString();
                Rect lineRect = list.GetRect(Text.LineHeight);
                Widgets.DrawHighlightIfMouseover(lineRect);
                TooltipHandler.TipRegion(lineRect, "RimFlix_PowerConsumptionOnTooltip".Translate());

                Rect labelRect = lineRect.LeftPartPixels(labelWidth);
                Rect tmpRect = lineRect.RightPartPixels(lineRect.width - labelRect.width);
                Rect inputRect = tmpRect.LeftPartPixels(inputWidth);
                Rect unitRect = tmpRect.RightPartPixels(unitWidth);
                Widgets.Label(labelRect, "RimFlix_PowerConsumptionOnLabel".Translate());
                Widgets.TextFieldNumeric(inputRect, ref this.settings.PowerConsumptionOn, ref buffer, 0, 10000);
                Widgets.Label(unitRect, " %");
                list.Gap(padding);
            }
            {
                string buffer = this.settings.PowerConsumptionOff.ToString();
                Rect lineRect = list.GetRect(Text.LineHeight);
                Widgets.DrawHighlightIfMouseover(lineRect);
                TooltipHandler.TipRegion(lineRect, "RimFlix_PowerConsumptionOffTooltip".Translate());

                Rect labelRect = lineRect.LeftPartPixels(labelWidth);
                Rect tmpRect = lineRect.RightPartPixels(lineRect.width - labelRect.width);
                Rect inputRect = tmpRect.LeftPartPixels(inputWidth);
                Rect unitRect = tmpRect.RightPartPixels(unitWidth);
                Widgets.Label(labelRect, "RimFlix_PowerConsumptionOffLabel".Translate());
                Widgets.TextFieldNumeric(inputRect, ref this.settings.PowerConsumptionOff, ref buffer, 0, 10000);
                Widgets.Label(unitRect, " %");
                list.Gap(padding);
            }
            {
                Rect lineRect = list.GetRect(Text.LineHeight);
                Widgets.DrawHighlightIfMouseover(lineRect);
                TooltipHandler.TipRegion(lineRect, "RimFlix_DrawTypeTooltip".Translate());

                Rect labelRect = lineRect.LeftPartPixels(labelWidth);
                Rect tmpRect = lineRect.RightPartPixels(lineRect.width - labelRect.width);
                Rect buttonRect = tmpRect.LeftPartPixels(inputWidth);
                Widgets.Label(labelRect, "RimFlix_DrawTypeLabel".Translate());
                if (Widgets.ButtonText(buttonRect, this.DrawTypeNames[(int)this.settings.DrawType]))
                {
                    Find.WindowStack.Add(new FloatMenu(this.DrawTypeMenu));
                }
                list.Gap(padding);
            }
            list.End();
        }

        private void DoStatus(Rect rect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Rect headerRect = new Rect(rect.x, rect.y - 8, rect.width, Text.LineHeight);
            Widgets.Label(headerRect, "RimFlix_ShowsLabel".Translate());

            Text.Font = GameFont.Small;
            float labelWidth = 200;
            float padding = 4f;
            Rect tableRect = new Rect(rect.x, rect.y + headerRect.height, rect.width, (Text.LineHeight + padding) * 3);
            GUI.BeginGroup(tableRect);
            {
                Rect labelRect = new Rect(0, 0, labelWidth, Text.LineHeight);
                Rect countRect = new Rect(labelWidth, 0, tableRect.width - labelWidth - 16, Text.LineHeight);
                int i = 0;
                foreach (KeyValuePair<string, int> item in this.ShowCounts)
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(labelRect, ThingDef.Named(item.Key).LabelCap);
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(countRect, $"{item.Value}");
                    labelRect.y = countRect.y = (Text.LineHeight + padding) * ++i;
                }
            }
            GUI.EndGroup();

            Rect addRect = new Rect(rect.x, rect.y + headerRect.height + tableRect.height + 8, rect.width - 16, Text.LineHeight + 4);
            TooltipHandler.TipRegion(addRect, "RimFlix_AddShowTooltip".Translate());
            if (Widgets.ButtonText(addRect, "RimFlix_AddShowButton".Translate()))
            {
                Find.WindowStack.Add(new Dialog_AddShow());
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DoShows(Rect rect)
        {
            // 864 x 418
            float cellHeight = Text.LineHeight;
            float cellPadding = 2;
            float nameWidth = 434;
            float framesWidth = 80;
            float timeWidth = 80;
            float tubeWidth = 40;
            float flatWidth = 40;
            float megaWidth = 40;
            float editWidth = 60;
            float deleteWidth = 60;

            // Header row
            GUI.skin.GetStyle("Label").alignment = TextAnchor.MiddleCenter;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect headerRect = rect.TopPartPixels(cellHeight);
            //Widgets.DrawMenuSection(headerRect);
            Widgets.DrawLineHorizontal(headerRect.x, headerRect.y + cellHeight, headerRect.width);
            float x = headerRect.x;
            float y = headerRect.y;

            Rect nameRect = new Rect(x, y, nameWidth, cellHeight);
            TooltipHandler.TipRegion(nameRect, $"{"RimFlix_SortHeader".Translate()} {"RimFlix_NameHeader".Translate()}");
            if (GUI.Button(nameRect, $"RimFlix_NameHeader".Translate(), GUI.skin.GetStyle("Label")))
            {
                this.sortType = SortType.Name;
                this.shows = GetSortedShows(true);
            }
            x += nameWidth + cellPadding;

            Rect framesRect = new Rect(x, y, framesWidth, cellHeight);
            TooltipHandler.TipRegion(framesRect, $"{"RimFlix_SortHeader".Translate()} {"RimFlix_FramesHeader".Translate()}");
            if (GUI.Button(framesRect, "RimFlix_FramesHeader".Translate(), GUI.skin.GetStyle("Label")))
            {
                this.sortType = SortType.Frames;
                this.shows = GetSortedShows(true);
            }
            x += framesWidth + cellPadding;

            Rect timeRect = new Rect(x, y, framesWidth, cellHeight);
            TooltipHandler.TipRegion(timeRect, $"{"RimFlix_SortHeader".Translate()} {"RimFlix_SecFrameHeader".Translate()}");
            if (GUI.Button(timeRect, "RimFlix_SecFrameHeader".Translate(), GUI.skin.GetStyle("Label")))
            {
                this.sortType = SortType.Time;
                this.shows = GetSortedShows(true);
            }
            x += timeWidth + cellPadding;

            Rect tubeRect = new Rect(x, y, tubeWidth, cellHeight);
            Widgets.DrawTextureFitted(tubeRect, this.TubeTex, 0.6f);
            TooltipHandler.TipRegion(tubeRect, ThingDef.Named("TubeTelevision").LabelCap);
            if (GUI.Button(tubeRect, "", GUI.skin.GetStyle("Label")))
            {
                this.sortType = SortType.Tube;
                this.shows = GetSortedShows(true);
            }
            x += tubeWidth + cellPadding;

            Rect flatRect = new Rect(x, y, flatWidth, cellHeight);
            Widgets.DrawTextureFitted(flatRect, this.FlatTex, 0.55f);
            TooltipHandler.TipRegion(flatRect, ThingDef.Named("FlatscreenTelevision").LabelCap);
            if (GUI.Button(flatRect, "", GUI.skin.GetStyle("Label")))
            {
                this.sortType = SortType.Flat;
                this.shows = GetSortedShows(true);
            }
            x += flatWidth + cellPadding;

            Rect megaRect = new Rect(x, y, megaWidth, cellHeight);
            Widgets.DrawTextureFitted(megaRect, this.MegaTex, 0.9f);
            TooltipHandler.TipRegion(megaRect, ThingDef.Named("MegascreenTelevision").LabelCap);
            if (GUI.Button(megaRect, "", GUI.skin.GetStyle("Label")))
            {
                this.sortType = SortType.Mega;
                this.shows = GetSortedShows(true);
            }
            x += megaWidth + cellPadding;

            Rect actionRect = new Rect(x, y, editWidth + deleteWidth + cellPadding, cellHeight);
            //Widgets.Label(actionRect, "RimFlix_ActionsHeader".Translate());
            if (GUI.Button(actionRect, "RimFlix_ActionsHeader".Translate(), GUI.skin.GetStyle("Label")))
            {
                this.sortType = SortType.Action;
                this.shows = GetSortedShows(true);
            }
            Rect editRect = new Rect(x, y, editWidth, cellHeight);
            Rect deleteRect = new Rect(x + editWidth + cellPadding, y, deleteWidth, cellHeight);

            // Table rows
            Text.Anchor = TextAnchor.UpperLeft;
            Rect tableRect = rect.BottomPartPixels(rect.height - cellHeight - cellPadding);
            x = tableRect.x;
            y = tableRect.y + cellPadding;
            float viewHeight = this.Shows.Count * (cellHeight + cellPadding) + 50f;
            Rect viewRect = new Rect(x, y, tableRect.width - this.scrollBarWidth, viewHeight);
            Rect rowRect = new Rect(x, y, rect.width, cellHeight);
            Widgets.BeginScrollView(tableRect, ref this.scrollPos, viewRect, true);
            int index = 0;
            foreach (ShowDef show in this.Shows)
            {
                rowRect.y = nameRect.y = framesRect.y = timeRect.y = tubeRect.y = flatRect.y = megaRect.y = actionRect.y = editRect.y = deleteRect.y = y + (cellHeight + cellPadding) * index++;

                if (index % 2 == 1)
                {
                    //Widgets.DrawAltRect(rectRow);
                    Widgets.DrawAltRect(nameRect);
                    Widgets.DrawAltRect(framesRect);
                    Widgets.DrawAltRect(timeRect);
                    Widgets.DrawAltRect(tubeRect);
                    Widgets.DrawAltRect(flatRect);
                    Widgets.DrawAltRect(megaRect);
                    Widgets.DrawAltRect(editRect);
                    Widgets.DrawAltRect(deleteRect);
                }
                if (show.disabled)
                {
                    GUI.color = this.disabledTextColor;
                }
                Widgets.DrawHighlightIfMouseover(rowRect);

                Text.Anchor = TextAnchor.MiddleLeft;
                TooltipHandler.TipRegion(nameRect, show.description);
                Widgets.Label(nameRect, show.SortName);

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(framesRect, $"{show.frames.Count} ");
                Widgets.Label(timeRect, $"{show.secondsBetweenFrames:F2} ");

                Text.Anchor = TextAnchor.MiddleCenter;
                TooltipHandler.TipRegion(tubeRect, ThingDef.Named("TubeTelevision").LabelCap);
                if (show.televisionDefs.Contains(ThingDef.Named("TubeTelevision")))
                {
                    Widgets.DrawTextureFitted(tubeRect, this.TubeTex, 0.6f);
                    TooltipHandler.TipRegion(tubeRect, ThingDef.Named("TubeTelevision").LabelCap);
                }

                TooltipHandler.TipRegion(flatRect, ThingDef.Named("FlatscreenTelevision").LabelCap);
                if (show.televisionDefs.Contains(ThingDef.Named("FlatscreenTelevision")))
                {
                    Widgets.DrawTextureFitted(flatRect, this.FlatTex, 0.55f);
                    TooltipHandler.TipRegion(flatRect, ThingDef.Named("FlatscreenTelevision").LabelCap);
                }

                TooltipHandler.TipRegion(megaRect, ThingDef.Named("MegascreenTelevision").LabelCap);
                if (show.televisionDefs.Contains(ThingDef.Named("MegascreenTelevision")))
                {
                    Widgets.DrawTextureFitted(megaRect, this.MegaTex, 0.9f);
                    TooltipHandler.TipRegion(megaRect, ThingDef.Named("MegascreenTelevision").LabelCap);
                }

                GUI.color = Color.white;
                if (show is UserShowDef userShow)
                {
                    if (Widgets.ButtonText(editRect, "RimFlix_EditButton".Translate()))
                    {
                        Find.WindowStack.Add(new Dialog_AddShow(userShow, this));
                    }
                    if (Widgets.ButtonText(deleteRect, "RimFlix_DeleteButton".Translate()))
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            "RimFlix_ConfirmDelete".Translate(userShow.SortName), delegate
                            {
                                UserContent.RemoveUserShow(userShow);
                            }, true, null));
                    }
                }
                else
                {
                    if (show.disabled)
                    {
                        if (Widgets.ButtonText(actionRect, "RimFlix_EnableButton".Translate()))
                        {
                            show.disabled = false;
                            this.settings.DisabledShows.Remove(show.defName);
                            // We want to alert CompScreen of show update, but avoid messing up sort order by requerying here
                            this.ShowUpdateTime = RimFlixSettings.showUpdateTime = RimFlixSettings.TotalSeconds;
                        }
                    }
                    else
                    {
                        if (Widgets.ButtonText(actionRect, "RimFlix_DisableButton".Translate()))
                        {
                            show.disabled = true;
                            this.settings.DisabledShows.Add(show.defName);
                            // We want to alert CompScreen of show update, but avoid messing up sort order by requerying here
                            this.ShowUpdateTime = RimFlixSettings.showUpdateTime = RimFlixSettings.TotalSeconds;
                        }
                    }
                }

                if (show.disabled)
                {
                    GUI.DrawTexture(rowRect, this.DisabledTex);
                    Vector2 leftVec = new Vector2(rowRect.x, rowRect.y + cellHeight / 2);
                    Vector2 rightVec = new Vector2(rowRect.x + rowRect.width - this.scrollBarWidth, rowRect.y + cellHeight / 2);
                    Widgets.DrawLine(leftVec, rightVec, this.disabledLineColor, 2f);
                }
            }
            Widgets.EndScrollView();
            GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperLeft;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            // 864 x 584
            float optionsWidth = 500f;
            float statusWidth = 250f;
            float optionsHeight = 150f;
            float showsHeight = 418f;

            // Adjust screen button
            Text.Font = GameFont.Medium;
            Vector2 titleSize = Text.CalcSize("RimFlix_Title".Translate());
            Rect adjustRect = new Rect(titleSize.x + 180, 2, this.AdjustTex.width, this.AdjustTex.height);
            TooltipHandler.TipRegion(adjustRect, "RimFlix_AdjustSreenTitle".Translate());
            if (Widgets.ButtonImage(adjustRect, this.AdjustTex))
            {
                Find.WindowStack.Add(new Dialog_AdjustScreen());
            }

            Text.Font = GameFont.Small;
            Rect topRect = rect.TopPartPixels(optionsHeight);
            Rect optionsRect = topRect.LeftPartPixels(optionsWidth);
            Rect statusRect = topRect.RightPartPixels(statusWidth);
            Rect showsRect = rect.BottomPartPixels(showsHeight);

            DoOptions(optionsRect);
            DoStatus(statusRect);
            DoShows(showsRect);

            base.DoSettingsWindowContents(rect);
        }

        public override string SettingsCategory()
        {
            return "RimFlix_Title".Translate();
        }

        // Use cached shows to avoid possible infinite recursion
        private List<ShowDef> GetSortedShows(bool toggleAsc)
        {
            int i = (int)this.sortType;
            if (toggleAsc)
            {
                this.sortAsc[i] = !this.sortAsc[i];
            }

            if (this.sortType == SortType.Name)
            {
                return this.sortAsc[i]
                    ? this.shows.OrderBy(s => s.SortName).ToList()
                    : this.shows.OrderByDescending(s => s.SortName).ToList();
            }
            if (this.sortType == SortType.Frames)
            {
                return this.sortAsc[i]
                    ? this.shows.OrderBy(s => s.frames.Count).ToList()
                    : this.shows.OrderByDescending(s => s.frames.Count).ToList();
            }
            if (this.sortType == SortType.Time)
            {
                return this.sortAsc[i]
                    ? this.shows.OrderBy(s => s.secondsBetweenFrames).ToList()
                    : this.shows.OrderByDescending(s => s.secondsBetweenFrames).ToList();
            }
            if (this.sortType == SortType.Tube)
            {
                return this.sortAsc[i]
                    ? this.shows.OrderBy(s => s.televisionDefs.Contains(ThingDef.Named("TubeTelevision"))).ToList()
                    : this.shows.OrderByDescending(s => s.televisionDefs.Contains(ThingDef.Named("TubeTelevision"))).ToList();
            }
            if (this.sortType == SortType.Flat)
            {
                return this.sortAsc[i]
                    ? this.shows.OrderBy(s => s.televisionDefs.Contains(ThingDef.Named("FlatscreenTelevision"))).ToList()
                    : this.shows.OrderByDescending(s => s.televisionDefs.Contains(ThingDef.Named("FlatscreenTelevision"))).ToList();
            }
            if (this.sortType == SortType.Mega)
            {
                return this.sortAsc[i]
                    ? this.shows.OrderBy(s => s.televisionDefs.Contains(ThingDef.Named("MegascreenTelevision"))).ToList()
                    : this.shows.OrderByDescending(s => s.televisionDefs.Contains(ThingDef.Named("MegascreenTelevision"))).ToList();
            }
            if (this.sortType == SortType.Action)
            {
                return this.sortAsc[i]
                    ? this.shows.OrderBy(s => s.disabled).ToList()
                    : this.shows.OrderByDescending(s => s.disabled).ToList();
            }
            return this.shows;
        }
    }

    public enum SortType
    {
        None,
        Name,
        Frames,
        Time,
        Tube,
        Flat,
        Mega,
        Action
    }

    public enum DrawType
    {
        Stretch,
        Fit,
        Fill
    }
}
