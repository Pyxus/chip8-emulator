using Chip8;

class Program
{

    static void Main(string[] args)
    {
        var chip8 = new Emulator();
        chip8.AdjustDisplay(15, "Chip8");
        chip8.Process();
    }
}