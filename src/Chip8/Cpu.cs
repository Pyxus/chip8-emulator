using System.Text;
using SFML.Audio;

namespace Chip8
{
    public class Cpu
    {
        private Memory _ram;
        private Memory _vram;
        private byte[] _vRegisters = new byte[16];
        private ushort _iRegister;
        private byte _soundTimer;
        private byte _delayTimer;
        private ushort _programCounter;
        private byte _stackPointer;
        private ushort[] _stack = new ushort[16];
        private ushort _opcode;
        private string _instruction = "NULL";
        private Keypad _keypad;
        private Random _random = new Random();
        private Sound sound = new Sound(new SoundBuffer("assets/sounds/beep.wav"));
        private Dictionary<byte, Action> _opHandlers = new Dictionary<byte, Action>();

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

            sound.Volume = 40f;
            sound.Loop = true;
        }

        public void Reset()
        {
            _soundTimer = 0;
            _delayTimer = 0;
            _programCounter = Emulator.InterpreterEndAddress;
        }

        public void Cycle()
        {
            Fetch();
            DecodeAndExecute();
            _keypad.Poll();

            if (_delayTimer > 0)
                _delayTimer--;

            if (_soundTimer > 0)
            {
                _soundTimer--;
                sound.Play();
            }
            else
            {
                sound.Stop();
            }
        }

        public string Dump()
        {
            var sb = new StringBuilder();
            sb.Append($"{_opcode:X4} - {_instruction}\n\n");
            sb.Append($"PC: {_programCounter:X2}\n");
            sb.Append($"I: {_iRegister:X2}\n");
            sb.Append($"DT: {_delayTimer}\n");
            sb.Append($"ST: {_soundTimer}\n");

            for (var i = 0; i < _vRegisters.Length; i++)
            {
                sb.Append($"V{i:X1}: {_vRegisters[i]:X2}\n");
            }

            sb.AppendFormat("\nSP: {0:X2}\n", _stackPointer);

            for (var i = 0; i < _stack.Length; i++)
            {
                sb.AppendFormat($"Stack[{i:X1}]: {_stack[i]:X2}\n");
            }

            return sb.ToString();
        }

        private void Fetch()
        {
            _opcode = (ushort)((_ram[_programCounter] << 8) | _ram[_programCounter + 1]);
            _programCounter += 2;
        }

        private void DecodeAndExecute()
        {
            var opID = (byte)((_opcode & 0xF000u) >> 12);
            var opHandler = OP_NULL;
            _opHandlers.TryGetValue(opID, out opHandler);
            opHandler?.Invoke();
        }

        private void OP_NULL()
        {
            _instruction = "NULL";
            throw new Exception($"Invalid Chip8 program provided. Unknown opcode '{_opcode.ToString("X4")}' read at address 0x{(_programCounter--).ToString("X4")}");
        }

        private void OP_00E()
        {
            if ((_opcode & 0x00FFu) == 0xE0u)
            {
                OP_OOEO();
            }
            else if ((_opcode & 0x00FFu) == 0xEEu)
            {
                OP_OOEE();
            }
            else
            {
                OP_NULL();
            }
        }

        private void OP_OOEO()
        {
            _vram.Clear();
            _instruction = "CLS";
        }

        private void OP_OOEE()
        {
            --_stackPointer;
            _programCounter = _stack[_stackPointer];
            _instruction = "RET";
        }

        private void OP_1nnn()
        {
            var address = (ushort) (_opcode & 0x0FFFu);
            _programCounter = address;
            _instruction = $"JP {address:X3}";
        }


        private void OP_2nnn()
        {
            var address = (ushort) (_opcode & 0x0FFFu);
            _stack[_stackPointer] = _programCounter;
            ++_stackPointer;
            _programCounter = address;
            _instruction = $"CALL {address:X3}";
        }

        private void OP_3xkk()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var kk = (byte) (_opcode & 0x00FF);

            if (_vRegisters[x] == kk)
            {
                _programCounter += 2;
            }

            _instruction = $"SE V{x:X1}, {kk:X3}";
        }

        private void OP_4xkk()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var kk = (byte) (_opcode & 0x00FF);

            if (_vRegisters[x] != kk)
            {
                _programCounter += 2;
            }

            _instruction = $"SNE V{x:X1}, {kk:X3}";
        }

        private void OP_5xy0()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);

            if (_vRegisters[x] == _vRegisters[y])
            {
                _programCounter += 2;
            }

            _instruction = $"SE V{x:X1}, V{y:X1}";
        }

        private void OP_6xkk()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var kk = (byte) (_opcode & 0x00FF);

            _vRegisters[x] = kk;
            _instruction = $"LD V{x:X1}, {kk:X2}";
        }

        private void OP_7xkk()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var kk = (byte) (_opcode & 0x00FF);

            _vRegisters[x] += kk;
            _instruction = $"ADD V{x:X1}, {kk:X2}";
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
                    OP_8xy3();
                    break;
                case 0x4:
                    OP_8xy4();
                    break;
                case 0x5:
                    OP_8xy5();
                    break;
                case 0x6:
                    OP_8xy6();
                    break;
                case 0x7:
                    OP_8xy7();
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
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);

            _vRegisters[x] = _vRegisters[y];
            _instruction = $"LD V{x:X1}, V{y:X1}";
        }

        private void OP_8xy1()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);

            _vRegisters[x] |= _vRegisters[y];
            _instruction = $"OR V{x:X1}, V{y:X1}";
        }

        private void OP_8xy2()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);

            _vRegisters[x] &= _vRegisters[y];
            _instruction = $"AND V{x:X1}, V{y:X1}";
        }

        private void OP_8xy3()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);

            _vRegisters[x] ^= _vRegisters[y];
            _instruction = $"XOR V{x:X1}, V{y:X1}";

        }
        private void OP_8xy4()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);
            var sum = _vRegisters[x] + _vRegisters[y];

            _vRegisters[0xF] = (byte) (sum > 0xFF ? 1 : 0);
            _vRegisters[x] = (byte) (sum & 0xFF);
            _instruction = $"ADD V{x:X1}, V{y:X1}";
        }

        private void OP_8xy5()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);

            _vRegisters[0xF] = (byte)(_vRegisters[x] > _vRegisters[y] ? 1 : 0);
            _vRegisters[x] -= _vRegisters[y];
            _instruction = $"SUB V{x:X1}, V{y:X1}";
        }

        private void OP_8xy6()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);

            _vRegisters[0xF] = (byte) (_vRegisters[x] & 0x1u); // TODO: Comprehend this operation
            _vRegisters[x] >>= 0x1;
            _instruction = $"SHR V{x:X1}, {{, V{y:X1}}}";
        }

        private void OP_8xy7()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);

            _vRegisters[0xF] = (byte) (_vRegisters[y] > _vRegisters[x] ? 1 : 0);
            _vRegisters[x] = (byte) (_vRegisters[y] - _vRegisters[x]);
            _instruction = $"SUBN V{x:X1}, V{y:X1}";
        }

        private void OP_8xyE()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);

            _vRegisters[0xF] = (byte) ((_vRegisters[x] & 0x80) >> 7); // TODO: Comprehend this operation
            _vRegisters[x] <<= 1;
            _instruction = $"SHL V{x:X1}, {{, V{y:X1}}}";
        }

        private void OP_9xy0()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);

            if (_vRegisters[x] != _vRegisters[y])
            {
                _programCounter += 2;
            }
            _instruction = $"SNE V{x:X1}, V{y:X1}";
        }

        private void OP_Annn()
        {
            var address = (ushort) (_opcode & 0x0FFF);
            _iRegister = address;
            _instruction = $"LD I, {address:X4}";
        }

        private void OP_Bnnn()
        {
            var address = (_opcode & 0x0FFF);
            _programCounter = (ushort) (_vRegisters[0] + address);
            _instruction = $"JP V0, {address:X3}";
        }

        private void OP_Cxkk()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var kk = (byte) (_opcode & 0x00FF);

            _vRegisters[x] = (byte)(_random.Next(Byte.MaxValue) & kk);
            _instruction = $"RND V{x:X1}, {kk:X2}";
        }

        private void OP_Dxyn()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var y = (byte) ((_opcode & 0x00F0) >> 4);
            var n = (byte) (_opcode & 0x000F);
            var xPos = _vRegisters[x] % Emulator.BaseWidth;
            var yPos = _vRegisters[y] % Emulator.BaseHeight;

            _vRegisters[0xF] = 0;

            for (var xDraw = 0; xDraw < n; ++xDraw)
            {
                var spriteByte = _ram[_iRegister + xDraw];

                for (var yDraw = 0; yDraw < 8; ++yDraw)
                {
                    var spritePixel = spriteByte & (0x80 >> yDraw);
                    var vRamAddress = ((yPos + xDraw) * (Emulator.BaseWidth) + (xPos + yDraw)) % (Emulator.BaseWidth * Emulator.BaseHeight);

                    if (spritePixel != 0)
                    {
                        if (_vram[vRamAddress] == 0xFF)
                            _vRegisters[0xF] = 1;

                        _vram[vRamAddress] ^= 0xFF;
                    }
                }
            }

            _instruction = $"DRW V{x:X1}, V{y:X1}, {n:X1}";
        }

        private void OP_Ex()
        {
            switch (_opcode & 0x00FF)
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
            var x = (byte) ((_opcode & 0xF00) >> 8);
            var key = _vRegisters[x];

            if (_keypad.IsPressed(key))
            {
                _programCounter += 2;
            }

            _instruction = $"SKP V{x:X1}";
        }

        private void OP_ExA1()
        {
            var x = (byte) ((_opcode & 0xF00) >> 8);
            var key = _vRegisters[x];

            if (!_keypad.IsPressed(key))
            {
                _programCounter += 2;
            }

            _instruction = $"SKNP V{x:X1}";
        }

        private void OP_Fx()
        {
            switch (_opcode & 0x00FF)
            {
                case 0x07:
                    OP_Fx07();
                    break;
                case 0x0A:
                    OP_Fx0A();
                    break;
                case 0x15:
                    OP_Fx15();
                    break;
                case 0x18:
                    OP_Fx18();
                    break;
                case 0x01E:
                    OP_Fx1E();
                    break;
                case 0x29:
                    OP_Fx29();
                    break;
                case 0x33:
                    OP_Fx33();
                    break;
                case 0x55:
                    OP_Fx55();
                    break;
                case 0x65:
                    OP_Fx65();
                    break;
                default:
                    OP_NULL();
                    break;
            }
        }

        private void OP_Fx07()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            _vRegisters[x] = _delayTimer;
            _instruction = $"LD V{x:X1}, DT";
        }

        private void OP_Fx0A()
        {
            var x = (byte) (_opcode & 0x0F00);

            _instruction = $"LD V{x:X1}, K";

            for (byte i = 0; i <= 0xF; i++)
            {
                if (_keypad.IsPressed(i))
                {
                    _vRegisters[x] = i;
                    return;
                }
            }

            _programCounter -= 2;
        }

        private void OP_Fx15()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            _delayTimer = _vRegisters[x];
            _instruction = $"LD DT, V{x:X1}";
        }

        private void OP_Fx18()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            _soundTimer = _vRegisters[x];
            _instruction = $"LD ST, V{x:X1}";
        }

        private void OP_Fx1E()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            _iRegister += _vRegisters[x];
            _instruction = $"ADD I, V{x:X1}";
        }

        private void OP_Fx29()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var digit = _vRegisters[x];

            _iRegister += (byte) (Emulator.FontStartAddress + (5 * digit));
            _instruction = $"LD F, V{x:X1}";
        }

        private void OP_Fx33()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);
            var value = (byte) _vRegisters[x];

            _ram[_iRegister + 2] = (byte) (value % 10);
            value /= 10;

            _ram[_iRegister + 1] = (byte) (value % 10);
            value /= 10;

            _ram[_iRegister] = (byte)(value % 10);

            _instruction = $"LD B, V{x:X1}";
        }

        private void OP_Fx55()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);

            for (byte i = 0; i <= x; ++i)
            {
                _ram[_iRegister + i] = _vRegisters[i];
            }

            _instruction = $"LD [I], V{x:X1}";
        }

        private void OP_Fx65()
        {
            var x = (byte) ((_opcode & 0x0F00) >> 8);

            for (byte i = 0; i <= x; ++i)
            {
                _vRegisters[i] = _ram[_iRegister + i];
            }

            _instruction = $"LD V{x:X1}, [I]";
        }
    }
}



