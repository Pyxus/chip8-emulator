using System.Text;
using System.Reflection.Emit;
using System.Collections.Generic;
namespace Chip8
{
    public class Cpu
    {
        private Memory _ram; // Holds our memory represented by an array of 4096 bytes.
        private Memory _vram; // Holds our video memory which will be used to draw to the display
        private byte[] _vRegisters = new byte[16]; // General purpose register. Think of it as a variables that store data current being used by the CPU
        private ushort _iRegister; // Specifications says its used to 'store memory addresses' but im not sure what programs would use it for...
        private byte _soundTimer; // Basically just a variable. I don't know what programs would actually use it for...
                                     // But we don't need to know that in order to implement it. 
        private byte _delayTimer; // SImilar case to the timer
        private ushort _programCounter; // Tells where in memory the next instruction should be read
        private byte _stackPointer; // Stores where in the call stack the program is.
                                    // Basically if multiple functions are called this combined with the stack helps keep track of all their return points.
        private ushort[] _stack = new ushort[16]; // The stack stores the address the interpreter should return to when a subroutine is finished.
        private ushort _opcode; // Stores the current instruction
        private Keypad _keypad;
        private Random _random = new Random();
        private Dictionary<byte, Action> _opHandlers = new Dictionary<byte, Action>(); // Basically a list of OpHanlder functions that can be retreived using the opID

        public Cpu(Memory ram, Memory vram, Keypad keypad)
        {
            _ram = ram;
            _vram = vram;
            _keypad = keypad;

            _opHandlers.Add(0x0, OP_00E);
            _opHandlers.Add(0x1, OP_1nnn);
            _opHandlers.Add(0x2, OP_2nnn);
            _opHandlers.Add(0x3, OP_3xkk);
            _opHandlers.Add(0x4, OP_4xkk);
            _opHandlers.Add(0x5, OP_5xy0);
            _opHandlers.Add(0x6, OP_6xkk);
            _opHandlers.Add(0x7, OP_7xkk);
            _opHandlers.Add(0x8, OP_8xy);
            _opHandlers.Add(0x9, OP_9xy0);
            _opHandlers.Add(0xA, OP_Annn);
            _opHandlers.Add(0xB, OP_Bnnn);
            _opHandlers.Add(0xC, OP_Cxkk);
            _opHandlers.Add(0xD, OP_Dxyn);
            _opHandlers.Add(0xE, OP_Ex);
            _opHandlers.Add(0xF, OP_Fx);
        }

        public void Reset()
        {
            _soundTimer = 0;
            _delayTimer = 0;
            _programCounter = Emulator.InterpreterEndAddress; // 0x00 to 0x1FF are reserved so the program counter starts at 0x200
        }

        public void Cycle()
        {
            Fetch();
            DecodeAndExecute();
            _keypad.Poll();

            if (_delayTimer > 0)
                _delayTimer--;
            
            if (_soundTimer > 0)
                _soundTimer--;
        }

        public string Dump()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Op: {0:X4}\n\n", _opcode);
            sb.AppendFormat("PC: {0:X2}\n", _programCounter);
            sb.AppendFormat("I: {0:X2}\n", _iRegister);

            for (var i = 0; i < _vRegisters.Length; i++)
            {
                sb.AppendFormat("V{0:X1}: {0:X2}\n", i, _vRegisters[i]);
            }

            sb.AppendFormat("\nSP: {0:X2}\n", _stackPointer);

            for (var i = 0; i < _stack.Length; i++)
            {
                sb.AppendFormat("Stack[{0:X1}]: {0:x2}\n", i, _stack[i]);
            }

            return sb.ToString();
        }

        private void Fetch()
        {
            /*
                Each location in memory only stores a byte but as per the chip8 specifications all instructions
                are 2 bytes long stored adjacent in memory. So in order to fetch an opcode we read the address 
                at the program counter and the address next to that one from memory.]

                For convinience these 2 bytes are stored as 16-bit value which is done by by left bit shifting '<<'
                the first byte and binary oring '|' it with the second byte. If you think of the byte as an array of 8 bits (1s and 0s)
                then you can imagine shifting by 8 to the left as moving the elemets of the 'array' left by 8.
                [00001010] << 8 -> [00001010XXXXXXXX] note: The X's would actually be 0 but I did it this way to make the shift easier to see

                The binary or can thought of as a logical or applied to each bit. So 1 | 1 = 1, 1 | 0 = 1, 0 | 0 = 0.
                But since the values at the right end will all be 0 after the shift this basically results in the 2nd byte
                being added on to the end.
                
                so if byte 1 = [00001010] and byte 2 = [10010001] the the operation below does...
                [00001010] << 8 -> [0000101000000000]

                [0000101000000000]
                |       [10010001]
                --------------------
                [0000101010010001]
                End result you have a 16-bit (or 2 byte) value containing the opcode
            */
            _opcode = (ushort)((_ram[_programCounter] << 8) | _ram[_programCounter + 1]);
            _programCounter += 2; // Incremeted twice since we read 2 bytes at a time above
        }

        private void DecodeAndExecute()
        {
            /*
                For our decoding we use the first 4 bits (nibble) as an ID to call the appropriate procedure. 
                In order to get that ID we & the op code against the 0xF000 mask which well set
                all but the first nibble to 0. If you think about it 0 & anything will be 0 while F,
                which is 1111 in binary, & anything will always be the original value.

                [1101000101111010] -> Random number
               &[1111000000000000] -> 0xF000
               ----------------------
                [1101000000000000]

                However aftering &ing the value is still 16 bit and we only need the first 4 so we bit shift to the right by 12.
                [1101000000000000] >> 12 -> [1101]
            */
            var opID = (byte)((_opcode & 0xF000u) >> 12);
            var opHandler = OP_NULL;
            _opHandlers.TryGetValue(opID, out opHandler);
            opHandler?.Invoke(); // This is a c# thing, basically it'll call the function stored in opHanlder
        }

        private void OP_NULL()
        {
            Console.WriteLine("Invalid Instruction read");
        }

        private void OP_00E()
        {
            /*
                Where using the & similiar to how we used it in the execute method.
                However we don't need to bit shift since the left most 0 bites are
                ignored and of course since all but the last nibble is being set to 0
                we end up with a value that's just that nibble.
            */
            if ((_opcode & 0x000Fu) == 0x0u)
            {
                OP_OOEO();
            }
            else if ((_opcode & 0x000Fu) == 0xEu)
            {
                OP_OOEE();
            }
            else
            {
                OP_NULL();
            }
        }

        // UNTESTED
        private void OP_OOEO()
        {
            _vram.Clear();
        }

        // UNTESTED
        private void OP_OOEE()
        {
            _programCounter = _stack[--_stackPointer];
        }

        // UNTESTED
        private void OP_1nnn()
        {
            var address = (ushort) (_opcode & 0x0FFFu);
            _programCounter = address;
        }


        private void OP_2nnn()
        {
            var address = (ushort) (_opcode & 0x0FFFu);
            _stack[++_stackPointer] = _programCounter;
            _programCounter = address;
        }

        private void OP_3xkk()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var kk = (byte)(_opcode & 0x00FF);

            if (_vRegisters[Vx] == kk)
            {
                _programCounter += 2;
            }
        }

        private void OP_4xkk()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var kk = (byte)(_opcode & 0x00FF);

            if (_vRegisters[Vx] != kk)
            {
                _programCounter += 2;
            }
        }

        private void OP_5xy0()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var Vy = (byte)((_opcode & 0x0F00) << 8);

            if (_vRegisters[Vx] == _vRegisters[Vy])
            {
                _programCounter += 2;
            }
        }

        private void OP_6xkk()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var kk = (byte)(_opcode & 0x00FF);
            
            _vRegisters[Vx] = kk;
        }

        private void OP_7xkk()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var kk = (byte)(_opcode & 0x00FF);

            _vRegisters[Vx] += kk;

        }

        private void OP_8xy()
        {
            switch (_opcode & 0x000f)
            {
                case 0x0:
                    OP_8xy0();
                    break;
                case 0x1:
                    OP_8xy1();
                    break;
                case 0x2:
                    OP_8xy2();
                    break;
                case 0x3:
                    OP_8xy1();
                    break;
                case 0x4:
                    OP_8xy1();
                    break;
                case 0x5:
                    OP_8xy1();
                    break;
                case 0x6:
                    OP_8xy1();
                    break;
                case 0x7:
                    OP_8xy1();
                    break;
                case 0xE:
                    OP_8xyE();
                    break;
                default:
                    OP_NULL();
                    break;
            }


        }

        private void OP_8xy0()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var Vy = (byte)((_opcode & 0x00F0) >> 4);

            _vRegisters[Vx] = _vRegisters[Vy];
        }

        private void OP_8xy1()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var Vy = (byte)((_opcode & 0x00F0) >> 4);

            _vRegisters[Vx] |= _vRegisters[Vy];
        }

        private void OP_8xy2()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var Vy = (byte)((_opcode & 0x00F0) >> 4);
            _vRegisters[Vx] &= _vRegisters[Vy];

        }

        private void OP_8xy3()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var Vy = (byte)((_opcode & 0x00F0) >> 4);
            _vRegisters[Vx] ^= _vRegisters[Vy];

        }
        private void OP_8xy4()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var Vy = (byte)((_opcode & 0x00F0) >> 4);
            var sum = _vRegisters[Vx] + _vRegisters[Vy];

            _vRegisters[0xF] =  (byte) (sum > 0xFF ? 1 : 0);
            _vRegisters[Vx] = (byte) (sum & 0xFF);
        }
        
        private void OP_8xy5()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var Vy = (byte)((_opcode & 0x00F0) >> 4);

            _vRegisters[0xF] = (byte) (_vRegisters[Vx] > _vRegisters[Vy] ? 1 : 0);
            _vRegisters[Vx] -= _vRegisters[Vy];
        }

        private void OP_8xy6()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            
            _vRegisters[0xF] = (byte) (_vRegisters[Vx] & 0x1);
            _vRegisters[Vx] >>= 1;   
        }

        private void OP_8xy7()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var Vy = (byte)((_opcode & 0x00F0) >> 4);

            _vRegisters[0xF] = (byte) (_vRegisters[Vy] > _vRegisters[Vx] ? 1 : 0);
            _vRegisters[Vx] = (byte) (_vRegisters[Vy] - _vRegisters[Vx]);
        }

        private void OP_8xyE()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            
            _vRegisters[0xF] = (byte) ((_vRegisters[Vx] & 0x80) >> 7);
            _vRegisters[Vx] <<= 1;   
        }

        private void OP_9xy0()
        {
            var Vx = (byte)((_opcode & 0x0F00) >> 8);
            var Vy = (byte)((_opcode & 0x00F0) >> 4);

            if (_vRegisters[Vx] != _vRegisters[Vy])
            {
                _programCounter += 2;
            }
        }

        private void OP_Annn()
        {
            _iRegister = (ushort) (_opcode & 0x0FFF);
        }

        private void OP_Bnnn()
        {
            _programCounter = (ushort) (_vRegisters[0] + (_opcode & 0xFFF));
        }

        private void OP_Cxkk()
        {
            var Vx = (byte) ((_opcode & 0x0F00) >> 8);
            var kk = (byte) (_opcode & 0x00FF);

            _vRegisters[Vx] = (byte) (_random.Next(Byte.MaxValue) & kk);
        }

        private void OP_Dxyn()
        {
            // Dispaly needs a bit more configuration before this is tackled
            var Vx = (byte) (_opcode & 0x0F00) >> 8;
            var Vy = (byte) (_opcode & 0x00F0) >> 4;
            var height = (byte) (_opcode & 0x000F);
            
            var xPos = _vRegisters[Vx] % 64;
            var yPos = _vRegisters[Vy] % 32;

            _vRegisters[0xF] = 0;

            for (byte x = 0; x < height; x++)
            {
                var spriteByte = _ram[_iRegister + x];

                for (byte y = 0; y < 8; y++)
                {
                    var spritePixel = (byte) spriteByte & (0x80 >> y);
                    var screenPixel = _vram[(yPos + x) * 64 + (xPos + y)];

                    if (spritePixel != 0)
                    {
                        if (screenPixel == 0xFF)
                        {
                            _vRegisters[0xF] = 1;
                        }

                        _vram[(yPos + x) * 64 + (xPos + y)] ^= 0xFF;
                    }
                }
            }
        }

        private void OP_Ex()
        {
            switch(_opcode & 0x00FF)
            {
                case 0x009E:
                    OP_Ex9E();
                    break;
                case 0x00A1:
                    OP_ExA1();
                    break;
                default:
                    OP_NULL();
                    break;
            }
        }

        private void OP_Ex9E()
        {
            var Vx = (byte) (_opcode & 0xF00) >> 8;
            var key = _vRegisters[Vx];

            if (_keypad.IsPressed(key))
            {
                _programCounter += 2;
            }
        }

        private void OP_ExA1()
        {
            var Vx = (byte) (_opcode & 0xF00) >> 8;
            var key = _vRegisters[Vx];

            if (!_keypad.IsPressed(key))
            {
                _programCounter += 2;
            }
        }

        private void OP_Fx()
        {
            switch(_opcode & 0x00FF)
            {
                case 0x07:
                    OP_Fx07();
                    break;
                case 0x0A:
                    OP_Fx0A();
                    break;
                case 0x15:
                    OP_Fx015();
                    break;
                case 0x18:
                    OP_Fx018();
                    break;
                case 0x01E:
                    OP_Fx01E();
                    break;
                case 0x29:
                    OP_Fx029();
                    break;
                case 0x33:
                    OP_Fx033();
                    break;
                case 0x55:
                    OP_Fx055();
                    break;
                case 0x65:
                    OP_Fx065();
                    break;
                default:
                    OP_NULL();
                    break;
            }
        }

        private void OP_Fx07()
        {
            var Vx = (byte) (_opcode & 0x0F00) >> 8;
            _vRegisters[Vx] = _delayTimer;
        }

        private void OP_Fx0A()
        {
            var Vx = (byte) (_opcode & 0x0F00);

            for (byte i = 0; i < 0xF; i++)
            {
                if (_keypad.IsPressed(i))
                {
                    _vRegisters[Vx] = i;
                    return;
                }
            }
          
            _programCounter -= 2;
        }

        private void OP_Fx015()
        {
            var Vx = (byte) (_opcode & 0x0F00) >> 8;
            _delayTimer = _vRegisters[Vx];
        }

        private void OP_Fx018()
        {
            var Vx = (byte) (_opcode & 0x0F00) >> 8;
            _soundTimer = _vRegisters[Vx];
        }

        private void OP_Fx01E()
        {
            var Vx = (byte) (_opcode & 0x0F00) >> 8;
            _iRegister += _vRegisters[Vx];
        }

        private void OP_Fx029()
        {
            var Vx = (byte) (_opcode & 0x0F00) >> 8;
            var digit = _vRegisters[Vx];

            _iRegister += (byte) (Emulator.FontStartAddress + (5 * digit));
        }

        private void OP_Fx033()
        {
            var Vx = (_opcode & 0x0F00) >>8;
            var value = _vRegisters[Vx];
            

            _ram[_iRegister + 2] = (byte)(value % 10);
            value /= 10;

            _ram[_iRegister + 1] = (byte)(value % 10);
            value /= 10;

            _ram[_iRegister] = (byte)(value % 10);
         

        }

        private void OP_Fx055()
        {
            var Vx = (byte) (_opcode & 0x0F00) >> 8;

            for (byte i = 0; i < Vx; i++)
            {
                _ram[_iRegister + i] = _vRegisters[i];
            }
        }

        private void OP_Fx065()
        {
            var Vx = (byte) (_opcode & 0x0F00) >> 8;

            for (byte i = 0; i < Vx; i++)
            {
                _vRegisters[i] = _ram[_iRegister + i];
            }
        }
    }
}



