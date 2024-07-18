using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGtoOBJGen;
using VR_Spy_Companion;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.VisualBasic;

namespace IGtoOBJGen {
    abstract class TypeConfig : ObjectData {
        abstract public string Execute();
        public List<ObjectData> ITEMDATA;
        public JObject JSON;
        public string name;
        public List<TrackExtrasData> EXTRAS;
    }
    class TrackerMuonV2 : TypeConfig {
        private List<TrackExtrasData> trackerMuonExtras;
        public JObject item;
        private List<TrackerMuonData> trackerMuonData;
        private string collection = "MuonTrackerExtras_V1";
        private string name = "TrackerMuons_V2";
        private string eventTitle;

        public TrackerMuonV2(JObject arg, string eventtitle) {
            JSON = arg;
            eventTitle = eventtitle;
        }
        public override string Execute() {
            /*Execution protocol        
              1. Parse trackerMuonExtras. DONE
              2. Create OBJ vector string arrays from extras DONE
              3. Write to file ...eventtitle/trackermuons_v2.obj DONE
              4. Parse trackerMuonData DONE
              5. return JObject of relevant data, to be appended to master JObject DONE
             */
            trackerMuonExtras = StaticLineHandlers.setExtras(JSON, "MuonTrackerExtras_V1");
            trackerMuonData = StaticLineHandlers.trackerMuonParse(JSON,2);
            GenerateTrackerMuonOBJ();
            string data = JsonConvert.SerializeObject(trackerMuonData);
            return ("\"trackerMuonDatas\": "+data);
        }
        public void GenerateTrackerMuonOBJ() {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(trackerMuonExtras, "TrackerMuons_V2");
            File.WriteAllText($"{eventTitle}\\TrackerMuons_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\TrackerMuons_V2.obj", dataList);
        }
    }
    class TrackerMuonV1 : TypeConfig
    {
        private List<List<double[]>> trackerMuonPoints;
        private List<TrackerMuonData> trackerMuonData;
        private string eventTitle;
        private string name = "TrackerMuons_V1";

        public TrackerMuonV1(JObject arg, string eventtitle)
        {
            JSON = arg;
            eventTitle = eventtitle;
            SetPoints();
        }
        public void SetPoints()
        {
            var assocsPoints = JSON["Associations"]["MuonTrackerPoints_V1"];
            trackerMuonPoints = StaticLineHandlers.makeTrackPoints("MuonTrackerPoints_V1", JSON);
        }
        public override string Execute()
        {
            /*Execution protocol

              1. Parse trackerMuonPoints. DONE
              2. Create OBJ vector string arrays from points DONE
              3. Write to file ...eventtitle/trackermuons_v2.obj DONE
              4. Parse trackerMuonData DONE
              5. return JObject of relevant data, to be appended to master JObject DONE

             */
            trackerMuonData = StaticLineHandlers.trackerMuonParse(JSON, 1);
            StaticLineHandlers.makeGeometryFromPoints(trackerMuonPoints, "TrackerMuons_V1", "TrackerMuons_V1", eventTitle); 
            string data = JsonConvert.SerializeObject(trackerMuonData);
            return ("\"trackerMuonDatas\": " + data);
        }
    }
    class TracksV1 : TypeConfig
    {
        private List<TrackExtrasData> trackExtras;
        private List<Track> trackData;
        private string eventTitle;
        public TracksV1(JObject arg, string eventtitle)
        {
            JSON = arg;
            eventTitle = eventtitle;
            SetExtras();
        }
        private void SetExtras()
        {
            var assocsExtras = JSON["Associations"]["TrackExtras_V1"];
            int firstAssoc = assocsExtras[0][1][1].Value<int>();
            int lastAssoc = assocsExtras.Last()[1][1].Value<int>();
            var extras = JSON["Collections"]["Extras_V1"].ToArray()[(firstAssoc)..(lastAssoc + 1)];
            foreach (var extra in extras)
            {
                TrackExtrasData currentItem = new TrackExtrasData();

                var children = extra.Children().Values<double>().ToArray();

                currentItem.pos1 = new double[3] { children[0], children[1], children[2] };

                double dir1mag = Math.Sqrt(  //dir1mag and dir2mag are for making sure the direction vectors are normalized
                    Math.Pow(children[3], 2) +
                    Math.Pow(children[4], 2) +
                    Math.Pow(children[5], 2)
                );
                currentItem.dir1 = new double[3] { children[3] / dir1mag, children[4] / dir1mag, children[5] / dir1mag };

                currentItem.pos2 = new double[3] { children[6], children[7], children[8] };

                double dir2mag = Math.Sqrt(
                    Math.Pow(children[9], 2) +
                    Math.Pow(children[10], 2) +
                    Math.Pow(children[11], 2)
                    );
                currentItem.dir2 = new double[3] { children[9] / dir2mag, children[10] / dir2mag, children[11] / dir2mag };

                double distance = Math.Sqrt(
                    Math.Pow((currentItem.pos1[0] - currentItem.pos2[0]), 2) +
                    Math.Pow(currentItem.pos1[1] - currentItem.pos2[1], 2) +
                    Math.Pow(currentItem.pos1[2] - currentItem.pos2[2], 2)
                     );

                double scale = distance * 0.25;

                currentItem.pos3 = new double[3] { children[0] + scale * currentItem.dir1[0], children[1] + scale * currentItem.dir1[1], children[2] + scale * currentItem.dir1[2] };
                currentItem.pos4 = new double[3] { children[6] - scale * currentItem.dir2[0], children[7] - scale * currentItem.dir2[1], children[8] - scale * currentItem.dir2[2] };

                trackExtras.Add(currentItem);
            }
        }
        public override string Execute()
        {
            trackData = StaticLineHandlers.trackDataParse(JSON, 1);
            GenerateTrackOBJ();
            string data = JsonConvert.SerializeObject(trackData);
            return ("\"trackDatas\": " + data);
            //return ("{\"trackDatas\":" + JsonConvert.SerializeObject(trackData) + "}");
        }
        private void GenerateTrackOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(trackExtras, "Tracks_V1");
            File.WriteAllText($"{eventTitle}\\Tracks_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\Tracks_V1.obj", dataList);
        }
    }
    class TracksV2 : TypeConfig
    {
        private List<TrackExtrasData> trackExtras;
        private List<Track> trackData;
        private string eventTitle;
        public TracksV2(JObject arg, string eventtitle)
        {
            JSON = arg;
            eventTitle = eventtitle;
            trackExtras = new List<TrackExtrasData>();
            SetExtras();
        }
        private void SetExtras()
        {
            var assocsExtras = JSON["Associations"]["TrackExtras_V1"];
            int firstAssoc = assocsExtras[0][1][1].Value<int>();
            int lastAssoc = assocsExtras.Last()[1][1].Value<int>();
            var extras = JSON["Collections"]["Extras_V1"].ToArray()[(firstAssoc)..(lastAssoc + 1)];
            foreach (var extra in extras)
            {
                TrackExtrasData currentItem = new TrackExtrasData();

                var children = extra.Children().Values<double>().ToArray();

                currentItem.pos1 = new double[3] { children[0], children[1], children[2] };

                double dir1mag = Math.Sqrt(  //dir1mag and dir2mag are for making sure the direction vectors are normalized
                    Math.Pow(children[3], 2) +
                    Math.Pow(children[4], 2) +
                    Math.Pow(children[5], 2)
                );
                currentItem.dir1 = new double[3] { children[3] / dir1mag, children[4] / dir1mag, children[5] / dir1mag };

                currentItem.pos2 = new double[3] { children[6], children[7], children[8] };

                double dir2mag = Math.Sqrt(
                    Math.Pow(children[9], 2) +
                    Math.Pow(children[10], 2) +
                    Math.Pow(children[11], 2)
                    );
                currentItem.dir2 = new double[3] { children[9] / dir2mag, children[10] / dir2mag, children[11] / dir2mag };

                double distance = Math.Sqrt(
                    Math.Pow((currentItem.pos1[0] - currentItem.pos2[0]), 2) +
                    Math.Pow(currentItem.pos1[1] - currentItem.pos2[1], 2) +
                    Math.Pow(currentItem.pos1[2] - currentItem.pos2[2], 2)
                     );

                double scale = distance * 0.25;

                currentItem.pos3 = new double[3] { children[0] + scale * currentItem.dir1[0], children[1] + scale * currentItem.dir1[1], children[2] + scale * currentItem.dir1[2] };
                currentItem.pos4 = new double[3] { children[6] - scale * currentItem.dir2[0], children[7] - scale * currentItem.dir2[1], children[8] - scale * currentItem.dir2[2] };
                
                trackExtras.Add(currentItem);
            }
        }
        public override string Execute()
        {
            trackData = StaticLineHandlers.trackDataParse(JSON, 2);
            GenerateTrackOBJ();
            string data = JsonConvert.SerializeObject(trackData);
            return ("\"trackDatas\":" + data);
        }
        private void GenerateTrackOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(trackExtras, "Tracks");
            File.WriteAllText($"{eventTitle}\\Tracks_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\Tracks_V2.obj", dataList);
        }
    }
    class TracksV3 : TypeConfig
    {
        private List<TrackExtrasData> trackExtras = new List<TrackExtrasData>();
        private List<Track> trackData;
        private string eventTitle;
        public TracksV3(JObject arg, string eventtitle)
        {
            JSON = arg;
            eventTitle = eventtitle;
            SetExtras();
        }
        private void SetExtras()
        {
            var assocsExtras = JSON["Associations"]["TrackExtras_V1"];
            int firstAssoc = assocsExtras[0][1][1].Value<int>();
            int lastAssoc = assocsExtras.Last()[1][1].Value<int>();
            var extras = JSON["Collections"]["Extras_V1"].ToArray()[(firstAssoc)..(lastAssoc + 1)];
            foreach (var extra in extras)
            {
                TrackExtrasData currentItem = new TrackExtrasData();

                var children = extra.Children().Values<double>().ToArray();

                currentItem.pos1 = new double[3] { children[0], children[1], children[2] };

                double dir1mag = Math.Sqrt(  //dir1mag and dir2mag are for making sure the direction vectors are normalized
                    Math.Pow(children[3], 2) +
                    Math.Pow(children[4], 2) +
                    Math.Pow(children[5], 2)
                );
                currentItem.dir1 = new double[3] { children[3] / dir1mag, children[4] / dir1mag, children[5] / dir1mag };

                currentItem.pos2 = new double[3] { children[6], children[7], children[8] };

                double dir2mag = Math.Sqrt(
                    Math.Pow(children[9], 2) +
                    Math.Pow(children[10], 2) +
                    Math.Pow(children[11], 2)
                    );
                currentItem.dir2 = new double[3] { children[9] / dir2mag, children[10] / dir2mag, children[11] / dir2mag };

                double distance = Math.Sqrt(
                    Math.Pow((currentItem.pos1[0] - currentItem.pos2[0]), 2) +
                    Math.Pow(currentItem.pos1[1] - currentItem.pos2[1], 2) +
                    Math.Pow(currentItem.pos1[2] - currentItem.pos2[2], 2)
                     );

                double scale = distance * 0.25;

                currentItem.pos3 = new double[3] { children[0] + scale * currentItem.dir1[0], children[1] + scale * currentItem.dir1[1], children[2] + scale * currentItem.dir1[2] };
                currentItem.pos4 = new double[3] { children[6] - scale * currentItem.dir2[0], children[7] - scale * currentItem.dir2[1], children[8] - scale * currentItem.dir2[2] };

                trackExtras.Add(currentItem);
            }
        }
        public override string Execute()
        {
            trackData = StaticLineHandlers.trackDataParse(JSON, 3);
            GenerateTrackOBJ(); 
            string data = JsonConvert.SerializeObject(trackData);
            return ("\"trackDatas\":" + data);
        }
        private void GenerateTrackOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(trackExtras, "Tracks_V3");
            File.WriteAllText($"{eventTitle}\\Tracks_V3.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\Tracks_V3.obj", dataList);
        }
    }
    class TracksV4 : TypeConfig
    {
        private List<TrackExtrasData> trackExtras;
        private List<Track> trackData;
        private string eventTitle;
        public TracksV4(JObject arg, string eventtitle)
        {
            JSON = arg;
            eventTitle = eventtitle;
            SetExtras();
        }
        private void SetExtras()
        {
            var assocsExtras = JSON["Associations"]["TrackExtras_V1"];
            int firstAssoc = assocsExtras[0][1][1].Value<int>();
            int lastAssoc = assocsExtras.Last()[1][1].Value<int>();
            var extras = JSON["Collections"]["Extras_V1"].ToArray()[(firstAssoc)..(lastAssoc + 1)];
            foreach (var extra in extras)
            {
                TrackExtrasData currentItem = new TrackExtrasData();

                var children = extra.Children().Values<double>().ToArray();

                currentItem.pos1 = new double[3] { children[0], children[1], children[2] };

                double dir1mag = Math.Sqrt(  //dir1mag and dir2mag are for making sure the direction vectors are normalized
                    Math.Pow(children[3], 2) +
                    Math.Pow(children[4], 2) +
                    Math.Pow(children[5], 2)
                );
                currentItem.dir1 = new double[3] { children[3] / dir1mag, children[4] / dir1mag, children[5] / dir1mag };

                currentItem.pos2 = new double[3] { children[6], children[7], children[8] };

                double dir2mag = Math.Sqrt(
                    Math.Pow(children[9], 2) +
                    Math.Pow(children[10], 2) +
                    Math.Pow(children[11], 2)
                    );
                currentItem.dir2 = new double[3] { children[9] / dir2mag, children[10] / dir2mag, children[11] / dir2mag };

                double distance = Math.Sqrt(
                    Math.Pow((currentItem.pos1[0] - currentItem.pos2[0]), 2) +
                    Math.Pow(currentItem.pos1[1] - currentItem.pos2[1], 2) +
                    Math.Pow(currentItem.pos1[2] - currentItem.pos2[2], 2)
                     );

                double scale = distance * 0.25;

                currentItem.pos3 = new double[3] { children[0] + scale * currentItem.dir1[0], children[1] + scale * currentItem.dir1[1], children[2] + scale * currentItem.dir1[2] };
                currentItem.pos4 = new double[3] { children[6] - scale * currentItem.dir2[0], children[7] - scale * currentItem.dir2[1], children[8] - scale * currentItem.dir2[2] };

                trackExtras.Add(currentItem);
            }
        }
        public override string Execute()
        {
            trackData = StaticLineHandlers.trackDataParse(JSON, 4);
            GenerateTrackOBJ(); 
            string data = JsonConvert.SerializeObject(trackData);
            return ("\"trackDatas\":" + data);
        }
        private void GenerateTrackOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(trackExtras, "Tracks_V4");
            File.WriteAllText($"{eventTitle}\\Tracks_V4.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\Tracks_V4.obj", dataList);
        }
    }
    class TrackDets_V1 : TypeConfig
    {
        private List<TrackerPieceData> trackerPieceData;
        private string eventTitle;
        public TrackDets_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            trackerPieceData = StaticBoxHandlers.trackerPieceParse(JSON, "TrackDets_V1");
            List<string> dataList = StaticBoxHandlers.generateTrackerPiece(trackerPieceData);
            File.WriteAllText($"{eventTitle}\\TrackDets_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\TrackDets_V1.obj", dataList);
            string data = JsonConvert.SerializeObject(trackerPieceData);
            //string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
            //File.WriteAllLines(Path.Combine(downloadsPath, "TrackDets_V1.obj"), dataList);
            return ("\"trackDetsV1Data\":" + data);
        }
    }
    class TrackingRecHits_V1 : TypeConfig
    {
        private List<trackingPoint> trackingRecHitData;
        private string eventTitle;
        public TrackingRecHits_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            trackingRecHitData = StaticLineHandlers.trackingpointParse(JSON, "TrackingRecHits_V1", 0);
            List<string> dataList = StaticLineHandlers.generatetrackingPoints(trackingRecHitData);
            File.WriteAllText($"{eventTitle}\\TrackingRecHits_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\TrackingRecHits_V1", dataList);
            string data = JsonConvert.SerializeObject(trackingRecHitData);
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
            File.WriteAllLines(Path.Combine(downloadsPath, "TrackingRecHits_V1.obj"), dataList);
            return ("\"TrackingRecHits_V1Data\":" + data);
        }
    }
    class SiStripClusters_V1 : TypeConfig
    {
        private List<trackingPoint> sistripClusterData;
        private string eventTitle;
        public SiStripClusters_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            sistripClusterData = StaticLineHandlers.trackingpointParse(JSON, "SiStripClusters_V1", 1);
            List<string> dataList = StaticLineHandlers.generatetrackingPoints(sistripClusterData);
            File.WriteAllText($"{eventTitle}\\SiStripClusters_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\SiStripClusters_V1", dataList);
            string data = JsonConvert.SerializeObject(sistripClusterData);
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
            File.WriteAllLines(Path.Combine(downloadsPath, "SiStripClusters_V1.obj"), dataList);
            return ("\"SiStripClusters_V1Data\":" + data);
        }
    }
    class SiPixelClusters_V1 : TypeConfig
    {
        private List<trackingPoint> sipixelClusterData;
        private string eventTitle;
        public SiPixelClusters_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            sipixelClusterData = StaticLineHandlers.trackingpointParse(JSON, "SiPixelClusters_V1", 1);
            List<string> dataList = StaticLineHandlers.generatetrackingPoints(sipixelClusterData);
            File.WriteAllText($"{eventTitle}\\SiPixelClusters_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\SiPixelClusters_V1", dataList);
            string data = JsonConvert.SerializeObject(sipixelClusterData);
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
            File.WriteAllLines(Path.Combine(downloadsPath, "SiPixelClusters_V1.obj"), dataList);
            return ("\"SiPixelClusters_V1Data\":" + data);
        }
    }
    class SuperClusters_V1 : TypeConfig
    {
        private List<SuperCluster> superClusterData;
        private List<List<RecHitFraction>> recHits;
        private List<RecHitFraction> recHitFractionDatas;
        private string eventTitle;
        public SuperClusters_V1(JObject args, string eventtitle)
        {
            JSON = args;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            recHitFractionDatas = StaticBoxHandlers.recHitFractionsParse(JSON);
            assignRecHitFractions();
            superClusterData = StaticBoxHandlers.superClusterParse(JSON);
            StaticBoxHandlers.GenerateSuperClusters(recHits, eventTitle);
            string data = JsonConvert.SerializeObject(superClusterData);
            return ("\"superClusters\":" + data);
        }
        private void assignRecHitFractions()
        {
            var dataList = new List<List<RecHitFraction>>();
            var indexer = 0;
            foreach (var item in JSON["Associations"]["SuperClusterRecHitFractions_V1"])
            {
                int index = item[0][1].Value<int>();
                if (dataList.Count() < index + 1)
                {
                    List<RecHitFraction> h = new List<RecHitFraction>();
                    dataList.Add(h);
                }
                dataList[index].Add(recHitFractionDatas[indexer]);
                indexer++;
            }
            recHits = dataList;
        }
    }
    /*class EEDigis_V1 : TypeConfig {

    }*/
    class EERecHits_V2 : TypeConfig
    {
        private List<CalorimetryTowers> caloTowerData;
        private string eventTitle;

        public EERecHits_V2(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            caloTowerData = StaticBoxHandlers.genericCaloParse(JSON, "EERecHits_V2");
            caloTowerData = StaticBoxHandlers.setCaloScale(caloTowerData);
            List<string> dataList = new List<string>();
            (List<string> strings, List<CalorimetryTowers> caloData) item = StaticBoxHandlers.generateCalorimetryBoxes(caloTowerData);
            dataList = item.strings; caloTowerData = item.caloData;
            File.WriteAllText($"{eventTitle}\\EERecHits_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\EERecHits_V2.obj", dataList);
            string data = JsonConvert.SerializeObject(caloTowerData);
            return ("\"eeHitDatas\":" + data);
            //return JsonConvert.SerializeObject(caloTowerData);
        }

    }
    /*class EBDigis_V1 : TypeConfig {

        public override string Execute()
        {
            throw new NotImplementedException()
        } 
    }*/
    class EBRecHits_V2 : TypeConfig
    {
        private List<CalorimetryTowers> caloTowerData;
        private string eventTitle;
        public EBRecHits_V2(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            caloTowerData = StaticBoxHandlers.genericCaloParse(JSON, "EBRecHits_V2");
            caloTowerData = StaticBoxHandlers.setCaloScale(caloTowerData);
            List<string> dataList = new List<string>();
            (List<string> strings, List<CalorimetryTowers> caloData) item = StaticBoxHandlers.generateCalorimetryBoxes(caloTowerData);
            dataList = item.strings; caloTowerData = item.caloData;
            File.WriteAllText($"{eventTitle}\\EBRecHits_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\EBRecHits_V2.obj", dataList);
            string data = JsonConvert.SerializeObject(caloTowerData);
            return ("\"ebHitDatas\":" + data);
        }
    }
    class ESRecHits_V2 : TypeConfig
    {
        private List<CalorimetryTowers> caloTowerData;
        private string eventTitle;
        public ESRecHits_V2(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            caloTowerData = StaticBoxHandlers.genericCaloParse(JSON, "ESRecHits_V2");
            caloTowerData = StaticBoxHandlers.setCaloScale(caloTowerData); 
            List<string> dataList;
            (List<string> strings, List<CalorimetryTowers> caloData) item = StaticBoxHandlers.generateCalorimetryBoxes(caloTowerData);
            dataList = item.strings; caloTowerData = item.caloData;
            File.WriteAllText($"{eventTitle}\\ESRecHits_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\ESRecHits_V2.obj", dataList);
            string data = JsonConvert.SerializeObject(caloTowerData);
            return ("\"esHitDatas\":" + data);
        }
    }
    /*class HGCEERecHits_V1 : TypeConfig {
     private List<CalorimetryTowers> caloTowerData;
     public override string Execute()
     {
         return JsonConvert.SerializeObject(caloTowerData);
     }
 }*/
    class HFRecHits_V2 : TypeConfig
    {
        private List<CalorimetryTowers> caloTowerData;
        private string eventTitle;
        public HFRecHits_V2(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            caloTowerData = StaticBoxHandlers.genericCaloParse(JSON, "HFRecHits_V2");
            caloTowerData = StaticBoxHandlers.setCaloScale(caloTowerData);
            List<string> dataList = new List<string>();
            (List<string> strings, List<CalorimetryTowers> caloData) item = StaticBoxHandlers.generateCalorimetryTowers(caloTowerData);
            dataList = item.strings; caloTowerData = item.caloData;
            File.WriteAllText($"{eventTitle}\\HFRecHits_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\HFRecHits_V2.obj", dataList);
            string data = JsonConvert.SerializeObject(caloTowerData);
            return ("\"hfHitDatas\":" + data);
        }
    }
    class HERecHits_V2 : TypeConfig
    {
        private List<CalorimetryTowers> caloTowerData;
        private string eventTitle;
        public HERecHits_V2(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            caloTowerData = StaticBoxHandlers.genericCaloParse(JSON, "HERecHits_V2");
            caloTowerData = StaticBoxHandlers.setCaloScale(caloTowerData);
            List<string> dataList = new List<string>();
            (List<string> strings, List<CalorimetryTowers> caloData) item = StaticBoxHandlers.generateCalorimetryTowers(caloTowerData);
            dataList = item.strings; caloTowerData = item.caloData;
            File.WriteAllText($"{eventTitle}\\HERecHits_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\HERecHits_V2.obj", dataList);
            string data = JsonConvert.SerializeObject(caloTowerData);
            return ("\"heHitDatas\":" + data);
        }
    }
    class HORecHits_V2 : TypeConfig
    {
        private List<CalorimetryTowers> caloTowerData;
        private string eventTitle;
        public HORecHits_V2(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            caloTowerData = StaticBoxHandlers.genericCaloParse(JSON, "HORecHits_V2");
            caloTowerData = StaticBoxHandlers.setCaloScale(caloTowerData);
            List<string> dataList = new List<string>();
            (List<string> strings, List<CalorimetryTowers> caloData) item = StaticBoxHandlers.generateCalorimetryTowers(caloTowerData);
            dataList = item.strings; caloTowerData = item.caloData;
            File.WriteAllText($"{eventTitle}\\HORecHits_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\HORecHits_V2.obj", dataList);
            string data = JsonConvert.SerializeObject(caloTowerData);
            return ("\"hoHitDatas\":" + data);
        }
    }
    class HBRecHits_V2 : TypeConfig
    {
        private List<CalorimetryTowers> caloTowerData;
        private string eventTitle;
        public HBRecHits_V2(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            caloTowerData = StaticBoxHandlers.genericCaloParse(JSON, "HBRecHits_V2");
            caloTowerData = StaticBoxHandlers.setCaloScale(caloTowerData);
            List<string> dataList = new List<string>();
            (List<string> strings, List<CalorimetryTowers> caloData) item = StaticBoxHandlers.generateCalorimetryTowers(caloTowerData);
            dataList = item.strings; caloTowerData = item.caloData;
            File.WriteAllText($"{eventTitle}\\HBRecHits_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\HBRecHits_V2.obj", dataList);
            string data = JsonConvert.SerializeObject(caloTowerData);
            return ("\"hbHitDatas\":" + data);
        }
    }
    /*class HGCHEBRecHits_V1 : TypeConfig {
     private List<CalorimetryTowers> caloTowerData;
     public override string Execute()
     {
         return JsonConvert.SerializeObject(caloTowerData);
     }
 }
	class HGCHEFRecHits_V1 : TypeConfig {
     private List<CalorimetryTowers> caloTowerData;
     public override string Execute()
     {
         return JsonConvert.SerializeObject(caloTowerData);
     }
 }
 class TrackDets_V1 : TypeConfig { }
 class TrackingRecHits_V1 : TypeConfig { }
 class SiStripClusters_V1 : TypeConfig { }
 class SiPixelClusters_V1 : TypeConfig { }*/
    class Event_V1 : TypeConfig
    {
        public Event_V1(JObject args, string eventtitle)
        {
            JSON = args;
        }
        public override string Execute()
        {
            return JsonConvert.SerializeObject(JSON["Collections"]["Event_V1"]);
        }
    }
    class Event_V2 : TypeConfig
    {
        public Event_V2(JObject args, string eventtitle)
        {
            JSON = args;
        }
        public override string Execute()
        {
            return JsonConvert.SerializeObject(JSON["Collections"]["Event_V2"]);
        }
    }
    class Event_V3 : TypeConfig
    {
        public Event_V3(JObject args, string eventtitle)
        {
            JSON = args;
        }
        public override string Execute()
        {
            return JsonConvert.SerializeObject(JSON["Collections"]["Event_V3"]);
        }
    }
    /*class DTRecHits_V1 : TypeConfig { }
    class DTRecSegment4D_V1 : TypeConfig { }*/
    class RPCRecHits_V1 : TypeConfig {
        private List<RPCRecHit> rpcRecHits;
        private string eventTitle;
        public RPCRecHits_V1(JObject args, string eventtitle)
        {
            JSON = args;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            rpcRecHits = StaticBoxHandlers.RPCRecHitParse(JSON);
            
            List<string> dataList = StaticBoxHandlers.GenerateRPCRecHits(rpcRecHits);
            File.WriteAllText($"{eventTitle}\\RPCRecHits_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\RPCRecHits_V1.obj", dataList);
            string data = JsonConvert.SerializeObject(rpcRecHits);
            return ("\"RPCRecHitDatas\":" + data);
        }
    }
    /*class MatchingGEMs_V1 : TypeConfig { }
    class GEMDigis_V2 : TypeConfig { }
    class GEMRecHits_V2 : TypeConfig { }
    class GEMSegments_V1 : TypeConfig { }
    class GEMSegments_V2 : TypeConfig { }
    class GEMSegments_V3 : TypeConfig { }
    class CSCStripDigis_V1 : TypeConfig { }
    class CSCWireDigis_V1 : TypeConfig { }
    class CSCStripDigis_V2 : TypeConfig { }
    class CSCWireDigis_V2 : TypeConfig { }
    class CSCLCTDigis_V1 : TypeConfig { }
    class CSCCorrelatedLCTDigis_V2 : TypeConfig { }*/
    class MatchingCSCs_V1 : TypeConfig
    {
        private List<matchingCSC> matchingCSCData;
        private string eventTitle;
        public MatchingCSCs_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            matchingCSCData = StaticBoxHandlers.matchingCSCParse(JSON, "MatchingCSCs_V1");
            List<string> dataList = StaticBoxHandlers.generateMatchingCSC(matchingCSCData);
            File.WriteAllText($"{eventTitle}\\MatchingCSCs_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\MatchingCSCs_V1", dataList);
            string data = JsonConvert.SerializeObject(matchingCSCData);
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
            File.WriteAllLines(Path.Combine(downloadsPath, "MatchingCSCs_V1.obj"), dataList);
            return ("\"MatchingCSCs_V1\":" + data);
        }
    }
    /*class CSCRecHit2Ds_V2 : TypeConfig { }*/
    class CSCSegments_V1 : TypeConfig {
        private List<CSCSegment> cscSegments;
        private string eventTitle;
        public CSCSegments_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            cscSegments = StaticBoxHandlers.ParseCSCSegmentsV1(JSON);
            var strings = StaticBoxHandlers.GenerateCSCSegments(cscSegments, 1);
            File.WriteAllText($"{eventTitle}\\CSCSegments_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\CSCSegments_V1.obj", strings);
            string data = JsonConvert.SerializeObject(cscSegments);
            return ("\"CSCSegmentV1Datas\":" + data);
        }
    }
    class CSCSegments_V2 : TypeConfig
    {
        private List<cscSegmentV2> cscSegmentData;
        private string eventTitle;
        public CSCSegments_V2(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            cscSegmentData = StaticLineHandlers.cscSegmentParse(JSON, "CSCSegments_V2");
            List<string> dataList = StaticLineHandlers.generateCSCSegment(cscSegmentData);
            File.WriteAllText($"{eventTitle}\\CSCSegments_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\CSCSegments_V2", dataList);
            string data = JsonConvert.SerializeObject(cscSegmentData);
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
            File.WriteAllLines(Path.Combine(downloadsPath, "CSCSegments_V2.obj"), dataList);
            return ("\"CSCSegments_V2\":" + data);
        }
    }
    //class CSCSegments_V3 : TypeConfig { }
    class MuonChambers_V1 : TypeConfig
    {
        private List<MuonChamberData> muonChamberData;
        private string eventTitle;
        public MuonChambers_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            muonChamberData = StaticBoxHandlers.muonChamberParse(JSON);
            StaticBoxHandlers.generateMuonChamberModels(muonChamberData, eventTitle);
            string data = JsonConvert.SerializeObject(muonChamberData);
            return ("\"muonChamberDatas\":" + data);
            //return JsonConvert.SerializeObject(muonChamberData);
        }
    }
    /*class CaloTowers_V2 : TypeConfig { }
    class METs_V1 : TypeConfig { }*/
    class PFMETs_V1 : TypeConfig
    {
        private METData met;
        private string eventTitle;
        public PFMETs_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            met = StaticLineHandlers.METParse(JSON);
            return ("\"PFMETs_V1\":" +JsonConvert.SerializeObject(met));
        }
    }
    /*class PATMETs_V1 : TypeConfig { }
    class Jets_V1 : TypeConfig { }*/
    class PFJets_V1 : TypeConfig
    {
        private List<JetV1Data> jetDatas;
        private string eventTitle;
        public PFJets_V1(JObject args, string eventtitle)
        {
            JSON = args;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            jetDatas = StaticBoxHandlers.jetV1Parse(JSON);
            StaticBoxHandlers.generateJetModels(jetDatas, eventTitle);
            string data = JsonConvert.SerializeObject(jetDatas);
            return ("\"jetDatas\":" + data);
            //return JsonConvert.SerializeObject(jetDatas);
        }
    }
    class PFJets_V2 : TypeConfig
    {
        private List<JetV2Data> jetDatas;
        private string eventTitle;
        public PFJets_V2(JObject args, string eventtitle)
        {
            JSON = args;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            jetDatas = StaticBoxHandlers.jetV2Parse(JSON);
            StaticBoxHandlers.generateJetModels(jetDatas, eventTitle);
            string data = JsonConvert.SerializeObject(jetDatas);
            return ("\"jetDatas\":" + data);
        }
    }
    /* class GenJets_V1 : TypeConfig { }
     class PATJets_V1 : TypeConfig { }*/
     class Photons_V1 : TypeConfig {
        private List<PhotonData> photons;
        private string eventTitle;
        public Photons_V1(JObject args, string eventtitle)
        {
            JSON = args;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            photons = StaticLineHandlers.photonParse(JSON);
            List<string> dataList = new List<string>();
            foreach(var item in photons)
            {
                dataList.Add(StaticLineHandlers.makePhoton(item));
            }
            File.WriteAllText($"{eventTitle}\\Photons_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\Photons_V1.obj", dataList);
            string data = JsonConvert.SerializeObject(photons);
            return ("\"photonDatas\":" + data);
        }
    }
    //class PATPhotons_V1 : TypeConfig { }
    class GlobalMuons_V1 : TypeConfig
    {
        private List<GlobalMuonData> globalMuons;
        private List<List<double[]>> points;
        private string eventTitle;
        private string collection = "MuonGlobalPoints_V1";
        public GlobalMuons_V1(JObject args, string eventtitle)
        {
            JSON = args;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            globalMuons = StaticLineHandlers.globalMuonParse(JSON, 1);
            points = StaticLineHandlers.makeTrackPoints(collection, JSON);
            StaticLineHandlers.makeGeometryFromPoints(points, "GlobalMuons_V1", "GlobalMuons_V1", eventTitle);
            string data = JsonConvert.SerializeObject(globalMuons);
            return ("\"globalMuonDatas\":" + data);
            //return JsonConvert.SerializeObject(globalMuons);
        }
    }
    class GlobalMuons_V2 : TypeConfig
    {
        private List<List<double[]>> points;
        private List<GlobalMuonData> globalMuons;
        private string assocation = "MuonGlobalPoints_V1";
        private string eventTitle;
        public GlobalMuons_V2(JObject args, string eventtitle)
        {
            JSON = args;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            globalMuons = StaticLineHandlers.globalMuonParse(JSON, 2);
            points = StaticLineHandlers.makeTrackPoints(assocation, JSON);
            StaticLineHandlers.makeGeometryFromPoints(points, "GlobalMuons_V2", "GlobalMuons_V2", eventTitle);
            string data = JsonConvert.SerializeObject(globalMuons);
            return ("\"globalMuonDatas\":" + data);
        }
    }
    /*class PATGlobalMuons_V1 : TypeConfig {

    }*/
    class StandaloneMuons_V1 : TypeConfig
    {
        private List<List<double[]>> points;
        private List<StandaloneMuonData> standaloneMuonData;
        private string collection = "MuonStandalonePoints_V1";
        private string eventTitle;

        public StandaloneMuons_V1(JObject arg, string eventtitle)
        {
            JSON = arg;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            standaloneMuonData = StaticLineHandlers.standaloneMuonParse(JSON, 1);
            points = StaticLineHandlers.makeTrackPoints(collection, JSON);
            StaticLineHandlers.makeGeometryFromPoints(points, "StandaloneMuons_V1", "StandaloneMuons_V1", eventTitle);
            string data = JsonConvert.SerializeObject(standaloneMuonData);
            return ("\"standaloneMuonDatas\":" + data);
            //return JsonConvert.SerializeObject(standaloneMuonData);
        }
    }
    class StandaloneMuons_V2 : TypeConfig
    {
        private List<TrackExtrasData> extras;
        private string eventTitle;
        private List<StandaloneMuonData> standaloneMuons;
        private string association = "MuonTrackExtras_V1";
        public StandaloneMuons_V2(JObject arg, string eventtitle)
        {
            JSON = arg;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            standaloneMuons = StaticLineHandlers.standaloneMuonParse(JSON, 2);
            extras = StaticLineHandlers.setExtras(JSON, association);
            GenerateStandaloneMuonOBJ(); 
            string data = JsonConvert.SerializeObject(standaloneMuons);
            return ("\"standaloneMuonDatas\":" + data);
        }
        public void GenerateStandaloneMuonOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(extras, "StandaloneMuons");
            File.WriteAllText($"{eventTitle}\\StandaloneMuons_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\StandaloneMuons_V2.obj", dataList);
        }
    }
    /*class PATStandaloneMuons_V1 : TypeConfig { }
    class PATTrackerMuons_V1 : TypeConfig { }
    class PATTrackerMuons_V2 : TypeConfig { }*/
    class GsfElectrons_V1 : TypeConfig
    {
        private List<GsfElectron> gsfElectrons;
        private List<TrackExtrasData> extras;
        private string association = "GsfElectronExtras_V1";
        private string eventTitle;
        public GsfElectrons_V1(JObject args, string eventtitle)
        {
            JSON = args;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            gsfElectrons = StaticLineHandlers.electronParse(JSON, 1);
            extras = StaticLineHandlers.setExtras(JSON, association);
            GenerateElectronOBJ();
            string data = JsonConvert.SerializeObject(gsfElectrons);
            return ("\"electronDatas\":" + data);
        }
        public void GenerateElectronOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(extras, "GsfElectrons");
            File.WriteAllText($"{eventTitle}\\GsfElectrons_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\GsfElectrons_V1.obj", dataList);
        }
    }
    class GsfElectrons_V2 : TypeConfig
    {
        private List<GsfElectron> gsfElectrons;
        private List<TrackExtrasData> extras;
        private string association = "GsfElectronExtras_V1";
        private string eventTitle;
        public GsfElectrons_V2(JObject args, string eventtitle)
        {
            JSON = args;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            gsfElectrons = StaticLineHandlers.electronParse(JSON, 2);
            extras = StaticLineHandlers.setExtras(JSON, association);
            GenerateElectronOBJ();
            string data = JsonConvert.SerializeObject(gsfElectrons);
            return ("\"electronDatas\":" + data);
        }
        public void GenerateElectronOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(extras, "GsfElectrons");
            File.WriteAllText($"{eventTitle}\\GsfElectrons_V2.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\GsfElectrons_V2.obj", dataList);
        }
    }
    class GsfElectrons_V3 : TypeConfig
    {
        private List<GsfElectron> gsfElectrons;
        private List<TrackExtrasData> extras;
        private string association = "GsfElectronExtras_V1";
        private string eventTitle;
        public GsfElectrons_V3(JObject args, string eventtitle)
        {
            JSON = args;
            eventTitle = eventtitle;
        }
        public override string Execute()
        {
            gsfElectrons = StaticLineHandlers.electronParse(JSON, 3);
            extras = StaticLineHandlers.setExtras(JSON, association);
            GenerateElectronOBJ();
            string data = JsonConvert.SerializeObject(gsfElectrons);
            return ("\"electronDatas\":" + data);
        }
        public void GenerateElectronOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(extras, "GsfElectrons");
            File.WriteAllText($"{eventTitle}\\GsfElectrons_V3.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\GsfElectrons_V3.obj", dataList);
        }
    }
    /*class PATElectrons_V1 : TypeConfig { }
    class ForwardProtons_V1 : TypeConfig { }*/
    class Vertices_V1 : TypeConfig
    {
        private List<Vertex> vertexDatas;
        private string eventTitle;
        public Vertices_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            vertexDatas = StaticBoxHandlers.vertexParse(JSON, "Vertices_V1");
            GenerateVertexOBJ();
            string data = JsonConvert.SerializeObject(vertexDatas);
            return ("\"vertexDatas\":" + data);
        }
        private void GenerateVertexOBJ()
        {
            StaticBoxHandlers.GenerateEllipsoidObj($@"{eventTitle}\Vertices_V1.obj", vertexDatas, 3.0);
        }
    }
    class PrimaryVertices_V1 : TypeConfig
    {
        private List<Vertex> primaryVertexDatas;
        private string eventTitle;
        public PrimaryVertices_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            primaryVertexDatas = StaticBoxHandlers.vertexParse(JSON, "PrimaryVertices_V1");
            GenerateVertexOBJ();
            string data = JsonConvert.SerializeObject(primaryVertexDatas);
            return ("\"primaryVertexDatas\":" + data);
        }
        private void GenerateVertexOBJ()
        {
            StaticBoxHandlers.GenerateEllipsoidObj($@"{eventTitle}\PrimaryVertices_V1.obj", primaryVertexDatas, 3.0);
        }
    }
    class SecondaryVertices_V1 : TypeConfig
    {
        private List<Vertex> secondaryVertexDatas;
        private string eventTitle;
        public SecondaryVertices_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            secondaryVertexDatas = StaticBoxHandlers.vertexParse(JSON, "SecondaryVertices_V1");
            GenerateVertexOBJ();
            string data = JsonConvert.SerializeObject(secondaryVertexDatas);
            return ("\"secondaryVertexDatas\":" + data);
        }
        private void GenerateVertexOBJ()
        {
            StaticBoxHandlers.GenerateEllipsoidObj($@"{eventTitle}\SecondaryVertices_V1.obj", secondaryVertexDatas, 3.0);
        }
    }
    /*class VertexCompositeCandidates_V1 : TypeConfig { }
    class SimVertices_V1 : TypeConfig { }*/
}