using RoguelikeDarling.Core;
using Microsoft.Xna.Framework;

internal class Program
{
    /// <summary>
    /// The main entry point for the application. 
    /// This creates an instance of your game and calls it's Run() method 
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    private static void Main(string[] args)
    {
        LaunchOptions options = LaunchOptions.Parse(args);
        using var game = new RoguelikeDarlingGame(options.CollisionDebugEnabled, options.CollisionDebugColor);
        game.Run();
    }

    private readonly struct LaunchOptions
    {
        private LaunchOptions(bool collisionDebugEnabled, Color collisionDebugColor)
        {
            CollisionDebugEnabled = collisionDebugEnabled;
            CollisionDebugColor = collisionDebugColor;
        }

        public bool CollisionDebugEnabled { get; }

        public Color CollisionDebugColor { get; }

        public static LaunchOptions Parse(string[] args)
        {
            const string debugCollisionArg = "--debug-collision";
            bool enabled = false;
            Color color = new Color(220, 30, 30);

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (!arg.StartsWith(debugCollisionArg, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                enabled = true;

                string rawColor = null;
                int equalsIndex = arg.IndexOf('=');
                if (equalsIndex > -1 && equalsIndex + 1 < arg.Length)
                {
                    rawColor = arg.Substring(equalsIndex + 1);
                }
                else if (i + 1 < args.Length && !args[i + 1].StartsWith("--", System.StringComparison.Ordinal))
                {
                    rawColor = args[i + 1];
                    i++;
                }

                if (TryParseColor(rawColor, out Color parsedColor))
                {
                    color = parsedColor;
                }
            }

            return new LaunchOptions(enabled, color);
        }

        private static bool TryParseColor(string rawColor, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(rawColor))
            {
                return false;
            }

            string[] parts = rawColor.Split(',');
            if (parts.Length != 3)
            {
                return false;
            }

            if (!byte.TryParse(parts[0].Trim(), out byte r)
                || !byte.TryParse(parts[1].Trim(), out byte g)
                || !byte.TryParse(parts[2].Trim(), out byte b))
            {
                return false;
            }

            color = new Color(r, g, b);
            return true;
        }
    }
}