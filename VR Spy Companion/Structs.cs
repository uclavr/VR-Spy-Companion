using System.Numerics;
//Wouldn't be the end of the world to replace the double arrays with Vector
interface ObjectData { };
struct RPCRecHit
{
    public double[] u1;
    public double[] u2;
    public double[] v1;
    public double[] v2;
    public double[] w1;
    public double[] w2;
    public int region;
    public int ring;
    public int sector;
    public int station;
    public int layer;
    public int subsector;
    public int roll;
    public int detid;
}
struct CSCSegment
{
    public int detid;
    public double[] pos1;
    public double[] pos2;
}
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
    public List<double[]> geometricVertices;
    public double scale;
    public double deltaPhi;
    public double deltaEta;
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
struct TrackerPieceData : ObjectData
{
    public string name;
    public int detid;
    public double[] front_1;
    public double[] front_2;
    public double[] front_3;
    public double[] front_4;
    public double[] back_1;
    public double[] back_2;
    public double[] back_3;
    public double[] back_4;
}
struct trackingPoint
{
    public string name;
    public int detid;
    public double X;
    public double Y;
    public double Z;
}
struct matchingCSC : ObjectData
{
    public string name;
    public int detid;
    public double[] front_1;
    public double[] front_2;
    public double[] front_3;
    public double[] front_4;
    public double[] back_1;
    public double[] back_2;
    public double[] back_3;
    public double[] back_4;
}

struct cscSegmentV2 : ObjectData
{
    public string name;
    public int detid;
    public double[] pos_1;
    public double[] pos_2;
    public int endcap;
    public int station;
    public int ring;
    public int chamber;
    public int layer;
}

struct DTRecHitsV1 : ObjectData
{
    public string name;
    public int wireId;
    public int layerId;
    public int superLayerId;
    public int sectorId;
    public int stationId;
    public int wheelId;
    public double digitime;
    public double[] wirePos;
    public double[] lPlusGlobalPos;
    public double[] lMinusGlobalPos;
    public double[] rPlusGlobalPos;
    public double[] rMinusGlobalPos;
    public double[] lGlobalPos;
    public double[] rGlobalPos;
    public double[] axis;
    public double angle;
    public double cellWidth;
    public double cellLength;
    public double cellHeight;

}

struct dtRecSegment4D_V1 : ObjectData
{
    public string name;
    public int detid;
    public double[] pos_1;
    public double[] pos_2;
    public int sectorId;
    public int stationId;
    public int wheelId;
}


struct cscRecHit2Ds_V2 : ObjectData
{
    public string name;
    public double[] u1;
    public double[] u2;
    public double[] v1;
    public double[] v2;
    public double[] w1;
    public double[] w2;
    public int endcap;
    public int station;
    public int ring;
    public int chamber;
    public int layer;
    public double tpeak;
    public double positionWithinStrip;
    public double errorWithinStrip;
    public string strips;
    public string WireGroups;
}

struct CaloTowersV2 : ObjectData
{
    public string name;
    public double et;
    public double eta;
    public double phi;
    public double iphi;
    public double hadEnergy;
    public double emEnergy;
    public double outerEnergy;
    public double ecalTime;
    public double hcalTime;
    public double[] emPosition;
    public double[] hadPosition;
    public double[] front_1;
    public double[] front_2;
    public double[] front_3;
    public double[] front_4;
    public double[] back_1;
    public double[] back_2;
    public double[] back_3;
    public double[] back_4;
}
