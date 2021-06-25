using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System;
using System.IO;
using System.Linq;
//using UnityEditor;
using System;
using System.Net;
using Newtonsoft.Json;

public class CityGenerator : MonoBehaviour {

    private int nB = 0;
    private Vector3 center;
    private int residential = 0;
    private bool _residential = false;

    GameObject cityMaker;

    [HideInInspector]
    public GameObject miniBorder;

    [HideInInspector]
    public GameObject smallBorder;

    [HideInInspector]
    public GameObject largeBorder;

    [HideInInspector]
    public GameObject mediumBorder;

    [HideInInspector]
    public GameObject[] largeBlocks;

    private bool[] _largeBlocks;


    [HideInInspector]
    public GameObject[] BB;  // Buildings in suburban areas (not in the corner)
    [HideInInspector]
    public GameObject[] BC;  // Down Town Buildings(Not in the corner)
    [HideInInspector]
    public GameObject[] BR;  // Residential buildings in suburban areas (not in the corner)
    [HideInInspector]
    public GameObject[] DC;  // Corner buildings that occupy both sides of the block
    [HideInInspector]
    public GameObject[] EB;  // Corner buildings in suburban areas
    [HideInInspector]
    public GameObject[] EC;  // Down Town Corner Buildings 
    [HideInInspector]
    public GameObject[] MB;  //  Buildings that occupy both sides of the block 
    [HideInInspector]
    public GameObject[] BK;  //  Buildings that occupy an entire block
    [HideInInspector]
    public GameObject[] SB;  //  Large buildings that occupy larger blocks 

    private int[] _BB;
    private int[] _BC;
    private int[] _BR;
    //private int[] _DC;
    private int[] _EB;
    private int[] _EC;
    private int[] _MB;  
    private int[] _BK;  
    private int[] _SB;


    private Material[] _Materials;
    private Material[] _laserMaterials;

    private GameObject[] tempArray;
    private int numB;
    private LineRenderer lr;


    float distCenter  = 300;
    int interval = 1; 
    float nextTime = 0;
    private int maxBuildings;

    private Dictionary<string, List<Building>> subnetGroups;
    private CityInfo cityInfo;
    public List<Building> currentSubnet;
    int currentBuildingIdx = 0;
    /*
    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateAllBuildings();
        }
    }
    */
    [Serializable]
    public class Building
    {
        [JsonProperty(PropertyName = "IP Address")]
        public string ipAddress;
        [JsonProperty(PropertyName = "Subnet")]
        public string subnet;
        [JsonProperty(PropertyName = "Hostname")]
        public string hostname;
        [JsonProperty(PropertyName = "Risk Ranking")]
        public string riskRanking;
        [JsonProperty(PropertyName = "Risk Rating")]
        public string riskRating;
        [JsonProperty(PropertyName = "Vulnerabilities")]
        public string vulnerabilities;
        [JsonProperty(PropertyName = "Risk")]
        public string risk;
        [JsonProperty(PropertyName = "Owner")]
        public string owner;
        [JsonProperty(PropertyName = "App")]
        public string app;
        [JsonProperty(PropertyName = "Exploits")]
        public string exploits;
        [JsonProperty(PropertyName = "Malware")]
        public string malware;
        [JsonProperty(PropertyName = "Type")]
        public string type;
        [JsonProperty(PropertyName = "Operating System")]
        public string operatingSystem;
        [JsonProperty(PropertyName = "Last Scan")]
        public string lastScan;
        [JsonProperty(PropertyName = "Building Size (sample)")]
        public string buildingSize;
        [JsonProperty(PropertyName = "Criticality")]
        public string criticality;

    }
    [Serializable]
    public class CityInfo
    {
        public List<Building> data;  
    }
    public Dictionary<string, List<Building>> GetSubnetGroups(CityInfo info){
        Dictionary<string, List<Building>> cityDict = new Dictionary<string, List<Building>>();
        foreach (var x in info.data)
        {
            // Debug.Log(x.subnet);
            // The Add method throws an exception if the new key is
            // already in the dictionary.
            if (x.ipAddress.Length > 0) {
                try
                {
                    cityDict.Add(x.subnet, new List<Building>{x});
                }
                catch (ArgumentException)
                {
                    // Debug.Log(string.Format("An element with Key {0} already exists.", x.subnet));
                    cityDict[x.subnet].Add(x);
                }
            }
            
        }
        return cityDict;
    }
    public void Start() {
        // Get API Call

        cityInfo = GetBuildings();
        subnetGroups = GetSubnetGroups(cityInfo);

        maxBuildings = cityInfo.data.Count;
        Debug.Log(maxBuildings);
    }
    public CityInfo GetBuildings()
    {
        //Valid: "https://dl.dropbox.com/s/4z4bzprj1pud3tq/Assets.json?dl=0"
        //Sample: https://dl.dropbox.com/s/fbh6jbyzrf86g0x/Assets-sample.json?dl=0
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://dl.dropbox.com/s/4z4bzprj1pud3tq/Assets.json?dl=0");
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();
        Debug.Log(jsonResponse);
        // CityInfo info = JsonUtility.FromJson<CityInfo>(jsonResponse);
        CityInfo info = JsonConvert.DeserializeObject<CityInfo>(jsonResponse); 
        return info;
    }
    public void GenerateStreetsVerySmall()
    {
        Debug.Log("Very Smallll");

        // Get API Call

        cityInfo = GetBuildings();
        subnetGroups = GetSubnetGroups(cityInfo);
        maxBuildings = cityInfo.data.Count;

        if (!cityMaker)
            cityMaker = GameObject.Find("City-Maker");

        if (cityMaker)
            DestroyImmediate(cityMaker);

        cityMaker = new GameObject("City-Maker");
        GenerateCustomStreets();

        // GameObject block;

        // distCenter = 150;
        // int nb = 0;

        // int le = largeBlocks.Length;
        // nb = UnityEngine.Random.Range(0, le);
  
        // block = (GameObject)Instantiate(largeBlocks[nb], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);


        // center = new Vector3(0,0,0);

        // block = (GameObject)Instantiate(miniBorder, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);

        // block.transform.SetParent(cityMaker.transform);
    }
    public void GenerateCustomStreets() {
        int count = 0;
        foreach (KeyValuePair<string, List<Building>> kvp in subnetGroups)
        {

            // var subnet = (GameObject) Instantiate(new GameObject(string.Format("Subnet_{0}", count + 1)), new Vector3(0, 0, count*600), Quaternion.Euler(0, 0, 0), cityMaker.transform);
            var subnet = new GameObject(string.Format("Subnet_{0}", count + 1));
            subnet.transform.parent = cityMaker.transform;
            subnet.transform.position += new Vector3(0f, 0f, count*600f);
            // subnet.transform.SetParent(cityMaker.transform);
            //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            Debug.Log(string.Format("Subnet = {0}", kvp.Key));
            // foreach(var x in kvp.Value) {
            //     Debug.Log(string.Format("Member = {0} - {1}", x.hostname, x.ipAddress));
            // }
            currentSubnet = kvp.Value;
            currentBuildingIdx = 0;
            // GenerateSubnet();
            GameObject block;

            distCenter = 150;
            int nb = 0;

            int le = largeBlocks.Length;
            nb = UnityEngine.Random.Range(0, le);

            block = (GameObject)Instantiate(largeBlocks[nb], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), subnet.transform);
            block.transform.SetParent(subnet.transform);
            block.transform.localPosition = Vector3.zero;

            center = new Vector3(0,0,0);

            block = (GameObject)Instantiate(miniBorder, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), subnet.transform);
            block.transform.SetParent(subnet.transform);
            block.transform.localPosition = Vector3.zero;

            count++;
            hello
            // break;
        }

    }
    public void GenerateStreetsSmall()
    {
        Debug.Log("Small");
        if (!cityMaker)
            cityMaker = GameObject.Find("City-Maker");

        if (cityMaker)
            DestroyImmediate(cityMaker);

        cityMaker = new GameObject("City-Maker");



        distCenter =  200;
        int nb = 0;

        int le = largeBlocks.Length;
        _largeBlocks = new bool[largeBlocks.Length];

        //Position and Rotation
        Vector3[] ps = new Vector3[3];

        int[] rt = new int[3];

        float s = UnityEngine.Random.Range(0, 6f);

        if (s < 3)
        {
            ps[1] = new Vector3(0, 0, 0); rt[1] = 0;
            ps[2] = new Vector3(0, 0, 300); rt[2] = 0;
        }
        else 
        {
            ps[1] = new Vector3(-150, 0, 150); rt[1] = 90;
            ps[2] = new Vector3(150, 0, 150); rt[2] = 90;
        }


        for (int qt = 1; qt < 3; qt++)
        {

            for (int lp = 0; lp < 100; lp++)
            {
                nb = UnityEngine.Random.Range(0, le);
                if (!_largeBlocks[nb]) break;
            }
            _largeBlocks[nb] = true;

            Instantiate(largeBlocks[nb], ps[qt], Quaternion.Euler(0, rt[qt], 0), cityMaker.transform);

        }

        center = ps[UnityEngine.Random.Range(1, 2)];

        GameObject block = (GameObject)Instantiate(smallBorder, new Vector3(0,0,0), Quaternion.Euler(0, 0, 0), cityMaker.transform);

        block.transform.SetParent(cityMaker.transform);

    }

    public void GenerateStreets()
    {
        Debug.Log("GenerateStreets()");
        if (!cityMaker)
            cityMaker = GameObject.Find("City-Maker");

        if (cityMaker)
            DestroyImmediate(cityMaker);

        cityMaker = new GameObject("City-Maker");

        distCenter = 300;

        int nb = 0;

        int le = largeBlocks.Length;
        _largeBlocks = new bool[largeBlocks.Length];

        //Position and Rotation
        Vector3[] ps = new Vector3[5];

        int[] rt = new int[5];

        float s = UnityEngine.Random.Range(0, 6f);

        if (s < 2) {

            ps[1] = new Vector3(0, 0, 0); rt[1] = 0;
            ps[2] = new Vector3(0, 0, 300); rt[2] = 0;
            ps[3] = new Vector3(450, 0, 150); rt[3] = 90;
            ps[4] = new Vector3(-450, 0, 150); rt[4] = 90;

        }
        else if (s < 3)
        {

            ps[1] = new Vector3(-450, 0, 150); rt[1] = 90;
            ps[2] = new Vector3(-150, 0, 150); rt[2] = 90;
            ps[3] = new Vector3(150, 0, 150); rt[3] = 90;
            ps[4] = new Vector3(450, 0, 150); rt[4] = 90;

        }
        else if (s < 4)
        {

            ps[1] = new Vector3(-450, 0, 150); rt[1] = 90;
            ps[2] = new Vector3(-150, 0, 150); rt[2] = 90;
            ps[3] = new Vector3(300, 0, 0); rt[3] = 0;
            ps[4] = new Vector3(300, 0, 300); rt[4] = 0;

        }
        else
        {

            ps[1] = new Vector3(450, 0, 150); rt[1] = 90;
            ps[2] = new Vector3(150, 0, 150); rt[2] = 90;
            ps[3] = new Vector3(-300, 0, 0); rt[3] = 0;
            ps[4] = new Vector3(-300, 0, 300); rt[4] = 0;

        }


        for (int qt = 1; qt < 5; qt++)
        {

            for (int lp = 0; lp < 100; lp++)
            {
                nb = UnityEngine.Random.Range(0, le);
                if (!_largeBlocks[nb]) break;
            }
            _largeBlocks[nb] = true;

            Instantiate(largeBlocks[nb], ps[qt], Quaternion.Euler(0, rt[qt], 0), cityMaker.transform);

        }

        center = ps[UnityEngine.Random.Range(1, 4)];

        GameObject block = (GameObject)Instantiate(mediumBorder, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);

        block.transform.SetParent(cityMaker.transform);

    }

    public void GenerateStreetsBig()
    {

        if (!cityMaker)
            cityMaker = GameObject.Find("City-Maker");

        if (cityMaker)
            DestroyImmediate(cityMaker);

        cityMaker = new GameObject("City-Maker");

        distCenter = 350;
        int nb = 0;

        int le = largeBlocks.Length;
        _largeBlocks = new bool[largeBlocks.Length];

        //Position and Rotation
        Vector3[] ps = new Vector3[7];

        int[] rt = new int[7];

        float s = UnityEngine.Random.Range(0, 6f);

        if (s < 3)
        {

            ps[1] = new Vector3(0, 0, 0); rt[1] = 0;
            ps[2] = new Vector3(0, 0, 300); rt[2] = 0;
            ps[3] = new Vector3(450, 0, 150); rt[3] = 90;
            ps[4] = new Vector3(-450, 0, 150); rt[4] = 90;
            ps[5] = new Vector3(-300, 0, 600); rt[5] = 0;
            ps[6] = new Vector3(300, 0, 600); rt[6] = 0;


        }
        else if (s < 3)
        {

            ps[1] = new Vector3(-450, 0, 150); rt[1] = 90;
            ps[2] = new Vector3(-150, 0, 150); rt[2] = 90;
            ps[3] = new Vector3(150, 0, 150); rt[3] = 90;
            ps[4] = new Vector3(450, 0, 150); rt[4] = 90;
            ps[5] = new Vector3(-300, 0, 600); rt[5] = 0;
            ps[6] = new Vector3(300, 0, 600); rt[6] = 0;

        }
        else if (s < 4)
        {

            ps[1] = new Vector3(-300, 0, 300); rt[1] = 0;
            ps[2] = new Vector3(-300, 0, 0); rt[2] = 0;
            ps[3] = new Vector3(150, 0, 150); rt[3] = 90;
            ps[4] = new Vector3(450, 0, 150); rt[4] = 90;
            ps[5] = new Vector3(-300, 0, 600); rt[5] = 0;
            ps[6] = new Vector3(300, 0, 600); rt[6] = 0;


        }
        else
        {

            ps[1] = new Vector3(-450, 0, 150); rt[1] = 90;
            ps[2] = new Vector3(300, 0, 0); rt[2] = 0;
            ps[3] = new Vector3(-150, 0, 150); rt[3] = 90;
            ps[4] = new Vector3(450, 0, 450); rt[4] = 90;
            ps[5] = new Vector3(-300, 0, 600); rt[5] = 0;
            ps[6] = new Vector3(150, 0, 450); rt[6] = 90;

        }


        for (int qt = 1; qt < 7; qt++)
        {

            for (int lp = 0; lp < 100; lp++)
            {
                nb = UnityEngine.Random.Range(0, le);
                if (!_largeBlocks[nb]) break;
            }
            _largeBlocks[nb] = true;

            Instantiate(largeBlocks[nb], ps[qt], Quaternion.Euler(0, rt[qt], 0), cityMaker.transform);

        }

        center = ps[UnityEngine.Random.Range(1, 6)];

        GameObject block = (GameObject)Instantiate(largeBorder, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);

        block.transform.SetParent(cityMaker.transform);

    }

    private GameObject pB;
    public void GenerateCustomBuildings(Dictionary<string, List<Building>> subnetDict) {
        foreach (KeyValuePair<string, List<Building>> kvp in subnetDict)
        {
            //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            Debug.Log(string.Format("Subnet = {0}", kvp.Key));
            // foreach(var x in kvp.Value) {
            //     Debug.Log(string.Format("Member = {0} - {1}", x.hostname, x.ipAddress));
            // }
            currentSubnet = kvp.Value;
            currentBuildingIdx = 0;
            // GenerateSubnet();
            break;
        }
    }

    public void GenerateSubnet(){
        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("Marcador")).ToArray();

        foreach (GameObject lines in tempArray) {

            _residential = (residential < 15 && Vector3.Distance(center, lines.transform.position) > 400 && UnityEngine.Random.Range(0, 100) < 30);

            foreach (Transform child in lines.transform) {

                if (child.name == "E")
                    CreateBuildingsInCorners(child.gameObject);
                    // CreateBuildingsInLine(child.gameObject, 90f);

                else
                {

                }
                    // CreateBuildingsInLine(child.gameObject, 90f);
                    // CreateBuildingsInCorners(child.gameObject);

            }
        }

            _residential = false;
    }
    public void GenerateAllBuildings()
    {
        


        _BB = new int[BB.Length];
        _BC = new int[BC.Length];
        _BR = new int[BR.Length];
        //_DC = new int[DC.Length];
        _EB = new int[EB.Length];
        _EC = new int[EC.Length];
        _MB = new int[MB.Length];   
        _BK = new int[BK.Length]; 
        _SB = new int[SB.Length];

        _Materials = new Material[4];

        _Materials[0] = (Material)Resources.Load("Atlas-Red", typeof(Material));
        _Materials[1] = (Material)Resources.Load("Atlas-Blue", typeof(Material));
        _Materials[2] = (Material)Resources.Load("Atlas-Yellow", typeof(Material));
        _Materials[3] = (Material)Resources.Load("Atlas-Gray", typeof(Material));

        _laserMaterials = new Material[4];
        _laserMaterials[0] = (Material)Resources.Load("Materials/laserred", typeof(Material));
        _laserMaterials[1] = (Material)Resources.Load("Materials/laserwhite", typeof(Material));
        _laserMaterials[2] = (Material)Resources.Load("Materials/laseryellow", typeof(Material));
        _laserMaterials[3] = (Material)Resources.Load("Materials/laserblue", typeof(Material));

        residential = 0;

        DestroyBuildings();

        GameObject pB = new GameObject();

        nB = 0;

        CreateBuildingsInSuperBlocks();
        CreateBuildingsInBlocks();
        CreateBuildingsInLines();
        CreateBuildingsInDouble();
        // GenerateCustomBuildings(subnetGroups);

        Debug.ClearDeveloperConsole();
        Debug.Log(nB + " buildings were created");


        DestroyImmediate(pB);

    }



    public void CreateBuildingsInLines() {

        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("Marcador")).ToArray();

        foreach (GameObject lines in tempArray) {

            _residential = (residential < 15 && Vector3.Distance(center, lines.transform.position) > 400 && UnityEngine.Random.Range(0, 100) < 30);

            foreach (Transform child in lines.transform) {

                if (child.name == "E")
                    CreateBuildingsInCorners(child.gameObject);
                    // CreateBuildingsInLine(child.gameObject, 90f);

                else
                    CreateBuildingsInLine(child.gameObject, 90f);
                    // CreateBuildingsInCorners(child.gameObject);

            }

            _residential = false;


        }

    }

    public void CreateBuildingsInCorners(GameObject child)
    {

        GameObject pBuilding;

        pB = null;
        int numB;
        int t = 0;
        float pWidth = 0;
        float wComprimento;

        float pScale;
        float remainingMeters;
        GameObject newMarcador;

        float distancia = Vector3.Distance(center, child.transform.position);

        int lp;
        lp = 0;

        while (t < 100)
        {

            t++;

            if (distancia < distCenter)
            {

                do
                {
                    lp++;
                    numB = UnityEngine.Random.Range(0, EC.Length);
                    if (_EC[numB] == 0) break;
                    if (lp > 100 && _EC[numB] <= 1) break;
                    if (lp > 150 && _EC[numB] <= 2) break;
                    if (lp > 200 && _EC[numB] <= 3) break;
                    if (lp > 250) break;
                } while (lp < 300);



                pWidth = GetWith(EC[numB]);
                if (pWidth <= 36.05f)
                {
                    _EC[numB] += 1;
                    pB = EC[numB];
                    //q = _EC[numB];
                    break;
                }
            }
            else
            {



                do
                {
                    lp++;
                    numB = UnityEngine.Random.Range(0, EB.Length);
                    if (_EB[numB] == 0) break;
                    if (lp > 100 && _EB[numB] <= 1) break;
                    if (lp > 150 && _EB[numB] <= 2) break;
                    if (lp > 200 && _EB[numB] <= 2) break;
                    if (lp > 250) break;
                } while (lp < 300);


                pWidth = GetWith(EB[numB]);
                if (pWidth <= 36.05f)
                {
                    _EB[numB] += 1;
                    pB = EB[numB];
                    //q = _EB[numB];
                    break;
                }

            }

        }

        pBuilding = (GameObject)Instantiate(pB, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
        pBuilding.name = pBuilding.name;

        // Debug.Log(pBuilding.name);
        pBuilding.transform.SetParent(child.transform);
        pBuilding.transform.localPosition = new Vector3(-(pWidth * 0.5f), 0, 0);
        pBuilding.transform.localRotation = Quaternion.Euler(0, 0, 0);
        //Color Rendering

        CreateColor(pBuilding);

        nB++;

        // Check space behind the corner building -------------------------------------------------------------------------------------------------------------------
        wComprimento = GetHeight(pB);
        if (wComprimento < 29.9f)
        {

            newMarcador = new GameObject("Marcador"); 

            newMarcador.transform.SetParent(child.transform);
            newMarcador.transform.localPosition = new Vector3(0, 0, -36);
            newMarcador.transform.localRotation = Quaternion.Euler(0, 0, 0);
            newMarcador.name = (36 - wComprimento).ToString();
            CreateBuildingsInLine(newMarcador, 90);

        }
        else
        {
            remainingMeters = 36 - wComprimento;
            pScale = 1 + (remainingMeters / wComprimento);
            pBuilding.transform.localScale = new Vector3(1, 1, pScale);

        }


        // Check space on the corner building -------------------------------------------------------------------------------------------------------------------


        if (pWidth < 29.9f)
        {

            newMarcador = new GameObject("Marcador"); 

            

            newMarcador.transform.SetParent(child.transform);
            newMarcador.transform.localPosition = new Vector3(-pWidth, 0, 0);
            newMarcador.transform.localRotation = Quaternion.Euler(0, 270, 0);
            newMarcador.name = (36 - pWidth).ToString();
            CreateBuildingsInLine(newMarcador, 90);

        }
        else
        {

            remainingMeters = 36 - pWidth;
            pScale = 1 + (remainingMeters / pWidth);
            pBuilding.transform.localScale = new Vector3(pScale, 1, 1);

        }

    }

    int RandRotation()
    {
        int r = 0;
        int i = UnityEngine.Random.Range(0, 4);
        if (i == 3) r = 180;
        else if (i == 2) r = 90;
        else if (i == 1) r = 270;
        else r = 0;

        return r;
     
     
    }
    public void CreateColor(GameObject building){
        // Debug.Log("CreateColor()");
        // Debug.Log(building);
        //Color Rendering
        MeshRenderer myRend;
        myRend = building.GetComponent<MeshRenderer>();

        if (myRend != null) {
            Material[] materials = myRend.sharedMaterials;
            int randIdx = UnityEngine.Random.Range(0, 4);
            Material newMat = _Materials[randIdx];
            for ( int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains("Atlas"))
                {
                    materials[i] = newMat;
                    // Debug.Log(materials[i].name);
                }
            }
            myRend.sharedMaterials = materials;

            Transform pTransform = building.transform;
            int ChildrenCount = pTransform.childCount;
            for (int i = 0; i < ChildrenCount; i++)
            // loop through children
            {
                GameObject child = pTransform.GetChild(i).gameObject;
                if (child.GetComponent<MeshRenderer>() != null)
                {
                    MeshRenderer cRend = child.GetComponent<MeshRenderer>();
                    Material[] cMaterials = cRend.sharedMaterials;
                    for (int j = 0; j < cMaterials.Length; j++)
                    {
                        if (cMaterials[j].name.Contains("Atlas"))
                        {
                            cMaterials[j] = newMat;
                            //Debug.Log(materials[i].name);
                        }
                    }
                    cRend.sharedMaterials = cMaterials;
                    // if (randIdx < 3) {
                    //     CreateLaser(child, randIdx);
                    // }
                }

                else {
                    // Debug.Log(name + " : Nope");

                }
            }
            //Rendering Laser
            // Make sure that you put your prefab in the folder Assets/Resources
            if (randIdx < 3) {
                if (UnityEngine.Random.Range(1, 100) <= 25) {
                    CreateLaser(building, randIdx);
                }
            }
        }
        else {
            Debug.Log("No Mesh Renderer!");
        }
    }
    public void CreateLaser(GameObject building, int idx){
        Debug.Log(building);
        // Vector3 buildingCenter = GetCenter(building);
        float buildingHeight = GetHeight(building);
        float buildingY = GetY(building);
        float laserHeight = UnityEngine.Random.Range(140f, 250.0f);

        if (buildingY >= 50){
            laserHeight = UnityEngine.Random.Range(80f, 200.0f);
        }
        else
            laserHeight = UnityEngine.Random.Range(140f, 450.0f);
        // Debug.Log(buildingHeight);
        GameObject _go = Resources.Load("Laser") as GameObject;
        GameObject laser = (GameObject) Instantiate(_go, new Vector3(0, 0, 0), Quaternion.Euler(0, 90, 0));
        laser.transform.SetParent(building.transform);
        lr = laser.GetComponent<LineRenderer>();
        lr.startWidth = 8f;
        lr.endWidth = 1f;
        lr.SetPosition(1, new Vector3(0f, 0f, laserHeight ));
        // lr.SetPosition(1, new Vector3(0f, 0f, 200f));

        lr.useWorldSpace = false;
        // laser.transform.localPosition = buildingCenter;
        if ((building.name.Contains("QD")) || (building.name.Contains("building"))){
            laser.transform.localPosition = new Vector3(0f, buildingY, -0.5f);
        }
        else {
            laser.transform.localPosition = new Vector3(0f, buildingY, -(buildingHeight*0.5f));
        }


        laser.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        Material newLaserMat = _laserMaterials[idx];
        laser.GetComponent<LineRenderer>().sharedMaterial = newLaserMat;

        // Debug.Log(laser);
    }
    public void CreateBuildingsInBlocks()
    {

        int numB = 0;

        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("Blocks")).ToArray();

        foreach (GameObject bks in tempArray)
        {

            foreach (Transform bk in bks.transform)
            {

                if (UnityEngine.Random.Range(0, 20) > 5)
                {

                    int lp = 0;
                    do
                    {
                        lp++;
                        numB = UnityEngine.Random.Range(0, BK.Length);
                        if (_BK[numB] == 0) break;
                        if (lp > 125 && _BK[numB] <= 1) break;
                        if (lp > 150 && _BK[numB] <= 2) break;
                        if (lp > 200 && _BK[numB] <= 3) break;
                        if (lp > 250) break;
                    } while (lp < 300);

                    _BK[numB] += 1;

                    GameObject newObject = Instantiate(BK[numB], bk.position, bk.rotation, bk);             
                    CreateColor(newObject);
                    nB++;

                }
                else
                {

                    for (int i = 1; i <= 4; i++)
                    {
                        GameObject nc = new GameObject("E");
                        nc.transform.SetParent(bk);
                        if (i == 1)
                        {
                            nc.transform.localPosition = new Vector3(-36, 0, -36);
                            nc.transform.localRotation = Quaternion.Euler(0, 180, 0);
                        }
                        if (i == 2)
                        {
                            nc.transform.localPosition = new Vector3(-36, 0, 36);
                            nc.transform.localRotation = Quaternion.Euler(0, 270, 0);
                        }
                        if (i == 3)
                        {
                            nc.transform.localPosition = new Vector3(36, 0, 36);
                            nc.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        }
                        if (i == 4)
                        {
                            nc.transform.localPosition = new Vector3(36, 0, -36);
                            nc.transform.localRotation = Quaternion.Euler(0, 90, 0);
                        }
                        CreateBuildingsInCorners(nc);

                    }
                }


            }
          
        }

    }

    public void CreateBuildingsInSuperBlocks()
    {

        int numB = 0;

        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("SuperBlocks")).ToArray();

        foreach (GameObject bks in tempArray)
        {

            foreach (Transform bk in bks.transform)
            {


                    int lp = 0;
                    do
                    {
                        lp++;
                        numB = UnityEngine.Random.Range(0, SB.Length);
                        if (_SB[numB] == 0) break;
                        if (lp > 125 && _SB[numB] <= 1) break;
                        if (lp > 150 && _SB[numB] <= 2) break;
                        if (lp > 200 && _SB[numB] <= 3) break;
                        if (lp > 250) break;
                    } while (lp < 300);

                    _SB[numB] += 1;

                    GameObject newObject = Instantiate(SB[numB], bk.position, bk.rotation, bk);

                    //Color Rendering
                    MeshRenderer myRend;
                    myRend = newObject.GetComponent<MeshRenderer>();
                    Material[] materials = myRend.sharedMaterials;
                    int randIdx = UnityEngine.Random.Range(0, 4);
                    Material newMat = _Materials[randIdx];
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i].name.Contains("Atlas"))
                        {
                            materials[i] = newMat;
                            Debug.Log(materials[i].name);
                        }
                    }
                    myRend.sharedMaterials = materials;
                    if (randIdx < 3){
                        CreateLaser(newObject, randIdx);
                    }
                    nB++;



            }

        }

    }

    private void CreateBuildingsInLine(GameObject line, float angulo)
	{

        int index = -1;
        GameObject[] pBuilding;
		pBuilding = new GameObject[50];

        float limit;

        if (line.name.Contains("."))
        {
            limit = float.Parse(line.name.Split('.')[0]) + float.Parse(line.name.Split('.')[1]) / float.Parse("1" + "0000000".Substring(0, line.name.Split('.')[1].Length ))  ;
        } else
        limit = float.Parse(line.name);

		float init = 0;
		float pWidth = 0;


		int tt = 0;
		int t;

        int lp;


        float distancia = Vector3.Distance(center, line.transform.position);

        while (tt < 100) {

			tt++;
			t = 0;


            lp = 0;
            while (t < 200 && init <= limit - 4){

                t++;

                if (distancia < distCenter)
                {

                    do
                    {
                        lp++;
                        numB = UnityEngine.Random.Range(0, BC.Length);
                        if (_BC[numB] == 0) break;
                        if (lp > 125 && _BC[numB] <= 1) break;
                        if (lp > 150 && _BC[numB] <= 2) break;
                        if (lp > 200 && _BC[numB] <= 3) break;
                        if (lp > 250) break;
                    } while (lp < 300);

                    pWidth = GetWith(BC[numB]);
                    if ((init + pWidth) <= (limit + 4))
                    {
                        pB = BC[numB];
                        _BC[numB] += 1;
                        break;
                    }

                }
                else if (_residential)
                {

                    do
                    {
                        lp++;
                        numB = UnityEngine.Random.Range(0, BR.Length);
                        if (_BR[numB] == 0) break;
                        if (lp > 100 && _BR[numB] <= 1) break;
                        if (lp > 150 && _BR[numB] <= 2) break;
                        if (lp > 200 && _BR[numB] <= 3) break;
                        if (lp > 250) break;
                    } while (lp < 300);

                    pWidth = GetWith(BR[numB]);
                    if ((init + pWidth) <= (limit + 4))
                    {
                        pB = BR[numB];
                        _BR[numB] += 1;
                        residential += 1;
                        break;
                    }
                }
                else
                {

                    do
                    {
                        lp++;
                        numB = UnityEngine.Random.Range(0, BB.Length);
                        if (_BB[numB] == 0) break;
                        if (lp > 100 && _BB[numB] <= 1) break;
                        if (lp > 150 && _BB[numB] <= 2) break;
                        if (lp > 200 && _BB[numB] <= 3) break;
                        if (lp > 250) break;
                    } while (lp < 300);

                    pWidth = GetWith(BB[numB]);
                    if ((init + pWidth) <= (limit + 4))
                    {
                        pB = BB[numB];
                        _BB[numB] += 1;
                        break;
                    }

                }


            }
            

            if (t >= 200 || init > limit - 4) {  // Não encontrou um que caiba no espaco existente

                    AdjustsWidth(pBuilding, index + 1, limit - init, 0);
					break;

			} else {
               
					index++;

                    pBuilding[index] = (GameObject)Instantiate(pB, new Vector3(0, 0, init + (pWidth * 0.5f)), Quaternion.Euler(0, angulo, 0));
                    nB++;

                    pBuilding[index].name = pBuilding[index].name;
					pBuilding [index].transform.SetParent (line.transform);

					pBuilding [index].transform.localPosition = new Vector3 (0, 0, init + (pWidth * 0.5f));
					pBuilding [index].transform.localRotation = Quaternion.Euler (0, angulo, 0);

					init += pWidth;

					if (init > limit - 6) { //72) {

                         AdjustsWidth(pBuilding, index + 1, limit - init, 0);

					}

                    //Color Rendering
                    CreateColor(pBuilding[index]);
            }


		}

        

    }

    
    private void CreateBuildingsInDoubleLine(GameObject line)
    {
        
        int index = -1;
        GameObject[] pBuilding;
        pBuilding = new GameObject[20];

        float limit;
        limit = float.Parse(line.name);

        float init = 0;
        float pWidth = 0;

        int tt = 0;
        int t;
        int lp;

        while (tt < 100)
        {

            tt++;
            t = 0;

            lp = 0;

            while (t < 200 && init <= limit - 4)
            {

                t++;
 
                do
                {
                    lp++;
                    numB = UnityEngine.Random.Range(0, MB.Length);
                    if (_MB[numB] == 0) break;
                    if (lp > 100 && _MB[numB] <= 1) break;
                    if (lp > 150 && _MB[numB] <= 2) break;
                    if (lp > 200) break;
                } while (lp < 300);

                pWidth = GetWith(MB[numB]);
                if ((init + pWidth) <= (limit + 4))
                {
                    _MB[numB] += 1;
                    break;
                }

            }

            if (t >= 200 || init > limit - 4)
            {
                AdjustsWidth(pBuilding, index + 1, (limit - init), 0);
                break;

            }
            else
            {

                index++;
   
                pBuilding[index] = (GameObject)Instantiate(MB[numB], new Vector3(0, 0, 0) , Quaternion.Euler(0, 90, 0), line.transform);
                nB++;

                pBuilding[index].name = "building";
                pBuilding[index].transform.SetParent(line.transform);
                pBuilding[index].transform.localPosition = new Vector3(0,0 , (init + (pWidth * 0.5f)));
                pBuilding[index].transform.localRotation = Quaternion.Euler(0, 90, 0);

                init += pWidth;

                if (init > limit - 6)
                {
                    AdjustsWidth(pBuilding, index + 1, (limit - init), 0);
                }

                //Color Rendering
                MeshRenderer myRend;
                myRend = pBuilding[index].GetComponent<MeshRenderer>();
                Material[] materials = myRend.sharedMaterials;
                int randIdx = UnityEngine.Random.Range(0, 4);
                Material newMat = _Materials[randIdx];
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].name.Contains("Atlas"))
                    {
                        materials[i] = newMat;
                        Debug.Log(materials[i].name);
                    }
                }
                myRend.sharedMaterials = materials;

                if (randIdx < 3){
                        CreateLaser(pBuilding[index], randIdx);
                    }

            }


        }

    }

    private void CreateBuildingsInDouble()
    {
        float limit;

        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("Double")).ToArray();

        GameObject DB;
        GameObject mc2;
        GameObject mc;


        foreach (GameObject dbCross in tempArray)
        {

            foreach (Transform line in dbCross.transform)
            {

                limit = float.Parse(line.name);

                if (UnityEngine.Random.Range(0, 10) < 5)
                {
                    //Bloks

                    float wl;
                    float wl2;

                    do
                    {
                        numB = UnityEngine.Random.Range(0, DC.Length);
                        wl = GetHeight(DC[numB]);
                    } while (wl > limit / 2);

                    GameObject e = (GameObject)Instantiate(DC[numB], line.transform.position, line.transform.rotation, line.transform);

                    nB++;

                    do
                    {
                        numB = UnityEngine.Random.Range(0, DC.Length);
                        wl2 = GetHeight(DC[numB]);
                    } while (wl2 > limit - (wl + 26));

                    e = (GameObject)Instantiate(DC[numB], line.transform.position, line.rotation, line.transform);
                    e.transform.SetParent(line.transform);
                    e.transform.localPosition = new Vector3(0, 0, -limit);
                    e.transform.localRotation = Quaternion.Euler(0, 180, 0);

                    DB = new GameObject("" + ((limit - wl - wl2)));
                    DB.transform.SetParent(line.transform);
                    DB.transform.localPosition = new Vector3(0, 0, -(limit - wl2));
                    DB.transform.localRotation = Quaternion.Euler(0, 0, 0);

                    DB.name = "" + ((limit - wl - wl2));

                    CreateBuildingsInDoubleLine(DB);

                }
                else
                {
                    //Lines and Corners

                    mc = new GameObject("Marcador");
                    mc.transform.SetParent(line);
                    mc.transform.localPosition = new Vector3(0, 0, 0);
                    mc.transform.localRotation = Quaternion.Euler(0, 0, 0);


                    for (int i = 1; i <= 4; i++)
                    {
                        mc2 = new GameObject("E");
                        mc2.transform.SetParent(mc.transform);

                        if (i == 1)
                        {
                            mc2.transform.localPosition = new Vector3(36, 0, -limit);
                            mc2.transform.localRotation = Quaternion.Euler(0, 90, 0);
                        }
                        if (i == 2)
                        {
                            mc2.transform.localPosition = new Vector3(36, 0, 0);
                            mc2.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        }
                        if (i == 3)
                        {
                            mc2.transform.localPosition = new Vector3(-36, 0, 0);
                            mc2.transform.localRotation = Quaternion.Euler(0, 270, 0);
                        }
                        if (i == 4)
                        {
                            mc2.transform.localPosition = new Vector3(-36, 0, -limit);
                            mc2.transform.localRotation = Quaternion.Euler(0, 180, 0);
                        }

                        CreateBuildingsInCorners(mc2);

                    }

                    mc2 = new GameObject("" + (limit - 72));
                    mc2.transform.SetParent(mc.transform);
                    mc2.transform.localPosition = new Vector3(-36, 0.001f, -36);
                    mc2.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    CreateBuildingsInLine(mc2, 90f);

                    mc2 = new GameObject("" + (limit - 72));
                    mc2.transform.SetParent(mc.transform);
                    mc2.transform.localPosition = new Vector3(36, 0.001f, -(limit-36));
                    mc2.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    CreateBuildingsInLine(mc2, 90f);

                }




            }



        }
        

    }


    private void AdjustsWidth(GameObject[] tBuildings, int quantity, float remainingMeters, float init){

		if (remainingMeters == 0)
			return;

		float ajuste = remainingMeters / quantity;

		float zInit = init; 
		float pWidth;
		float pScale;
        float gw;


        for (int i = 0; i < quantity; i++){

            gw = GetWith(tBuildings[i]);
            if (gw > 0)
            {
                pScale = 1 + (ajuste / gw);
                pWidth = gw + ajuste;

                tBuildings[i].transform.localPosition = new Vector3(tBuildings[i].transform.localPosition.x, tBuildings[i].transform.localPosition.y, zInit + (pWidth * 0.5f));
                tBuildings[i].transform.localScale = new Vector3(pScale, 1, 1);
                zInit += pWidth;
            }    

		}

	}


	private float GetWith(GameObject building){


        if (building.transform.GetComponent<MeshFilter>() != null)
            return building.transform.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
        else
        {
            Debug.LogError("Error:  " + building.name + " does not have a mesh renderer at the root. The prefab must be the floor/base mesh. I nside it you place the building. More info im https://youtu.be/kVrWir_WjNY");
            return 0;
        }
    }	

    private Vector3 GetCenter(GameObject building){


        if (building.transform.GetComponent<MeshFilter>() != null)
            return building.transform.GetComponent<MeshFilter>().sharedMesh.bounds.center;
        else
        {
            Debug.LogError("Error:  " + building.name + " does not have a mesh renderer at the root. The prefab must be the floor/base mesh. I nside it you place the building. More info im https://youtu.be/kVrWir_WjNY");
            return new Vector3(0,0,0);
        }
    }	

	private float GetHeight(GameObject building){

		if(building.GetComponent<MeshFilter> () != null)
			return building.GetComponent<MeshFilter> ().sharedMesh.bounds.size.z;
		else
        {
            Debug.LogError("Error:  " + building.name + " does not have a mesh renderer at the root. The prefab must be the floor/base mesh. I nside it you place the building. More info im https://youtu.be/kVrWir_WjNY");
            return 0;
        }

    }	

    private float GetY(GameObject building){

		if(building.GetComponent<MeshFilter> () != null)
			return building.GetComponent<MeshFilter> ().sharedMesh.bounds.size.y;
		else
        {
            Debug.LogError("Error:  " + building.name + " does not have a mesh renderer at the root. The prefab must be the floor/base mesh. I nside it you place the building. More info im https://youtu.be/kVrWir_WjNY");
            return 0;
        }

    }

	


	public void DestroyBuildings() {


        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("Marcador")).ToArray();
        for (int i = 1 ; i<8; i++)
			foreach (GameObject objt in tempArray) {
				foreach (Transform child in objt.transform)	{
					DestryObjetcs2 (child.gameObject, "All");
				}
			}	



        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("Blocks")).ToArray();

        for (int i = 1; i < 8; i++)
            foreach (GameObject objt in tempArray)
            {
                foreach (Transform child in objt.transform)
                {
                    DestryObjetcs2(child.gameObject, "All"); 
                }
            }


        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("SuperBlocks")).ToArray();

        for (int i = 1; i < 8; i++)
            foreach (GameObject objt in tempArray)
            {
                foreach (Transform child in objt.transform)
                {
                    DestryObjetcs2(child.gameObject, "All"); 
                }
            }



        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("Double")).ToArray();

        for (int i = 1; i < 8; i++)
            foreach (GameObject objt in tempArray)
            {
                foreach (Transform child in objt.transform)
                {
                    DestryObjetcs2(child.gameObject, "All");
                }
            }

    }

	private void DestryObjetcs2(GameObject line, string nameObj)
	{

		foreach (Transform child in line.transform)
		{

			//if(child.CompareTag("Sapata")){
			if ((nameObj == "All"))
				DestroyImmediate (child.gameObject);
			else if(child.name == nameObj){
				DestroyImmediate (child.gameObject);
			}

		}

	}







}
