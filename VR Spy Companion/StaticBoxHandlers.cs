using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Globalization;

namespace VR_Spy_Companion
{
        static class StaticBoxHandlers
        {
    
            static public List<MuonChamberData> muonChamberParse(JObject data)
            {
                /* 
                 * UGH, I have to work on a way to get the canvases in unity aligned properly with the muon chambers so that we can display 
                 * detector ID. I want to try changing the way the muon chamber data gets parsed out here. The problem is that the CSCs
                 * narrow as they move inwards, so I need to set the vertical/horizontal component to only choose the smaller one.
                 * Doing that will make sure that the canvasses don't extend past the models and the detector ID can be shown 
                 * directly on the chamber. Add a ternary operator that chooses the centroid vector based on whether back1 or front1 is closer to origin
                */
                var dataList = new List<MuonChamberData>();
                var vectorlist = new List<string>();
                int idNumber = 0;
                var V = Vector<double>.Build;
                if (data["Collections"]["MuonChambers_V1"] != null)
                {
                    foreach (var igChamberData in data["Collections"]["MuonChambers_V1"])
                    {
                        if (vectorlist.Contains(igChamberData[1].ToString()))
                        {
                            continue;
                        }
                        else
                        {
                            vectorlist.Add(igChamberData[1].ToString());
                        }
                        MuonChamberData muonChamberData = new MuonChamberData();
                        var children = igChamberData.Children().Values<double>().ToArray();

                        muonChamberData.name = "MuonChambers_V1";
                        muonChamberData.id = idNumber;
                        muonChamberData.detid = (int)children[0];
                        muonChamberData.front_1 = new double[] { children[1], children[2], children[3] };
                        muonChamberData.front_2 = new double[] { children[4], children[5], children[6] };
                        muonChamberData.front_3 = new double[] { children[7], children[8], children[9] };
                        muonChamberData.front_4 = new double[] { children[10], children[11], children[12] };
                        muonChamberData.back_1 = new double[] { children[13], children[14], children[15] };
                        muonChamberData.back_2 = new double[] { children[16], children[17], children[18] };
                        muonChamberData.back_3 = new double[] { children[19], children[20], children[21] };
                        muonChamberData.back_4 = new double[] { children[22], children[23], children[24] };
                        muonChamberData.vertical = new double[] { children[1] - children[10], children[2] - children[11], children[3] - children[12] };
                        muonChamberData.horizontal = new double[] { children[1] - children[4], children[2] - children[5], children[3] - children[6] };

                        var front1 = V.DenseOfArray(muonChamberData.front_1);
                        var back1 = V.DenseOfArray(muonChamberData.back_1);

                        if (front1.L2Norm() < back1.L2Norm())
                        {
                            muonChamberData.centroid = new double[] { (children[1] + children[4] + children[7] + children[10]) / 4.0, (children[2] + children[5] + children[8] + children[11])/4.0,
                        (children[3]+children[6]+children[9]+children[12])/4.0};
                        }
                        else
                        {
                            muonChamberData.centroid = new double[] { (children[13] + children[16] + children[19] + children[22]) / 4.0, (children[14] + children[17] + children[20] + children[23])/4.0,
                    (children[15]+children[18]+children[21]+children[24])/4.0};
                        }


                        idNumber++;

                        dataList.Add(muonChamberData);
                    }
                }
                return dataList;
            }
            static public void generateMuonChamberModels(List<MuonChamberData> data, string eventTitle)
            {
                if (data.Count() == 0) { return; }

                List<string> dataStrings = new List<string>();
                List<string> normals = new List<string>();
                int index = 0;
                int counter = 1;
                //dataStrings.Add("vn -1.0000 -0.0000 -0.0000\nvn -0.0000 -0.0000 -1.0000\nvn 1.0000 -0.0000 -0.0000\nvn -0.0000 -0.0000 1.0000\nvn -0.0000 -1.0000 -0.0000\nvn -0.0000 1.0000 -0.0000");

                foreach (var chamber in data)
                {
                    dataStrings.Add($"o MuonChamber_{index}");
                    dataStrings.Add($"v {String.Join(' ', chamber.front_1)}");
                    dataStrings.Add($"v {String.Join(' ', chamber.front_2)}");
                    dataStrings.Add($"v {String.Join(' ', chamber.front_3)}");
                    dataStrings.Add($"v {String.Join(' ', chamber.front_4)}");
                    dataStrings.Add($"v {String.Join(' ', chamber.back_1)}");
                    dataStrings.Add($"v {String.Join(' ', chamber.back_2)}");
                    dataStrings.Add($"v {String.Join(' ', chamber.back_3)}");
                    dataStrings.Add($"v {String.Join(' ', chamber.back_4)}");
                  
                    dataStrings.Add($"f {counter + 3} {counter + 2} {counter + 1} {counter}");
                    dataStrings.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
                    dataStrings.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
                    dataStrings.Add($"f {counter + 4} {counter + 7} {counter + 3} {counter}");
                    dataStrings.Add($"f {counter + 2} {counter + 3} {counter + 7} {counter + 6}");
                    dataStrings.Add($"f {counter} {counter + 1} {counter + 5} {counter + 4}");
                    index++;
                    counter += 8;
                }
                File.WriteAllText($"{eventTitle}\\MuonChambers_V1.obj", String.Empty);
                File.WriteAllLines($"{eventTitle}\\MuonChambers_V1.obj", dataStrings);
            }
        static public List<CalorimetryTowers> setCaloScale(List<CalorimetryTowers> towers)
        {
            double scaler = towers.Select(x => x.energy).Max();
            List<CalorimetryTowers> result = towers;
            for(int i = 0;i<towers.Count();i++)
            {
                CalorimetryTowers calo = towers[i];
                calo.scale = towers[i].energy / scaler;
                towers[i] = calo;
            }

            return towers;
        }
            static public List<CalorimetryTowers> genericCaloParse(JObject data, string name)// double scale)
            {
                List<CalorimetryTowers> dataList = new List<CalorimetryTowers>();
                foreach (var item in data["Collections"][name])
                {
                    CalorimetryTowers caloItem = new CalorimetryTowers();
                    var children = item.Children().Values<double>().ToArray();
                    caloItem.energy = children[0];
                    //caloItem.scale = caloItem.energy / scale;
                    caloItem.eta = children[1];
                    caloItem.phi = children[2];
                    caloItem.time = children[3];
                    caloItem.detid = (int)children[4];
                    caloItem.front_1 = new double[] { children[5], children[6], children[7] };
                    caloItem.front_2 = new double[] { children[8], children[9], children[10] };
                    caloItem.front_3 = new double[] { children[11], children[12], children[13] };
                    caloItem.front_4 = new double[] { children[14], children[15], children[16] };
                    caloItem.back_1 = new double[] { children[17], children[18], children[19] };
                    caloItem.back_2 = new double[] { children[20], children[21], children[22] };
                    caloItem.back_3 = new double[] { children[23], children[24], children[25] };
                    caloItem.back_4 = new double[] { children[26], children[27], children[28] };
                    dataList.Add(caloItem);
                }

                return dataList;
            }
            /*public void makeHFRec()
            {
                HFData = genericCaloParse("HFRecHits_V2", HFSCALE);
                if (HFData.Count == 0)
                {
                    File.WriteAllText($"{eventTitle}\\6_HFRecHits_V2.obj", String.Empty);
                    return;
                }
                List<string> dataList = generateCalorimetryBoxes(HFData);
                File.WriteAllText($"{eventTitle}\\6_HFRecHits_V2.obj", String.Empty);
                File.WriteAllLines($"{eventTitle}\\6_HFRecHits_V2.obj", dataList);
            }
            public void makeHBRec()
            {
                HBData = genericCaloParse("HBRecHits_V2", HBSCALE);
                List<string> dataList = generateCalorimetryBoxes(HBData);
                if (HBData.Count == 0)
                {
                    File.WriteAllText($"{eventTitle}\\6_HBRecHits_V2.obj", String.Empty);
                    return;
                }
                File.WriteAllText($"{eventTitle}\\6_HBRecHits_V2.obj", String.Empty);
                File.WriteAllLines($"{eventTitle}\\6_HBRecHits_V2.obj", dataList);
            }
            public void makeHERec()
            {
                HEData = genericCaloParse("HERecHits_V2", HESCALE);
                List<string> dataList = generateCalorimetryBoxes(HEData);
                if (HEData.Count == 0)
                {
                    File.WriteAllText($"{eventTitle}\\6_HERecHits_V2.obj", String.Empty);
                    return;
                }
                File.WriteAllText($"{eventTitle}\\6_HERecHits_V2.obj", String.Empty);
                File.WriteAllLines($"{eventTitle}\\6_HERecHits_V2.obj", dataList);
            }
            public void makeHORec()
            {
                HOData = genericCaloParse("HORecHits_V2", HOSCALE);
                List<string> dataList = generateCalorimetryTowers(HOData);
                if (HOData.Count == 0)
                {
                    File.WriteAllText($"{eventTitle}\\6_HORecHits_V2.obj", String.Empty);

                    return;
                }
                File.WriteAllText($"{eventTitle}\\6_HORecHits_V2.obj", String.Empty);
                File.WriteAllLines($"{eventTitle}\\6_HORecHits_V2.obj", dataList);
            }
            public void makeEBRec()
            {
                EBData = genericCaloParse("EBRecHits_V2", EBSCALE);
                List<string> dataList = generateCalorimetryTowers(EBData);
                if (EBData.Count == 0)
                {
                    File.WriteAllText($"{eventTitle}\\5_EBRecHits_V2.obj", String.Empty);
                    return;
                }
                File.WriteAllText($"{eventTitle}\\5_EBRecHits_V2.obj", String.Empty);
                File.WriteAllLines($"{eventTitle}\\5_EBRecHits_V2.obj", dataList);
            }
            public void makeEERec()
            {
                EEData = genericCaloParse("EERecHits_V2", EESCALE);
                List<string> dataList = generateCalorimetryTowers(EEData);
                if (EEData.Count == 0)
                {
                    File.WriteAllText($"{eventTitle}\\5_EERecHits_V2.obj", String.Empty);
                    return;
                }
                File.WriteAllText($"{eventTitle}\\5_EERecHits_V2.obj", String.Empty);
                File.WriteAllLines($"{eventTitle}\\5_EERecHits_V2.obj", dataList);
            }
            public void makeESRec()
            {
                ESData = genericCaloParse("ESRecHits_V2", ESSCALE);
                List<string> dataList = generateCalorimetryTowers(ESData);
                if (ESData.Count == 0)
                {
                    File.WriteAllText($"{eventTitle}\\5_ESRecHits_V2.obj", String.Empty);

                    return;
                }
                File.WriteAllText($"{eventTitle}\\5_ESRecHits_V2.obj", String.Empty);
                File.WriteAllLines($"{eventTitle}\\5_ESRecHits_V2.obj", dataList);
            }*/
            static public List<JetV1Data> jetV1Parse(JObject data)
            {
                int idNumber = 0;
                List<JetV1Data> datalist = new List<JetV1Data>();

                foreach (var item in data["Collections"]["PFJets_V1"])
                {

                    JetV1Data currentJet = new JetV1Data();
                    var children = item.Children().Values<double>().ToArray();

                    currentJet.id = idNumber;
                    currentJet.et = children[0];
                    currentJet.eta = children[1];
                    currentJet.theta = children[2];
                    currentJet.phi = children[3];

                    idNumber++;
                    datalist.Add(currentJet);
                }

                return datalist;
            }
        static public List<JetV2Data> jetV2Parse(JObject data)
        {
            int idNumber = 0;
            List<JetV2Data> datalist = new List<JetV2Data>();

            foreach (var item in data["Collections"]["PFJets_V2"])
            {

                JetV2Data currentJet = new JetV2Data();
                var children = item.Children().Values<double>().ToArray();

                currentJet.id = idNumber;
                currentJet.et = children[0];
                currentJet.eta = children[1];
                currentJet.theta = children[2];
                currentJet.phi = children[3];
                currentJet.vertex = new[] { children[4], children[5], children[6] };

                idNumber++;
                datalist.Add(currentJet);
            }

            return datalist;
        }
        static public void generateJetModels(List<JetV1Data> data, string eventTitle)
            {
                double maxZ = 2.25;
                double maxR = 1.10;
                double radius = 0.3 * (1.0 / (1 + 0.001));
                int numSections = 64;
                int iterNumber = 0;
                int index = 0;
                List<string> dataList = new List<string>();

                foreach (var item in data)
                {
                    iterNumber++;
                    double ct = Math.Cos(item.theta);
                    double st = Math.Sin(item.theta);

                    double length1 = (ct != 0.0) ? maxZ / Math.Abs(ct) : maxZ;
                    double length2 = (st != 0.0) ? maxR / Math.Abs(st) : maxR;
                    double length = length1 < length2 ? length1 : length2;

                    dataList = jetGeometry(item, radius, length, numSections, index, dataList);
                    index++;
                }
                File.WriteAllLines($"{eventTitle}//PFJets.obj", dataList);
            }
        static public void generateJetModels(List<JetV2Data> data, string eventTitle)
        {
            double maxZ = 2.25;
            double maxR = 1.10;
            double radius = 0.3 * (1.0 / (1 + 0.001));
            int numSections = 64;
            int iterNumber = 0;
            int index = 0;
            List<string> dataList = new List<string>();

            foreach (var item in data)
            {
                iterNumber++;
                double ct = Math.Cos(item.theta);
                double st = Math.Sin(item.theta);

                double length1 = (ct != 0.0) ? maxZ / Math.Abs(ct) : maxZ;
                double length2 = (st != 0.0) ? maxR / Math.Abs(st) : maxR;
                double length = length1 < length2 ? length1 : length2;

                dataList = jetGeometry(item, radius, length, numSections, index, dataList);
                index++;
            }
            File.WriteAllLines($"{eventTitle}//0_PFJets_V2.obj", dataList);
        }
        static public List<RPCRecHit> RPCRecHitParse(JObject data)
        {
            var inputData = data["Collections"]["RPCRecHits_V1"];
            var dataList = new List<RPCRecHit>();
            foreach(var item in inputData)
            {
                var newItem = new RPCRecHit();

                var children = item.Children().Values<double>().ToArray();

                newItem.u1 = children[0..3];
                newItem.u2 = children[3..6];
                newItem.v1 = children[6..9];
                newItem.v2 = children[9..12];
                newItem.w1 = children[12..15];
                newItem.w2 = children[15..18];
                newItem.region = (int)children[18];
                newItem.ring = (int)children[19];
                newItem.sector = (int)children[20];
                newItem.station = (int)children[21];
                newItem.layer = (int)children[22];
                newItem.subsector = (int)children[23];
                newItem.roll = (int)children[24];
                newItem.detid = (int)children[25];

                dataList.Add(newItem);
            }
            return dataList;
        }
        static public List<string> GenerateRPCRecHits(List<RPCRecHit> data)
        {
            var dataList = new List<string>();
            int counter = 0;
            foreach(var hit in data)
            {
                dataList.Add($"o RPCRecHits_V1_{counter}");
                dataList.Add("v "+String.Join(' ',hit.u1));
                dataList.Add($"v {hit.u1[0]} {hit.u1[1]+0.001} {hit.u1[2]}");
                dataList.Add("v " + String.Join(' ', hit.u2));
                dataList.Add($"v {hit.u2[0]} {hit.u2[1] + 0.001} {hit.u2[2]}");
                dataList.Add("v "+String.Join(' ',hit.v1));
                dataList.Add($"v {hit.v1[0]} {hit.v1[1] + 0.001} {hit.v1[2]}");
                dataList.Add("v "+String.Join(' ',hit.v2));
                dataList.Add($"v {hit.v2[0]} {hit.v2[1] + 0.001} {hit.v2[2]}");
                dataList.Add("v "+String.Join(' ',hit.w1));
                dataList.Add($"v {hit.w1[0]} {hit.w1[1] + 0.001} {hit.w1[2]}");
                dataList.Add("v "+String.Join(' ',hit.w2));
                dataList.Add($"v {hit.w2[0]} {hit.w2[1] + 0.001} {hit.w2[2]}");
                dataList.Add($"f {counter + 1} {counter + 2} {counter + 3} {counter + 4}");
                dataList.Add($"f {counter + 4} {counter + 3} {counter + 2} {counter + 1}");
                dataList.Add($"f {counter + 5} {counter + 6} {counter + 7} {counter + 8}");
                dataList.Add($"f {counter + 8} {counter + 7} {counter + 6} {counter + 5}");
                dataList.Add($"f {counter + 9} {counter + 10} {counter + 11} {counter + 12}");
                dataList.Add($"f {counter + 12} {counter + 11} {counter + 10} {counter + 9}");

                counter += 12;
            }
            return dataList;
        }
        static public List<string> jetGeometry(JetV1Data item, double radius, double length, int sections, int index, List<string> dataList)
            {
                List<string> normals = new List<string>();
                List<string> normals1 = new List<string>();
                List<string> normals2 = new List<string>();
                List<string> section1 = new List<string>();
                List<string> topsection = new List<string>();
                List<Vector3D> radialpoints = new List<Vector3D>();
                var M = Matrix<double>.Build;
                var V = Vector<double>.Build;

                double[,] xRot =
                    { { 1, 0, 0 },
                { 0, Math.Cos(item.theta), -1.0 * Math.Sin(item.theta) },
                { 0, Math.Sin(item.theta), Math.Cos(item.theta) } };

                double[,] zRot =
                    { { Math.Cos(item.phi+Math.PI/2.0), -1.0 * Math.Sin(item.phi+Math.PI/2.0), 0 },
                { Math.Sin(item.phi+Math.PI/2.0), Math.Cos(item.phi+Math.PI/2.0), 0 },
                { 0, 0, 1 } };

                var rx = M.DenseOfArray(xRot); //Rotation matrices
                var rz = M.DenseOfArray(zRot);
                normals.Add($"o Jets_{index}");


                for (double i = 1.0; i <= sections; i++)
                {
                    double radian = (2.0 * i * Math.PI) / (double)sections;

                    string bottompoint = "v 0 0 0";
                    section1.Add(bottompoint);

                    double[] feederArray = { radius * Math.Cos(radian), radius * Math.Sin(radian), length };
                    Vector<double> temptop = Vector<double>.Build.DenseOfArray(feederArray);

                    var rotation = rz * rx;
                    var top = rotation * temptop;

                    //We can use the toppoint list as the vector list to generate normals with. Make a new for loop to handle this
                    string toppoint = $"v {top[0]} {top[1]} {top[2]}";
                    topsection.Add(toppoint);
                    radialpoints.Add(new Vector3D(top[0], top[1], top[2]));
                }
                section1.AddRange(topsection);
                for (int i = 0; i < radialpoints.Count; i++)
                {
                    if (i == radialpoints.Count - 1)
                    {
                        var vector_1 = radialpoints[i];
                        var vector_2 = radialpoints[0];
                        Vector3D norm = vector_1.CrossProduct(vector_2);
                        normals.Add($"vn {norm.X} {norm.Y} {norm.Z}");
                        normals2.Add($"vn {-norm.X} {-norm.Y} {-norm.Z}");
                        break;
                    }
                    var vector1 = radialpoints[i];
                    var vector2 = radialpoints[i + 1];

                    Vector3D normalresult = vector1.CrossProduct(vector2);
                    normals.Add($"vn {normalresult.X} {normalresult.Y} {normalresult.Z}");
                    normals2.Add($"vn {-normalresult.X} {-normalresult.Y} {-normalresult.Z}");
                }
                normals.AddRange(normals2);
                int n = 1;

                while (n < sections)
                {
                    string face = $"f {n + (2 * sections * index)}//{n + (2 * sections * index)} {n + sections + (2 * sections * index)}//{n + (2 * sections * index)} {n + 1 + sections + (2 * sections * index)}//{n + (2 * sections * index)} {n + 1 + (2 * sections * index)}//{n + (2 * sections * index)}";
                    //string face = $"f {n} {n + sections} {n + 1 + sections} {n + 1}";
                    string revface = $"f {n + 1 + (2 * sections * index)}//{n + sections + (2 * sections * index)} {n + 1 + sections + (2 * sections * index)}//{n + sections + (2 * sections * index)} {n + sections + (2 * sections * index)}//{n + sections + (2 * sections * index)} {n + (2 * sections * index)}//{n + sections + (2 * sections * index)}";
                    section1.Add(face);
                    section1.Add(revface);
                    n++;
                }

                section1.Add($"f {2 * sections * index + sections}//{2 * sections * index + sections} {2 * sections * index + 2 * sections}//{2 * sections * index + sections} {2 * sections * index + sections + 1}//{2 * sections * index + sections} {2 * sections * index + 1}//{2 * sections * index + sections}\n" +
                    $"f {2 * sections * index + 1}//{2 * sections * index + 2 * sections} {2 * sections * index + sections + 1}//{2 * sections * index + 2 * sections} {2 * sections * index + 2 * sections}//{2 * sections * index + 2 * sections} {2 * sections * index + sections}//{2 * sections * index + 2 * sections}");
                normals.AddRange(section1);
                dataList.AddRange(normals);
                return dataList;
            }
        static public List<string> jetGeometry(JetV2Data item, double radius, double length, int sections, int index, List<string> dataList)
        {
            List<string> normals = new List<string>();
            List<string> normals1 = new List<string>();
            List<string> normals2 = new List<string>();
            List<string> section1 = new List<string>();
            List<string> topsection = new List<string>();
            List<Vector3D> radialpoints = new List<Vector3D>();
            var M = Matrix<double>.Build;
            var V = Vector<double>.Build;

            double[,] xRot =
                { { 1, 0, 0 },
                { 0, Math.Cos(item.theta), -1.0 * Math.Sin(item.theta) },
                { 0, Math.Sin(item.theta), Math.Cos(item.theta) } };

            double[,] zRot =
                { { Math.Cos(item.phi+Math.PI/2.0), -1.0 * Math.Sin(item.phi+Math.PI/2.0), 0 },
                { Math.Sin(item.phi+Math.PI/2.0), Math.Cos(item.phi+Math.PI/2.0), 0 },
                { 0, 0, 1 } };

            var rx = M.DenseOfArray(xRot); //Rotation matrices
            var rz = M.DenseOfArray(zRot);
            normals.Add($"o Jets_{index}");


            for (double i = 1.0; i <= sections; i++)
            {
                double radian = (2.0 * i * Math.PI) / (double)sections;

                string bottompoint = $"v {item.vertex[0]} {item.vertex[1]} {item.vertex[2]}";
                section1.Add(bottompoint);

                double[] feederArray = { radius * Math.Cos(radian), radius * Math.Sin(radian), length };
                Vector<double> temptop = Vector<double>.Build.DenseOfArray(feederArray);

                var rotation = rz * rx;
                var top = rotation * temptop;

                //We can use the toppoint list as the vector list to generate normals with. Make a new for loop to handle this
                string toppoint = $"v {item.vertex[0]+top[0]} {item.vertex[1]+top[1]} {item.vertex[2]+top[2]}";
                topsection.Add(toppoint);
                radialpoints.Add(new Vector3D(item.vertex[0] + top[0], item.vertex[1] + top[1], item.vertex[2] + top[2]));
            }
            section1.AddRange(topsection);
            for (int i = 0; i < radialpoints.Count; i++)
            {
                if (i == radialpoints.Count - 1)
                {
                    var vector_1 = radialpoints[i];
                    var vector_2 = radialpoints[0];
                    Vector3D norm = vector_1.CrossProduct(vector_2);
                    normals.Add($"vn {norm.X} {norm.Y} {norm.Z}");
                    normals2.Add($"vn {-norm.X} {-norm.Y} {-norm.Z}");
                    break;
                }
                var vector1 = radialpoints[i];
                var vector2 = radialpoints[i + 1];

                Vector3D normalresult = vector1.CrossProduct(vector2);
                normals.Add($"vn {normalresult.X} {normalresult.Y} {normalresult.Z}");
                normals2.Add($"vn {-normalresult.X} {-normalresult.Y} {-normalresult.Z}");
            }
            normals.AddRange(normals2);
            int n = 1;

            while (n < sections)
            {
                string face = $"f {n + (2 * sections * index)}//{n + (2 * sections * index)} {n + sections + (2 * sections * index)}//{n + (2 * sections * index)} {n + 1 + sections + (2 * sections * index)}//{n + (2 * sections * index)} {n + 1 + (2 * sections * index)}//{n + (2 * sections * index)}";
                //string face = $"f {n} {n + sections} {n + 1 + sections} {n + 1}";
                string revface = $"f {n + 1 + (2 * sections * index)}//{n + sections + (2 * sections * index)} {n + 1 + sections + (2 * sections * index)}//{n + sections + (2 * sections * index)} {n + sections + (2 * sections * index)}//{n + sections + (2 * sections * index)} {n + (2 * sections * index)}//{n + sections + (2 * sections * index)}";
                section1.Add(face);
                section1.Add(revface);
                n++;
            }

            section1.Add($"f {2 * sections * index + sections}//{2 * sections * index + sections} {2 * sections * index + 2 * sections}//{2 * sections * index + sections} {2 * sections * index + sections + 1}//{2 * sections * index + sections} {2 * sections * index + 1}//{2 * sections * index + sections}\n" +
                $"f {2 * sections * index + 1}//{2 * sections * index + 2 * sections} {2 * sections * index + sections + 1}//{2 * sections * index + 2 * sections} {2 * sections * index + 2 * sections}//{2 * sections * index + 2 * sections} {2 * sections * index + sections}//{2 * sections * index + 2 * sections}");
            normals.AddRange(section1);
            dataList.AddRange(normals);
            return dataList;
        }
        static public List<string> generateCalorimetryBoxes(List<CalorimetryTowers> inputData)
            {
                List<string> geometryData = new List<string>();
                int counter = 1;

                geometryData.Add("vn -1.0000 -0.0000 -0.0000\nvn -0.0000 -0.0000 -1.0000\nvn 1.0000 -0.0000 -0.0000\nvn -0.0000 -0.0000 1.0000\nvn -0.0000 -1.0000 -0.0000\nvn -0.0000 1.0000 -0.0000");

                var V = Vector<double>.Build;

                foreach (CalorimetryTowers box in inputData)
                {
                    double scale = box.scale;

                    var v0 = V.DenseOfArray(box.front_1);
                    var v1 = V.DenseOfArray(box.front_2);
                    var v2 = V.DenseOfArray(box.front_3);
                    var v3 = V.DenseOfArray(box.front_4);
                    var v4 = V.DenseOfArray(box.back_1);
                    var v5 = V.DenseOfArray(box.back_2);
                    var v6 = V.DenseOfArray(box.back_3);
                    var v7 = V.DenseOfArray(box.back_4);

                    var center = v0 + v1;
                    center += v2;
                    center += v3;
                    center += v4;
                    center += v5;
                    center += v6;
                    center += v7;
                    center /= 8.0;

                    v0 -= center;
                    v0 *= scale;
                    v0 += center;
                    v1 -= center;
                    v1 *= scale;
                    v1 += center;
                    v2 -= center;
                    v2 *= scale;
                    v2 += center;
                    v3 -= center;
                    v3 *= scale;
                    v3 += center;
                    v4 -= center;
                    v4 *= scale;
                    v4 += center;
                    v5 -= center;
                    v5 *= scale;
                    v5 += center;
                    v6 -= center;
                    v6 *= scale;
                    v6 += center;
                    v7 -= center;
                    v7 *= scale;
                    v7 += center;

                    geometryData.Add($"v {String.Join(' ', v0)}");
                    geometryData.Add($"v {String.Join(' ', v1)}");
                    geometryData.Add($"v {String.Join(' ', v2)}");
                    geometryData.Add($"v {String.Join(' ', v3)}");
                    geometryData.Add($"v {String.Join(' ', v4)}");
                    geometryData.Add($"v {String.Join(' ', v5)}");
                    geometryData.Add($"v {String.Join(' ', v6)}");
                    geometryData.Add($"v {String.Join(' ', v7)}");

                    geometryData.Add($"f {counter}//1 {counter + 1}//1 {counter + 2}//1 {counter + 3}//1");
                    geometryData.Add($"f {counter + 3}//1 {counter + 2}//1 {counter + 1}//1 {counter}//1");
                    geometryData.Add($"f {counter + 4}//2 {counter + 5}//2 {counter + 6}//2 {counter + 7}//2");
                    geometryData.Add($"f {counter + 7}//2 {counter + 6}//2 {counter + 5}//2 {counter + 4}//2");
                    geometryData.Add($"f {counter}//3 {counter + 3}//3 {counter + 7}//3 {counter + 4}//3");
                    geometryData.Add($"f {counter + 4}//3 {counter + 7}//3 {counter + 3}//3 {counter}//3");
                    geometryData.Add($"f {counter + 1}//4 {counter + 2}//4 {counter + 6}//4 {counter + 5}//4");
                    geometryData.Add($"f {counter + 5}//4 {counter + 6}//4 {counter + 2}//4 {counter + 1}//4");
                    geometryData.Add($"f {counter + 3}//5 {counter + 2}//5 {counter + 6}//5 {counter + 7}//5");
                    geometryData.Add($"f {counter + 7}//5 {counter + 6}//5 {counter + 2}//5 {counter + 3}//5");
                    geometryData.Add($"f {counter + 1}//6 {counter}//6 {counter + 4}//6 {counter + 5}//6");
                    geometryData.Add($"f {counter + 5}//6 {counter + 4}//6 {counter}//6 {counter + 1}//6");

                    counter += 8;
                }
                return geometryData;
            }
            static public List<string> generateCalorimetryTowers(List<CalorimetryTowers> inputData)
            {
                List<string> geometryData = new List<string>();
                int counter = 1;

                geometryData.Add("vn -1.0000 -0.0000 -0.0000\nvn -0.0000 -0.0000 -1.0000\nvn 1.0000 -0.0000 -0.0000\nvn -0.0000 -0.0000 1.0000\nvn -0.0000 -1.0000 -0.0000\nvn -0.0000 1.0000 -0.0000");

                var V = Vector<double>.Build;

                foreach (CalorimetryTowers box in inputData)
                {
                    var v0 = V.DenseOfArray(box.front_1);
                    var v1 = V.DenseOfArray(box.front_2);
                    var v2 = V.DenseOfArray(box.front_3);
                    var v3 = V.DenseOfArray(box.front_4);
                    var v4 = V.DenseOfArray(box.back_1);
                    var v5 = V.DenseOfArray(box.back_2);
                    var v6 = V.DenseOfArray(box.back_3);
                    var v7 = V.DenseOfArray(box.back_4);

                    v4 -= v0;
                    v5 -= v1;
                    v6 -= v2;
                    v7 -= v3;

                    double v4mag = v4.L2Norm();
                    double v5mag = v5.L2Norm();
                    double v6mag = v6.L2Norm();
                    double v7mag = v7.L2Norm();

                    v4 /= v4mag;
                    v5 /= v5mag;
                    v6 /= v6mag;
                    v7 /= v7mag;
                    double scale = (box.energy / box.scale);
                    v4 *= (box.scale);
                    v5 *= (box.scale);
                    v6 *= (box.scale);
                    v7 *= (box.scale);

                    v4 += v0;
                    v5 += v1;
                    v6 += v2;
                    v7 += v3;

                    geometryData.Add($"v {String.Join(' ', v0)}");
                    geometryData.Add($"v {String.Join(' ', v1)}");
                    geometryData.Add($"v {String.Join(' ', v2)}");
                    geometryData.Add($"v {String.Join(' ', v3)}");
                    geometryData.Add($"v {String.Join(' ', v4)}");
                    geometryData.Add($"v {String.Join(' ', v5)}");
                    geometryData.Add($"v {String.Join(' ', v6)}");
                    geometryData.Add($"v {String.Join(' ', v7)}");

                    geometryData.Add($"f {counter}//1 {counter + 1}//1 {counter + 2}//1 {counter + 3}//1");
                    geometryData.Add($"f {counter + 3}//1 {counter + 2}//1 {counter + 1}//1 {counter}//1");
                    geometryData.Add($"f {counter + 4}//2 {counter + 5}//2 {counter + 6}//2 {counter + 7}//2");
                    geometryData.Add($"f {counter + 7}//2 {counter + 6}//2 {counter + 5}//2 {counter + 4}//2");
                    geometryData.Add($"f {counter}//3 {counter + 3}//3 {counter + 7}//3 {counter + 4}//3");
                    geometryData.Add($"f {counter + 4}//3 {counter + 7}//3 {counter + 3}//3 {counter}//3");
                    geometryData.Add($"f {counter + 1}//4 {counter + 2}//4 {counter + 6}//4 {counter + 5}//4");
                    geometryData.Add($"f {counter + 5}//4 {counter + 6}//4 {counter + 2}//4 {counter + 1}//4");
                    geometryData.Add($"f {counter + 3}//5 {counter + 2}//5 {counter + 6}//5 {counter + 7}//5");
                    geometryData.Add($"f {counter + 7}//5 {counter + 6}//5 {counter + 2}//5 {counter + 3}//5");
                    geometryData.Add($"f {counter + 1}//6 {counter}//6 {counter + 4}//6 {counter + 5}//6");
                    geometryData.Add($"f {counter + 5}//6 {counter + 4}//6 {counter}//6 {counter + 1}//6");

                    counter += 8;
                }
                return geometryData;
            }
            /*public void setScales()
            {
                //Hadronic scaling factor is equivalent to the largest energy value in each respective set (HE,HB,HO,HF)
                List<string> CALSETS = new List<string>() { "HERecHits_V2", "HBRecHits_V2", "HFRecHits_V2", "HORecHits_V2", "EBRecHits_V2", "ESRecHits_V2", "EERecHits_V2" };
                foreach (string CALSET in CALSETS)
                {
                    var collection = data["Collections"][CALSET];

                    if (collection == null || collection.HasValues == false)
                    {
                        continue;
                    }

                    List<double> energies = new List<double>();
                    foreach (var item in collection)
                    {
                        energies.Add((double)item[0].Value<double>());
                    }

                    double scaleEnergy = energies.ToArray().Max();

                    switch (CALSET)
                    {
                        case "HERecHits_V2":
                            HESCALE = scaleEnergy;
                            break;
                        case "HBRecHits_V2":
                            HBSCALE = scaleEnergy;
                            break;
                        case "HFRecHits_V2":
                            HFSCALE = scaleEnergy;
                            break;
                        case "HORecHits_V2":
                            HOSCALE = scaleEnergy;
                            break;
                        case "EBRecHits_V2":
                            EBSCALE = scaleEnergy;
                            break;
                        case "EERecHits_V2":
                            EESCALE = scaleEnergy;
                            break;
                        case "ESRecHits_V2":
                            ESSCALE = scaleEnergy;
                            break;
                    }
                }
            }*/
            static public List<RecHitFraction> recHitFractionsParse(JObject data)
            {
                List<RecHitFraction> dataList = new List<RecHitFraction>();
                if (data["Collections"]["RecHitFractions_V1"] == null)
                {
                    return dataList;
                }
                foreach (var item in data["Collections"]["RecHitFractions_V1"])
                {
                    RecHitFraction thing = new RecHitFraction();
                    var children = item.Children().Values<double>().ToList();
                    thing.detid = (int)children[0];
                    thing.fraction = children[1];
                    thing.front_1 = new[] { children[2], children[3], children[4] };
                    thing.front_2 = new[] { children[5], children[6], children[7] };
                    thing.front_3 = new[] { children[8], children[9], children[10] };
                    thing.front_4 = new[] { children[11], children[12], children[13] };
                    thing.back_1 = new[] { children[14], children[15], children[16] };
                    thing.back_2 = new[] { children[17], children[18], children[19] };
                    thing.back_3 = new[] { children[20], children[21], children[22] };
                    thing.back_4 = new[] { children[23], children[24], children[25] };
                    dataList.Add(thing);
                }
                return dataList;
            }
            /*public List<List<RecHitFraction>> assignRecHitFractions(List<RecHitFraction> extras)
            {
                List<List<RecHitFraction>> dataList = new List<List<RecHitFraction>>();
                int indexer = 0;
                if (data["Associations"]["SuperClusterRecHitFractions_V1"] == null)
                {
                    return dataList;
                }
                foreach (var item in data["Associations"]["SuperClusterRecHitFractions_V1"])
                {
                    int index = item[0][1].Value<int>();
                    if (dataList.Count() < index + 1)
                    {
                        List<RecHitFraction> h = new List<RecHitFraction>();
                        dataList.Add(h);
                    }
                    dataList[index].Add(extras[indexer]);
                    indexer++;
                }
                return dataList;
            }*/
            static public List<SuperCluster> superClusterParse(JObject data)
            {
                List<SuperCluster> dataList = new List<SuperCluster>();
                int idNumber = 0;
                if (data["Collections"]["SuperClusters_V1"] == null)
                {
                    return dataList;
                }
                foreach (var item in data["Collections"]["SuperClusters_V1"])
                {
                    SuperCluster cluster = new SuperCluster();
                    var values = item.Children().Values<string>().ToArray();

                    cluster.id = idNumber;
                    cluster.energy = Double.Parse(values[0]);
                    cluster.pos = new[] { Double.Parse(values[1]), Double.Parse(values[2]), Double.Parse(values[3]) };
                    cluster.eta = Double.Parse(values[4]);
                    cluster.phi = Double.Parse(values[5]);
                    cluster.algo = values[6];
                    cluster.etaWidth = Double.Parse(values[7]);
                    cluster.phiWidth = Double.Parse(values[8]);
                    cluster.rawEnergy = Double.Parse(values[9]);
                    cluster.preshowerEnergy = Double.Parse(values[10]);

                    dataList.Add(cluster);
                    idNumber++;
                }
                return dataList;
            }
            static public void GenerateSuperClusters(List<List<RecHitFraction>> recHits, string eventTitle)
            {
                List<string> dataList = new List<string>();
                List<string> faces = new List<string>();
                int index = 0;
                int counter = 1;
                foreach (var item in recHits)
                {
                    dataList.Add($"o SuperCluster_{index}");

                    foreach (RecHitFraction hit in item)
                    {
                        dataList.Add($"v {String.Join(' ', hit.front_1)}");
                        dataList.Add($"v {String.Join(' ', hit.front_2)}");
                        dataList.Add($"v {String.Join(' ', hit.front_3)}");
                        dataList.Add($"v {String.Join(' ', hit.front_3)}");
                        dataList.Add($"v {String.Join(' ', hit.front_4)}");
                        dataList.Add($"v {String.Join(' ', hit.front_1)}");
                        /*dataList.Add($"v {String.Join(' ', hit.back_1)}");
                        dataList.Add($"v {String.Join(' ', hit.back_2)}");
                        dataList.Add($"v {String.Join(' ', hit.back_3)}");
                        dataList.Add($"v {String.Join(' ', hit.back_4)}");*/

                        faces.Add($"f {counter} {counter + 1} {counter + 2}");
                        faces.Add($"f {counter + 2} {counter + 1} {counter}");
                        faces.Add($"f {counter + 3} {counter + 4} {counter + 5}");
                        faces.Add($"f {counter + 5} {counter + 4} {counter + 3}");
                        counter += 6;
                    }
                    dataList.AddRange(faces);
                    faces.Clear();
                    index++;
                }
                File.WriteAllLines($"{eventTitle}//SuperClusters_V1.obj", dataList);
            }
            /*static List<Vertex> vertexParse()
            {
                List<Vertex> dataList = new List<Vertex>();
                if (data["Collections"]["Vertices_V1"] == null)
                {
                    return dataList;
                }
                foreach (var item in data["Collections"]["Vertices_V1"])
                {
                    var children = item.Children().Values<double>().ToList();
                    Vertex vertex = new Vertex();

                    vertex.isValid = (int)children[0];
                    vertex.isFake = (int)children[1];
                    vertex.pos = new double[] { children[2], children[3], children[4] };
                    vertex.xError = children[5];
                    vertex.yError = children[6];
                    vertex.zError = children[7];
                    vertex.chi2 = children[8];
                    vertex.ndof = children[9];

                    dataList.Add(vertex);
                }

                return dataList;
            }*/
            public static List<Vertex> vertexParse(JObject data, string vertexVersion)
            {
                List<Vertex> dataList = new List<Vertex>();
                if (data["Collections"][vertexVersion] == null)
                {
                    return dataList;
                }
                foreach (var item in data["Collections"][vertexVersion])
                {
                    var children = item.Children().Values<double>().ToList();
                    Vertex vertex = new Vertex();

                    vertex.isValid = (int)children[0];
                    vertex.isFake = (int)children[1];
                    vertex.pos = new double[] { children[2], children[3], children[4] };
                    vertex.xError = children[5];
                    vertex.yError = children[6];
                    vertex.zError = children[7];
                    vertex.chi2 = children[8];
                    vertex.ndof = children[9];

                    dataList.Add(vertex);
                }

                return dataList;
            }
        public static List<Vertex> primaryVertexParse(JObject data)
            {
                List<Vertex> dataList = new List<Vertex>();
             
                foreach (var item in data["Collections"]["PrimaryVertices_V1"])
                {
                    var children = item.Children().Values<double>().ToList();
                    Vertex vertex = new Vertex();

                    vertex.isValid = (int)children[0];
                    vertex.isFake = (int)children[1];
                    vertex.pos = new double[] { children[2], children[3], children[4] };
                    vertex.xError = children[5];
                    vertex.yError = children[6];
                    vertex.zError = children[7];
                    vertex.chi2 = children[8];
                    vertex.ndof = children[9];

                    dataList.Add(vertex);
                }

                return dataList;
            }
            public static List<Vertex> secondaryVertexParse(JObject data)
            {
                List<Vertex> dataList = new List<Vertex>();

                foreach (var item in data["Collections"]["SecondaryVertices_V1"])
                {
                    var children = item.Children().Values<double>().ToList();
                    Vertex vertex = new Vertex();

                    vertex.isValid = (int)children[0];
                    vertex.isFake = (int)children[1];
                    vertex.pos = new double[] { children[2], children[3], children[4] };
                    vertex.xError = children[5];
                    vertex.yError = children[6];
                    vertex.zError = children[7];
                    vertex.chi2 = children[8];
                    vertex.ndof = children[9];

                    dataList.Add(vertex);
                }

                return dataList;
            }
            public static void GenerateOBJ(List<(Point3D center, Point3D width)> ellipsoids, string filePath)
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    int vertexCount = 1;
                    int objectCount = 1;
                    int current = 0;

                    foreach (var ellipsoid in ellipsoids)
                    {
                        writer.WriteLine($"o Object{objectCount++}");

                        // Generate vertices
                        List<Point3D> vertices = GenerateEllipsoidVertices(ellipsoid.center, ellipsoid.width);

                        // Write vertices
                        foreach (var vertex in vertices)
                        {
                            writer.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");
                        }

                        // Write faces
                        int n = (int)Math.Sqrt(vertices.Count); // Assuming square grid
                        for (int i = 0; i < n - 1; i++)
                        {
                            for (int j = 0; j < n - 1; j++)
                            {
                                int currentIndex = i * n + j + vertexCount;
                                int nextIndex = currentIndex + 1;
                                int bottomIndex = currentIndex + n;
                                int nextBottomIndex = nextIndex + n;

                                writer.WriteLine($"f {currentIndex} {nextIndex} {nextBottomIndex} {bottomIndex}");
                            }

                        }
                        vertexCount += vertices.Count;
                    }
                }
            }

            private static List<Point3D> GenerateEllipsoidVertices(Point3D center, Point3D width)
            {
                List<Point3D> vertices = new List<Point3D>();

                int segments = 20; // Adjust as needed for smoother ellipsoids
                double thetaStep = 2 * Math.PI / segments;
                double phiStep = Math.PI / segments;

                for (int i = 0; i <= segments; i++)
                {
                    double theta = i * thetaStep;
                    for (int j = 0; j <= segments; j++)
                    {
                        double phi = j * phiStep;
                        double x = center.X + width.X * Math.Sin(phi) * Math.Cos(theta);
                        double y = center.Y + width.Y * Math.Sin(phi) * Math.Sin(theta);
                        double z = center.Z + width.Z * Math.Cos(phi);
                        vertices.Add(new Point3D(x, y, z));
                    }
                }

                return vertices;
            }

            public static void GenerateEllipsoidObj(string filePath, List<Vertex> vertexList, double sigmaFactor)
            {
                int vertexNumber = 0;
                int indexer = 0;
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var item in vertexList)
                    {
                        writer.WriteLine($"o Vertex_{vertexNumber}");
                        double[] pos = item.pos;
                        double xDiameter = sigmaFactor * item.xError; double yDiameter = sigmaFactor * item.yError; double zDiameter = sigmaFactor * item.zError;

                        int index = 0;
                        // Generate vertices
                        int numVertices = 100;
                        for (int i = 0; i < numVertices; i++)
                        {
                            double theta = 2.0 * Math.PI * i / numVertices;
                            for (int j = 0; j < numVertices / 2; j++)
                            {
                                double phi = Math.PI * j / (numVertices / 2);

                                double x = pos[0] + xDiameter * Math.Sin(phi) * Math.Cos(theta);
                                double y = pos[1] + yDiameter * Math.Sin(phi) * Math.Sin(theta);
                                double z = pos[2] + zDiameter * Math.Cos(phi);

                                writer.WriteLine($"v {x} {y} {z}");
                            }
                        }

                        int vIndex = 1; // Vertex indices start from 1 in OBJ format
                        for (int i = 0; i < numVertices; i++)
                        {
                            for (int j = 0; j < numVertices / 2 - 1; j++)
                            {
                                int v1 = vIndex + j;
                                int v2 = vIndex + (j + 1) % (numVertices / 2);
                                int v3 = vIndex + (j + 1) % (numVertices / 2) + numVertices / 2;
                                int v4 = vIndex + j + numVertices / 2;

                                writer.WriteLine($"f {indexer + v1} {indexer + v2} {indexer + v3} {indexer + v4}");
                                //writer.WriteLine($"f {indexer + v1} {indexer + v3} {indexer + v4}");
                                index = v4;
                            }
                            vIndex += numVertices / 2;
                        }
                        indexer += index;
                        vertexNumber++;
                    }
                }

            }
    }
        struct Point3D
        {
            public double X;
            public double Y;
            public double Z;

            public Point3D(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }
    }
