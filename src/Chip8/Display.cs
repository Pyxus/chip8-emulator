using Raylib_cs;

namespace Chip8
{
    public class Display
    {
        public int WindowScale
        {
            set 
            {
                _scale = Math.Max(1, value);
                _windowSize = (BaseX * _scale, BaseY * _scale);
                _pixelSize = (_windowSize.X / BaseX, _windowSize.Y / BaseY);
                Raylib.SetWindowSize(_windowSize.X, _windowSize.Y);
            }
            get => _scale;
        }

        public string WindowTitle
        {
            set
            {
                _title = value;
                Raylib.SetWindowTitle(_title);
            }
            get => _title;
        }

        private const int BaseX = 64;
        private const int BaseY = 32;

        private bool[,] _display = new bool[BaseX, BaseY];
        private int _scale = 15;
        private string _title = "Chip8";
        private (int X, int Y) _windowSize;
        private (int X, int Y) _pixelSize;

        public Display(int scale = 15, string title = "Chip8")
        {
            WindowScale = scale;
            WindowTitle = title;

            Raylib.InitWindow(_windowSize.X, _windowSize.Y, title);
            _display[10, 10] = true;
            _display[15, 10] = true;
            _display[9, 14] = true;
            _display[16, 14] = true;
            for (int i = 0; i <= 5; i++)
            {
                _display[10 + i, 15] = true;
            }
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

        public void Clear()
        {
            Raylib.ClearBackground(Color.BLACK);
        }

        public void Close()
        {
            if (Raylib.IsWindowReady())
                Raylib.CloseWindow();
        }
    

        public bool IsWindowClosed()
        {
            return Raylib.WindowShouldClose();
        }
    }
}