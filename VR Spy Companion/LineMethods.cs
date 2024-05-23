using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IGtoOBJGen
{
    static class LineMethods
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
    }
}
