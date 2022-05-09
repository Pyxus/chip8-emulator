using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace Chip8
{
public class Debugger : Emulator
    {
        private RenderWindow _debugerApp;
        private Text _registerText;
        private Text _memoryText;
        private Text _vramText;
        private Text _keypadText;
        private Text _helpText;
        private Text _crashText;
        private int _memoryLineSkip;
        private int _vramLineSkip;
        private bool _isPaused;
        private bool _canStepFoward;
        private bool _isProgramCrashed;
        private int _stepCount;

        public Debugger(bool startPaused = false)
        {
            var font = new Font("assets/fonts/JetBrainsMono-Regular.ttf");

            _isPaused = startPaused;
            _debugerApp = new RenderWindow(new VideoMode(1280, 720), "Chip8 - Debugger");
            _debugerApp.Closed += OnDebuggerWindowClose;
            _debugerApp.MouseWheelScrolled += OnWindowMouseWheelScrolled;     
            _debugerApp.KeyPressed += OnWindowKeyPressed;  
            _debugerApp.KeyReleased += OnWindowKeyReleased;
            _debugerApp.Resized += OnWindowResized;

            _registerText = new Text()
            {
                Font = font,
                CharacterSize = 12,
                FillColor = Color.White,
                Position = new Vector2f(0, 0),
            };

            _keypadText = new Text()
            {
                Font = font,
                CharacterSize = 12,
                FillColor = Color.White,
                Position = new Vector2f(1044, 630),
            };

            _memoryText = new Text()
            {
                Font = font,
                CharacterSize = 12,
                FillColor = Color.White,
                Position = new Vector2f(250, 0),
            };

            _vramText = new Text()
            {
                Font = font,
                CharacterSize = 12,
                FillColor = Color.White,
                Position = new Vector2f(650, 0),
            };

            _helpText = new Text()
            {
                Font = font,
                CharacterSize = 12,
                FillColor = Color.Cyan,
                Position = new Vector2f(960, 0),
            };

            _crashText = new Text()
            {
                Font = font,
                CharacterSize = 18,
                FillColor = Color.Red,
                Position = new Vector2f(1044, 250),
                Style = Text.Styles.Bold

            };
            App.RequestFocus();
        }

        public override void Process()
        {
            long prevTime = DateTime.Now.Ticks, delta = 0, accumulator = 0;
            
            try
            {
                while (App.IsOpen && _debugerApp.IsOpen)
                {
                    _debugerApp.DispatchEvents();
                    App.DispatchEvents();

                    delta = DateTime.Now.Ticks - prevTime;
                    prevTime = DateTime.Now.Ticks;
                    if (!_isPaused)
                    {
                        accumulator += delta;

                        while (accumulator > RefreshRate)
                        {
                            Cpu.Cycle();
                            accumulator -= RefreshRate;
                        }

                        Render();
                        _stepCount++;
                    }
                    else if (_canStepFoward)
                    {
                        Cpu.Cycle();
                        Render();
                        _canStepFoward = false;
                        _stepCount++;
                    }
                    Update(Cpu.Dump(), Ram.Dump(0x10), Vram.Dump(), Keypad.ToString());
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
                _isProgramCrashed = true;
                
                while (_debugerApp.IsOpen)
                {
                    _debugerApp.DispatchEvents();
                    App.DispatchEvents();
                    Update(Cpu.Dump(), Ram.Dump(0x10), Vram.Dump(), Keypad.ToString());
                }
            }
        }

        private void Update(string registerText, string memoryText, string vramText, string keypadText)
        {
            _debugerApp.DispatchEvents();
            _debugerApp.Clear();

            var memoryLines = memoryText.Replace("\r", "").Split('\n');
            var memoryColumns = memoryLines[..1];
            var memoryRows = memoryLines[2..];
            var memString = "[RAM]\n";
            _memoryLineSkip = Math.Clamp(_memoryLineSkip, 0, memoryRows.Length);

            for (var i = 0; i < memoryColumns.Length; i++)
            {
                memString += memoryColumns[i] + "\n\n";
            }

            for (var i = _memoryLineSkip; i < memoryRows.Length; i++)
            {
                memString += memoryRows[i] + "\n";
            }

            var vramLines = vramText.Replace("\r", "").Split('\n');
            var vramColumns = vramLines[..1];
            var vramRows = vramLines[2..];
            var vramString = "[VRAM]\n";
            _vramLineSkip = Math.Clamp(_vramLineSkip, 0, vramRows.Length);

            for (var i = 0; i < vramColumns.Length; i++)
            {
                vramString += vramColumns[i] + "\n\n";
            }

            for (var i = _memoryLineSkip; i < vramRows.Length; i++)
            {
                vramString += vramRows[i] + "\n";
            }

            _registerText.DisplayedString = registerText;
            _keypadText.DisplayedString = "[Keypad]\n" + keypadText;
            _memoryText.DisplayedString = memString;
            _vramText.DisplayedString = vramString;
            _helpText.DisplayedString = $@"
            [CONTROLS]
            Mouse Scroll / UpDown Arrows:
                Scroll through memory map
            Hold LSHIFT
                Increases scroll speed.
            Spacebar:
                Pause emulator
            Right Arrow:
                Step foward when paused
            
            [STATE]
            Is Paused: {_isPaused}
            Steps: {_stepCount}
            ";

            _crashText.DisplayedString = _isProgramCrashed ? "PROGRAM CRASHED!!!" : "";

            var memoryColumnUnderline = new RectangleShape(new Vector2f(328, 2))
            {
                Position = _memoryText.Position + new Vector2f(50, 35)
            };
            
            var vramColumnUnderline = new RectangleShape(new Vector2f(328, 2))
            {
                Position = _vramText.Position + new Vector2f(50, 35)
            };

            _debugerApp.Draw(_registerText);
            _debugerApp.Draw(_keypadText);
            _debugerApp.Draw(_memoryText);
            _debugerApp.Draw(memoryColumnUnderline);
            _debugerApp.Draw(_vramText);
            _debugerApp.Draw(vramColumnUnderline);
            _debugerApp.Draw(_helpText);
            _debugerApp.Draw(_crashText);
            _debugerApp.Display();
        }

        private void OnDebuggerWindowClose(object? sender, EventArgs args)
        {
            _debugerApp.Close();
            App.Close();
        }

        private void OnWindowMouseWheelScrolled(object? sender, MouseWheelScrollEventArgs args)
        {
            if (args.Delta >= 1)
            {
                _memoryLineSkip -= Keyboard.IsKeyPressed(Keyboard.Key.LShift) ? 4 : 1;
                _vramLineSkip -= Keyboard.IsKeyPressed(Keyboard.Key.LShift) ? 4 : 1;
            }
            else if (args.Delta <= -1)
            {
                _memoryLineSkip += Keyboard.IsKeyPressed(Keyboard.Key.LShift) ? 4 : 1;
                _vramLineSkip += Keyboard.IsKeyPressed(Keyboard.Key.LShift) ? 4 : 1;
            }
        }

        private void OnWindowKeyPressed(object? sender, KeyEventArgs args)
        {
            if (args.Code == Keyboard.Key.Up)
            {
                _memoryLineSkip -= args.Shift ? 4 : 1;
                _vramLineSkip -= args.Shift ? 4 : 1;
            }
            else if (args.Code == Keyboard.Key.Down)
            {
                _memoryLineSkip += args.Shift ? 4 : 1;
                _vramLineSkip += args.Shift ? 4 : 1;
            } 
        }

        private void OnWindowKeyReleased(object? sender, KeyEventArgs args)
        {
            if (args.Code == Keyboard.Key.Space)
            {
                _isPaused = !_isPaused;
            }

            if (args.Code == Keyboard.Key.Right)
            {
                _canStepFoward = true;
            }
        }

        private void OnWindowResized(object? sender, SizeEventArgs args)
        {
            var rect = new FloatRect(0, 0, args.Width, args.Height);
            _debugerApp.SetView(new View(rect));
        }
    }
}
