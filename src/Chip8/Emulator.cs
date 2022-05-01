namespace Chip8
{
    public class Emulator
    {
        const long RefreshRate = (long) ((1 / 60.0) * TimeSpan.TicksPerSecond);
        public static ushort InterpreterEndAddress = 0x200;
        public static byte FontStartAddress = 0x50;
        
        private Display _display;
        private Memory _ram;
        private Memory _vram;
        private Cpu _cpu;
        private Debugger? _debugger;
        private Keypad _keypad;

        public Emulator(bool isDebuggerEnabled = false)
        {
            if (isDebuggerEnabled)
                _debugger = new Debugger();

            _ram = new Memory(4096);
            _vram = new Memory(64*32);
            _display = new Display(_vram.AsReadOnly());
            _keypad = new Keypad();
            _cpu = new Cpu(_ram, _vram, _keypad);
        }

        public void Initialize()
        {
            _vram.Clear();
            _ram.Clear();
            _cpu.Reset();
            _display.Clear();
            LoadFonts();
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

            var storeAddress = InterpreterEndAddress;
            foreach(var b in File.ReadAllBytes(path))
            {
                _ram.Write(storeAddress, b);
                _ram[storeAddress] = b;
                storeAddress++;
            }
            
            _display.SetWindowTitle(Path.GetFileName(path));
        }

        public void Process()
        {
            long prevTime = DateTime.Now.Ticks, delta = 0, accumulator = 0;
            
            while (_display.IsWindowOpen())
            {
                delta = DateTime.Now.Ticks - prevTime;
                prevTime = DateTime.Now.Ticks;
                accumulator += delta;

                // 60Hz update
                while (accumulator > RefreshRate)
                {
                    _cpu.Cycle();
                    accumulator -= RefreshRate;
                }

                _display.Update();

                if (_debugger != null)
                {
                    _debugger.Update(_cpu.Dump(), _ram.Dump(InterpreterEndAddress, _ram.Size, 0x10), _vram.Dump(), _keypad.ToString());

                    if (!_debugger.IsWindowOpen())
                        _display.Close();
                }
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
                _ram[i] = _fontset[i];
            }
        }
    }
}
