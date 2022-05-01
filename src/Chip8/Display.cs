using System.Collections.ObjectModel;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace Chip8
{
    public class Display
    {
        private const int BaseWidth = 64;
        private const int BaseHeight = 32;
        private const float AspectRatio =  BaseWidth / (float) BaseHeight;
        private RenderWindow _window;
        private ReadOnlyCollection<byte> _frameBuffer;

        public Display(ReadOnlyCollection<byte> frameBuffer)
        {
            _frameBuffer = frameBuffer;
            _window = new RenderWindow(new VideoMode(BaseWidth * 15, BaseHeight * 15), "Chip8 - Display");
            _window.Closed += OnWindowClose;
            _window.Resized += OnWindowResized;
        }
        
        public void Clear()
        {
            _window.Clear();
        }

        public void Close()
        {
            _window.Close();
        }

        public bool IsWindowOpen()
        {
            return _window.IsOpen;
        }

        public void Update()
        {
            _window.DispatchEvents();
            _window.Clear();
            
            for (int i = 0, yPos = -1; i < _frameBuffer.Count; i++)
            {
                var xPos = i % BaseWidth;
                if (i % BaseWidth == 0)
                    yPos++;

                if (_frameBuffer[i] != 0)
                {
                    var size = new Vector2f(_window.Size.X / BaseWidth, (_window.Size.X / BaseHeight));
                    var pixel = new RectangleShape(){
                        Size = size,
                        Position = new Vector2f(xPos * size.X, yPos * size.Y),
                        FillColor = Color.White
                    };

                    _window.Draw(pixel);
                }
            }

            _window.Display();
        }

        public void SetWindowTitle(string title)
        {
            _window.SetTitle(title);
        }

        private void OnWindowClose(object? sender, EventArgs args)
        {
            _window.Close();
        }

        private void OnWindowResized(object? sender, EventArgs args)
        {
            var win = (SizeEventArgs) args;
            var view = _window.DefaultView;
            var windowRatio = win.Width / (float) win.Height;
            var viewRatio = view.Size.X / (float) view.Size.Y;
            var sizeX = 1f;
            var sizeY = 1f;
            var posX = 0f;
            var posY = 0f;

            if (windowRatio >= viewRatio) 
            {
                sizeX = viewRatio / windowRatio;
                posX = (1 - sizeX) / 2f;
            }
            else 
            {
                sizeY = windowRatio / viewRatio;
                posY = (1 - sizeY) / 2f;
            }

            view.Viewport = new FloatRect(posX, posY, sizeX, sizeY);
            _window.SetView(view);

        }

        private Color GenRanColor()
        {
            var random = new Random();
            return new Color((byte) random.Next(255), (byte) random.Next(255), (byte) random.Next(255));
        }
    }
}