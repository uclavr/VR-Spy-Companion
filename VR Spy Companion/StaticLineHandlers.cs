using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml.Linq;

namespace IGtoOBJGen
{
    static class StaticLineHandlers
    {
        static public List<PhotonData> photonParse(JObject data)
        {
            List<PhotonData> dataList = new List<PhotonData>();
            int idNumber = 0;
            var dataValues = data["Collections"]["Photons_V1"];
            if (dataValues == null || dataValues == null)
            {
                return dataList;
            }
            foreach (var igPhotonData in dataValues)
            {
                PhotonData currentPhotonItem = new PhotonData();

                var children = igPhotonData.Children().Values<double>().ToArray();

                currentPhotonItem.id = idNumber;
                currentPhotonItem.energy = children[0];
                currentPhotonItem.et = children[1];
                currentPhotonItem.eta = children[2];
                currentPhotonItem.phi = children[3];
                currentPhotonItem.position = new Vector3((float)children[4], (float)children[5], (float)children[6]);

                idNumber++;
                dataList.Add(currentPhotonItem);
            }
            return dataList;
        }
        static public string makePhoton(PhotonData inputData)
        {
            double lEB = 3.0; //half-length of ECAL barrel in meters
            double rEB = 1.24; //radius of ECAL barrel in meters
            double eta = inputData.eta;
            double phi = inputData.phi;
            double px = Math.Cos(phi);
            double py = Math.Sin(phi);
            double pz = Math.Sinh(eta);
            double x0 = inputData.position.X;
            double y0 = inputData.position.Y;
            double z0 = inputData.position.Z;
            double t;

            if (Math.Abs(eta) > 1.48)
            {
                t = Math.Abs((lEB - z0) / pz);
            }
            else
            {
                double a = px * px + py * py;
                double b = 2 * x0 * px + 2 * y0 * py;
                double c = x0 * x0 + y0 * y0 - rEB * rEB;
                t = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
            }

            string Contents;
            Contents = $"v {x0} {y0} {z0}\nv {x0 + 0.001} {y0 + 0.001} {(z0 + 0.001)}\nv {x0 + px * t} {y0 + py * t} {(z0 + pz * t)}\nv {x0 + px * t + 0.001} {y0 + py * t + 0.001} {(z0 + pz * t + 0.001)}";
            //Output a string of obj vectors that define the photon path
            return Contents;
        }
        static public void generatePhotonModels(List<PhotonData> dataList, string eventTitle)
        {
            if (dataList == null || dataList.Count() == 0)
            {
                File.WriteAllText($"{eventTitle}\\8_Photons_V1.obj", String.Empty);
                return;
            }
            //Write obj files for the photons
            List<string> dataStrings = new List<string>();
            int counter = 1;
            foreach (var igPhotonData in dataList)
            {
                string objData = makePhoton(igPhotonData);
                //Hey! That's the function from above!
                dataStrings.Add(objData);
                dataStrings.Add($"f {counter} {counter + 1} {counter + 3} {counter + 2}");
                counter += 4;
            }

            File.WriteAllText($"{eventTitle}\\8_Photons_V1.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\8_Photons_V1.obj", dataStrings);
        }
        static public List<string> trackCubicBezierCurve(List<TrackExtrasData> inputData, string objectName)
        {
            //Same as the above function, except it outputs all the tracks into their own singular file. This allows me to keep Tracks_V3 as its 
            //own single file since it doesn't need to be matched to data.
            List<string> dataList = new List<string>();
            List<string> testList = new List<string>();
            List<int> exclusion_indeces = new List<int>();
            int numVerts = 32;
            int n = 0;
            int num = 0;
            int j = 0;
            foreach (var item in inputData)
            {
                testList.Clear();
                dataList.Add($"o {objectName}_{num}");
                for (double i = 0.0; i <= numVerts; i++)
                {

                    double t = (double)(i) / (double)(numVerts);

                    double t1 = Math.Pow(1.0 - t, 3);
                    double t2 = 3 * t * Math.Pow(1.0 - t, 2);
                    double t3 = 3 * t * t * (1.0 - t);
                    double t4 = Math.Pow(t, 3);

                    // Check out the wikipedia page for bezier curves if you want to understand the math. That's where I learned it!
                    // also we're using double arrays because i dont like Vector3 and floats. I'm the one who has to go through the headaches of working with double arrays
                    // instead of Vector3 so i get to make that call. i also wrote this before i realized i couldn't avoid using MathNET and i can't be bothered to 
                    // change it such that it uses MathNET vectors

                    // UPDATE: I should have refactored to use MathNET for everything when I had the chance

                    double[] term1 = { t1 * item.pos1[0], t1 * item.pos1[1], t1 * item.pos1[2] };
                    double[] term2 = { t2 * item.pos3[0], t2 * item.pos3[1], t2 * item.pos3[2] };
                    double[] term3 = { t3 * item.pos4[0], t3 * item.pos4[1], t3 * item.pos4[2] };
                    double[] term4 = { t4 * item.pos2[0], t4 * item.pos2[1], t4 * item.pos2[2] };
                    double[] point = { term1[0] + term2[0] + term3[0] + term4[0], term1[1] + term2[1] + term3[1] + term4[1], term1[2] + term2[2] + term3[2] + term4[2] };

                    string poin_t = $"v {point[0]} {point[1]} {point[2]}";
                    string point_t2 = $"v {point[0]} {point[1] + 0.001} {point[2]}";

                    dataList.Add(poin_t); dataList.Add(point_t2);
                    testList.Add(poin_t); testList.Add(point_t2);
                    n += 2;
                }
                for (int r = 1; r < (numVerts * 2); r++)
                {
                    string faces1 = $"f {r + (j * 33)} {r + 1 + (j * 33)} {r + 3 + (j * 33)} {r + 2 + (j * 33)}";
                    string faces2 = $"f {r + 2 + (j * 33)} {r + 3 + (j * 33)} {r + 1 + (j * 33)} {r + (j * 33)}";
                    dataList.Add(faces1); dataList.Add(faces2);
                }
                num++;
                j += 2;
                exclusion_indeces.Add(n);
            }

            return dataList;
        }
        static public List<TrackExtrasData> trackExtrasParse(JObject data)
        {
            List<TrackExtrasData> dataList = new List<TrackExtrasData>();
            var dataValues = data["Collections"]["Extras_V1"];
            if (dataValues == null)
            {
                return dataList;
            }
            foreach (var extra in data["Collections"]["Extras_V1"])
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

                dataList.Add(currentItem);
            }
            return dataList;
        }
        static public List<TrackExtrasData> setExtras(JObject data,string association) 
        {
            List<TrackExtrasData> dataList = new List<TrackExtrasData>();
            var assocsExtras = data["Associations"][association];
            int firstAssoc = assocsExtras[0][1][1].Value<int>();
            int lastAssoc = assocsExtras.Last()[1][1].Value<int>();
            var extras = data["Collections"]["Extras_V1"].ToArray()[(firstAssoc)..(lastAssoc + 1)];
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

                currentItem.pos2 = new double[3] { children[6], children[7],  children[8] };

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

                dataList.Add(currentItem);
            }
            return dataList;
        }
        static public List<GlobalMuonData> globalMuonParse(JObject data,int version)
        {
            List<GlobalMuonData> dataList = new List<GlobalMuonData>();
            int idNumber = 0;
            
            foreach (var item in data["Collections"][$"GlobalMuons_V{version}"])
            {
                GlobalMuonData muonData = new GlobalMuonData();
                var children = item.Children().Values<double>().ToArray();

                muonData.id = idNumber;
                muonData.pt = children[0];
                muonData.charge = (int)children[1];
                muonData.position = new double[] { children[2], children[3], children[4] };
                muonData.phi = children[5];
                muonData.eta = children[6];
                muonData.caloEnergy = children[7];

                idNumber++;
                dataList.Add(muonData);
            }

            return dataList;
        }

        /*static public void makeGlobalMuons()
        {
            if (globalMuonPoints == null) { return; }
            makeGeometryFromPoints(globalMuonPoints, "2_globalMuons", "globalMuons");
        }*/
        static public List<TrackerMuonData> trackerMuonParse(JObject data,int version)
        {
            List<TrackerMuonData> dataList = new List<TrackerMuonData>();
            int idNumber = 0;

            var assocsExtras = data["Associations"]["MuonTrackerExtras_V1"];
            var assocsPoints = data["Associations"]["MuonTrackerPoints_V1"];
            var datapoints = data["Collections"]["TrackerMuons_V1"];
            /*if ((assocsExtras == null || assocsExtras.HasValues == false) && (assocsPoints == null || assocsPoints.HasValues == false))
            {
                trackerMuonExtras = new List<TrackExtrasData>();
                return dataList;
            }
            if (datapoints == null)
            {
                trackerMuonExtras = new List<TrackExtrasData>();
                return dataList;
            }*/
            
            foreach (var item in data["Collections"][$"TrackerMuons_V{version}"])
            {
                TrackerMuonData muonData = new TrackerMuonData();
                var children = item.Children().Values<double>().ToArray();

                muonData.id = idNumber;
                muonData.pt = children[0];
                muonData.charge = (int)children[1];
                muonData.position = new double[] { children[2], children[3], children[4] };
                muonData.phi = children[5];
                muonData.eta = children[6];

                idNumber++;
                dataList.Add(muonData);
            }
            /*if (assocsExtras.Count() >= 1)
            {
                int firstassoc = assocsExtras[0][1][1].Value<int>();
                trackerMuonExtras = trackExtrasData.GetRange(firstassoc, assocsExtras.Last()[1][1].Value<int>() - firstassoc + 1);
            }
            else
            {
                trackerMuonExtras = new List<TrackExtrasData>();
                trackerMuonExtras.Clear();
            }
            if (assocsPoints.HasValues)
            {
                trackerMuonPoints = makeTrackPoints(assocsPoints);
            }*/

            return dataList;
        }
        /*static public List<TrackExtrasData> trackerExtrasParse(JObject data)
        {
            var assocsExtras = data["Associations"]["MuonTrackerExtras_V1"];
            int firstassoc = assocsExtras[0][1][1].Value<int>();
            List<TrackExtrasData> trackerMuonExtras = trackExtrasData.GetRange(firstassoc, assocsExtras.Last()[1][1].Value<int>() - firstassoc + 1);
        }*/
        static public void makeTrackerMuons(List<TrackExtrasData> trackerMuonExtras,string eventTitle)
        {
            
            
        }
        static public void makeTrackerMuons(List<List<double[]>> trackerMuonPoints, string eventtitle){

                makeGeometryFromPoints(trackerMuonPoints, "3_TrackerMuons", "TrackerMuons",eventtitle);
        }
        static public List<StandaloneMuonData> standaloneMuonParse(JObject data, int version)
        {
            List<StandaloneMuonData> dataList = new List<StandaloneMuonData>();
            int idNumber = 0;
            var assocs = data["Associations"]["MuonTrackExtras_V1"];
            var assocsPoints = data["Associations"]["MuonStandalonePoints_V1"];
            if ((assocs == null || assocs.HasValues == false) && (assocsPoints == null || assocsPoints.HasValues == false))
            {
                return dataList;
            }
            foreach (var item in data["Collections"][$"StandaloneMuons_V{version}"])
            {
                StandaloneMuonData muon = new StandaloneMuonData();
                var children = item.Children().Values<double>().ToArray();
                muon.id = idNumber;
                muon.pt = children[0];
                muon.charge = (int)children[1];
                muon.position = new double[] { children[2], children[3], children[4] };
                muon.phi = children[5];
                muon.eta = children[6];
                muon.caloEnergy = children[7];

                idNumber++;
                dataList.Add(muon);

            }
            //int firstassoc = assocs[0][1][1].Value<int>();
            //standaloneMuonExtras = trackExtrasData.GetRange(firstassoc, assocs.Last()[1][1].Value<int>() - firstassoc + 1);
            //try { standaloneMuonPoints = makeTrackPoints(assocsPoints); } catch (Exception ex) { }

            return dataList;
        }
        /*public void makeStandaloneMuonsV1()
        {
            if (standaloneMuonExtras == null && standaloneMuonPoints == null)
            {
                File.WriteAllText($"{eventTitle}\\3_standaloneMuons.obj", String.Empty);
                return;
            }            if (standaloneMuonPoints == null)
            {
                List<string> dataList = trackCubicBezierCurve(standaloneMuonExtras, "standaloneMuons");
                File.WriteAllText($"{eventTitle}\\3_standaloneMuons.obj", String.Empty);
                File.WriteAllLines($"{eventTitle}\\3_standaloneMuons.obj", dataList);
            }
            else
            {
                makeGeometryFromPoints(standaloneMuonPoints, "3_standaloneMuons", "standaloneMuons");
            }
        }
        public List<Track> tracksParse()
        {
            List<Track> dataList = new List<Track>();
            var assocs = data["Associations"]["TrackExtras_V1"];

            if (assocs == null || assocs.HasValues == false)
            {
                return dataList;
            }
            foreach (var item in data["Collections"]["Tracks_V3"])
            {
                Track track = new Track();
                var children = item.Children().Values<double>().ToArray();

                track.pos = new double[] { children[0], children[1], children[2] };
                track.dir = new double[] { children[3], children[4], children[5] };
                track.pt = children[6];
                track.phi = children[7];
                track.eta = children[8];
                track.charge = (int)children[9];
                track.chi2 = children[10];
                track.ndof = children[11];
                dataList.Add(track);
            }
            int firstassoc = assocs[0][1][1].Value<int>();
            subTrackExtras = trackExtrasData.GetRange(firstassoc, assocs.Last()[1][1].Value<int>() - firstassoc + 1);

            return dataList;
        }
        static public void makeTracks()
        {
            if (subTrackExtras == null) { return; }
            List<string> dataList = trackCubicBezierCurve(subTrackExtras, "Tracks");
            File.WriteAllText($"{eventTitle}\\9_Tracks.obj", String.Empty);
            File.WriteAllLines($"{eventTitle}\\9_Tracks.obj", dataList);
        }*/
        static public List<GsfElectron> electronParse(JObject data,int version)
        {
            List<GsfElectron> dataList = new List<GsfElectron>();
            int idNumber = 0;
        
            foreach (var item in data["Collections"][$"GsfElectrons_V{version}"])
            {
                GsfElectron electron = new GsfElectron();
                var children = item.Children().Values<double>().ToArray();

                electron.id = idNumber;
                electron.pt = children[0];
                electron.eta = children[1];
                electron.phi = children[2];
                electron.charge = (int)children[3];
                electron.pos = new double[] { children[4], children[5], children[6] };
                electron.dir = new double[] { children[7], children[8], children[9] };

                idNumber++;
                dataList.Add(electron);
            }

            return dataList;
        }
        static public METData METParse(JObject data)
        {
            METData met = new METData();
            JToken metdata;
           
            metdata = data["Collections"]["PFMETs_V1"][0];
           
            var children = metdata.Values<double>().ToList();
            met.phi = children[0];
            met.pt = children[1];
            met.px = children[2];
            met.py = children[3];
            met.pz = children[4];

            return met;
        }
        static public List<Track> trackDataParse(JObject data, int version)
        {
            int n = 0;
            List<Track> dataList = new List<Track>();
            var dataValues = data["Collections"][$"Tracks_V{version}"];
            if (dataValues == null)
            {
                return dataList;
            }
            foreach (var item in data["Collections"][$"Tracks_V{version}"])
            {
                Track track = new Track();
                var children = item.Children().Values<double>().ToArray();

                track.id = n;
                track.pos = new double[] { children[0], children[1], children[2] };
                track.dir = new double[] { children[3], children[4], children[5] };
                track.pt = children[6];
                track.phi = children[7];
                track.eta = children[8];
                track.charge = (int)children[9];
                track.chi2 = children[10];
                track.ndof = children[11];
                dataList.Add(track);
                n++;
            }
            return dataList;
        }
        static public List<TrackExtrasData> removeMuonsFromTracks(List<TrackExtrasData> trackerMuonExtras, List<TrackExtrasData> subTrackExtras)
        {
            foreach (TrackExtrasData muon in trackerMuonExtras)
            {
                var index = subTrackExtras.FindIndex(x => x.pos1[0] == muon.pos1[0]);
                if (index > -1)
                {
                    subTrackExtras.RemoveAt(index);
                }
            }
            return subTrackExtras;
        }
        static public List<List<double[]>> makeTrackPoints(string association, JObject data)
        {
            List<List<double[]>> positions = new List<List<double[]>>();
            var assoc = data["Associations"][association];
            var extras = data["Collections"]["Points_V1"];

            if (assoc == null || assoc.HasValues == false || extras.HasValues == false || extras == null)
            {
                return positions;
            }

            int mi;
            int pi;

            foreach (var item in assoc)
            {
                mi = item[0][1].Value<int>();
                pi = item[1][1].Value<int>();
                if (positions.Count() <= mi) { List<double[]> blank = new List<double[]>(); positions.Add(blank); }
                double[] point = extras[pi][0].ToObject<double[]>();
                positions[mi].Add(point);
                
            }
            return positions;
        }
        static public void makeGeometryFromPoints(List<List<double[]>> points, string name, string path, string eventTitle)
        {
            List<List<string>> dataLists = new List<List<string>>();
            int accountingfactor = 0;
            List<string> strings = new List<string>();
            int counter = 0;
            foreach (var subitem in points)
            {
                List<string> medi = new List<string>();
                medi.Add($"o {name}_{counter}");
                foreach (var item in subitem)
                {
                    string line1 = $"v {item[0]} {item[1]} {item[2]}";
                    string line2 = $"v {item[0]} {item[1] + 0.001} {item[2]}";
                    medi.Add(line1); medi.Add(line2);
                }
                dataLists.Add(medi);
                counter++;
            }
            foreach (var item in dataLists)
            {
                var ble = item;
                int count = ble.Count();
                for (int i = 1; i < (count - 3); i += 2)
                {
                    string bleh = $"f {accountingfactor + i} {accountingfactor + i + 1} {accountingfactor + i + 2} {accountingfactor + i + 3}";
                    ble.Add(bleh);
                    string bleh1 = $"f {accountingfactor + i + 3} {accountingfactor + i + 2} {accountingfactor + i + 1} {accountingfactor + i}";
                    ble.Add(bleh1);
                }
                accountingfactor += count - 1;
                strings.AddRange(ble);
            }
            File.WriteAllLines(@$"{eventTitle}/{path}.obj", strings);
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
            File.WriteAllLines(Path.Combine(downloadsPath, @$"{eventTitle}/{path}.obj"), strings);
        }
        static public List<trackingPoint> trackingpointParse(JObject data, string name, int index)
        {
            List<trackingPoint> dataList = new List<trackingPoint>();
            foreach (var item in data["Collections"][name])
            {

                var children = item.Children().Values<double>().ToArray();
                trackingPoint trackingPoint = new trackingPoint();
                if (index == 1)
                {
                    trackingPoint.detid = (int)children[0];
                }
                trackingPoint.name = name;
                trackingPoint.X = children[index];
                trackingPoint.Y = children[index + 1];
                trackingPoint.Z = children[index + 2];
                dataList.Add(trackingPoint);
            }
            return dataList;
        }
        static public List<string> generatetrackingPoints(List<trackingPoint> inputData)
        {
            List<string> geometryData = new List<string>();
            //double boxLength = 0.00025;
            double boxLength = 0.005;
            //double boxLength = 0.000005;
            int counter = 1;
            foreach (trackingPoint point in inputData)
            {
                double halfLength = boxLength / 2.0;
                double[][] vertices = new double[8][];
                vertices[0] = new double[] { point.X - halfLength, point.Y - halfLength, point.Z - halfLength };
                vertices[1] = new double[] { point.X + halfLength, point.Y - halfLength, point.Z - halfLength };
                vertices[2] = new double[] { point.X + halfLength, point.Y + halfLength, point.Z - halfLength };
                vertices[3] = new double[] { point.X - halfLength, point.Y + halfLength, point.Z - halfLength };
                vertices[4] = new double[] { point.X - halfLength, point.Y - halfLength, point.Z + halfLength };
                vertices[5] = new double[] { point.X + halfLength, point.Y - halfLength, point.Z + halfLength };
                vertices[6] = new double[] { point.X + halfLength, point.Y + halfLength, point.Z + halfLength };
                vertices[7] = new double[] { point.X - halfLength, point.Y + halfLength, point.Z + halfLength };

                foreach (var vertex in vertices)
                {
                    geometryData.Add($"v {vertex[0]} {vertex[1]} {vertex[2]}");
                }
                geometryData.Add($"f {counter} {counter + 1} {counter + 5} {counter + 4}"); // Side 1 (Front)
                geometryData.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}"); // Side 2 (Right)
                geometryData.Add($"f {counter + 2} {counter + 3} {counter + 7} {counter + 6}"); // Side 3 (Back)
                geometryData.Add($"f {counter + 3} {counter} {counter + 4} {counter + 7}"); // Side 4 (Left)
                geometryData.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}"); // Top
                geometryData.Add($"f {counter} {counter + 3} {counter + 2} {counter + 1}"); // Bottom
                counter += 8;

            }
            return geometryData;
        }
        static public List<cscSegmentV2> cscSegmentParse(JObject data, string name)
        {
            List<cscSegmentV2> dataList = new List<cscSegmentV2>();
            foreach (var item in data["Collections"][name])
            {

                var children = item.Children().Values<double>().ToArray();
                cscSegmentV2 cscSegmentV2 = new cscSegmentV2();
                cscSegmentV2.name = name;
                cscSegmentV2.detid = (int)children[0];
                cscSegmentV2.pos_1 = new double[] { children[1], children[2], children[3] };
                cscSegmentV2.pos_2 = new double[] { children[4], children[5], children[6] };
                cscSegmentV2.endcap = (int)children[7];
                cscSegmentV2.station = (int)children[8];
                cscSegmentV2.ring = (int)children[9];
                cscSegmentV2.chamber = (int)children[10];
                cscSegmentV2.layer = (int)children[11];
                dataList.Add(cscSegmentV2);
            }
            return dataList;
        }

        static void GeneratePrism(double[] startPoint, double[] endPoint, ref List<string> geometryData, ref int vertexIndex, float size)
        {
            Vector3 start = new Vector3((float)startPoint[0], (float)startPoint[1], (float)startPoint[2]);
            Vector3 end = new Vector3((float)endPoint[0], (float)endPoint[1], (float)endPoint[2]);
            Vector3 line = end - start;
            float length = line.Length();
            Vector3 direction = Vector3.Normalize(end - start);

            // Choose a fallback axis that's not aligned with the direction
            Vector3 fallback = Math.Abs(direction.Y) < 0.99 ? Vector3.UnitY : Vector3.UnitX;

            // Calculate right as perpendicular to direction
            Vector3 right = Vector3.Normalize(Vector3.Cross(fallback, direction));

            // Verify right is perpendicular to direction
            if (Vector3.Dot(direction, right) > 1e-5)
            {
                throw new Exception("Right vector is not perpendicular to direction.");
            }

            // Calculate up as perpendicular to both direction and right
            Vector3 up = Vector3.Normalize(Vector3.Cross(direction, right));

            // Verify up is perpendicular to both direction and right
            if (Vector3.Dot(direction, up) > 1e-5 || Vector3.Dot(right, up) > 1e-5)
            {
                throw new Exception("Up vector is not perpendicular to both direction and right.");
            }
            right = Vector3.Normalize(Vector3.Cross(direction, up));

            Vector3 corner1 = start + right * (size / 2) + up * (size / 2);
            Vector3 corner2 = start - right * (size / 2) + up * (size / 2);
            Vector3 corner3 = start - right * (size / 2) - up * (size / 2);
            Vector3 corner4 = start + right * (size / 2) - up * (size / 2);

            Vector3 corner5 = end + right * (size / 2) + up * (size / 2);
            Vector3 corner6 = end - right * (size / 2) + up * (size / 2);
            Vector3 corner7 = end - right * (size / 2) - up * (size / 2);
            Vector3 corner8 = end + right * (size / 2) - up * (size / 2);


            geometryData.Add($"v {corner1.X} {corner1.Y} {corner1.Z}");
            geometryData.Add($"v {corner2.X} {corner2.Y} {corner2.Z}");
            geometryData.Add($"v {corner3.X} {corner3.Y} {corner3.Z}");
            geometryData.Add($"v {corner4.X} {corner4.Y} {corner4.Z}");

            geometryData.Add($"v {corner5.X} {corner5.Y} {corner5.Z}");
            geometryData.Add($"v {corner6.X} {corner6.Y} {corner6.Z}");
            geometryData.Add($"v {corner7.X} {corner7.Y} {corner7.Z}");
            geometryData.Add($"v {corner8.X} {corner8.Y} {corner8.Z}");

            geometryData.Add($"f {vertexIndex} {vertexIndex + 1} {vertexIndex + 2} {vertexIndex + 3}"); // Start face
            geometryData.Add($"f {vertexIndex + 4} {vertexIndex + 5} {vertexIndex + 6} {vertexIndex + 7}"); // End face

            geometryData.Add($"f {vertexIndex} {vertexIndex + 1} {vertexIndex + 5} {vertexIndex + 4}");
            geometryData.Add($"f {vertexIndex + 1} {vertexIndex + 2} {vertexIndex + 6} {vertexIndex + 5}");
            geometryData.Add($"f {vertexIndex + 2} {vertexIndex + 3} {vertexIndex + 7} {vertexIndex + 6}");
            geometryData.Add($"f {vertexIndex + 3} {vertexIndex} {vertexIndex + 4} {vertexIndex + 7}");

            geometryData.Add($"f {vertexIndex + 3} {vertexIndex + 2} {vertexIndex + 1} {vertexIndex}"); // Reverse start face
            geometryData.Add($"f {vertexIndex + 7} {vertexIndex + 6} {vertexIndex + 5} {vertexIndex + 4}"); // Reverse end face

            geometryData.Add($"f {vertexIndex + 4} {vertexIndex + 5} {vertexIndex + 1} {vertexIndex}");
            geometryData.Add($"f {vertexIndex + 5} {vertexIndex + 6} {vertexIndex + 2} {vertexIndex + 1}");
            geometryData.Add($"f {vertexIndex + 6} {vertexIndex + 7} {vertexIndex + 3} {vertexIndex + 2}");
            geometryData.Add($"f {vertexIndex + 7} {vertexIndex + 4} {vertexIndex} {vertexIndex + 3}");

            vertexIndex += 8;
        }
        static public List<string> generateCSCSegment(List<cscSegmentV2> inputData)
        {
            List<string> geometryData = new List<string>();
            int vertexIndex = 1;
            float size = 0.0025f;
            foreach (cscSegmentV2 seg in inputData)
            {
                GeneratePrism(seg.pos_1, seg.pos_2, ref geometryData, ref vertexIndex, size);
            }
            return geometryData;
        }
        static public List<cscSegmentV1> ParseCSCSegmentsV1(JObject data, string name)
        {
            var dataList = new List<cscSegmentV1>();
            foreach (var item in data["Collections"]["CSCSegments_V1"])
            {
                var children = item.Children().Values<double>().ToArray();
                cscSegmentV1 cscSegmentV1 = new cscSegmentV1();
                cscSegmentV1.name = name;
                cscSegmentV1.detid = (int)children[0];
                cscSegmentV1.pos_1 = new double[] { children[1], children[2], children[3] };
                cscSegmentV1.pos_2 = new double[] { children[4], children[5], children[6] };
                dataList.Add(cscSegmentV1);
            }
            return dataList;
        }
        static public List<string> GenerateCSCSegmentV1(List<cscSegmentV1> inputData)
        {
            List<string> geometryData = new List<string>();
            int vertexIndex = 1;
            float size = 0.0025f;
            foreach (cscSegmentV1 seg in inputData)
            {
                GeneratePrism(seg.pos_1, seg.pos_2, ref geometryData, ref vertexIndex, size);
            }
            return geometryData;
        }
        static public List<dtRecSegment4D_V1> dtRecSegmentParse(JObject data, string name)
        {
            List<dtRecSegment4D_V1> dataList = new List<dtRecSegment4D_V1>();
            foreach (var item in data["Collections"][name])
            {
                dtRecSegment4D_V1 dtRecSegment = new dtRecSegment4D_V1();
                var children = item.Children().Values<double>().ToArray();
                dtRecSegment.name = name;
                dtRecSegment.detid = (int)children[0];
                dtRecSegment.pos_1 = new double[] { children[1], children[2], children[3] };
                dtRecSegment.pos_2 = new double[] { children[4], children[5], children[6] };
                dtRecSegment.sectorId = (int)children[7];
                dtRecSegment.stationId = (int)children[8];
                dtRecSegment.wheelId = (int)children[9];
                dataList.Add(dtRecSegment);
            }
            return dataList;
        }
        static public List<string> generateDTRecSegment(List<dtRecSegment4D_V1> inputData)
        {
            List<string> geometryData = new List<string>();
            int vertexIndex = 1;
            float size = 0.005f;
            foreach (dtRecSegment4D_V1 point in inputData)
            {
                GeneratePrism(point.pos_1, point.pos_2, ref geometryData, ref vertexIndex, size);
            }
            return geometryData;
        }
        
        static public List<cscRecHit2Ds_V2> ParseCSCRecHits2Ds_V2(JObject data, string name)
        {
            List<cscRecHit2Ds_V2> dataList = new List<cscRecHit2Ds_V2>();
            foreach (var item in data["Collections"][name])
            {
                cscRecHit2Ds_V2 cscRecHit = new cscRecHit2Ds_V2();
                var children = item.Children().Values<string>().ToArray();
                cscRecHit.name = name;
                cscRecHit.u1 = new double[] { Double.Parse(children[0]), Double.Parse(children[1]), Double.Parse(children[2]) };
                cscRecHit.u2 = new double[] { Double.Parse(children[3]), Double.Parse(children[4]), Double.Parse(children[5]) };
                cscRecHit.v1 = new double[] { Double.Parse(children[6]), Double.Parse(children[7]), Double.Parse(children[8]) };
                cscRecHit.v2 = new double[] { Double.Parse(children[9]), Double.Parse(children[10]), Double.Parse(children[11]) };
                cscRecHit.w1 = new double[] { Double.Parse(children[12]), Double.Parse(children[13]), Double.Parse(children[14]) };
                cscRecHit.w2 = new double[] { Double.Parse(children[15]), Double.Parse(children[16]), Double.Parse(children[17]) };
                cscRecHit.endcap = (int)Double.Parse(children[18]);
                cscRecHit.station = (int)Double.Parse(children[19]);
                cscRecHit.ring = (int)Double.Parse(children[20]);
                cscRecHit.chamber = (int)Double.Parse(children[21]);
                cscRecHit.layer = (int)Double.Parse(children[22]);
                cscRecHit.tpeak = Double.Parse(children[23]);
                cscRecHit.positionWithinStrip = Double.Parse(children[24]);
                cscRecHit.errorWithinStrip = Double.Parse(children[25]);
                cscRecHit.strips = children[26];
                cscRecHit.WireGroups = children[27];
                dataList.Add(cscRecHit);
            }
            return dataList;
        }
     
        static public List<string> GenerateCSCRecHits(List<cscRecHit2Ds_V2> inputData)
        {
            List<string> geometryData = new List<string>();
            int vertexIndex = 1;
            float size = 0.0005f;
            //float size = 1;
            foreach (cscRecHit2Ds_V2 point in inputData)
            {
                //float size = (float) point.errorWithinStrip;
                GeneratePrism(point.u1, point.u2, ref geometryData, ref vertexIndex, size);
                GeneratePrism(point.v1, point.v2, ref geometryData, ref vertexIndex, size);
                GeneratePrism(point.w1, point.w2, ref geometryData, ref vertexIndex, size);
            }
            return geometryData;
        }
    }
}
