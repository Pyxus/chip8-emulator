namespace Chip8
{
    public class Cpu
    {
        private byte[] _vRegisters = new byte[16];
        private short _iRegister;
        private byte _timerRegister;
        private byte _delayRegister;
        private short _programCounter;
        private byte _stackPointer;
        private short[] _stack = new short[16];

        public void Cycle(Memory ram)
        {
            //TODO: Fetch
            //TODO: Decode
            //TODO: Execute
        }
    }
}