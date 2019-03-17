using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;
using UnityEngine;
using System.Text;

namespace RimFlix
{
    [StaticConstructorOnStartup]
    public static class UserContent
    {
        public static ModContentPack RimFlixMod
        {
            get
            {
                return (from mod in LoadedModManager.RunningMods
                        where mod.Name == "RimFlix"
                        select mod).First();
            }
        }

        public static ModContentHolder<Texture2D> RimFlixContent
        {
            get
            {
                return RimFlixMod.GetContentHolder<Texture2D>();
            }
        }

        static UserContent()
        {
            // Load user shows
            LoadUserShowDefs();

            // Set disabled status from settings
            ResolveDisabledShows();

            RimFlixSettings.showUpdateTime = RimFlixSettings.TotalSeconds;
            RimFlixSettings.screenUpdateTime = RimFlixSettings.TotalSeconds;

            //AllTexturesLoaded();
        }

        public static void ResolveDisabledShows()
        {
            RimFlixSettings settings = LoadedModManager.GetMod<RimFlixMod>().GetSettings<RimFlixSettings>();
            IEnumerable<ShowDef> shows = DefDatabase<ShowDef>.AllDefs;
            foreach (ShowDef show in shows)
            {
                show.disabled = settings.DisabledShows == null ? false : settings.DisabledShows.Contains(show.defName);
            }
        }

        public static void LoadUserShowDefs()
        {
            RimFlixSettings settings = LoadedModManager.GetMod<RimFlixMod>().GetSettings<RimFlixSettings>();
            List<UserShowDef> invalidShows = new List<UserShowDef>();
            int count = 0;
            foreach (UserShowDef userShow in settings.UserShows)
            {
                if (LoadUserShow(userShow))
                {
                    count++;
                }
                else
                {
                    invalidShows.Add(userShow);
                    Log.Message($"Removed {userShow.defName} : {userShow.label} from list.");
                }
            }
            if (count != settings.UserShows.Count)
            {
                Log.Message($"RimFlix: {count} out of {settings.UserShows.Count} UserShowDefs loaded.");
            }
            settings.UserShows.RemoveAll(show => invalidShows.Contains(show));
        }

        public static bool LoadUserShow(UserShowDef userShow, bool addToDefDatabase = true)
        {
            // Get images in path
            IEnumerable<string> filePaths = Enumerable.Empty<string>();
            if (!Directory.Exists(userShow.path))
            {
                Log.Message($"RimFlix {userShow.defName} : {userShow.label}: Path <{userShow.path}> does not exist.");
                return false;
            }
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(userShow.path);
                filePaths = from file in dirInfo.GetFiles()
                            where file.Name.ToLower().EndsWith(".jpg") || file.Name.ToLower().EndsWith(".png")
                            where (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden
                            select file.FullName;
            }
            catch
            {
                Log.Message($"RimFlix {userShow.defName} : {userShow.label}: Error trying to load files from <{userShow.path}>.");
                return false;
            }
            if (!filePaths.Any())
            {
                Log.Message($"RimFlix {userShow.defName} : {userShow.label}: No images found in <{userShow.path}>.");
                // User may want to keep a show with an empty directory for future use.
                //return false;
            }

            // Load textures for images
            userShow.frames = new List<GraphicData>();
            foreach (string filePath in filePaths)
            {
                // RimWorld sets internalPath to filePath without extension
                // This causes problems with files that have same name but different extension (file.jpg, file.png)
                string internalPath = filePath.Replace('\\', '/');
                if (!RimFlixContent.contentList.ContainsKey(internalPath))
                {
                    LoadedContentItem<Texture2D> loadedContentItem = ModContentLoader<Texture2D>.LoadItem(filePath);
                    loadedContentItem.internalPath = internalPath;
                    RimFlixContent.contentList.Add(loadedContentItem.internalPath, loadedContentItem.contentItem);
                }
                userShow.frames.Add(new GraphicData
                {
                    texPath = internalPath,
                    graphicClass = typeof(Graphic_Single)
                });
            }

            // Create televisionDefs list
            userShow.televisionDefs = new List<ThingDef>();
            foreach (string televisionDefString in userShow.televisionDefStrings)
            {
                userShow.televisionDefs.Add(ThingDef.Named(televisionDefString));
            }

            // Add user show to def database
            if (!DefDatabase<ShowDef>.AllDefs.Contains(userShow))
            {
                DefDatabase<ShowDef>.Add(userShow);
            }
            return true;
        }

        public static bool RemoveUserShow(UserShowDef userShow)
        {
            // Remove from settings
            RimFlixSettings settings = LoadedModManager.GetMod<RimFlixMod>().GetSettings<RimFlixSettings>();
            if (!settings.UserShows.Contains(userShow))
            {
                Log.Message($"RimFlix: Could not find show {userShow.defName} : {userShow.label}");
                return false;
            }
            bool result = settings.UserShows.Remove(userShow);

            // We can't delete from DefDatabase, so mark as deleted
            userShow.deleted = true;

            // Do not remove graphic data if there are other shows with same path
            IEnumerable<UserShowDef> twins = DefDatabase<UserShowDef>.AllDefs.Where(s => (s.path?.Equals(userShow.path) ?? false) && !s.deleted);
            if (!twins.Any())
            {
                foreach (GraphicData frame in userShow.frames)
                {
                    result = RimFlixContent.contentList.Remove(frame.texPath);
                }
            }
            RimFlixSettings.showUpdateTime = RimFlixSettings.TotalSeconds;
            return true;
        }

        public static void AllTexturesLoaded()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("There are " + RimFlixContent.contentList.Count + " graphics loaded for RimFlix.");
            int num = 0;
            foreach (KeyValuePair<string, Texture2D> entry in RimFlixContent.contentList)
            {
                stringBuilder.AppendLine($"{num} - {entry.Key} : {entry.Value}");
                if (num % 50 == 49)
                {
                    Log.Message(stringBuilder.ToString());
                    stringBuilder = new StringBuilder();
                }
                num++;
            }
            Log.Message(stringBuilder.ToString());
        }

        public static string GetUniqueId()
        {
            // 31,536,000 seconds in a year
            // 10 digit number string allows ids to remain sorted for ~300 years
            string diffStr = Math.Floor(RimFlixSettings.TotalSeconds).ToString();
            return $"UserShow_{diffStr.PadLeft(10, '0')}";
        }


    }

}
