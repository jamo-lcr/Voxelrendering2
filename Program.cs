using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics.CodeAnalysis;
namespace Voxelrendering2
{
    internal static class Program
    {
        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(2000, 2000),
                Title = "Simple Voxel"
            };

            var gameWindowSettings = GameWindowSettings.Default;

            // Create and run the Renderer
            using (var renderer = new Renderer(gameWindowSettings, nativeWindowSettings))
            {
                renderer.UpdateFrequency = 120.0;
                renderer.Run();
            }
        }
    }
}