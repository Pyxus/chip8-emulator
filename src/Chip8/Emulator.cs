namespace Chip8
{
    public class Emulator
    {
        const long refreshRate = (long) ((1 / 60.0) * TimeSpan.TicksPerSecond);

        private Display _display = new Display();
        private Memory _ram = new Memory();
        private Cpu _cpu = new Cpu();

        public void AdjustDisplay(int windowScale, string windowTitle)
        {
            _display.WindowScale = windowScale;
            _display.WindowTitle = windowTitle;
        }

        public void Initialize()
        {
            /*
                -Implement initialization procedure -
                
                [] Program Counter starts at 0x200
                [] current opcode resets to 0
                [] reset I register
                [] reset stack pointer
                [] clear dispaly
                [] clear registers V0-VF
                [] clear memory
                [] load fontset into memory
                [] reset timers
            */
        }

        public void Load(string path)
        {
            //TODO: Load ROM file into memory
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
                while (accumulator > refreshRate)
                {
                    _cpu.Cycle(_ram);
                    accumulator -= refreshRate;
                }

                _display.Update();
            }

            _display.Close();
        }
    }
}
