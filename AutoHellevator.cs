using System;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace AutoHellevator
{
    [ApiVersion(2, 1)]
    public sealed class AutoHellevator : TerrariaPlugin
    {
        public override string Name => "AutoHellevator";

        public override string Author => "ChatGPT";

        public override string Description =>
            "Instantly creates a 3x3 hellevator with Stone Slab sides and Stone Slab Wall background.";

        public override Version Version => new Version(1, 0, 0, 0);

        public AutoHellevator(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(
                new Command(
                    "hellelevator.use",
                    HellevatorCommand,
                    "hellelevator"
                )
                {
                    HelpText = "Creates an instant 3x3 hellevator straight down."
                }
            );
        }

        private static void HellevatorCommand(CommandArgs args)
        {
            if (args.Player == null || !args.Player.Active)
            {
                return;
            }

            int centerX = (int)(args.Player.X / 16f);
            int firstY = (int)(args.Player.Y / 16f) + 1;

            // Keep the 5-tile-wide structure safely inside the world.
            if (centerX - 2 < 1 || centerX + 2 >= Main.maxTilesX - 1)
            {
                args.Player.SendErrorMessage(
                    "You are too close to the world edge."
                );
                return;
            }

            int lastY = Main.maxTilesY - 2;

            if (firstY >= lastY)
            {
                args.Player.SendErrorMessage(
                    "There is not enough room below you."
                );
                return;
            }

            for (int y = firstY; y <= lastY; y++)
            {
                // 3x3 center shaft.
                for (int x = centerX - 1; x <= centerX + 1; x++)
                {
                    // Remove whatever block is there.
                    WorldGen.KillTile(
                        x,
                        y,
                        false,
                        false,
                        true
                    );

                    // Remove the existing background wall.
                    WorldGen.KillWall(
                        x,
                        y,
                        false
                    );

                    // Put Stone Slab Wall behind the empty shaft.
                    WorldGen.PlaceWall(
                        x,
                        y,
                        WallID.StoneSlab,
                        true
                    );
                }

                // Left Stone Slab block.
                WorldGen.KillTile(
                    centerX - 2,
                    y,
                    false,
                    false,
                    true
                );

                WorldGen.PlaceTile(
                    centerX - 2,
                    y,
                    TileID.StoneSlab,
                    true,
                    true
                );

                // Right Stone Slab block.
                WorldGen.KillTile(
                    centerX + 2,
                    y,
                    false,
                    false,
                    true
                );

                WorldGen.PlaceTile(
                    centerX + 2,
                    y,
                    TileID.StoneSlab,
                    true,
                    true
                );

                // Re-frame the completed row.
                WorldGen.SquareTileFrame(
                    centerX - 2,
                    y,
                    true
                );

                WorldGen.SquareTileFrame(
                    centerX - 1,
                    y,
                    true
                );

                WorldGen.SquareTileFrame(
                    centerX,
                    y,
                    true
                );

                WorldGen.SquareTileFrame(
                    centerX + 1,
                    y,
                    true
                );

                WorldGen.SquareTileFrame(
                    centerX + 2,
                    y,
                    true
                );
            }

            // Sync the changed area to connected players.
            const int syncHeight = 60;

            for (int y = firstY; y <= lastY; y += syncHeight)
            {
                int height = Math.Min(
                    syncHeight,
                    lastY - y + 1
                );

                NetMessage.SendTileSquare(
                    -1,
                    centerX,
                    y + height / 2,
                    5,
                    height
                );
            }

            args.Player.SendSuccessMessage(
                "AutoHellevator complete! 3x3 shaft with Stone Slab sides and Stone Slab Wall background."
            );
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Commands.ChatCommands.RemoveAll(
                    command =>
                        command.CommandDelegate == HellevatorCommand
                );
            }

            base.Dispose(disposing);
        }
    }
}
