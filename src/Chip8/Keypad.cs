using System.Text;
using System.Collections.Generic;
using SFML.Window;

namespace Chip8
{
    public class Keypad
    {
        private bool[] _keypad = new bool[16];
        private Dictionary<byte, Keyboard.Key> _keyBinds = new Dictionary<byte, Keyboard.Key>()
        {
            {0x1, Keyboard.Key.Num1},{0x2, Keyboard.Key.Num2}, {0x3, Keyboard.Key.Num3}, {0xC, Keyboard.Key.Num4},
            {0x4, Keyboard.Key.Q}, {0x5, Keyboard.Key.W}, {0x6, Keyboard.Key.E}, {0xD, Keyboard.Key.R},
            {0x7, Keyboard.Key.A}, {0x8, Keyboard.Key.S}, {0x9, Keyboard.Key.D}, {0xE, Keyboard.Key.F},
            {0xA, Keyboard.Key.Z}, {0x0, Keyboard.Key.X}, {0xB, Keyboard.Key.C}, {0xF, Keyboard.Key.V},
        };

        public void Poll()
        {
            for (byte keypadId = 0; keypadId < _keypad.Length; keypadId++)
            {
                _keypad[keypadId] = Keyboard.IsKeyPressed(_keyBinds[keypadId]);
            }
        }

        public bool IsPressed(byte keypadId)
        {
            return _keypad[keypadId];
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var rowEnd = 0xC;
            
            for (byte keypadId = 1; keypadId < 10; keypadId++)
            {
                sb.Append($"[{keypadId:X1}:{Convert.ToByte(_keypad[keypadId])}]\t");

                if (keypadId  % 3 == 0)
                {
                    sb.Append($"[{rowEnd:X1}:{Convert.ToByte(_keypad[rowEnd])}]\t");
                    sb.Append("\n");
                    rowEnd++;
                }
            }

            sb.Append($"[{0xA:X1}:{Convert.ToByte(_keypad[0xA])}]\t");
            sb.Append($"[{0x0:X1}:{Convert.ToByte(_keypad[0x0])}]\t");
            sb.Append($"[{0xB:X1}:{Convert.ToByte(_keypad[0xB])}]\t");
            sb.Append($"[{0xF:X1}:{Convert.ToByte(_keypad[0xF])}]\t");
            return sb.ToString();
        }
    }
}