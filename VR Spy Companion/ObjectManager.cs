using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGtoOBJGen
{
    internal class ObjectManager
    {
        private List<TypeConfig> eventObjects;
        private List<ObjectData> dataObjects;
        private List<TrackExtrasData> EXTRAS;
        private JObject INPUTJSON;
        private List<string> TYPES;
        private string EVENTPATH;
        private FileInfo INPUTFILE;
        private FileInfo OUTPUTFILE;
        private List<(string,JObject)> JObjects;
        private List<string> outputJSONStrings;
        private bool EXTRASFLAG;
        private bool POINTSFLAG;

        /*
         TODO:
            add cleanup methods and listeners
            implement adb
            go into maui
         */
        public ObjectManager(JObject JSON, string eventTitle)
        {
            dataObjects = new List<ObjectData>();
            JObjects = new List<(string,JObject)>();
            eventObjects = new List<TypeConfig>();
            INPUTJSON = JSON;
            TYPES = ((JObject)INPUTJSON["Types"]).Properties().Select(p => p.Name).ToList();
            EVENTPATH = eventTitle;
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
        public void createJSON()
        {
            //JObject obj = new JObject();
            //foreach(var item in JObjects)
            //{
            //    obj.Add(new JProperty(item.Item1, item.Item2));
            //}
            //File.WriteAllText($"{EVENTPATH}\\totaldata.json", JsonConvert.SerializeObject(obj, Formatting.Indented));
            JObject obj = new JObject();
            foreach (var item in JObjects)
            {
                obj.Add(new JProperty(item.Item1, item.Item2));
            }

            using (var sw = new StreamWriter($"{EVENTPATH}\\totaldata.json", false, Encoding.UTF8, bufferSize: 65536))
            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, obj);
            }
        }
        public void generateJSON()
        {
            //List<object> dataList = new List<object>();
            //string data = "{";
            //foreach(var json in outputJSONStrings)
            //{
            //    data += json+',';
            //}
            //data = data.Substring(0, data.Length - 1);
            //data += '}';

            //File.WriteAllText($"{EVENTPATH}\\totaldata.json",data);
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendJoin(",", outputJSONStrings);
            sb.Append("}");

            File.WriteAllText($"{EVENTPATH}\\totaldata.json", sb.ToString());
        }
        public void CreateObjects()
        {
            Parallel.ForEach(eventObjects, obj =>
            {
                string json = obj.Execute();

                lock (outputJSONStrings)
                {
                    outputJSONStrings.Add(json);
                }
            });
        }
        public void InstantiateObjects()
        {
                foreach (string t in TYPES) {
                if (INPUTJSON["Collections"][t].HasValues == true) { 
                    switch (t) {
                        case "TrackerMuons_V1":
                            TrackerMuonV1 trackerMuon = new TrackerMuonV1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(trackerMuon);
                            break;
                        case "TrackerMuons_V2":
                            TrackerMuonV2 trackerMuonV2 = new TrackerMuonV2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(trackerMuonV2);
                            break;
                        case "GsfElectrons_V1":
                            GsfElectrons_V1 electronv1 = new GsfElectrons_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(electronv1);
                            break;
                        case "GsfElectrons_V2":
                            GsfElectrons_V2 electronv2 = new GsfElectrons_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(electronv2);
                            break;
                        case "GsfElectrons_V3":
                            GsfElectrons_V3 electronv3 = new GsfElectrons_V3(INPUTJSON, EVENTPATH);
                            eventObjects.Add(electronv3);
                            break;
                        case "Vertices_V1":
                            Vertices_V1 vertexv1 = new Vertices_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(vertexv1);
                            break;
                        case "PrimaryVertices_V1":
                            PrimaryVertices_V1 primaryVertices_V1 = new PrimaryVertices_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(primaryVertices_V1);
                            break;
                        case "SecondaryVertices_V1":
                            SecondaryVertices_V1 secondary = new SecondaryVertices_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(secondary);
                            break;
                        case "SuperClusters_V1":
                            SuperClusters_V1 cluster = new SuperClusters_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(cluster);
                            break;
                        case "EERecHits_V2":
                            EERecHits_V2 eeRecHits_V2 = new EERecHits_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(eeRecHits_V2);
                            break;
                        case "EBRecHits_V2":
                            EBRecHits_V2 ebRecHits_V2 = new EBRecHits_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(ebRecHits_V2);
                            break;
                        case "ESRecHits_V2":
                            ESRecHits_V2 esRecHits_V2 = new ESRecHits_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(esRecHits_V2);
                            break;
                        case "HGCEERecHits_V1":
                            break;
                        case "HFRecHits_V2":
                            HFRecHits_V2 hfRecHits_V2 = new HFRecHits_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(hfRecHits_V2);
                            break;
                        case "HORecHits_V2":
                            HORecHits_V2 hoRecHits_V2 = new HORecHits_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(hoRecHits_V2);
                            break;
                        case "HERecHits_V2":
                            HERecHits_V2 heRecHits_V2 = new HERecHits_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(heRecHits_V2);
                            break;
                        case "HBRecHits_V2":
                            HBRecHits_V2 hbRecHits_V2 = new HBRecHits_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(hbRecHits_V2);
                            break;
                        case "HGCHEBRecHits_V1":
                            break;
                        case "HGCHEFRecHits_V1":
                            break;
                        case "Tracks_V1":
                            TracksV1 tracksv1 = new TracksV1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(tracksv1);
                            break;
                        case "Tracks_V2":
                            TracksV2 tracksv2 = new TracksV2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(tracksv2);
                            break;
                        case "Tracks_V3":
                            TracksV3 tracksv3 = new TracksV3(INPUTJSON, EVENTPATH);
                            eventObjects.Add(tracksv3);
                            break;
                        case "Tracks_V4":
                            TracksV4 tracksv4 = new TracksV4(INPUTJSON, EVENTPATH);
                            eventObjects.Add(tracksv4);
                            break;
                        case "TrackDets_V1":
                            TrackDets_V1 trackdetsv1 = new TrackDets_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(trackdetsv1);
                            break;
                        case "TrackingRecHits_V1":
                            TrackingRecHits_V1 trackingrechitsv1 = new TrackingRecHits_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(trackingrechitsv1);
                            break;
                        case "SiStripClusters_V1":
                            SiStripClusters_V1 sistripclustersv1 = new SiStripClusters_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(sistripclustersv1);
                            break;
                        case "SiPixelClusters_V1":
                            SiPixelClusters_V1 sipixelclustersv1 = new SiPixelClusters_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(sipixelclustersv1);
                            break;
                        case "DTRecHits_V1":
                            DTRecHits_V1 dtrechitsv1 = new DTRecHits_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(dtrechitsv1);
                            break;
                        case "DTRecSegment4D_V1":
                            DTRecSegment4D_V1 dtrecsegmentsv1 = new DTRecSegment4D_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(dtrecsegmentsv1);
                            break;
                        case "RPCRecHits_V1":
                            RPCRecHits_V1 rpc = new RPCRecHits_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(rpc);
                            break;
                        case "MatchingGEMS_V!":
                            break;
                        case "GEMDigis_V2": 
                            break;
                        case "GEMRecHits_V2": 
                            break;
                        case "GEMSegments_V1": 
                            break;
                        case "GEMSegments_V2": 
                            break;
                        case "GEMSegments_V3": 
                            break;
                        case "CSCStripDigis_V1": 
                            break;
                        case "CSCWireDigis_V1": 
                            break;
                        case "CSCStripDigis_V2": 
                            break;
                        case "CSCWireDigis_V2": 
                            break;
                        case "CSCLCTDigis_V1": 
                            break;
                        case "CSCCorrelatedLCTDigis_V2": 
                            break;
                        case "MatchingCSCs_V1":
                            MatchingCSCs_V1 matchingcsvsv1 = new MatchingCSCs_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(matchingcsvsv1);
                            break;
                        case "CSCRecHit2Ds_V2":
                            CSCRecHit2Ds_V2 cscrecv2 = new CSCRecHit2Ds_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(cscrecv2);
                            break;
                        case "CSCSegments_V1":
                            CSCSegments_V1 cscseg1 = new CSCSegments_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(cscseg1);
                            break;
                        case "CSCSegments_V2":
                            CSCSegments_V2 cscsegmentsv2 = new CSCSegments_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(cscsegmentsv2);
                            break;
                        case "CSCSegments_V3": 
                            break;
                        case "MuonChambers_V1":
                            MuonChambers_V1 muonchambers = new MuonChambers_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(muonchambers);
                            break;
                        case "CaloTowers_V2":
                            CaloTowers_V2 calotowersv2 = new CaloTowers_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(calotowersv2);
                            break;
                        case "METs_V1": 
                            break;
                        case "PFMETs_V1":
                            PFMETs_V1 pfMets = new PFMETs_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(pfMets);
                            break;
                        case "PATMETs_V1": 
                            break;
                        case "Jets_V1": 
                            break;
                        case "PFJets_V1":
                            PFJets_V1 pfJets = new PFJets_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(pfJets);
                            break;
                        case "PFJets_V2":
                            PFJets_V2 pfJetsv2 = new PFJets_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(pfJetsv2);
                            break;
                        case "GenJets_V1":
                            break;
                        case "PATJets_V1":
                            break;
                        case "Photons_V1":
                            break;
                        case "PATPhotons_V1":
                            break;
                        case "GlobalMuons_V1":
                            GlobalMuons_V1 globalMuons_v1 = new GlobalMuons_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(globalMuons_v1);
                            break;
                        case "GlobalMuons_V2":
                            GlobalMuons_V2 globalMuons_v2 = new GlobalMuons_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(globalMuons_v2);
                            break;
                        case "PATGlobalMuons_V1":
                            break;
                        case "StandaloneMuons_V1":
                            StandaloneMuons_V1 standaloneMuons_V1 = new StandaloneMuons_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(standaloneMuons_V1);
                            break;
                        case "StandaloneMuons_V2":
                            StandaloneMuons_V2 standaloneMuons_V2 = new StandaloneMuons_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(standaloneMuons_V2);
                            break;
                        case "PATStandaloneMuons_V1":
                            break;
                        case "PATTrackerMuons_V1":
                            break;
                        case "PATTrackerMuons_V2":
                            break;
                        case "PATElectrons_V1":
                            break;
                        case "ForwardProtons_V1":
                            break;
                        case "VertexCompositeCandidates_V1":
                            break;
                        case "SimVertices_V1":
                            break;
                        case "Event_V1":
                            Event_V1 event_V1 = new Event_V1(INPUTJSON, EVENTPATH);
                            eventObjects.Add(event_V1);
                            break;
                        case "Event_V2":
                            Event_V2 event_V2 = new Event_V2(INPUTJSON, EVENTPATH);
                            eventObjects.Add(event_V2);
                            break;
                        case "Event_V3":
                            Event_V3 event_V3 = new Event_V3(INPUTJSON, EVENTPATH);
                            eventObjects.Add(event_V3);
                            break;
                        default:
                            break;
                    }
                }
            } 
        }
    }
}
