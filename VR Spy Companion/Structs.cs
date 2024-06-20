using System.Numerics;
//Wouldn't be the end of the world to replace the double arrays with Vector
interface ObjectData { };
struct MuonChamberData : ObjectData
{
    public string name;
    public int detid;
    public int id;
    public double[] front_1;
    public double[] front_2;
    public double[] front_3;
    public double[] front_4;
    public double[] back_1;
    public double[] back_2;
    public double[] back_3;
    public double[] back_4;
    public double[] vertical;
    public double[] horizontal;
    public double[] centroid;
}
struct Vertex : ObjectData
{
    public int isValid;
    public int isFake;
    public double[] pos;
    public double xError;
    public double yError;
    public double zError;
    public double chi2;
    public double ndof;
}
struct CalorimetryTowers : ObjectData
{
    public double energy;
    public double eta;
    public double phi;
    public double time;
    public int detid;
    public double[] front_1;
    public double[] front_2;
    public double[] front_3;
    public double[] front_4;
    public double[] back_1;
    public double[] back_2;
    public double[] back_3;
    public double[] back_4;
    public double scale;
}
struct METData : ObjectData
{
    public double phi;
    public double pt;
    public double px;
    public double py;
    public double pz;
}
struct JetV1Data : ObjectData
{
    public int id;
    public double et;
    public double eta;
    public double theta;
    public double phi;
}
struct JetV2Data : ObjectData
{
    public int id;
    public double et;
    public double eta;
    public double theta;
    public double phi;
    public double[] vertex; 
}
struct GlobalMuonData : ObjectData
{
    public int id;
    public double pt;
    public int charge;
    public double[] position;
    public double phi;
    public double eta;
    public double caloEnergy;
}
struct StandaloneMuonData : ObjectData
{
    public int id;
    public double pt;
    public int charge;
    public double[] position;
    public double phi;
    public double eta;
    public double caloEnergy;
}
struct TrackerMuonData : ObjectData
{
    public int id;
    public double pt;
    public int charge;
    public double[] position;
    public double phi;
    public double eta;
}
struct PhotonData : ObjectData
{
    public int id;
    public string name;
    public double energy;
    public double et;
    public double eta;
    public double phi;
    public Vector3 position;
}
struct TrackExtrasData : ObjectData
{
    public double[] pos1;
    public double[] dir1;
    public double[] pos2;
    public double[] dir2;
    public double[] pos3;
    public double[] pos4;
}
struct Track : ObjectData
{
    public int id;
    public double[] pos;
    public double[] dir;
    public double pt;
    public double phi;
    public double eta;
    public int charge;
    public double chi2;
    public double ndof;
}
struct GsfElectron : ObjectData
{
    public int id;
    public double pt;
    public double eta;
    public double phi;
    public int charge;
    public double[] pos;
    public double[] dir;
}
struct SuperCluster : ObjectData
{
    public int id;
    public double energy;
    public double[] pos;
    public double eta;
    public double phi;
    public string algo;
    public double etaWidth;
    public double phiWidth;
    public double rawEnergy;
    public double preshowerEnergy;
}
struct RecHitFraction : ObjectData
{
    public int detid;
    public double fraction;
    public double[] front_1;
    public double[] front_2;
    public double[] front_3;
    public double[] front_4;
    public double[] back_1;
    public double[] back_2;
    public double[] back_3;
    public double[] back_4;
}
