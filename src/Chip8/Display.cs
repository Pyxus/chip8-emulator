using Raylib_cs;

namespace Chip8
{
    public class Display
    {
        private const int BaseX = 64;
        private const int BaseY = 32;

        private bool[,] _display = new bool[BaseX, BaseY];
        private (int X, int Y) _windowSize;
        private (int X, int Y) _pixelSize;

        public Display(int scale = 15, string title = "Chip8")
        {
            _windowSize = (BaseX * scale, BaseY * scale);
            _pixelSize = (_windowSize.X / BaseX, _windowSize.Y / BaseY);

            Raylib.InitWindow(_windowSize.X, _windowSize.Y, title);
        }

        public void Update()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);

            for (int x = 0; x < BaseX; x++)
            {
                for (int y = 0; y < BaseY; y++)
                {
                    if (_display[x, y])
                        Raylib.DrawRectangle(x * _pixelSize.X, y * _pixelSize.Y, _pixelSize.X, _pixelSize.Y, Color.WHITE);
                }
            }

            Raylib.EndDrawing();
        }

        public void Close()
        {
            if (Raylib.IsWindowReady())
                Raylib.CloseWindow();
        }
    }
}