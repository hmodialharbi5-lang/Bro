# AutoHellevator

TShock plugin for Terraria 1.4.5.6 / TShock 6.1.0.

## Command

`/hellelevator`

## What it does

Creates an instant vertical hellevator with this layout:

```text
W *** W
W *** W
W *** W
```

- `W` = Stone Slab block
- `***` = 3x3 empty shaft
- Background behind the shaft = Stone Slab Wall
- The shaft continues straight down
- The plugin performs the digging instantly

## Permission

`hellelevator.use`

Give a player the permission with your normal TShock permission command.

## Build

Put these files together with the TShock/Terraria DLLs used by your server:

- `AutoHellevator.cs`
- `AutoHellevator.csproj`
- `TerrariaServer.dll`
- `TShockAPI.dll`

Then build the project with:

`dotnet build AutoHellevator.csproj`

The resulting `AutoHellevator.dll` goes in your server's `ServerPlugins` folder.
