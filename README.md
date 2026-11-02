
# ![SpeedMeter Mod](https://old.thunderstore.io/thumbnail-serve/repository/icons/CoomGod-SpeedMeter-0.4.5.png/?width=64&height=64)  SpeedMeter mod 
The first mod i made for [BeetleBall](https://store.steampowered.com/app/2776270/Beetleball/), it adds a speedometer that tracks your speed and can be mildly customized


## Building from source

So like i made this mod without really thinking of making it future proof to build again so youll probably have to rewrite all the paths to dlls on the .csproj. I made this on linux and for some reason the melonloader team decided to make the wizard for making melonloader projects on Visual Studio which works exclusively on Windows, so i had to make a tool for wiring all the extracted dlls from the game.

Truth be told i dont use every single reference so you probably can delete 90% of them and leave the melonloader one and a few more.

## Usage
Other than that just make sure that the assets are properly installed to work
   ```
   Beetleball/
   └── Mods/
       ├── SpeedMeterMod.dll
       └── assets/
           ├── needle.png
           └── speedmeter.png
   ```
Also after the first launch a config file should appear in that lets you customize some things
   ```
   Beetleball/
   └── UserData/
       └── MelonPreferences.cfg
   ```
Should have this:
```
[SpeedMeterMod]
ShowSpeedMeterOnStartup = true
ShowSpeedPeaks = false
ShowActualSpeed = false
NeedleSmooth = 5.0
MaxSpeed = 240.0
FunnySpeedMultiplier = 1.0
SpeedPeakResetTimer = 3000.0
```
Modify it to your liking, also f4 disables the speedometer