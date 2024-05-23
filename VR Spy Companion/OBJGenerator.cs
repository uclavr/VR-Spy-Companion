using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using IGtoOBJGen;
using System.IO;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using System.Xml.Linq;
using System.Runtime.Loader;
using System;

class OBJGenerator
{
    static void Main(string[] args)
    {
        bool inputState;
        string eventName;
        string targetPath;
        Unzip zipper;
        StreamReader file;
        JsonTextReader reader;
        JObject o2;
        List<string> fileNames = new List<string>();
        string adbPath = Directory.GetCurrentDirectory() + "/platform-tools/adb";
        inputState = args.Length == 0;
        
        if (args.Count() > 1)
        {
            targetPath = "";
            foreach (char flag in args[1].ToCharArray())
            {
                switch (flag)
                {
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
        else
        {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
            targetPath = tempFolder;
            Console.CancelKeyPress += delegate { Directory.Delete(tempFolder, true); };
        }
        Console.WriteLine(Directory.GetCurrentDirectory());

        if (inputState)
        {
            zipper = new Unzip(@"/IGdata/Hto4l_120-130GeV (1).ig");
        }
        else
        {
            zipper = new Unzip(args[0]);
        }

        Console.CancelKeyPress += delegate { zipper.destroyStorage(); };
        zipper.Run();

        //Timer stopwatch
        var watch = new Stopwatch();
        watch.Start();

        if (inputState == true)
        {
            file = File.OpenText(@"/IGdata/Event_1096322990");
            eventName = "Event_1096322990";
        }
        else
        {
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
            string newText = text.Replace("nan,", "null,");

            File.WriteAllText($"{args[0]}.tmp", newText);
            file = File.OpenText($"{args[0]}.tmp");
            Console.CancelKeyPress += delegate { file.Close(); File.Delete($"{args[0]}.tmp"); };
        }

        var deletionPath = Path.GetDirectoryName(targetPath);
        targetPath += "\\" + eventName;

        reader = new JsonTextReader(file);
        o2 = (JObject)JToken.ReadFrom(reader);

        file.Close();

        if (inputState == false)
        {
            File.Delete($"{args[0]}.tmp");
        }

        IGTracks t = new IGTracks(o2, targetPath);
        IGBoxes b = new IGBoxes(o2, targetPath);

        var totaljson = JsonConvert.SerializeObject(new { b.jetDatas, b.EEData, b.EBData, b.ESData, b.HEData, b.HBData, b.HOData, b.HFData, b.superClusters, b.muonChamberDatas, t.globalMuonDatas, t.trackerMuonDatas, t.standaloneMuonDatas, t.electronDatas, t.trackDatas }, Formatting.Indented);
        File.WriteAllText($"{targetPath}//totalData.json", totaljson);

        File.WriteAllText($"{Directory.GetCurrentDirectory()}//totalData.json", totaljson);

        string temp_name = Path.GetFileNameWithoutExtension(Path.GetFileName(targetPath)); // i.e. tmp900y20.tmp
        var cleanup = new Cleanup(temp_name, deletionPath);
        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
        {
            cleanup.callCleanUp();
        };

        zipper.destroyStorage();

        try
        {
            Console.WriteLine(targetPath);
            Console.ReadLine();
            Communicate bridge = new Communicate(adbPath);
            bridge.UploadFiles(targetPath);
        }
        catch (Exception e)
        {

            if (e is System.ArgumentOutOfRangeException)
            {
                Console.WriteLine("System.ArgumentOutOfRangeException thrown while trying to locate ADB.\nPlease check that ADB is installed and the proper path has been provided. The default path for Windows is C:\\Users\\[user]\\adbPath\\Local\\Android\\sdk\\platform-tools\n");
            }
            else if (e is SharpAdbClient.Exceptions.AdbException)
            {
                Console.WriteLine("An ADB exception has been thrown.\nPlease check that the Oculus is connected to the computer.");
            }
            Environment.Exit(1);
        }

        Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms"); // See how fast code runs. Code goes brrrrrrr on fancy office pc. It makes me happy. :)
    }
}
