using System.Data;
using Chip8;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Starting emulator with no arguments. This is just for testing and should be disabled for release.");
            var chip8 = new Emulator(true);
            chip8.Initialize();
            chip8.Load("D:/Code/C Sharp/COSC439_Chip8_Project/chip8-roms/games/Pong (1 player).ch8");
            chip8.Process();
            return;
        }

        var fileArg = new Argument<string>("file", "Path to chip8 program file.");
        var debugOption = new Option<bool>(new string[]{"--debug", "-d"}, "Launch program with debugger");
        var rootCommand = new RootCommand
        {
            debugOption
        };

        rootCommand.Description = "Chip8 emulator which can interpret and execute chip8 programs.";
        rootCommand.AddArgument(fileArg);
        
        rootCommand.SetHandler(()=>{
            var parseResult = rootCommand.Parse(args);
            var filePath = parseResult.GetValueForArgument(fileArg);
            var isDebuggerEnabled = parseResult.GetValueForOption(debugOption);

            var chip8 = new Emulator(isDebuggerEnabled);
            chip8.Initialize();
            chip8.Load(filePath);
            chip8.Process();
        });

        rootCommand.Invoke(args);
    }
}