using Chip8;
using System.CommandLine;

class Program
{
    static void Main(string[] args)
    {
        #region Testing - Remove For Release
        if (args.Length == 0)
        {
            Console.WriteLine("Starting emulator with no arguments. This is just for testing and should be disabled for release.");
            RunChip8(@"D:\Code\C Sharp\COSC439_Chip8_Project\build\roms\games\Pong (1 player).ch8", true, true);
            //RunChip8Bytes(new byte[]{0xD0, 0x05});
            return;
        }
        #endregion

        var fileArg = new Argument<string>("file", "Path to chip8 program file.");
        var runCommand = new Command("run", "Run Chip8 program");
        runCommand.AddArgument(fileArg);
        runCommand.SetHandler(() =>{
            var parseResult = runCommand.Parse(args);
            var filePath = parseResult.GetValueForArgument(fileArg);
            RunChip8(filePath);
        });

        var startPausedOption = new Option<bool>(new string[]{"--paused", "-p"}, "Launch with debugger paused on start.");
        var debugCommand = new Command("debug", "Run Chip8 program with debugger.");
        debugCommand.AddOption(startPausedOption);
        debugCommand.AddArgument(fileArg);
        debugCommand.SetHandler(() => {
            var parseResult = debugCommand.Parse(args);
            var filePath = parseResult.GetValueForArgument(fileArg);
            var isPaused = parseResult.GetValueForOption(startPausedOption);
            RunChip8(filePath, true, isPaused);
        });

        var rootCommand = new RootCommand("Chip8 emulator which can interpret and execute chip8 programs.");
        rootCommand.AddCommand(runCommand);
        rootCommand.AddCommand(debugCommand);
        rootCommand.Invoke(args);
    }

    static void RunChip8(string filePath, bool withDebugger = false, bool launchPaused = false)
    {
        try
        {
            var chip8 = withDebugger ? new Debugger(launchPaused) : new Emulator();
            chip8.Initialize();
            chip8.Load(filePath);
            chip8.Process();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }   
    }

    static void RunChip8Bytes(byte[] bytes, string programName = "Chip8 Program")
    {
        try
        {
            var chip8 = new Debugger(true);
            chip8.Initialize();
            chip8.Load(bytes, programName);
            chip8.Process();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }   
    }
}