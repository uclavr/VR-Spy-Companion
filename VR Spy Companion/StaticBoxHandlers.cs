﻿using System;
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
using MathNet.Numerics;
using IGtoOBJGen;
using MathNet.Numerics.Distributions;

namespace VR_Spy_Companion
{
        static class StaticBoxHandlers
        {
    
            static public List<MuonChamberData> muonChamberParse(JObject data, string name)
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
                        muonChamberData.name = name;
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
            static public List<String> generateMuonChamberModels(List<MuonChamberData> data)
            {
                int index = 0;
                int counter = 1;
                List<string> geometryData = new List<string>();
                if (data.Count() == 0) { return geometryData; }
                foreach (var chamber in data)
                    {
                    geometryData.Add($"o MuonChamber_{index}");
                    geometryData.Add($"v {String.Join(' ', chamber.front_1)}");
                    geometryData.Add($"v {String.Join(' ', chamber.front_2)}");
                    geometryData.Add($"v {String.Join(' ', chamber.front_3)}");
                    geometryData.Add($"v {String.Join(' ', chamber.front_4)}");
                    geometryData.Add($"v {String.Join(' ', chamber.back_1)}");
                    geometryData.Add($"v {String.Join(' ', chamber.back_2)}");
                    geometryData.Add($"v {String.Join(' ', chamber.back_3)}");
                    geometryData.Add($"v {String.Join(' ', chamber.back_4)}");

                    geometryData.Add($"f {counter + 3} {counter + 2} {counter + 1} {counter}");
                    geometryData.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
                    geometryData.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
                    geometryData.Add($"f {counter + 4} {counter + 7} {counter + 3} {counter}");
                    geometryData.Add($"f {counter + 2} {counter + 3} {counter + 7} {counter + 6}");
                    geometryData.Add($"f {counter} {counter + 1} {counter + 5} {counter + 4}");
                    counter += 8;
                    index++; 
                }
                return geometryData;
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
                    if (children.Length >= 30)//deltaPhi and deltaEta present
                    {
                        caloItem.deltaEta = children[29];
                        caloItem.deltaPhi = children[30];
                    }
                    else // old data
                    {
                        caloItem.deltaEta = 0;
                        caloItem.deltaPhi = 0;
                    }
                    
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
                    File.WriteAllLines($"{eventTitle}\\PFJets.obj", dataList);
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
                File.WriteAllLines($"{eventTitle}\\PFJets_V2.obj", dataList);
            }
            // Fix RPCRecHit Assignment
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
                    //newItem.detid = (int)children[25];

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
                    dataList.Add($"v {hit.u1[0]} {hit.u1[1]+0.01} {hit.u1[2]}");
                    dataList.Add("v " + String.Join(' ', hit.u2));
                    dataList.Add($"v {hit.u2[0]} {hit.u2[1] + 0.01} {hit.u2[2]}");
                    dataList.Add("v "+String.Join(' ',hit.v1));
                    dataList.Add($"v {hit.v1[0]} {hit.v1[1] + 0.01} {hit.v1[2]}");
                    dataList.Add("v "+String.Join(' ',hit.v2));
                    dataList.Add($"v {hit.v2[0]} {hit.v2[1] + 0.01} {hit.v2[2]}");
                    dataList.Add("v "+String.Join(' ',hit.w1));
                    dataList.Add($"v {hit.w1[0]} {hit.w1[1] + 0.01} {hit.w1[2]}");
                    dataList.Add("v "+String.Join(' ',hit.w2));
                    dataList.Add($"v {hit.w2[0]} {hit.w2[1] + 0.01} {hit.w2[2]}");
                    dataList.Add($"f {12*counter + 1} {12 * counter + 2} {12 * counter + 3} {12 * counter + 4}");
                    dataList.Add($"f {12 * counter + 4} {12 * counter + 3} {12 * counter + 2} {12 * counter + 1}");
                    dataList.Add($"f {12 * counter + 5} {12 * counter + 6} {12 * counter + 7} {12 * counter + 8}");
                    dataList.Add($"f {12 * counter + 8} {12 * counter + 7} {12 * counter + 6} {12 * counter + 5}");
                    dataList.Add($"f {12 * counter + 9} {12 * counter + 10} {12 * counter + 11} {12 * counter + 12}");
                    dataList.Add($"f {12 * counter + 12} {12 * counter + 11} {12 * counter + 10} {12 * counter + 9}");

                    counter++;
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
            static public (List<string>,List<CalorimetryTowers>) generateCalorimetryBoxes(List<CalorimetryTowers> inputData, string dataType)
                {
                    List<string> geometryData = new List<string>();
                    int counter = 1;
                    List<CalorimetryTowers> deltas = new List<CalorimetryTowers>();
                    geometryData.Add("vn -1.0000 -0.0000 -0.0000\nvn -0.0000 -0.0000 -1.0000\nvn 1.0000 -0.0000 -0.0000\nvn -0.0000 -0.0000 1.0000\nvn -0.0000 -1.0000 -0.0000\nvn -0.0000 1.0000 -0.0000");

                    var V = Vector<double>.Build;

                    for(int i = 0; i<=inputData.Count-1;i++)
                    {
                        var box = inputData[i];
                        double scale = box.scale;

                        var v0 = V.DenseOfArray(box.front_1);
                        var v1 = V.DenseOfArray(box.front_2);
                        var v2 = V.DenseOfArray(box.front_3);
                        var v3 = V.DenseOfArray(box.front_4);
                        var v4 = V.DenseOfArray(box.back_1);
                        var v5 = V.DenseOfArray(box.back_2);
                        var v6 = V.DenseOfArray(box.back_3);
                        var v7 = V.DenseOfArray(box.back_4);
                        var xVector = V.DenseOfArray(new[] { 1.0, 0.0, 0.0 });
                        var yVector = V.DenseOfArray(new[] { 0.0, 1.0, 0.0 });
                        var zVector = V.DenseOfArray(new[] { 0.0, 0.0, 1.0 });

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

                box.geometricVertices = new List<double[]>();
                box.geometricVertices.Add(v0.ToArray());
                box.geometricVertices.Add(v1.ToArray());
                box.geometricVertices.Add(v2.ToArray());
                box.geometricVertices.Add(v3.ToArray());
                box.geometricVertices.Add(v4.ToArray());
                box.geometricVertices.Add(v5.ToArray());
                box.geometricVertices.Add(v6.ToArray());
                box.geometricVertices.Add(v7.ToArray());
               

                geometryData.Add($"o {dataType}_{i}");
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


                //double deltaPhi = 1.0;
                //double deltaEta = 1.0;

                //double sin_theta1 = Math.Sqrt(Math.Pow(v0[0], 2) + Math.Pow(v0[1], 2)) / v0.L2Norm();
                //double cos_theta1 = v0[2] / v0.L2Norm();
                //double sin_theta4 = Math.Sqrt(Math.Pow(v3[0], 2) + Math.Pow(v3[1], 2)) / v3.L2Norm();
                //double cos_theta4 = v3[2] / v3.L2Norm();
                //double deltaEta = Math.Log(((1 - cos_theta4) / (1 - cos_theta1)) * (sin_theta1 / sin_theta4));
                //Console.WriteLine(deltaEta);


                //box.deltaPhi = deltaPhi;
                //box.deltaEta = deltaEta;
                deltas.Add(box);
                    counter += 8;
                }
                return (geometryData,deltas);
            }
            static public (List<string>, List<CalorimetryTowers>) generateCalorimetryTowers(List<CalorimetryTowers> inputData, string dataType)
            {
                List<string> geometryData = new List<string>();
                int counter = 1;
                List<CalorimetryTowers> deltas = new List<CalorimetryTowers>();
                geometryData.Add("vn -1.0000 -0.0000 -0.0000\nvn -0.0000 -0.0000 -1.0000\nvn 1.0000 -0.0000 -0.0000\nvn -0.0000 -0.0000 1.0000\nvn -0.0000 -1.0000 -0.0000\nvn -0.0000 1.0000 -0.0000");

                var V = Vector<double>.Build;

                for (int i =0; i<inputData.Count;i++)
                {
                var box = inputData[i];
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

                box.geometricVertices = new List<double[]>();
                box.geometricVertices.Add(v0.ToArray());
                box.geometricVertices.Add(v1.ToArray());
                box.geometricVertices.Add(v2.ToArray());
                box.geometricVertices.Add(v3.ToArray());
                box.geometricVertices.Add(v4.ToArray());
                box.geometricVertices.Add(v5.ToArray());
                box.geometricVertices.Add(v6.ToArray());
                box.geometricVertices.Add(v7.ToArray());

                geometryData.Add($"o {dataType}_{i}");
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

                v1 /= v1.L2Norm();
                v2 /= v2.L2Norm();
                v4 /= v4.L2Norm();

                var xVector = V.DenseOfArray(new[] { 1.0, 0.0, 0.0 });
                //double deltaPhi = 1.0;
                //double deltaEta = 1.0;

                //double sin_theta1 = Math.Sqrt(Math.Pow(v0[0], 2) + Math.Pow(v0[1], 2)) / v0.L2Norm();
                //double cos_theta1 = v0[2] / v0.L2Norm();
                //double sin_theta4 = Math.Sqrt(Math.Pow(v1[0], 2) + Math.Pow(v1[1], 2)) / v1.L2Norm();
                //double cos_theta4 = v1[2] / v1.L2Norm();
                //double deltaEta = Math.Log(((1 - cos_theta4) / (1 - cos_theta1)) * (sin_theta1 / sin_theta4));
                

                //box.deltaPhi = deltaPhi;
                //box.deltaEta = deltaEta;
                deltas.Add(box);

                counter += 8;
                }
                return (geometryData,deltas);
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
                File.WriteAllLines($"{eventTitle}\\SuperClusters_V1.obj", dataList);
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
            static public List<TrackerPieceData> trackerPieceParse(JObject data, string name)
            {
                List<TrackerPieceData> dataList = new List<TrackerPieceData>();
                foreach (var item in data["Collections"][name])
                {
                    TrackerPieceData TrackerPieceData = new TrackerPieceData();
                    var children = item.Children().Values<double>().ToArray();
                    TrackerPieceData.name = name;
                    TrackerPieceData.detid = (int)children[0];
                    TrackerPieceData.front_1 = new double[] { children[1], children[2], children[3] };
                    TrackerPieceData.front_2 = new double[] { children[4], children[5], children[6] };
                    TrackerPieceData.front_3 = new double[] { children[7], children[8], children[9] };
                    TrackerPieceData.front_4 = new double[] { children[10], children[11], children[12] };
                    TrackerPieceData.back_1 = new double[] { children[13], children[14], children[15] };
                    TrackerPieceData.back_2 = new double[] { children[16], children[17], children[18] };
                    TrackerPieceData.back_3 = new double[] { children[19], children[20], children[21] };
                    TrackerPieceData.back_4 = new double[] { children[22], children[23], children[24] };
                    dataList.Add(TrackerPieceData);
                }
                return dataList;
            }
            static public List<string> generateTrackerPiece(List<TrackerPieceData> inputData)
            {
                List<string> geometryData = new List<string>();
                int counter = 1;
                foreach (TrackerPieceData box in inputData)
                {

                    geometryData.Add($"v {box.front_1[0]} {box.front_1[1]} {box.front_1[2]}");
                    geometryData.Add($"v {box.front_2[0]} {box.front_2[1]} {box.front_2[2]}");
                    geometryData.Add($"v {box.front_3[0]} {box.front_3[1]} {box.front_3[2]}");
                    geometryData.Add($"v {box.front_4[0]} {box.front_4[1]} {box.front_4[2]}");

                    geometryData.Add($"v {box.back_1[0]} {box.back_1[1]} {box.back_1[2]}");
                    geometryData.Add($"v {box.back_2[0]} {box.back_2[1]} {box.back_2[2]}");
                    geometryData.Add($"v {box.back_3[0]} {box.back_3[1]} {box.back_3[2]}");
                    geometryData.Add($"v {box.back_4[0]} {box.back_4[1]} {box.back_4[2]}");

                    geometryData.Add($"f {counter} {counter + 1} {counter + 5} {counter + 4}"); 
                    geometryData.Add($"f {counter+4} {counter + 5} {counter + 1} {counter}"); 
                    geometryData.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}"); 
                    geometryData.Add($"f {counter + 5} {counter + 6} {counter + 2} {counter + 1}"); 
                    geometryData.Add($"f {counter + 2} {counter + 3} {counter + 7} {counter + 6}"); 
                    geometryData.Add($"f {counter + 6} {counter + 7} {counter + 3} {counter + 2}"); 
                    geometryData.Add($"f {counter + 3} {counter} {counter + 4} {counter + 7}");
                    geometryData.Add($"f {counter + 7} {counter+4} {counter} {counter + 3}"); 

                    counter += 8;
                }
                return geometryData;
            }
            static public List<matchingCSC> matchingCSCParse(JObject data, string name)
            {
                List<matchingCSC> dataList = new List<matchingCSC>();
                foreach (var item in data["Collections"][name])
                {
                    matchingCSC matchingCSC = new matchingCSC();
                    var children = item.Children().Values<double>().ToArray();
                    matchingCSC.name = name;
                    matchingCSC.detid = (int)children[0];
                    matchingCSC.front_1 = new double[] { children[1], children[2], children[3] };
                    matchingCSC.front_2 = new double[] { children[4], children[5], children[6] };
                    matchingCSC.front_3 = new double[] { children[7], children[8], children[9] };
                    matchingCSC.front_4 = new double[] { children[10], children[11], children[12] };
                    matchingCSC.back_1 = new double[] { children[13], children[14], children[15] };
                    matchingCSC.back_2 = new double[] { children[16], children[17], children[18] };
                    matchingCSC.back_3 = new double[] { children[19], children[20], children[21] };
                    matchingCSC.back_4 = new double[] { children[22], children[23], children[24] };
                    dataList.Add(matchingCSC);
                }
                return dataList;
            }
            static public List<string> generateMatchingCSC(List<matchingCSC> inputData)
            {
                List<string> geometryData = new List<string>();
                int counter = 1;
                foreach (matchingCSC box in inputData)
                {

                    geometryData.Add($"v {box.front_1[0]} {box.front_1[1]} {box.front_1[2]}");
                    geometryData.Add($"v {box.front_2[0]} {box.front_2[1]} {box.front_2[2]}");
                    geometryData.Add($"v {box.front_3[0]} {box.front_3[1]} {box.front_3[2]}");
                    geometryData.Add($"v {box.front_4[0]} {box.front_4[1]} {box.front_4[2]}");

                    geometryData.Add($"v {box.back_1[0]} {box.back_1[1]} {box.back_1[2]}");
                    geometryData.Add($"v {box.back_2[0]} {box.back_2[1]} {box.back_2[2]}");
                    geometryData.Add($"v {box.back_3[0]} {box.back_3[1]} {box.back_3[2]}");
                    geometryData.Add($"v {box.back_4[0]} {box.back_4[1]} {box.back_4[2]}");

                    // Front face
                    geometryData.Add($"f {counter} {counter + 1} {counter + 2}");
                    geometryData.Add($"f {counter + 2} {counter + 3} {counter}");
                    geometryData.Add($"f {counter + 2} {counter + 1} {counter}");  // Back-facing front
                    geometryData.Add($"f {counter} {counter + 3} {counter + 2}");

                    // Back face
                    geometryData.Add($"f {counter + 4} {counter + 5} {counter + 6}");
                    geometryData.Add($"f {counter + 6} {counter + 7} {counter + 4}");
                    geometryData.Add($"f {counter + 6} {counter + 5} {counter + 4}");  // Back-facing back
                    geometryData.Add($"f {counter + 4} {counter + 7} {counter + 6}");

                    // Left side
                    geometryData.Add($"f {counter + 4} {counter + 5} {counter + 1}");
                    geometryData.Add($"f {counter + 1} {counter} {counter + 4}");
                    geometryData.Add($"f {counter + 1} {counter + 5} {counter + 4}");  // Back-facing left
                    geometryData.Add($"f {counter + 4} {counter} {counter + 1}");

                    // Right side
                    geometryData.Add($"f {counter + 7} {counter + 6} {counter + 2}");
                    geometryData.Add($"f {counter + 2} {counter + 3} {counter + 7}");
                    geometryData.Add($"f {counter + 2} {counter + 6} {counter + 7}");  // Back-facing right
                    geometryData.Add($"f {counter + 7} {counter + 3} {counter + 2}");

                    // Top face
                    geometryData.Add($"f {counter} {counter + 3} {counter + 7}");
                    geometryData.Add($"f {counter + 7} {counter + 4} {counter}");
                    geometryData.Add($"f {counter + 7} {counter + 3} {counter}");  // Back-facing top
                    geometryData.Add($"f {counter} {counter + 4} {counter + 7}");

                    // Bottom face
                    geometryData.Add($"f {counter + 1} {counter + 5} {counter + 6}");
                    geometryData.Add($"f {counter + 6} {counter + 2} {counter + 1}");
                    geometryData.Add($"f {counter + 6} {counter + 5} {counter + 1}");  // Back-facing bottom
                    geometryData.Add($"f {counter + 1} {counter + 2} {counter + 6}");

                    counter += 8;
                }
                return geometryData;
            }
            static public List<DTRecHitsV1> dtRecHitParse(JObject data, string name)
            {
                List<DTRecHitsV1> dataList = new List<DTRecHitsV1>();
                foreach (var item in data["Collections"][name])
                {
                    DTRecHitsV1 dtRecHit = new DTRecHitsV1();
                    var children = item.Children().Values<double>().ToArray();
                    dtRecHit.name = name;
                    dtRecHit.wireId = (int)children[0];
                    dtRecHit.layerId = (int)children[1];
                    dtRecHit.superLayerId = (int)children[2];
                    dtRecHit.sectorId = (int)children[3];
                    dtRecHit.stationId = (int)children[4];
                    dtRecHit.wheelId = (int)children[5];
                    dtRecHit.digitime = (int)children[6];
                    dtRecHit.wirePos = new double[] { children[7], children[8], children[9] };
                    dtRecHit.lPlusGlobalPos = new double[] { children[10], children[11], children[12] };
                    dtRecHit.lMinusGlobalPos = new double[] { children[13], children[14], children[15] };
                    dtRecHit.rPlusGlobalPos = new double[] { children[16], children[17], children[18] };
                    dtRecHit.rMinusGlobalPos = new double[] { children[19], children[20], children[21] };
                    dtRecHit.lGlobalPos = new double[] { children[22], children[23], children[24] };
                    dtRecHit.rGlobalPos = new double[] { children[25], children[26], children[27] };
                    dtRecHit.axis = new double[] { children[28], children[29], children[30] };
                    dtRecHit.angle = (double)children[31];
                    dtRecHit.cellWidth = (double)children[32];
                    dtRecHit.cellLength = (double)children[33];
                    dtRecHit.cellHeight = (double)children[34];
                    dataList.Add(dtRecHit);
                }
                return dataList;
            }
            static public List<string> generateDTRecHit(List<DTRecHitsV1> inputData)
            {
                List<string> geometryData = new List<string>();
                int counter = 1;
                int objectIndex = 0;
                foreach (DTRecHitsV1 box in inputData)
                {
                    double[] pos = box.wirePos;
                    double[] axis = box.axis;
                    double angle = box.angle;
                    double w = box.cellWidth*0.5;
                    double h = box.cellLength*0.5;
                    double d = box.cellHeight*0.5;
                    double[,] vertices = new double[,]
                    {
                        {-w,  h, -d},
                        { w,  h, -d},
                        { w,  h,  d},
                        {-w,  h,  d},
                        {-w, -h,  d},
                        { w, -h,  d},
                        { w, -h, -d},
                        {-w, -h, -d}
                    };
                    static double[] RotatePointAroundAxis(double[] point, double[] axis, double angle)
                    {
                        double ux = axis[0];
                        double uy = axis[1];
                        double uz = axis[2];
                        double cosTheta = Math.Cos(angle);
                        double sinTheta = Math.Sin(angle);
                        double oneMinusCosTheta = 1 - cosTheta;

                        double[,] rotationMatrix = new double[,]
                        {
                            { cosTheta + ux*ux*oneMinusCosTheta, ux*uy*oneMinusCosTheta - uz*sinTheta, ux*uz*oneMinusCosTheta + uy*sinTheta },
                            { uy*ux*oneMinusCosTheta + uz*sinTheta, cosTheta + uy*uy*oneMinusCosTheta, uy*uz*oneMinusCosTheta - ux*sinTheta },
                            { uz*ux*oneMinusCosTheta - uy*sinTheta, uz*uy*oneMinusCosTheta + ux*sinTheta, cosTheta + uz*uz*oneMinusCosTheta }
                        };
                        double[] result = new double[3];
                        for (int i = 0; i < 3; i++)
                        {
                            result[i] = 0;
                            for (int j = 0; j < 3; j++)
                            {
                                result[i] += rotationMatrix[i, j] * point[j];
                            }
                        }

                        return result;
                    }
                    geometryData.Add($"o DTRecHits_V1_{objectIndex}");
                    for (int i = 0; i < vertices.GetLength(0); i++)
                    {
                        double[] point = { vertices[i, 0], vertices[i, 1], vertices[i, 2] };
                        double[] rotatedPoint = RotatePointAroundAxis(point, axis, angle);
                        vertices[i, 0] = rotatedPoint[0] + pos[0];
                        vertices[i, 1] = rotatedPoint[1] + pos[1];
                        vertices[i, 2] = rotatedPoint[2] + pos[2];
                        geometryData.Add($"v {vertices[i, 0]} {vertices[i, 1]} {vertices[i, 2]}");

                    }
                    geometryData.Add($"f {counter} {counter + 1} {counter + 2} {counter + 3}");
                    geometryData.Add($"f {counter + 3} {counter + 2} {counter + 1} {counter}");
                    geometryData.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
                    geometryData.Add($"f {counter + 7} {counter + 6} {counter + 5} {counter + 4}");
                    geometryData.Add($"f {counter} {counter + 3} {counter + 7} {counter + 4}");
                    geometryData.Add($"f {counter + 4} {counter + 7} {counter + 3} {counter}");
                    geometryData.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
                    geometryData.Add($"f {counter + 5} {counter + 6} {counter + 2} {counter + 1}");
                    geometryData.Add($"f {counter + 3} {counter + 2} {counter + 6} {counter + 7}");
                    geometryData.Add($"f {counter + 7} {counter + 6} {counter + 2} {counter + 3}");
                    geometryData.Add($"f {counter + 1} {counter} {counter + 4} {counter + 5}");
                    geometryData.Add($"f {counter + 5} {counter + 4} {counter} {counter + 1}");
                    counter += 8;
                    objectIndex += 1;
                }
                return geometryData;
            }

            static public List<CaloTowersV2> caloTowerV2Parse(JObject data, string name)// double scale)
            {
                List<CaloTowersV2> dataList = new List<CaloTowersV2>();
                foreach (var item in data["Collections"][name])
                {
                    CaloTowersV2 caloItem = new CaloTowersV2();
                    var children = item.Children().Values<double>().ToArray();
                    caloItem.et = children[0];
                    caloItem.eta = children[1];
                    caloItem.phi = children[2];
                    caloItem.iphi = children[3];
                    caloItem.hadEnergy = children[4];
                    caloItem.emEnergy = children[5];
                    caloItem.outerEnergy = children[6];
                    caloItem.ecalTime = children[7];
                    caloItem.hcalTime = children[8];
                    caloItem.emPosition = new double[] { children[9], children[10], children[11] };
                    caloItem.hadPosition = new double[] { children[12], children[13], children[14] };
                    caloItem.front_1 = new double[] { children[15], children[16], children[17] };
                    caloItem.front_2 = new double[] { children[18], children[19], children[20] };
                    caloItem.front_3 = new double[] { children[21], children[22], children[23] };
                    caloItem.front_4 = new double[] { children[24], children[25], children[26] };
                    caloItem.back_1 = new double[] { children[27], children[28], children[29] };
                    caloItem.back_2 = new double[] { children[30], children[31], children[32] };
                    caloItem.back_3 = new double[] { children[33], children[34], children[35] };
                    caloItem.back_4 = new double[] { children[36], children[37], children[38] };
                    dataList.Add(caloItem);
                }

                return dataList;
            }
            static public List<string> generateCaloTowerV2(List<CaloTowersV2> data)
            {
                var dataList = new List<string>();
                int counter = 1;
                data = setCaloV2Scale(data);
                foreach (var tower in data)
                {
                    double theta = 2 * Math.Atan(Math.Exp(-1 * tower.eta));
                    //double scale = tower.scale;
                    double scale = 0.1; //ispy
                    double min_energy = 0.1;

                    if (tower.et > min_energy)
                    {
                        var f1 = Vector<double>.Build.DenseOfArray(tower.front_1);
                        var f2 = Vector<double>.Build.DenseOfArray(tower.front_2);
                        var f3 = Vector<double>.Build.DenseOfArray(tower.front_3);
                        var f4 = Vector<double>.Build.DenseOfArray(tower.front_4);

                        var b1e = Vector<double>.Build.DenseOfArray(tower.back_1);
                        var b2e = Vector<double>.Build.DenseOfArray(tower.back_2);
                        var b3e = Vector<double>.Build.DenseOfArray(tower.back_3);
                        var b4e = Vector<double>.Build.DenseOfArray(tower.back_4);

                        var b1h = b1e.Clone();
                        var b2h = b2e.Clone();
                        var b3h = b3e.Clone();
                        var b4h = b4e.Clone();

                        double escale;
                        double hscale;

                        if (tower.emEnergy > 0) escale = scale * tower.emEnergy * Math.Sin(theta);
                        else escale = 0;
                        if (tower.hadEnergy > 0) hscale = scale * tower.hadEnergy * Math.Sin(theta);
                        else hscale = 0;

                        if (escale > 0)
                        {
                            b1e /= b1e.L2Norm();
                            b2e /= b2e.L2Norm();
                            b3e /= b3e.L2Norm();
                            b4e /= b4e.L2Norm();

                            b1e *= escale;
                            b2e *= escale;
                            b3e *= escale;
                            b4e *= escale;

                            b1e += f1;
                            b2e += f2;
                            b3e += f3;
                            b4e += f4;

                            dataList.Add($"v {String.Join(' ', f1)}");
                            dataList.Add($"v {String.Join(' ', f2)}");
                            dataList.Add($"v {String.Join(' ', f3)}");
                            dataList.Add($"v {String.Join(' ', f4)}");

                            dataList.Add($"v {String.Join(' ', b1e)}");
                            dataList.Add($"v {String.Join(' ', b2e)}");
                            dataList.Add($"v {String.Join(' ', b3e)}");
                            dataList.Add($"v {String.Join(' ', b4e)}");

                            dataList.Add($"f {counter} {counter + 1} {counter + 2} {counter + 3}");
                            dataList.Add($"f {counter + 3} {counter + 2} {counter + 1} {counter}");
                            dataList.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
                            dataList.Add($"f {counter + 7} {counter + 6} {counter + 5} {counter + 4}");
                            dataList.Add($"f {counter} {counter + 3} {counter + 7} {counter + 4}");
                            dataList.Add($"f {counter + 4} {counter + 7} {counter + 3} {counter}");
                            dataList.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
                            dataList.Add($"f {counter + 5} {counter + 6} {counter + 2} {counter + 1}");
                            dataList.Add($"f {counter + 3} {counter + 2} {counter + 6} {counter + 7}");
                            dataList.Add($"f {counter + 7} {counter + 6} {counter + 2} {counter + 3}");
                            dataList.Add($"f {counter + 1} {counter} {counter + 4} {counter + 5}");
                            dataList.Add($"f {counter + 5} {counter + 4} {counter} {counter + 1}");
                            counter += 8;
                        }

                        if (hscale > 0)
                        {
                            List<Vector<double>> vectors = new List<Vector<double>>();
                            if (escale > 0)
                            {
                                vectors.Add(b1e);
                                vectors.Add(b2e);
                                vectors.Add(b3e);
                                vectors.Add(b4e);
                            }
                            else
                            {
                                vectors.Add(f1);
                                vectors.Add(f2);
                                vectors.Add(f3);
                                vectors.Add(f4);
                            }

                            b1h /= b1h.L2Norm();
                            b2h /= b2h.L2Norm();
                            b3h /= b3h.L2Norm();
                            b4h /= b4h.L2Norm();

                            b1h *= hscale;
                            b2h *= hscale;
                            b3h *= hscale;
                            b4h *= hscale;

                            if (escale > 0)
                            {
                                b1h += b1e;
                                b2h += b2e;
                                b3h += b3e;
                                b4h += b4e;
                            }
                            else
                            {
                                b1h += f1;
                                b2h += f2;
                                b3h += f3;
                                b4h += f4;
                            }

                            vectors.Add(b1h);
                            vectors.Add(b2h);
                            vectors.Add(b3h);
                            vectors.Add(b4h);

                            dataList.Add($"v {String.Join(' ', f1)}");
                            dataList.Add($"v {String.Join(' ', f2)}");
                            dataList.Add($"v {String.Join(' ', f3)}");
                            dataList.Add($"v {String.Join(' ', f4)}");
                            dataList.Add($"v {String.Join(' ', b1h)}");
                            dataList.Add($"v {String.Join(' ', b2h)}");
                            dataList.Add($"v {String.Join(' ', b3h)}");
                            dataList.Add($"v {String.Join(' ', b4h)}");

                            dataList.Add($"f {counter} {counter + 1} {counter + 2} {counter + 3}");
                            dataList.Add($"f {counter + 3} {counter + 2} {counter + 1} {counter}");
                            dataList.Add($"f {counter + 4} {counter + 5} {counter + 6} {counter + 7}");
                            dataList.Add($"f {counter + 7} {counter + 6} {counter + 5} {counter + 4}");
                            dataList.Add($"f {counter} {counter + 3} {counter + 7} {counter + 4}");
                            dataList.Add($"f {counter + 4} {counter + 7} {counter + 3} {counter}");
                            dataList.Add($"f {counter + 1} {counter + 2} {counter + 6} {counter + 5}");
                            dataList.Add($"f {counter + 5} {counter + 6} {counter + 2} {counter + 1}");
                            dataList.Add($"f {counter + 3} {counter + 2} {counter + 6} {counter + 7}");
                            dataList.Add($"f {counter + 7} {counter + 6} {counter + 2} {counter + 3}");
                            dataList.Add($"f {counter + 1} {counter} {counter + 4} {counter + 5}");
                            dataList.Add($"f {counter + 5} {counter + 4} {counter} {counter + 1}");
                            counter += 8;
                        }
                    }

                }
                return dataList;
            }

            static public List<CaloTowersV2> setCaloV2Scale(List<CaloTowersV2> towers)
            {
                double scaler = towers.Select(x => x.et).Max();
                List<CaloTowersV2> result = towers;
                for (int i = 0; i < towers.Count(); i++)
                {
                    CaloTowersV2 calo = towers[i];
                    calo.scale = towers[i].et / scaler;
                    towers[i] = calo;
                    //Console.WriteLine(calo.scale);
                }
                return towers;
            }
    }
}
