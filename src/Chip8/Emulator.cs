namespace Chip8
{
    public class Emulator
    {
        const long RefreshRate = (long) ((1 / 60.0) * TimeSpan.TicksPerSecond);
        
        private Display _display;
        private Memory _ram;
        private Cpu _cpu;

        public Emulator()
        {
            _display = new Display();
            _ram = new Memory();
            _cpu = new Cpu(_ram);
        }

        public void AdjustDisplay(int windowScale, string windowTitle)
        {
            _display.WindowScale = windowScale;
            _display.WindowTitle = windowTitle;
        }

        public void Initialize()
        {
            /*
                -Implement initialization procedure -
                
                [X] Program Counter starts at 0x200
                [] current opcode resets to 0
                [] reset I register
                [] reset stack pointer
                [X] clear dispaly
                [] clear registers V0-VF
                [X] clear memory
                [X] load fontset into memory
                [] reset timers
            */
            _ram.Clear();
            _cpu.Reset();
            _display.Clear();
            LoadFonts();
        }

        public void Load(string path)
        {
            var storeAddress = 0x200;
            foreach(var b in File.ReadAllBytes(path))
            {
                _ram.Write(storeAddress, b);
                _ram[storeAddress] = b;
                storeAddress++;
            }

            _ram.PrintHex();
        }

        public void Process()
        {
            long prevTime = DateTime.Now.Ticks, delta = 0, accumulator = 0;
            
            while (!_display.IsWindowClosed())
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
            }

            _display.Close();
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

            for(var i = Memory.FontStartAddress; i < _fontset.Length; i++)
            {
                _ram[i] = _fontset[i];
            }
        }
    }
}
