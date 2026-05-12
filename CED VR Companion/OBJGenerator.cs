using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using IGtoOBJGen;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
class OBJGenerator
{
    static void flip(string targetPath)
    {
        string[] files = Directory.GetFiles(targetPath);
        foreach (string file in files)
        {
            if (Path.GetExtension(file) == ".obj")
            {
                string[] lines = File.ReadAllLines(file);
                string pattern = @"v (\d+) (\d+) (\d+)";
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line[0] == 'v')
                    {
                        string temp = line;
                        temp = temp.Trim();
                        string[] arr = temp.Split();
                        double z = Double.Parse(arr[3]); //convert string to double
                        z *= -1; //negate z
                        arr[3] = z.ToString();
                        temp = string.Join(" ", arr);
                        lines[i] = temp;
                    }
                }
                File.WriteAllLines(file, lines);
            }
        }
    }
    static void Main(string[] args)
    {
        string targetPath;
        Unzip zipper;
        List<string> fileNames = new List<string>();
        string adbPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\platform-tools\\adb.exe"; //windows
        //string adbPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/platform-tools/adb"; //macos
        bool single = false;
        bool range = false;
        if (args.Count() > 1)
        {
            targetPath = "";
            foreach (char flag in args[1].ToCharArray())
            {
                switch (flag)
                {
                    //output to desktop
                    //case 'd':
                    //    targetPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/" + Path.GetFileNameWithoutExtension(args[0]); ;
                    //    //Console.WriteLine("targetPath: " + targetPath);
                    //    break;
                    case 's':
                        single = true;
                        break;
                    case 'c':
                        range = true;
                        break;
                    default:
                        targetPath = "hui";
                        Console.WriteLine("Invalid Argument");
                        Environment.Exit(1);
                        break;
                }
            }
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
            targetPath = tempFolder + "\\" + Path.GetFileNameWithoutExtension(args[0]);                                    
            Console.CancelKeyPress += delegate { Directory.Delete(tempFolder, true); };
        }
        else
        {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
            targetPath = tempFolder + "\\" + Path.GetFileNameWithoutExtension(args[0]); //maybe update this when more flags come out
            //Console.WriteLine("targetPath: " + targetPath);
            Console.CancelKeyPress += delegate { Directory.Delete(tempFolder, true); };
        }
        zipper = new Unzip(args[0]);
        Console.CancelKeyPress += delegate { zipper.destroyStorage(); };
        //Timer stopwatch
        var watch = new Stopwatch();
        watch.Start();
        if (single)
        {
            int selection = zipper.RunSingle();
            string eventName = zipper.files[selection];
            string eventTargetPath = targetPath;

            generateOBJ(eventName, eventTargetPath, args);

        }
        else if (range)
        {
            zipper.RunRange();
            int total = zipper.files.Count();
            for (int i = 0; i < total; i++)
                {
                    string eventName = zipper.files[i];
                    string eventTargetPath = targetPath;
                    Directory.CreateDirectory(eventTargetPath);

                    PrintProgressBar(i + 1, total, Path.GetFileName(eventName));
                    generateOBJ(eventName, eventTargetPath, args);
                }
            Console.WriteLine("\nAll events processed!");
        }
        else
        {
            zipper.Run();
            int total = zipper.files.Count();
            for (int i = 0; i < total; i++)
            {
                string eventName = zipper.files[i];
                string eventTargetPath = targetPath;
                Directory.CreateDirectory(eventTargetPath);

                PrintProgressBar(i + 1, total, Path.GetFileName(eventName));


                generateOBJ(eventName, eventTargetPath, args);
            }
            Console.WriteLine("\nAll events processed!");
        }
        generateMetaInfo(targetPath,zipper.runName, Path.GetFileName(args[0]),zipper.files.Count());
        Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms"); // See how fast code runs. Code goes brrrrrrr on fancy office pc. It makes me happy. :)

        Console.WriteLine($"OBJ Files written to: {targetPath}\n\nPress ENTER to continue and move files from your device and onto the Oculus Quest");
        Console.ReadLine();
        try
        {
            Communicate bridge = new Communicate(adbPath);
            bridge.ClearFiles();
            bridge.UploadFiles(targetPath);

            zipper.destroyStorage();
            var deletionPath = Path.GetDirectoryName(targetPath);
            cleanUp(deletionPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            //delete regardless if communicate fails
            zipper.destroyStorage();
            var deletionPath = Path.GetDirectoryName(targetPath);
            cleanUp(deletionPath);
        }
    }
    static void generateOBJ(string eventPath, string currentTargetPath, string[] args)
    {
        StreamReader file;
        JsonTextReader reader;
        JObject o2;
        string eventName;

        //replace NAN with null
        string destination = eventPath;
        string[] split = destination.Split('\\');
        eventName = split.Last();
        string text = File.ReadAllText($"{destination}");
        string newText = text.Replace("nan,", "null,").Replace('(', '[').Replace(')', ']');
        File.WriteAllText($"{args[0]}.tmp", newText);
        file = File.OpenText($"{args[0]}.tmp");
        Console.CancelKeyPress += delegate { file.Close(); File.Delete($"{args[0]}.tmp"); };

        eventName = Path.GetFileName(eventName);
        currentTargetPath += "/" + eventName;
        Directory.CreateDirectory(currentTargetPath);
        reader = new JsonTextReader(file);
        o2 = (JObject)JToken.ReadFrom(reader);
        file.Close();
        File.Delete($"{args[0]}.tmp");

        ObjectManager manager = new ObjectManager(o2, currentTargetPath);
        manager.Execute();


        flip(currentTargetPath);
    }
    static void cleanUp(string deletionPath)
    {
        try
        {
            if (deletionPath != null)
            {
                Directory.Delete(deletionPath, recursive: true);
            }
            else
            {
                Console.WriteLine("deletionPath is null. Unable to perform cleanup.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during cleanup: {ex.Message}");
        }
    }

    static void PrintProgressBar(int current, int total, string currentItem)
    {
        int barLength = 20;
        double percent = (double)current / total;
        int filled = (int)(percent * barLength);
        int empty = barLength - filled;

        string bar = "[" + new string('#', filled) + new string('-', empty) + "]";
        string message = $"{bar} {(int)(percent * 100)}% Processing {currentItem}";

        Console.Write("\r" + message.PadRight(Console.WindowWidth - 1));
    }
    static void generateMetaInfo(string targetPath, string runName, string igFileName, int eventCount)
        {
            var jsonData = new {runName = runName, igFileName = igFileName, eventCount = eventCount};
            string json = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
            File.WriteAllText(Path.Combine(targetPath, "MetaInfo.json"), json);
        }
}