using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimFlix
{
    class Dialog_AddShow : Window
    {
        private RimFlixSettings settings;
        private RimFlixMod mod;
        private UserShowDef currentUserShow;

        // Widget sizes
        private float padding = 8;
        private float scrollBarWidth = 16;
        private float drivesWidth = 80;
        private float filesWidth = 370;
        private float optionsWidth = 250;
        private float pathHeight = Text.LineHeight;
        private float buttonsHeight = 32;
        private float timeInputWidth;
        private float timeUnitWidth;

        // File explorer panel
        private string[] drives;
        private string currentPath;
        private string lastPath = "";
        private Regex pathValidator;
        private Texture2D refreshTex;
        private bool dirInfoDirty = true;
        private DirectoryInfo dirInfo;
        private IEnumerable<DirectoryInfo> dirs;
        private IEnumerable<FileInfo> files;
        private Color drivesBackgroundColor = new Color(0.17f, 0.18f, 0.19f);
        private Color filesBackgroundColor = new Color(0.08f, 0.09f, 0.11f);
        private Color filesTextColor = new Color(0.6f, 0.6f, 0.6f);
        private Vector2 drivesScrollPos = Vector2.zero;
        private Vector2 filesScrollPos = Vector2.zero;

        // Options panel
        private int framesCount;
        private string showName;
        private float timeValue;
        private TimeUnit timeUnit = TimeUnit.Second;
        private string[] timeUnitLabels;
        private List<FloatMenuOption> timeUnitMenu;
        private bool playTube;
        private bool playFlat;
        private bool playMega;

        public Dialog_AddShow(UserShowDef userShow = null, RimFlixMod mod = null)
        {
            // Window properties
            this.doCloseX = true;
            this.doCloseButton = false;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;

            // Initialize object
            this.settings = LoadedModManager.GetMod<RimFlixMod>().GetSettings<RimFlixSettings>();
            this.refreshTex = ContentFinder<Texture2D>.Get("UI/Buttons/Refresh", true);
            Vector2 v1 = Text.CalcSize("RimFlix_TimeSeconds".Translate());
            Vector2 v2 = Text.CalcSize("RimFlix_TimeTicks".Translate());
            this.timeUnitWidth = Math.Max(v1.x, v2.x) + this.padding * 2;
            this.timeInputWidth = this.optionsWidth - this.timeUnitWidth - this.padding * 2;
            this.timeUnitMenu = new List<FloatMenuOption>
            {
                new FloatMenuOption("RimFlix_TimeSeconds".Translate(), delegate { this.timeUnit = TimeUnit.Second; }),
                new FloatMenuOption("RimFlix_TimeTicks".Translate(), delegate { this.timeUnit = TimeUnit.Tick; })
            };
            this.timeUnitLabels = new string[]
            {
                "RimFlix_TimeSeconds".Translate(),
                "RimFlix_TimeTicks".Translate()
            };
            string s = new string(Path.GetInvalidFileNameChars());
            this.pathValidator = new Regex(string.Format("[^{0}]*", Regex.Escape(s)));
            try
            {
                this.drives = Directory.GetLogicalDrives();
            }
            catch (Exception ex)
            {
                Log.Message($"RimFlix: Exception for GetLogicalDrives():\n{ex}");
                this.filesWidth += this.drivesWidth;
                this.drivesWidth = 0;
            }

            // Show properties
            if (userShow != null)
            {
                this.showName = userShow.label;
                this.currentPath = userShow.path;
                this.timeValue = userShow.secondsBetweenFrames;
                this.playTube = userShow.televisionDefStrings.Contains("TubeTelevision");
                this.playFlat = userShow.televisionDefStrings.Contains("FlatscreenTelevision");
                this.playMega = userShow.televisionDefStrings.Contains("MegascreenTelevision");
            }
            else
            {
                this.showName = "RimFlix_DefaultName".Translate();
                this.currentPath = Directory.Exists(this.settings.lastPath) ? this.settings.lastPath : this.settings.defaultPath;
                this.timeValue = 10;
                this.playTube = false;
                this.playFlat = false;
                this.playMega = false;
            }
            this.currentUserShow = userShow;
            this.mod = mod;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(736f, 536f);
            }
        }

        private void DoExplorer(Rect rect)
        {
            Rect rectPath = rect.TopPartPixels(this.pathHeight);
            DoPath(rectPath);

            Rect rectItems = rect.BottomPartPixels(rect.height - this.pathHeight - this.padding);
            if (drives != null)
            {
                Rect rectDrives = rectItems.LeftPartPixels(drivesWidth);
                DoDrives(rectDrives);
            }
            Rect rectFiles = rectItems.RightPartPixels(filesWidth);
            DoFiles(rectFiles);
        }

        private void DoPath(Rect rect)
        {
            //Widgets.DrawBox(rect);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            float labelWidth = Text.CalcSize("RimFlix_CurrentDirectoryLabel".Translate()).x + this.padding;
            Widgets.Label(rect.LeftPartPixels(labelWidth), "RimFlix_CurrentDirectoryLabel".Translate());
            Rect rightRect = rect.RightPartPixels(rect.width - labelWidth);
            Rect refreshRect = rightRect.RightPartPixels(rightRect.height);
            if (Widgets.ButtonImage(refreshRect, this.refreshTex, Color.gray, GenUI.SubtleMouseoverColor))
            {
                this.dirInfoDirty = true;
                this.soundAppear.PlayOneShotOnCamera(null);
            }
            // Using Color.gray for button changes default GUI color, so we need to change it back to white
            GUI.color = Color.white;
            currentPath = Widgets.TextField(rightRect.LeftPartPixels(rightRect.width - refreshRect.width - this.padding), currentPath, int.MaxValue, this.pathValidator);

        }

        private void DoDrives(Rect rect)
        {
            //Widgets.DrawBox(rect);
            Widgets.DrawBoxSolid(rect, this.drivesBackgroundColor);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            float buttonHeight = 24;
            Rect rectView = new Rect(0, 0, rect.width - this.scrollBarWidth, buttonHeight * this.drives.Count());
            Widgets.BeginScrollView(rect, ref this.drivesScrollPos, rectView);
            int index = 0;
            foreach (string drive in this.drives)
            {
                Rect rectButton = new Rect(rectView.x, rectView.y + index * buttonHeight, rectView.width + this.scrollBarWidth, buttonHeight);
                if (Widgets.ButtonText(rectButton, $" {drive}", false, false, true))
                {
                    this.currentPath = drive;
                    this.dirInfoDirty = true;
                    this.soundAmbient.PlayOneShotOnCamera(null);
                }
                if (drive == Path.GetPathRoot(this.currentPath))
                {
                    Widgets.DrawHighlightSelected(rectButton);
                }
                else
                {
                    Widgets.DrawHighlightIfMouseover(rectButton);
                }
                index++;
            }
            Widgets.EndScrollView();
        }

        private void UpdateDirInfo(string path)
        {
            if (!this.dirInfoDirty && path.Equals(this.lastPath))
            {
                return;
            }
            try
            {
                this.dirInfo = new DirectoryInfo(this.currentPath);

                this.dirs = from dir in dirInfo.GetDirectories()
                            where (dir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden
                            orderby dir.Name
                            select dir;

                this.files = from file in this.dirInfo.GetFiles()
                             where file.Name.ToLower().EndsWith(".jpg") || file.Name.ToLower().EndsWith(".png")
                             where (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden
                             orderby file.Name
                             select file;
            }
            catch
            {
                this.dirs = Enumerable.Empty<DirectoryInfo>();
                this.files = Enumerable.Empty<FileInfo>();
            }
            this.lastPath = this.currentPath;
            settings.lastPath = this.currentPath;
            this.dirInfoDirty = false;
        }

        private void DoFiles(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, this.filesBackgroundColor);
            this.framesCount = 0;
            if (!Directory.Exists(this.currentPath))
            {
                return;
            }

            Text.Font = GameFont.Small;
            UpdateDirInfo(this.currentPath);
            int count = this.dirs.Count() + this.files.Count();
            if (dirInfo.Parent != null)
            {
                count++;
            }
            float buttonHeight = Text.LineHeight;
            Rect rectView = new Rect(0, 0, rect.width - this.scrollBarWidth, buttonHeight * count);
            Widgets.BeginScrollView(rect, ref this.filesScrollPos, rectView);
            int index = 0;

            // Parent directory
            Rect rectButton = new Rect(rectView.x, rectView.y + index * buttonHeight, rectView.width + this.scrollBarWidth, buttonHeight);
            if (dirInfo.Parent != null)
            {
                //Rect rectButton = new Rect(rectView.x, rectView.y + index * buttonHeight, rectView.width + this.scrollBarWidth, buttonHeight);
                Widgets.DrawAltRect(rectButton);
                Widgets.DrawHighlightIfMouseover(rectButton);
                if (Widgets.ButtonText(rectButton, " ..", false, false, Color.white, true))
                {
                    this.currentPath = dirInfo.Parent.FullName;
                    this.dirInfoDirty = true;
                }
                rectButton.y += buttonHeight;
                index++;
            }

            // Directories
            foreach (DirectoryInfo d in dirs)
            {
                //Rect rectButton = new Rect(rectView.x, rectView.y + index * buttonHeight, rectView.width + this.scrollBarWidth, buttonHeight);
                if (index % 2 == 0)
                {
                    Widgets.DrawAltRect(rectButton);
                }
                Widgets.DrawHighlightIfMouseover(rectButton);
                if (Widgets.ButtonText(rectButton, $" {d.Name}", false, false, Color.white, true))
                {
                    d.Refresh();
                    if (d.Exists)
                    {
                        this.currentPath = d.FullName;
                        this.dirInfoDirty = true;
                    }
                    else
                    {
                        Find.WindowStack.Add(new Dialog_MessageBox($"{"RimFlix_DirNotFound".Translate()}: {d.FullName}"));
                        this.dirInfoDirty = true;
                    }
                }
                rectButton.y += buttonHeight;
                index++;
            }

            // Files
            foreach (FileInfo f in files)
            {
                this.framesCount++;
                //Rect rectButton = new Rect(rectView.x, rectView.y + index * buttonHeight, rectView.width + this.scrollBarWidth, buttonHeight);
                if (index % 2 == 0)
                {
                    Widgets.DrawAltRect(rectButton);
                }
                Widgets.DrawHighlightIfMouseover(rectButton);
                if (Widgets.ButtonText(rectButton, $" {f.Name}", false, false, this.filesTextColor, true))
                {
                    f.Refresh();
                    if (f.Exists)
                    {
                        Find.WindowStack.Add(new Dialog_Preview(f.FullName, f.Name));
                    }
                    else
                    {
                        Find.WindowStack.Add(new Dialog_MessageBox($"{"RimFlix_FileNotFound".Translate()}: {f.FullName}"));
                        this.dirInfoDirty = true;
                    }
                }
                rectButton.y += buttonHeight;
                index++;
            }
            Widgets.EndScrollView();
        }

        private void DoOptions(Rect rect)
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            float y = rect.y;
            float x = rect.x + this.padding;
            float width = rect.width - this.padding;

            // Frame count
            string framesText = this.framesCount == 1 ? "RimFlix_FrameLabel".Translate(this.framesCount) : "RimFlix_FramesLabel".Translate(this.framesCount);
            Widgets.Label(new Rect(x, y, width, Text.LineHeight), framesText);
            y += Text.LineHeight + this.padding / 2;
            Widgets.Label(new Rect(x, y, width, Text.LineHeight), "RimFlix_InfoLabel".Translate());
            y += Text.LineHeight + this.padding * 4;

            // Show name
            Widgets.Label(new Rect(x, y, width, Text.LineHeight), "RimFlix_NameLabel".Translate());
            y += Text.LineHeight + this.padding / 2;
            this.showName = Widgets.TextField(new Rect(x, y, width, Text.LineHeight), this.showName);
            y += Text.LineHeight + this.padding * 4;

            // Time between frames
            Widgets.Label(new Rect(x, y, width, Text.LineHeight), "RimFlix_TimeLabel".Translate());
            y += Text.LineHeight + this.padding / 2;
            Rect rectTimeInput = new Rect(x, y, width, Text.LineHeight);
            string buffer = this.timeValue.ToString();
            Widgets.TextFieldNumeric(new Rect(rectTimeInput.LeftPartPixels(this.timeInputWidth)), ref this.timeValue, ref buffer, 1f, float.MaxValue - 1);
            if (Widgets.ButtonText(new Rect(rectTimeInput.RightPartPixels(this.timeUnitWidth)), this.timeUnitLabels[(int)this.timeUnit]))
            {
                Find.WindowStack.Add(new FloatMenu(this.timeUnitMenu));
            }
            y += Text.LineHeight;
            Widgets.Label(new Rect(x, y, width, Text.LineHeight), string.Format("(1 {0} = {1} {2})", "RimFlix_TimeSecond".Translate(), 60, "RimFlix_TimeTicks".Translate()));
            y += Text.LineHeight + this.padding * 4;

            // Television types
            float checkX = x + this.padding;
            float checkWidth = width - this.padding * 2;
            Widgets.Label(new Rect(x, y, width, Text.LineHeight), "RimFlix_PlayLabel".Translate());
            y += Text.LineHeight + this.padding / 2;
            Rect rectTube = new Rect(checkX, y, checkWidth, Text.LineHeight);
            Widgets.DrawHighlightIfMouseover(rectTube);
            Widgets.CheckboxLabeled(rectTube, ThingDef.Named("TubeTelevision").LabelCap, ref this.playTube);
            y += Text.LineHeight;
            Rect rectFlat = new Rect(checkX, y, checkWidth, Text.LineHeight);
            Widgets.DrawHighlightIfMouseover(rectFlat);
            Widgets.CheckboxLabeled(rectFlat, ThingDef.Named("FlatscreenTelevision").LabelCap, ref this.playFlat);
            y += Text.LineHeight;
            Rect rectMega = new Rect(checkX, y, checkWidth, Text.LineHeight);
            Widgets.DrawHighlightIfMouseover(rectMega);
            Widgets.CheckboxLabeled(rectMega, ThingDef.Named("MegascreenTelevision").LabelCap, ref this.playMega);
            y += Text.LineHeight + this.padding * 4;

            // Note
            GUI.color = Color.gray;
            Widgets.Label(new Rect(x, y, width, Text.LineHeight * 3), "RimFlix_Note01".Translate());
            GUI.color = Color.white;

        }

        private void DoButtons(Rect rect)
        {
            Vector2 cancelSize = Text.CalcSize("RimFlix_CancelButton".Translate());
            cancelSize.Set(cancelSize.x + 4 * this.padding, cancelSize.y + 1 * this.padding);
            Rect rectCancel = new Rect(rect.x + rect.width - cancelSize.x, rect.y, cancelSize.x, cancelSize.y);
            TooltipHandler.TipRegion(rectCancel, "RimFlix_CancelTooltip".Translate());
            if (Widgets.ButtonText(rectCancel, "RimFlix_CancelButton".Translate(), true, false, true))
            {
                this.Close(false);
            }

            Vector2 createSize = Text.CalcSize("RimFlix_AddShowButton".Translate());
            createSize.Set(createSize.x + 4 * this.padding, createSize.y + 1 * this.padding);
            Rect rectCreate = new Rect(rect.x + rect.width - cancelSize.x - createSize.x - this.padding, rect.y, createSize.x, createSize.y);
            TooltipHandler.TipRegion(rectCreate, "RimFlix_AddShowTooltip2".Translate());
            string buttonLabel = this.currentUserShow == null ? "RimFlix_AddShowButton".Translate() : "RimFlix_EditShowButton".Translate();
            if (Widgets.ButtonText(rectCreate, buttonLabel, true, false, true))
            {
                if (CreateShow())
                {
                    this.Close(true);
                }
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            Rect rectExplorer = rect.LeftPartPixels(this.drivesWidth + this.filesWidth);
            Rect rectRight = rect.RightPartPixels(this.optionsWidth);
            Rect rectOptions = rectRight.TopPartPixels(rectRight.height - this.buttonsHeight);
            Rect rectButtons = rectRight.BottomPartPixels(this.buttonsHeight);

            DoExplorer(rectExplorer);
            DoOptions(rectOptions);
            DoButtons(rectButtons);
        }

        private bool CreateShow()
        {
            // Check if path contains images
            if (!Directory.Exists(this.currentPath))
            {
                Find.WindowStack.Add(new Dialog_MessageBox("RimFlix_DirNotFound".Translate(this.currentPath)));
                return false;
            }

            // Check if name exists
            if (this.showName.NullOrEmpty())
            {
                Find.WindowStack.Add(new Dialog_MessageBox("RimFlix_NoShowName".Translate()));
                return false;
            }

            // Check if at least one television type is selected
            if (!(this.playTube || this.playFlat || this.playMega))
            {
                Find.WindowStack.Add(new Dialog_MessageBox("RimFlix_NoTelevisionType".Translate()));
                return false;
            }

            // Create or modify user show
            UserShowDef userShow = this.currentUserShow ?? new UserShowDef() { defName = UserContent.GetUniqueId() };
            userShow.path = this.currentPath;
            userShow.label = this.showName;
            userShow.description = $"{"RimFlix_CustomDescPrefix".Translate(this.currentPath, DateTime.Now.ToString())}";
            userShow.secondsBetweenFrames = (this.timeUnit == TimeUnit.Tick) ? this.timeValue / 60f : this.timeValue;
            userShow.televisionDefStrings = new List<string>();
            if (this.playTube)
            {
                userShow.televisionDefStrings.Add("TubeTelevision");
            }
            if (this.playFlat)
            {
                userShow.televisionDefStrings.Add("FlatscreenTelevision");
            }
            if (this.playMega)
            {
                userShow.televisionDefStrings.Add("MegascreenTelevision");
            }

            // Load show assets and add to def database
            if (!UserContent.LoadUserShow(userShow))
            {
                Log.Message($"RimFlix: Unable to load assets for {userShow.defName} : {userShow.label}");
                Find.WindowStack.Add(new Dialog_MessageBox($"{"RimFlix_LoadError".Translate()}"));
                return false;
            }

            // Add show to settings
            if (this.currentUserShow == null)
            {
                this.settings.UserShows.Add(userShow);
            }

            // Mark shows as dirty
            RimFlixSettings.showUpdateTime = RimFlixSettings.TotalSeconds;

            // If editing a show, avoid requerying shows to keep sort order
            if (this.currentUserShow != null)
            {
                userShow.SortName = $"{"RimFlix_UserShowLabel".Translate()} : {userShow.label}";
                mod.ShowUpdateTime = RimFlixSettings.showUpdateTime;
            }
            return true;
        }

        public override void OnAcceptKeyPressed()
        {
            if (CreateShow())
            {
                base.OnAcceptKeyPressed();
            }
        }
    }

    public enum TimeUnit
    {
        Second = 0,
        Tick = 1,
    }
}
