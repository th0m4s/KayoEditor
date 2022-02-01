using System;
using System.IO;

namespace KayoEditor
{
    class Program
    {
        static string AskString(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine();
        }

        static int AskInt(string question)
        {
            return int.Parse(AskString(question));
        }

        static float AskFloat(string question)
        {
            return float.Parse(AskString(question));
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to KayoEditor!");
            bool quit = false;

            while(!quit)
            {
                Console.Clear();
                string filename = AskString("Please enter a file path (or nothing to quit):");
                if(filename.Trim().Length == 0)
                {
                    quit = true;
                    break;
                }

                if(File.Exists(filename))
                {
                    ImagePSI image = new ImagePSI(filename);
                    bool changefile = false;

                    while(!changefile)
                    {
                        int choice = AskInt("\nSelect a choice:\n 1 - Greyscale\n 2 - Black and white\n 3 - Flip\n 4 - Scale\n 5 - Save\n 6 - Change file\n 7 - Quit");
                        switch(choice)
                        {
                            case 1:
                                image = image.Greyscale();
                                Console.WriteLine("Image set to greyscale");
                                break;
                            case 2:
                                image = image.BlackAndWhite();
                                Console.WriteLine("Image set to black and white");
                                break;
                            case 3:
                                string mode = AskString("How do you want to flip your image? ('x', 'y' or 'both')");
                                bool flipped = true;
                                switch(mode)
                                {
                                    case "x":
                                        image = image.Flip(FlipMode.FlipX);
                                        break;
                                    case "y":
                                        image = image.Flip(FlipMode.FlipY);
                                        break;
                                    case "both":
                                        image = image.Flip(FlipMode.FlipBoth);
                                        break;
                                    default:
                                        flipped = false;
                                        break;
                                }
                                Console.WriteLine(flipped ? "Image flipped" : "Invalid flip mode!");
                                break;
                            case 4:
                                float scale = AskFloat("Scale?");
                                if(scale > 0)
                                {
                                    image = image.Scale(scale);
                                    Console.WriteLine("Image scaled");
                                } else
                                {
                                    Console.WriteLine("scale must be strictly positive!");
                                }
                                break;
                            case 5:
                                string newPath = AskString("Where do you want to save the image? (nothing to cancel or 'source' to overwrite the opened image)");
                                if(newPath.Trim().Length > 0)
                                {
                                    if (newPath == "source")
                                        newPath = filename;

                                    image.Save(newPath);
                                }
                                break;
                            case 6:
                                changefile = true;
                                break;
                            case 7:
                                changefile = true;
                                quit = true;
                                break;
                        }
                    }
                }
            }
        }
    }
}
