using Raylib_cs;

namespace Chip8
{
    public class Emulator
    {
        const long refreshRate = (long) ((1 / 60.0) * TimeSpan.TicksPerSecond);

        private Display _display = new Display();
        private Memory _ram = new Memory();
        private Cpu _cpu = new Cpu();

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
            //TODO: Load ROM file
        }

        public void Cycle()
        {
            long delta = 0, prevTime = DateTime.Now.Ticks, accumulator = 0;
            
            while (!Raylib.WindowShouldClose())
            {
                delta = DateTime.Now.Ticks - prevTime;
                prevTime = DateTime.Now.Ticks;
                accumulator += delta;

                // 60Hz update
                while (accumulator > refreshRate)
                {
                    //TODO: Interpret Opcodes
                    accumulator -= refreshRate;
                }

                _display.Update();
            }

            _display.Close();
        }
    }
}
