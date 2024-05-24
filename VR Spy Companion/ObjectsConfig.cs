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

namespace IGtoOBJGen {
    abstract class TypeConfig : ObjectData {
        abstract public string Execute();
        public List<ObjectData> ITEMDATA;
        public JObject JSON;
        public List<TrackExtrasData> EXTRAS;
    }
    class TrackerMuonV2 : TypeConfig {
        private List<TrackExtrasData> trackerMuonExtras;
        private List<TrackerMuonData> trackerMuonData;
        private string collection = "MuonTrackerExtras_V1";
        private string name = "TrackerMuons_V2";
        private string eventTitle;

        public TrackerMuonV2(JObject arg, string eventtitle) {
            JSON = arg;
            eventTitle = eventtitle;
            SetExtras();
        }
        public override string Execute() {
            /*Execution protocol
             
              1. Parse trackerMuonExtras. DONE
              2. Create OBJ vector string arrays from extras DONE
              3. Write to file ...eventtitle/trackermuons_v2.obj DONE
              4. Parse trackerMuonData DONE
              5. return JObject of relevant data, to be appended to master JObject DONE
              
             */

            trackerMuonData = StaticLineHandlers.trackerMuonParse(JSON,2);
            GenerateTrackerMuonOBJ();
            return JsonConvert.SerializeObject(trackerMuonData);
        }
        public void SetExtras() {
            var assocsExtras = JSON["Associations"]["MuonTrackerExtras_V1"];
            int firstAssoc = assocsExtras[0][1][1].Value<int>();
            int lastAssoc = assocsExtras.Last()[1][1].Value<int>();
            var extras = JSON["Collections"]["Extras_V1"].ToArray()[(firstAssoc)..(lastAssoc + 1)];
            foreach (var extra in extras) {
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

                trackerMuonExtras.Add(currentItem);
            }
        }
        public void GenerateTrackerMuonOBJ() {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(trackerMuonExtras, "TrackerMuons");
            File.WriteAllText($"{eventTitle}\\1_TrackerMuons.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\1_TrackerMuons.obj", dataList);
        }
    }
    class TrackerMuonV1 : TypeConfig {
        private List<List<double[]>> trackerMuonPoints;
        private List<TrackerMuonData> trackerMuonData;
        private string eventTitle;
        private string name = "TrackerMuons_V1";

        public TrackerMuonV1(JObject arg, string eventtitle) {
            JSON = arg;
            eventTitle = eventtitle;
            SetPoints();
        }
        public void SetPoints() {
            var assocsPoints = JSON["Associations"]["MuonTrackerPoints_V1"];
            trackerMuonPoints = StaticLineHandlers.makeTrackPoints(assocsPoints,JSON);
        }
        public override string Execute() {
            /*Execution protocol
             
              1. Parse trackerMuonPoints. DONE
              2. Create OBJ vector string arrays from points DONE
              3. Write to file ...eventtitle/trackermuons_v2.obj DONE
              4. Parse trackerMuonData DONE
              5. return JObject of relevant data, to be appended to master JObject DONE
              
             */
            trackerMuonData = StaticLineHandlers.trackerMuonParse(JSON, 1);
            StaticLineHandlers.makeGeometryFromPoints(trackerMuonPoints, "3_TrackerMuons", "TrackerMuons", eventTitle);
            return JsonConvert.SerializeObject(trackerMuonData);
        }
    }
    class TracksV1 : TypeConfig {
        private List<TrackExtrasData> trackExtras;
        private List<Track> trackData;
        private string eventTitle;
        public TracksV1(JObject arg, string eventtitle) {
            JSON = arg;
            eventTitle = eventtitle;
            SetExtras();
        }
        private void SetExtras() {
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
        public override string Execute() {
            trackData = StaticLineHandlers.trackDataParse(JSON, 1);
            GenerateTrackOBJ();
            return JsonConvert.SerializeObject(trackData);
        }
        private void GenerateTrackOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(trackExtras, "Tracks");
            File.WriteAllText($"{eventTitle}\\9_Tracks.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\9_Tracks.obj", dataList);
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
            return JsonConvert.SerializeObject(trackData);
        }
        private void GenerateTrackOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(trackExtras, "Tracks");
            File.WriteAllText($"{eventTitle}\\9_Tracks.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\9_Tracks.obj", dataList);
        }
    }
    class TracksV3 : TypeConfig
    {
        private List<TrackExtrasData> trackExtras;
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
            return JsonConvert.SerializeObject(trackData);
        }
        private void GenerateTrackOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(trackExtras, "Tracks");
            File.WriteAllText($"{eventTitle}\\9_Tracks.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\9_Tracks.obj", dataList);
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
            return JsonConvert.SerializeObject(trackData);
        }
        private void GenerateTrackOBJ()
        {
            List<string> dataList = StaticLineHandlers.trackCubicBezierCurve(trackExtras, "Tracks");
            File.WriteAllText($"{eventTitle}\\9_Tracks.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\9_Tracks.obj", dataList);
        }
    }
    class SuperClusters_V1 : TypeConfig {
        private List<SuperCluster> superClusterData;
        public override string Execute()
        {
            return JsonConvert.SerializeObject(superClusterData);
        }
    }
    class EEDigis_V1 : TypeConfig {

    }
    class EERecHits_V2 : TypeConfig {
        private List<CalorimetryTowers> caloTowerData;
        public override string Execute()
        {
            return JsonConvert.SerializeObject(caloTowerData);
        }

    }
	class EBDigis_V1 : TypeConfig {
        public override string Execute()
        {
            throw new NotImplementedException()
        }
    }
	class EBRecHits_V2 : TypeConfig {
        private List<CalorimetryTowers> caloTowerData;
        public override string Execute()
        {
            return JsonConvert.SerializeObject(caloTowerData);
        }
    }
    class ESRecHits_V2 : TypeConfig {
        private List<CalorimetryTowers> caloTowerData;
        public override string Execute()
        {
            return JsonConvert.SerializeObject(caloTowerData);
        }
    }
	class HGCEERecHits_V1 : TypeConfig {
        private List<CalorimetryTowers> caloTowerData;
        public override string Execute()
        {
            return JsonConvert.SerializeObject(caloTowerData);
        }
    }
	class HFRecHits_V2 : TypeConfig {
        private List<CalorimetryTowers> caloTowerData;
        public override string Execute()
        {
            return JsonConvert.SerializeObject(caloTowerData);
        }
    }
	class HORecHits_V2 : TypeConfig {
        private List<CalorimetryTowers> caloTowerData;
        public override string Execute()
        {
            return JsonConvert.SerializeObject(caloTowerData);
        }
    }
    class HBRecHits_V2 : TypeConfig {
        private List<CalorimetryTowers> caloTowerData;
        public override string Execute()
        {
            return JsonConvert.SerializeObject(caloTowerData);
        }
    }
	class HGCHEBRecHits_V1 : TypeConfig {
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
    class SiPixelClusters_V1 : TypeConfig { }
    class Event_V1 : TypeConfig { 
        public Event_V1(JObject args, string eventtitle) {
            JSON = args;
        }
        public override string Execute() {
            return JsonConvert.SerializeObject(JSON["Collections"]["Event_V1"]);
        }
    }
    class Event_V2 : TypeConfig {
        public Event_V2(JObject args, string eventtitle)
        {
            JSON = args;
        }
        public override string Execute()
        {
            return JsonConvert.SerializeObject(JSON["Collections"]["Event_V2"]);
        }
    }
    class Event_V3 : TypeConfig {
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
    class DTRecSegment4D_V1 : TypeConfig { }
    class RPCRecHits_V1 : TypeConfig { }
    class MatchingGEMs_V1 : TypeConfig { }
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
    class CSCCorrelatedLCTDigis_V2 : TypeConfig { }
    class MatchingCSCs_V1 : TypeConfig { }
    class CSCRecHit2Ds_V2 : TypeConfig { }
    class CSCSegments_V1 : TypeConfig { }
    class CSCSegments_V2 : TypeConfig { }
    class CSCSegments_V3 : TypeConfig { }*/
    class MuonChambers_V1 : TypeConfig {
        private List<MuonChamberData> muonChamberData;
        public override string Execute()
        {
            return JsonConvert.SerializeObject(muonChamberData);
        }
    }
    class CaloTowers_V2 : TypeConfig { }
    class METs_V1 : TypeConfig { }
    class PFMETs_V1 : TypeConfig { }
    class PATMETs_V1 : TypeConfig { }
    class Jets_V1 : TypeConfig { }
    class PFJets_V1 : TypeConfig { }
    class PFJets_V2 : TypeConfig { }
    class GenJets_V1 : TypeConfig { }
    class PATJets_V1 : TypeConfig { }
    class Photons_V1 : TypeConfig { }
    class PATPhotons_V1 : TypeConfig { }
    class GlobalMuons_V1 : TypeConfig { }
    class GlobalMuons_V2 : TypeConfig { }
    class PATGlobalMuons_V1 : TypeConfig { }
    class StandaloneMuons_V1 : TypeConfig { }
    class StandaloneMuons_V2 : TypeConfig { }
    class PATStandaloneMuons_V1 : TypeConfig { }
    class PATTrackerMuons_V1 : TypeConfig { }
    class PATTrackerMuons_V2 : TypeConfig { }
    class GsfElectrons_V1 : TypeConfig { }
    class GsfElectrons_V2 : TypeConfig { }
    class GsfElectrons_V3 : TypeConfig { }
    class PATElectrons_V1 : TypeConfig { }
    class ForwardProtons_V1 : TypeConfig { }
    class Vertices_V1 : TypeConfig {
        private List<Vertex> vertexDatas;
        private string eventTitle;
        public Vertices_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            primaryVertexDatas = StaticBoxHandlers.vertexParse(JSON, "Vertices_V1");
            GenerateVertexOBJ();
            return JsonConvert.SerializeObject(vertexDatas);
        }
        private void GenerateVertexOBJ()
        {
            StaticBoxHandlers.GenerateEllipsoidObj($@"{eventTitle}\Vertices_V1.obj", vertexDatas, 3.0);
        }
    }
    class PrimaryVertices_V1 : TypeConfig {
        private List<Vertex> primaryVertexDatas;
        private string eventTitle;
        public PrimaryVertices_V1(JObject args, string eventtitle)
        {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute()
        {
            primaryVertexDatas = StaticBoxHandlers.vertexParse(JSON,"PrimaryVertices_V1");
            GenerateVertexOBJ();
            return JsonConvert.SerializeObject(primaryVertexDatas);
        }
        private void GenerateVertexOBJ()
        {
            StaticBoxHandlers.GenerateEllipsoidObj($@"{eventTitle}\PrimaryVertices_V1.obj", primaryVertexDatas, 3.0);
        }
    }
    class SecondaryVertices_V1 : TypeConfig { 
        private List<Vertex> secondaryVertexDatas;
        private string eventTitle;
        public SecondaryVertices_V1( JObject args, string eventtitle) {
            eventTitle = eventtitle;
            JSON = args;
        }
        public override string Execute() {
            secondaryVertexDatas = StaticBoxHandlers.vertexParse(JSON,"SecondaryVertices_V1");
            GenerateVertexOBJ();
            return JsonConvert.SerializeObject(secondaryVertexDatas);
        }
        private void GenerateVertexOBJ() {
            StaticBoxHandlers.GenerateEllipsoidObj($@"{eventTitle}\SecondaryVertices_V1.obj", secondaryVertexDatas, 3.0);
        }
    }
    class VertexCompositeCandidates_V1 : TypeConfig { }
    class SimVertices_V1 : TypeConfig { }
}