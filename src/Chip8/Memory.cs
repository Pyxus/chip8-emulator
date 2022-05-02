using System.Collections.ObjectModel;
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

        public ReadOnlyCollection<byte> AsReadOnly()
        {
            return Array.AsReadOnly<byte>(_memory);
        }

        public string Dump(int from, int to, int bytesPerRow = 0x10)
        {
            var byteDigitCount = $"{Size-1:X}".Length;
            var hex = new StringBuilder();

            for (var i = 0; i < byteDigitCount + 4; i++)
            {
                hex.Append(" ");
            }

            for (var i = 0; i < bytesPerRow; i++)
            {
                hex.AppendFormat("{0:X2} ", i);
            }

            hex.Append("\n");

            var hexRowCount = from;
            for (var i = from; i < to; i++)
            {
                if (i % bytesPerRow == 0)
                {   
                    hex.Append("\n");
                    
                    var hexFormat = "{" + $"{0}:X{byteDigitCount}" + "}";
                    
                    hex.AppendFormat($"│{hexFormat}│  ", hexRowCount);
                    hexRowCount += bytesPerRow;
                }

                hex.AppendFormat("{0:X2} ", _memory[i]);
            }
            return hex.ToString();
        }

        public string Dump(int bytesPerRow = 0x10)
        {
            return Dump(0, Size);
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
    }
}