using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using IGtoOBJGen;
using System.Reflection;

class OBJGenerator {
    static void Main(string[] args) {
        bool inputState;
        string eventName;
        string targetPath;
        Unzip zipper;
        StreamReader file;
        JsonTextReader reader;
        JObject o2;
        List<string> fileNames = new List<string>();
        string adbPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/platform-tools/adb.exe";

        inputState = args.Length == 0;
        
        if (args.Count() > 1) {
            targetPath = "";
            foreach (char flag in args[1].ToCharArray()) {
                switch (flag) {
                    case 's':
                        targetPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        Console.WriteLine(targetPath);
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
            targetPath = tempFolder;
            Console.CancelKeyPress += delegate { Directory.Delete(tempFolder, true); };
        }
        zipper = new Unzip(args[0]);

        Console.CancelKeyPress += delegate { zipper.destroyStorage(); };
        zipper.Run();

        //Timer stopwatch
        var watch = new Stopwatch();
        watch.Start();

        if (inputState == true) {
            file = File.OpenText(@"/IGdata/Event_1096322990");
            eventName = "Event_1096322990";
        }
        else {
            /*  Right so what's all this? We get the name of the event and then
            find and replace all occurrences of nan that are in the original file
            with null so that the JSON library can properly parse it. Store the revisions in a temp file that
            is deleted at the end of the program's execution so that the original file goes unchanged and can 
            still be used with iSpy  */
            //zipper = new Unzip(args[0]);
            string destination = zipper.currentFile;
            string[] split = destination.Split('\\');
            eventName = split.Last();

            string text = File.ReadAllText($"{destination}");
            string newText = text.Replace("nan,", "null,").Replace('(','[').Replace(')',']');


            File.WriteAllText($"{args[0]}.tmp", newText);
            file = File.OpenText($"{args[0]}.tmp");
            Console.CancelKeyPress += delegate { file.Close(); File.Delete($"{args[0]}.tmp"); };
        }

        var deletionPath = Path.GetDirectoryName(targetPath);
        string temp_Folder = targetPath;
        targetPath += "\\" + eventName;
        Directory.CreateDirectory(targetPath);
        reader = new JsonTextReader(file);
        o2 = (JObject)JToken.ReadFrom(reader);
        file.Close();
        File.Delete($"{args[0]}.tmp");

        ObjectManager manager = new ObjectManager(o2, targetPath);
        manager.Execute();
        string temp_name = Path.GetFileNameWithoutExtension(Path.GetFileName(targetPath)); // i.e. tmp900y20.tmp
        var cleanup = new Cleanup(temp_name, deletionPath);
        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => {
            cleanup.callCleanUp();
        };

        zipper.destroyStorage();

        
            Console.WriteLine(targetPath);
            Console.ReadLine();
            Communicate bridge = new Communicate(adbPath);
            bridge.UploadFiles(targetPath);
            Directory.Delete(temp_Folder, true);
        
        /*catch (Exception e) {
            if (e is System.ArgumentOutOfRangeException) {
                Console.WriteLine("System.ArgumentOutOfRangeException thrown while trying to locate ADB.\nPlease check that ADB is installed and the proper path has been provided. The default path for Windows is C:\\Users\\[user]\\adbPath\\Local\\Android\\sdk\\platform-tools\n");
            }
            else if (e is SharpAdbClient.Exceptions.AdbException) {
                Console.WriteLine("An ADB exception has been thrown.\nPlease check that the Oculus is connected to the computer.");
            }
            Directory.Delete(temp_Folder, true);
            Environment.Exit(1);
        }*/
        Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms"); // See how fast code runs. Code goes brrrrrrr on fancy office pc. It makes me happy. :)
    }
}
