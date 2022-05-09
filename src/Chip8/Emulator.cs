namespace Chip8
{
    using SFML.Graphics;
    using SFML.Window;
    using SFML.System;

    public class Emulator
    {
        public const long RefreshRate = (long) ((1 / 240.0) * TimeSpan.TicksPerSecond);
        public const ushort InterpreterEndAddress = 0x200;
        public const byte FontStartAddress = 0x000;
        public const int BaseWidth = 64;
        public const int BaseHeight = 32;
        
        protected RenderWindow App;
        protected Memory Ram;
        protected Memory Vram;
        protected Cpu Cpu;
        protected Keypad Keypad;

        private bool _isProgramLoaded;
        private byte[] _programData = new byte[0];
        private string _programName = "";

        public Emulator()
        {
            App = new RenderWindow(new VideoMode(BaseWidth * 8, BaseHeight * 8), "Chip8 - Display", Styles.Titlebar | Styles.Close);
            App.Resized += OnWindowResized;
            App.KeyPressed += OnWindowKeyPressed;
            App.Closed += OnWindowClosed;
            Ram = new Memory(4096);
            Vram = new Memory(BaseWidth * BaseHeight);
            Keypad = new Keypad();
            Cpu = new Cpu(Ram, Vram, Keypad);
        }

        public void Initialize()
        {
            Vram.Clear();
            Ram.Clear();
            Cpu.Reset();
            App.Clear();
            LoadFonts();
            _isProgramLoaded = false;

            /*
            var random = new Random();
            for(var i = 0; i < Vram.Size; i++)
            {
                Vram[i] = (byte) random.Next(0xFF);
            }
            */
        }

        public void Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("ERROR: Failed to read file, path may be invalid");
            }

            if (Path.GetExtension(path) != ".ch8")
            {
                Console.WriteLine("WARNING: Chip8 files typically end with the exntesion '.ch8'. This file may not contain chip8 code.");
            }

            Load(File.ReadAllBytes(path), Path.GetFileName(path));
        }

        public void Load(byte[] bytes, string programName = "Chip8 Program")
        {
            var storeAddress = InterpreterEndAddress;
            foreach(var b in bytes)
            {
                Ram.Write(storeAddress, b);
                Ram[storeAddress] = b;
                storeAddress++;
            }
            
            App.SetTitle(programName);
            _isProgramLoaded = true;
            _programData = bytes;
            _programName = programName;
        }

        public virtual void Process()
        {
            if (!_isProgramLoaded)
                throw new Exception("Terminating emulator. Program was never loaded into memory.");

            long prevTime = DateTime.Now.Ticks, delta = 0, accumulator = 0;
            
            while (App.IsOpen)
            {
                delta = DateTime.Now.Ticks - prevTime;
                prevTime = DateTime.Now.Ticks;
                accumulator += delta;

                // 60Hz update
                while (accumulator > RefreshRate)
                {
                    Cpu.Cycle();
                    accumulator -= RefreshRate;
                }
                Render();
            }
        }
    

        private void LoadFonts()
        {
            var _fontset = new byte[]
            {
                0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
                0x20, 0x60, 0x20, 0x20, 0x70, // 1
                0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
                0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
                0x90, 0x90, 0xF0, 0x10, 0x10, // 4
                0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
                0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
                0xF0, 0x10, 0x20, 0x40, 0x40, // 7
                0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
                0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
                0xF0, 0x90, 0xF0, 0x90, 0x90, // A
                0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
                0xF0, 0x80, 0x80, 0x80, 0xF0, // C
                0xE0, 0x90, 0x90, 0x90, 0xE0, // D
                0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
                0xF0, 0x80, 0xF0, 0x80, 0x80, // F
            };

            for(var i = FontStartAddress; i < _fontset.Length; i++)
            {
                Ram[i] = _fontset[i];
            }
        }

        protected void Render()
        {
            App.DispatchEvents();
            App.Clear();
            
            for (var yDraw = 0; yDraw < Emulator.BaseHeight; yDraw++)
            {
                for (var xDraw = 0; xDraw < Emulator.BaseWidth; xDraw++)
                {
                    if (Vram[(yDraw * Emulator.BaseWidth) + xDraw] != 0)
                    {
                        var size = new Vector2f(App.Size.X / BaseWidth, (App.Size.Y / BaseHeight));
                        var pixel = new RectangleShape(){
                            Size = size,
                            Position = new Vector2f(xDraw * size.X, yDraw * size.Y),
                            FillColor = Color.White
                        };

                        App.Draw(pixel);
                    }
                }
            }

            App.Display();
        }

        private Color GenRanColor()
        {
            var random = new Random();
            return new Color((byte) random.Next(255), (byte) random.Next(255), (byte) random.Next(255));
        }

        private void OnWindowKeyPressed(object? sender, KeyEventArgs args)
        {
            if (args.Code == Keyboard.Key.Escape)
            {
                App.Close();
            }
            if (args.Code == Keyboard.Key.F5)
            {
                Initialize();
                Load(_programData, _programName);
            }
        }

        private void OnWindowResized(object? sender, SizeEventArgs args)
        {
            var win = (SizeEventArgs) args;
            var view = App.DefaultView;
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
            App.SetView(view);
        }

        private void OnWindowClosed(object? sender, EventArgs args)
        {
            App.Close();
        }
    }
}
