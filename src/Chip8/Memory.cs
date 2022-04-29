using System.Text;
namespace Chip8
{
    public class Memory
    {
        public const int Size = 4096;
        public const int InterpreterEndAddress = 0x200;
        public const int FontStartAddress = 0x50;

        private readonly byte[] _memory = new byte[Size];
        
        public byte this[int i]
        {
            get { return Read(i); }
            set { Write(i, value); }
        }
  
        public void Clear()
        {
            for (int i = 0; i < Size; i++)
            {
                _memory[i] = 0;
            }
        }

        public void Write(int location, byte value)
        {
            _memory[location] = value;
        }

        public byte Read(int location)
        {
            return _memory[location];
        }

        public void PrintHex()
        {
            var hex = new StringBuilder();

            hex.Append("\n\nINTERPRETER RESERVE \n");
            for (int i = 0; i < InterpreterEndAddress; i++)
            {
                hex.AppendFormat("{0:x2} ", _memory[i]);

                if ((i + 1) % 32 == 0)
                    hex.Append("\n");
            }

            hex.Append("\n\nPROGRAM DATA \n");
            for (int i = InterpreterEndAddress; i < Size; i++)
            {
                hex.AppendFormat("{0:x2} ", _memory[i]);

                if ((i + 1) % 32 == 0)
                    hex.Append("\n");
            }

            Console.WriteLine(hex);
        }
    }
}