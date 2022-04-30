using System.Text;
namespace Chip8
{
    public class Memory
    {
        public readonly int Size;

        private readonly byte[] _memory;
        
        public Memory(int size)
        {
            Size = size;
            _memory = new byte[size];
        }

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
            PrintHex(0, Size);
        }

        public void PrintHex(int from, int to)
        {
            var hex = new StringBuilder();
            for (var i = from; i < to; i++)
            {
                hex.AppendFormat("{0:x2} ", _memory[i]);

                if ((i + 1) % 32 == 0)
                    hex.Append("\n");
            }

            Console.WriteLine(hex);
        }

        public override string ToString()
        {
            var hex = new StringBuilder();
            for (var i = 0; i < Size; i++)
            {
                hex.AppendFormat("{0:x2} ", _memory[i]);

                if ((i + 1) % 32 == 0)
                    hex.Append("\n");
            }
            return hex.ToString();
        }
    }
}