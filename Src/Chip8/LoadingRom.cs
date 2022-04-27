namespace Chip8
{
    public class LoadingRom
    {

        public static byte[] ReadAllBytes(string path)
        {

            byte[] readingRoms = File.ReadAllBytes(path);
            return readingRoms;
        }

        public byte[] ReadAllBytes()
        {
            Console.WriteLine("Enter the path of file starting with C:");
            string path = Console.ReadLine();
            byte[] readingRoms = File.ReadAllBytes(path);
            return readingRoms;
        }

        public void PrintBytes(byte[] readingRoms)
        {
            foreach (byte k in readingRoms)
            {
                Console.WriteLine(k);
            }
        }



    }





}




