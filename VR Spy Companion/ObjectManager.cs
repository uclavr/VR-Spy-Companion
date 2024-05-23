using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGtoOBJGen
{
    internal class ObjectManager
    {
        private List<TypeConfig> eventObjects;
        private List<TrackExtrasData> EXTRAS;
        private JObject INPUTJSON;
        private List<string> TYPES;
        private string EVENTPATH;
        private FileInfo INPUTFILE;
        private FileInfo OUTPUTFILE;
        private List<string> outputJSONStrings;
        private bool EXTRASFLAG;
        private bool POINTSFLAG;


        /*
         TODO:
            add cleanup methods and listeners
            implement adb
            go into maui
         */
        public ObjectManager(FileInfo inputFile)
        {
            INPUTFILE = inputFile;
            StreamReader stream = new StreamReader(INPUTFILE.FullName);
            JsonTextReader reader = new JsonTextReader(stream);
            INPUTJSON = (JObject)JToken.ReadFrom(reader);
            TYPES = ((JObject)INPUTJSON["Types"]).Properties().Select(p => p.Name).ToList();
            EVENTPATH = CreateTempFolder();
            EXTRASFLAG = false;
            POINTSFLAG = false;
            EXTRAS = StaticLineHandlers.trackExtrasParse(INPUTJSON);
            outputJSONStrings = new List<string>();
        }
        public void Execute()
        {
            InstantiateObjects();
            CreateObjects();
            generateJSON();
        }
        public void generateJSON()
        {
            List<object> dataList = new List<object>();
            foreach(var json in outputJSONStrings)
            {
                dataList.Add(JsonConvert.DeserializeObject(json));
            }
            File.WriteAllText($"{EVENTPATH}//totaldata.json",JsonConvert.SerializeObject(dataList));
        }
        private string CreateTempFolder()
        {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
            Console.CancelKeyPress += delegate { Directory.Delete(tempFolder, true); };
            return tempFolder;
        }
        public void CreateObjects()
        {
            foreach(var item in eventObjects)
            {
                outputJSONStrings.Add(item.Execute());
            }
        }
        public void InstantiateObjects()
        {
            foreach(string t in TYPES) {
                switch(t){
                    case "TrackerMuons_V1":
                        break;
                    case "TrackerMuons_V2":
                        TrackerMuonV2 trackerMuonV2 = new TrackerMuonV2(INPUTJSON, EVENTPATH);
                        eventObjects.Add(trackerMuonV2);  
                        break;
                    case "GsfElectrons_V1":
                        break;
                    case "GsfElectrons_V2":
                        break;
                    case "GsfElectrons_V3":
                        break;
                    case "Vertices_V1":
                        break;
                    case "PrimaryVertices_V1":
                        break;
                    case "SecondaryVertices_V1":
                        break;
                    case "SuperClusters_V1":
                        break;
                    case "EERecHits_V2":
                        break;
                    case "EBRecHits_V2":
                        break;
                    case "ESRecHits_V2":
                        break;
                    case "HGCEERecHits_V1":
                        break;
                    case "HFRecHits_V2":
                        break;
                    case "HORecHits_V2":
                        break;
                    case "HERecHits_V2":
                        break;
                    case "HBRecHits_V2":
                        break;
                    case "HGCHEBRecHits_V1":
                        break;
                    case "HGCHEFRecHits_V1":
                        break;
                    case "Tracks_V1":
                        break;
                    case "Tracks_V2":
                        break;
                    case "Tracks_V3":
                        break;
                    case "Tracks_V4":
                        break;
                    case "TrackDets_V1":
                        break;
                    case "TrackingRecHits_V1":
                        break;
                    case "SiStripClusters_V1":
                        break;
                    case "SiPixelClusters_V1":
                        break;
                    case "DTRecHits_V1":
                        break;
                    case "DTRecSegment4D_V1":
                        break;
                    case "RPCRecHits_V1":
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
