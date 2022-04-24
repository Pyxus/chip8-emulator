// TODO: Evaluate if CPU should have any behavior or if it should be kept as a data class
// TODO: If kept as a data class make registers public properties
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
    }
}