<p align="center">
  <img width="320" height="180" src="https://github.com/ritsu/RimFlix/blob/master/About/preview.png?raw=true" />
</p>

**RimFlix** is a *Rimworld mod* that allows players to display custom images in televisions. Players create *shows* from images in a local directory, assign a seconds-between-images value for the show, and set it to play on one or more TV types. A show with random images and a 10 second interval will act like a slideshow. A show with animation frames for images and a 0.033 second interval will act like a 30 fps animation. 

RimFlix can be safely added to and removed from existing games.

## How to create a show
1. Click the **Create Show** button
2. Browse to a directory that contains images you want to include in your show. All images in the directory will be included. You can click an image in the file browser to see how they will look in game.
3. Enter a name for your show.
4. Enter a time interval between images for your show.
5. Select the types of TVs on which you want your show to be played. 
6. Click **Create Show**

## Settings
**Play shows when no pawns are watching**: If enabled, TVs will play shows all the time, even when no pawns are watching. If disabled, TVs will only play shows when pawns are watching.

**Seconds until pawns change shows**: If allowed, pawns will change shows after watching the same show for this many seconds. TVs have new menu options for allowing pawns to change shows as well as to manually change shows.

**Power consumption when playing**: Set the power consumption for TVs when a show is playing. 100% is the same as vanilla.

**Power consumption when not playing**: Set the power consumption for TVs when now shows are playing. 100% is the same as vanilla.

**Resize images to TV screen using**: 
  * **Stretch**: Images will be stretched to fit the screen, ignoring aspect ratio. 
  * **Fit**: Images will be resized as large as possible while staying within the screen's boundaries and keeping aspect ratio.

**Adjust sreen size and position**: Click the icon at the top of the Settings window to open a new window that allows you to change the size and position of what the mod considers the "screen area" for each TV type.

## Adding shows from Steam Workshop and Github:
You can add shows others have created either from Steam Workshop or Github. <a href="https://github.com/ritsu/RimFlix-Anime-Loops">Here is an example.</a> You can also share your shows with others (see below).

## Artists and animators
You can create and share RimFlix shows by uploading them to Steam Workshop. No coding is required. You only need the image files and an XML file that defines each show. For an example of a mod with several shows featuring animation loops, see <a href = "https://github.com/ritsu/RimFlix-Anime-Loops">RimFlix - Anime Loops</a>. Other players can add your shows to their game as long as they have the RimFlix mod installed. 

## Translators
If you would like to help translate the settings and menu options, please contact me or submit a pull request. All translatable strings are in [Settings.xml](https://github.com/ritsu/RimFlix/blob/master/Languages/English/Keyed/Settings.xml). Thanks!




