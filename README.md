## Artisan Beer Bannerlord Mod Tutorial

* Youtube Playlist: https://www.youtube.com/playlist?list=PLzebdAxJeltRwfJ8jzsNolgHkRvLjoCRC
* Nexus Mods link: TBD

This repository contains the code for the YouTube Modding tutorial series. Most of the commits here correspond 1-to-1 with the code written in the corresponding episode.

If you want your local files to match the state as it was during a specific episode, you can use 'git checkout' to go to a past state. For example, to follow along Episode 8, you can 'git checkout' the previous episode (#7) commit.
* Git checkout in Visual Studio https://devblogs.microsoft.com/visualstudio/introducing-new-git-features-to-visual-studio-2022/#checkout-commits
* Generic git tutorial https://www.atlassian.com/git/tutorials/using-branches/git-checkout

### What is _Module folder?
It is part of the BUTR mod template. Everything inside this folder is copied to the game Module folder (the output).

There are some complications with this system, like not being able to use hot reloading while editing GauntletUI, files not updating if the build is skipped/fails, or that if you put your assets in _Module and later update them with the mod kit, your changes might be overwritten if you're not careful.

So to keep things simple in the tutorial series, I mostly just edited the assets and the .xml files directly in the game module directory. But as a consequence the files in this git repo are spread into *_Module*, *ModuleData* and *GUI* folders. 

### Assets used
* Beer mugs model by: thesyntox https://blendswap.com/blend/27498
* Sound effect by: Ashe Kirk https://freesound.org/people/OwlStorm/sounds/320139/
