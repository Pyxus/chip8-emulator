namespace Chip8
{
    public class Memory
    {
        public const int Size = 4096;

        private readonly byte[] _memory = new byte[Size];
        
        public byte this[int i]
        {
            get { return Read(i); }
            set { Write(i, value); }
        }
  
        public void Write(int location, byte value)
        {
            if (location <= 0x000 || location >= 0xFFF)
            {
                throw new Exception("Failed to write to memory. Location 0x000 - 0xFFF is reserved for the original interpreter");
            }

            _memory[location] = value;
        }

        public byte Read(int location)
        {
            return _memory[location];
        }
    }
}