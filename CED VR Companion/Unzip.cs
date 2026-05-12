using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
namespace IGtoOBJGen
{
    class Unzip
    {
        private string directoryName { get; set; }
        public string[] files;
        public string runFolder;
        public string runName;
        private string tempStorageDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"/Temp/IGtoOBJGenExtraction";
        private string tempTransmitDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"/Temp/IGtoOBJGenTransmission";
        public Unzip(string filename)
        {
            if (Directory.Exists(tempStorageDirectory))
            {
                Directory.Delete(tempStorageDirectory, true);
                Directory.CreateDirectory(tempStorageDirectory);
            }
            else
            {
                Directory.CreateDirectory(tempStorageDirectory);
            }
            string extractPath = tempDirectoryPath();
            ZipFile.ExtractToDirectory(filename, tempStorageDirectory);
            directoryName = tempStorageDirectory;

        }
        public void Run()
        {
            runFolder = selectFolderFromFolder(directoryName + "\\Events");
            //string file = selectFileFromFolder(runFolder);
            //currentFile = file;
            files = Directory.GetFiles(runFolder);

        }
        public int RunSingle()
        {
            runFolder = selectFolderFromFolder(directoryName + "\\Events");
            files = Directory.GetFiles(runFolder);
            return selectFileFromFolder(runFolder);
        }
        public void RunRange()
        {
            runFolder = selectFolderFromFolder(directoryName + "\\Events");
            runName = runFolder.Split('\\').Last();
            files = selectFileRangeFromFolder(runFolder);
        }
        public void destroyStorage() //make sure this destroys temp storage
        {
            Directory.Delete(directoryName, true);
            //Console.WriteLine("Temp storage cleared!");
        }
        private static string tempDirectoryPath()
        {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
        public static string selectFolderFromFolder(string path)
        {
            string[] folders = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (string folder in folders)
            {
                int index = Array.IndexOf(folders, folder);
                Console.WriteLine($"{index}) {folder}");
            }
            Console.WriteLine("Enter ID # of desired path:");
            int selection = int.Parse(Console.ReadLine());
            return folders[selection];
        }
        public static int selectFileFromFolder(string path)
        {
            string[] files = Directory.GetFiles(path);
            SortByNumericOrder(ref files);
            foreach (string file in files)
            {
                int index = Array.IndexOf(files, file);
                Console.WriteLine($"{index}) {file}");
            }
            Console.WriteLine("Enter ID # of desired event file:");

            int selection = int.Parse(Console.ReadLine());

            Console.WriteLine(files[selection]);

            //return files[selection];
            return selection;
        }
                public static string[] selectFileRangeFromFolder(string path)
        {
            //print out event options
            string[] files = Directory.GetFiles(path);
            SortByNumericOrder(ref files);
            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine($"{i}) {files[i]}");
            }
            Console.WriteLine("Enter range # of desired events (e.g. 1-5):");
            string range = Console.ReadLine();
            string[] rangeArray = range.Split('-');
            int start = int.Parse(rangeArray[0]);
            int end = int.Parse(rangeArray[1]);
            if (start < 0 || end >= files.Length || end < start)
            {
                Console.WriteLine("Invalid range. Please try again.");
                return Array.Empty<string>();
            }
            string[] selectedFiles = new string[end - start + 1];
            Array.Copy(files, start, selectedFiles, 0, end - start + 1);
            return selectedFiles;
        }
        public static void SortByNumericOrder(ref string[] paths)
        {
            Array.Sort(paths, (a, b) =>
            {
                int numA = ExtractNumber(a);
                int numB = ExtractNumber(b);
                return numA.CompareTo(numB);
            });
        }
        private static int ExtractNumber(string path)
        {
            string fileName = Path.GetFileName(path); // remove prefix from the event number
            Match match = Regex.Match(fileName, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }
    }
}