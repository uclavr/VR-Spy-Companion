using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using IGtoOBJGen;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

class OBJGenerator {
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
    static void Main(string[] args) {
        string targetPath;
        Unzip zipper;

        List<string> fileNames = new List<string>();
        //string adbPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\platform-tools\adb.exe"; //windows
        string adbPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/platform-tools/adb"; //macos
        
        if (args.Count() > 1) {
            targetPath = "";
            foreach (char flag in args[1].ToCharArray()) {
                switch (flag) {
                    case 's':
                        targetPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        Console.WriteLine("targetPath: " + targetPath);
                        break;
                    default:
                        targetPath = "hui";
                        Console.WriteLine("Invalid Argument");
                        Environment.Exit(1);
                        break;
                }
            }
        }
        else {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
            targetPath = tempFolder + "/" + Path.GetFileNameWithoutExtension(args[0]); //maybe update this when more flags come out
            Console.WriteLine("targetPath: " + targetPath);
            Console.CancelKeyPress += delegate { Directory.Delete(tempFolder, true); };
        }
        zipper = new Unzip(args[0]);

        Console.CancelKeyPress += delegate { zipper.destroyStorage(); };
        zipper.Run();

        //Timer stopwatch
        var watch = new Stopwatch();
        watch.Start();

        foreach (string eventName in zipper.files)
        {
            string eventTargetPath = targetPath;
            Directory.CreateDirectory(eventTargetPath);
            generateOBJ(eventName, eventTargetPath, args);
        }

        Console.WriteLine($"OBJ Files written to: {targetPath}\n\nPress ENTER to continue and move files from your device and onto the Oculus Quest");
        Console.ReadLine();

        Communicate bridge = new Communicate(adbPath);
        bridge.ClearFiles();
        bridge.UploadFiles(targetPath);
        zipper.destroyStorage();
        var deletionPath = Path.GetDirectoryName(targetPath);
        cleanUp(deletionPath);
        Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms"); // See how fast code runs. Code goes brrrrrrr on fancy office pc. It makes me happy. :)
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
        flip(currentTargetPath); //verify this
    }


    static void cleanUp(string deletionPath)
    {
        // Code inside this block will be executed just before the program exits
        Console.WriteLine("Executing cleanup before exit...");
        try
        {
            if (deletionPath != null)
            {
                Directory.Delete(deletionPath, recursive: true);
            }
            else
            {
                // Handle the case when deletionPath is null (if needed)
                Console.WriteLine("deletionPath is null. Unable to perform cleanup.");
            }
        }
        catch (Exception ex)
        {
            // Handle the exception
            Console.WriteLine($"An error occurred during cleanup: {ex.Message}");
        }
    }
}
