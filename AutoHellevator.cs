using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace AutoHellevator
{
    [ApiVersion(2, 1)]
    public sealed class AutoHellevator : TerrariaPlugin
    {
        public override string Name => "AutoHellevator";
        public override string Author => "ChatGPT";
        public override string Description =>
            "Instant 3x3 hellevator with Stone Slab side blocks and Stone Slab Wall background.";
        public override Version Version => new Version(1, 0, 0, 0);

        public AutoHellevator(Main game) : base(game)
        {
            Order = 1;
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(
                new Command("hellelevator.use", HellevatorCommand, "hellelevator")
                {
                    HelpText = "Instantly creates a 3x3 hellevator straight down."
                });
        }

        private static void HellevatorCommand(CommandArgs args)
        {
            int centerX = (int)(args.Player.X / 16f);
            int startY = (int)(args.Player.Y / 16f) + 1;

            // Layout at every Y row:
            // W *** W
            // W *** W
            // W *** W
            // W = Stone Slab block
            // * = empty tile with Stone Slab Wall behind it

            if (centerX < 2 || centerX > Main.maxTilesX - 3)
            {
                args.Player.SendErrorMessage("You are too close to the world edge.");
                return;
            }

            int firstY = Math.Max(1, startY);
            int lastY = Main.maxTilesY - 2;

            if (firstY >= lastY)
            {
                args.Player.SendErrorMessage("There is not enough room below you.");
                return;
            }

            for (int y = firstY; y <= lastY; y++)
            {
                // Clear the 3-wide shaft and set Stone Slab Wall as its background.
                for (int x = centerX - 1; x <= centerX + 1; x++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile == null)
                        continue;

                    tile.ClearTile();
                    tile.WallType = WallID.StoneSlab;
                    tile.WallFrameNumber = 0;
                }

                // Put Stone Slab blocks on both sides.
                SetStoneSlab(centerX - 2, y);
                SetStoneSlab(centerX + 2, y);

                // Reframe the 5-wide strip.
                WorldGen.RangeFrame(centerX - 2, y, centerX + 2, y);
            }

            // Tell connected clients about the changed world.
            const int syncHeight = 100;

            for (int y = firstY; y <= lastY; y += syncHeight)
            {
                int height = Math.Min(syncHeight, lastY - y + 1);
                NetMessage.SendTileSquare(-1, centerX, y + height / 2, 3, height);
            }

            args.Player.SendSuccessMessage(
                "AutoHellevator complete! 3x3 shaft + Stone Slab side blocks + Stone Slab Wall background.");
        }

        private static void SetStoneSlab(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            if (tile == null)
                return;

            tile.HasTile = true;
            tile.TileType = TileID.StoneSlab;
            tile.Slope = SlopeType.Solid;
            tile.IsHalfBlock = false;
            tile.IsActuated = false;
            tile.TileFrameX = 0;
            tile.TileFrameY = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Commands.ChatCommands.RemoveAll(
                    c => c.CommandDelegate == HellevatorCommand);
            }

            base.Dispose(disposing);
        }
    }
}
