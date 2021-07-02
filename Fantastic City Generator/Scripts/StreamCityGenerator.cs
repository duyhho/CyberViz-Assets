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


public class StreamCityGenerator : MonoBehaviour {

    private int nB = 0;
    private Vector3 center;
    private int residential = 0;
    private bool _residential = false;

    GameObject cityMaker;

    [HideInInspector]
    public GameObject mediumBorder;

    public GameObject[] largeBlocks;

    private bool[] _largeBlocks;



    public GameObject[] BB;  // Buildings in suburban areas (not in the corner)

    public GameObject[] BC;  // Down Town Buildings(Not in the corner)

    public GameObject[] BR;  // Residential buildings in suburban areas (not in the corner)

    public GameObject[] DC;  // Corner buildings that occupy both sides of the block

    public GameObject[] EB;  // Corner buildings in suburban areas

    public GameObject[] EC;  // Down Town Corner Buildings 

    public GameObject[] MB;  //  Buildings that occupy both sides of the block 

    public GameObject[] BK;  //  Buildings that occupy an entire block

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
    private Material[] _internalLaserMaterials;


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
    int currentSubnetSize = 0;
    bool[] customRendered;
    Coroutine coroutine = null;

    Dictionary<string, GameObject> allBuildings = new Dictionary<string, GameObject>() ;
    void Start()
    {
        Debug.Log("Start!");
        GenerateCustomStreets();
        GenerateCustomBuildings();
        coroutine = this.StartCoroutine(onCoroutine());

    }
    void Update()
    {
        // Debug.Log("Update!");
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("Update!");
        }
    }
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
        var myList = cityDict.ToList();
        myList.Sort((pair1,pair2) => pair2.Value.Count.CompareTo(pair1.Value.Count));
        Debug.Log(myList);
        // foreach (KeyValuePair<string, List<Building>> kvp in myList){
        //     Debug.Log(string.Format("Key = {0}", kvp.Key));
        //     foreach(var x in kvp.Value) {
        //         Debug.Log(string.Format("Member = {0} - {1}", x.hostname, x.ipAddress));
        //     }
        // }
        var sortedDict = myList.ToDictionary(x => x.Key, x => x.Value);
        return sortedDict;
    }
    // public void Start() {
    //     // Get API Call

    //     cityInfo = GetBuildings();
    //     subnetGroups = GetSubnetGroups(cityInfo);

    //     maxBuildings = cityInfo.data.Count;
    //     Debug.Log(maxBuildings);
    // }


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
    public void GenerateCustomStreets() {
        cityInfo = GetBuildings();
        subnetGroups = GetSubnetGroups(cityInfo);
        maxBuildings = cityInfo.data.Count;

        if (!cityMaker)
            cityMaker = GameObject.Find("City-Maker");

        if (cityMaker)
            DestroyImmediate(cityMaker);

        cityMaker = new GameObject("City-Maker");
        int count = 0;
        foreach (KeyValuePair<string, List<Building>> kvp in subnetGroups)
        {
            bool fatSquaredRequired = false;
            // var subnet = (GameObject) Instantiate(new GameObject(string.Format("Subnet_{0}", count + 1)), new Vector3(0, 0, count*600), Quaternion.Euler(0, 0, 0), cityMaker.transform);
            var subnet = new GameObject(string.Format("Subnet_{0}", count + 1));
            subnet.transform.parent = cityMaker.transform;
            subnet.transform.position += new Vector3(0f, 0f, count*315f);
            // subnet.transform.SetParent(cityMaker.transform);
            //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            // Debug.Log(string.Format("Subnet = {0}", kvp.Key));
            // foreach(var x in kvp.Value) {
            //     Debug.Log(string.Format("Member = {0} - {1}", x.hostname, x.ipAddress));
            // }
            currentSubnet = kvp.Value;
            foreach (Building profile in currentSubnet){
                if (profile.type == "Network") {
                    fatSquaredRequired = true;
                    break;
                }
            }
            currentBuildingIdx = 0;
            // GenerateSubnet();
            GameObject block;

            distCenter = 200;
            int nb = 0;

            int le = largeBlocks.Length;
            nb = UnityEngine.Random.Range(0, le);
            while ((largeBlocks[nb].name.Contains("05") || largeBlocks[nb].name.Contains("01") || largeBlocks[nb].name.Contains("02") || largeBlocks[nb].name.Contains("10"))  || (fatSquaredRequired && (largeBlocks[nb].name.Contains("04")))) {
                    nb = UnityEngine.Random.Range(0, le);
            }
            // Debug.Log(largeBlocks[nb].name);
            block = (GameObject)Instantiate(largeBlocks[nb], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), subnet.transform);
            block.transform.SetParent(subnet.transform);
            block.transform.localPosition = Vector3.zero;

            // center = new Vector3(0,0,0);

            // block = (GameObject)Instantiate(miniBorder, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), subnet.transform);
            // block.transform.SetParent(subnet.transform);
            // block.transform.localPosition = Vector3.zero;

            count++;
            // break;
        }
        // Debug.Log(gameObject.name);
        // Debug.Log(gameObject.activeInHierarchy ? "Active" : "Inactive");

        DestroyTrees();

    }

    private GameObject pB;
    public void GenerateCustomBuildings() {
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

        _Materials[0] = (Material)Resources.Load("Atlas-Blue", typeof(Material));
        _Materials[1] = (Material)Resources.Load("Atlas-Yellow", typeof(Material));
        _Materials[2] = (Material)Resources.Load("Atlas-Red", typeof(Material));
        _Materials[3] = (Material)Resources.Load("Atlas-Gray", typeof(Material));

        _laserMaterials = new Material[4];
        _laserMaterials[0] = (Material)Resources.Load("Materials/laserwhite", typeof(Material));
        _laserMaterials[1] = (Material)Resources.Load("Materials/laseryellow", typeof(Material));
        _laserMaterials[2] = (Material)Resources.Load("Materials/laserred", typeof(Material));
        // _laserMaterials[3] = (Material)Resources.Load("Materials/laserblue", typeof(Material));

        _internalLaserMaterials = new Material[4];
        _internalLaserMaterials[0] = (Material)Resources.Load("Materials/laserblue_dashed", typeof(Material));
        _internalLaserMaterials[1] = (Material)Resources.Load("Materials/laseryellow_dashed", typeof(Material));
        _internalLaserMaterials[2] = (Material)Resources.Load("Materials/laserred_dashed", typeof(Material));

        residential = 0;

        DestroyBuildings();

        GameObject pB = new GameObject();

        nB = 0;



        int count = 1;
        foreach (KeyValuePair<string, List<Building>> kvp in subnetGroups)
        {
            //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            // Debug.Log(string.Format("Subnet = {0}", kvp.Key));
            // foreach(var x in kvp.Value) {
            //     Debug.Log(string.Format("Member = {0} - {1}", x.hostname, x.ipAddress));
            // }
            currentSubnet = kvp.Value;
            currentSubnetSize = currentSubnet.Count;
            Debug.Log(string.Format("Subnet Size: {0}", currentSubnetSize));
            customRendered = new bool[currentSubnetSize];

            GenerateSubnet(count);
            count++;

            int remaining = 0;
            for (int i = 0; i < customRendered.Length; i++) {
                if (customRendered[i] == false) {
                    Debug.Log(string.Format("{0} (size {1}) is not rendered", currentSubnet[i].ipAddress, currentSubnet[i].buildingSize));
                    remaining++;
                }
            }
            Debug.Log(string.Format("{0} are left", remaining));
            // DestroyUnassigned();
            // break;
        }
        Debug.ClearDeveloperConsole();
        Debug.Log(nB + " buildings were created");


        DestroyImmediate(pB);
    }

    public void GenerateSubnet(int subnetIdx){
        CreateBuildingsInLinesCustom(subnetIdx);
        CreateBuildingsInSuperBlocksCustom(subnetIdx);
        CreateBuildingsInBlocksCustom(subnetIdx);
        // CreateBuildingsInDoubleCustom(subnetIdx);
    }
    public string CallAPI()
    {
        string[] apiURLs = new string[] {
        "https://dl.dropbox.com/s/4z4bzprj1pud3tq/Assets.json?dl=0",
        "https://dl.dropbox.com/s/xc56hb2qmqb3zq6/Assets%20-%20Modified.json?dl=0",
        "https://dl.dropbox.com/s/bu7uwvm0b8olw41/Assets%20-%20Modified-v2.json?dl=0"
        };
        //Valid: "https://dl.dropbox.com/s/4z4bzprj1pud3tq/Assets.json?dl=0"
        //Sample: https://dl.dropbox.com/s/fbh6jbyzrf86g0x/Assets-sample.json?dl=0
        int randIdx = UnityEngine.Random.Range(0, 3);
        Debug.Log(apiURLs[randIdx]);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiURLs[randIdx]);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();
        Debug.Log(jsonResponse);
        return jsonResponse;

    }
    IEnumerator onCoroutine()
     {
        while(true)
        {
            Debug.Log ("Calling API...");
            // Debug.Log(cityGenerator.gameObject);
            string jsonResponse = CallAPI();
            TrackChanges(jsonResponse);

            // cityGenerator.GenerateAllBuildings();
            yield return new WaitForSeconds(5f);
        }
    }
    public void TrackChanges(string jsonResponse) {

        CityInfo info = JsonConvert.DeserializeObject<CityInfo>(jsonResponse);
        Dictionary<string, List<Building>> newSubnetGroups = GetSubnetGroups(info);
        Debug.Log("Received!");
        if (subnetGroups.Count == 0) {
            Debug.Log("No Prior Info: Assigning New");
            subnetGroups = newSubnetGroups;
        }
        else {
            Debug.Log("Tracking Changes...");
            foreach (KeyValuePair<string, List<Building>> kvp in newSubnetGroups)
            {
                Debug.Log(string.Format("Subnet = {0}", kvp.Key));
                for (int i = 0; i < kvp.Value.Count; i++){
                    Building buildingProfile = kvp.Value[i];
                    string targetIPAddress = buildingProfile.ipAddress;
                    // Debug.Log(string.Format("Member = {0} - {1}", buildingProfile.hostname, buildingProfile.ipAddress));
                    if (subnetGroups[kvp.Key][i].riskRating != buildingProfile.riskRating) {
                        if (allBuildings.ContainsKey(targetIPAddress)) {
                            GameObject targetBuilding = allBuildings[targetIPAddress];
                            CreateColor(targetBuilding, buildingProfile);
                        }
                        else {
                            Debug.Log(targetIPAddress + " does not exist in city yet! (Not Rendered)");
                        }


                    }

                }
            }
            subnetGroups = newSubnetGroups;
        }

    }

    // public static int CompareLand(GameObject go1, GameObject go2)
    // {
    //     // TODO: Comparison logic :)
    //     float distance1 = Vector3.Distance(CityGenerator.center, go1.transform.position);
    //     float distance2 = Vector3.Distance(CityGenerator.center, go2.transform.position);

    //     return distance1.CompareTo(distance2);
    // }
    public void CreateBuildingsInLinesCustom(int subnetIdx){ //work on this shit//
        tempArray = GameObject.FindObjectsOfType(typeof(GameObject))
                                .Select(g => g as GameObject)
                                .Where(g => (g.name == ("Marcador")) &&
                                        g.transform.parent.parent.parent.name ==
                                        string.Format("Subnet_{0}", subnetIdx))

                                .OrderBy(g=> Vector3.Distance(center, g.GetComponentsInChildren<Transform>()[1].position))
                                .ToArray();
        // tempArray.Sort(CompareLand);
        foreach(GameObject lines in tempArray){
            // Debug.Log(Vector3.Distance(center, lines.GetComponentsInChildren<Transform>()[1].position));
            foreach (Transform child in lines.transform) {
                _residential = (residential < 15 && Vector3.Distance(center, lines.transform.position) > 400 && UnityEngine.Random.Range(0, 100) < 30);
                _residential = false;
                // _residential = true;
                // Debug.Log(Vector3.Distance(center, child.transform.position));
                if (child.name == "E") {
                    CreateBuildingsInCornersCustom(child.gameObject);

                }
                else
                {
                    CreateBuildingsInLineCustom(child.gameObject, 90f);
                }
            }
            // break;
        }
    }


    public void CreateBuildingsInCornersCustom(GameObject child)
    {

        GameObject pBuilding;

        pB = null;

        string buildingIP = "";

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
        // foreach(GameObject building in EC){
        //     Debug.Log(string.Format("EC BUILDING: {0}", building.name));
        // }
        // foreach(GameObject building in EB){
        //     Debug.Log(string.Format("EB BUILDING: {0}", building.name));
        // }
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



                pWidth = GetWidth(EC[numB]);
                if (pWidth <= 36.05f)
                {
                    _EC[numB] += 1;
                    pB = EC[numB];
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


                pWidth = GetWidth(EB[numB]);
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

        // pBuilding.name = string.Format("IP_{0}", nB);
        // Debug.Log(buildingIP);
        // pBuilding.name = (buildingIP != "") ? string.Format("{0} - {1}", pBuilding.name, buildingIP) : string.Format("{0} - Unassigned", pBuilding.name);
        Building assignedBuilding = AssignIP(pBuilding);
        pBuilding.name = string.Format("{0} - {1}", pBuilding.name, assignedBuilding.ipAddress);
        // Debug.Log(pBuilding.name);
        if (assignedBuilding.ipAddress != "Unassigned")
            allBuildings.Add(assignedBuilding.ipAddress, pBuilding);
        pBuilding.transform.SetParent(child.transform);
        pBuilding.transform.localPosition = new Vector3(-(pWidth * 0.5f), 0, -(pWidth * 0.5f));
        pBuilding.transform.localRotation = Quaternion.Euler(0, 0, 0);
        //Color Rendering

        CreateColor(pBuilding, assignedBuilding);
        //CreateTeleportationPad(pBuilding, assignedBuilding);
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
            CreateBuildingsInLineCustom(newMarcador, 90);

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
            CreateBuildingsInLineCustom(newMarcador, 90);

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
    public Building AssignIP(GameObject pBuilding) {
        string buildingIP = "Unassigned";
        Building buildingProfile = new Building();
        buildingProfile.ipAddress = "Unassigned";
        if (pBuilding.name.Contains("Triangle")){
            for (int i = 0; i < customRendered.Length; i++) {
                Building building = currentSubnet[i];
                if (customRendered[i] == false) {
                    string buildingType = building.type;
                    if (buildingType == "Endpoint"){
                    // Debug.Log(building.rendered);
                        // Debug.Log(string.Format("rendering small building for {0}", building.ipAddress));
                        customRendered[i] = true;
                        buildingIP = building.ipAddress;
                        buildingProfile = building;
                        break;
                    }
                }
            }
        }
        else if (pBuilding.name.Contains("Rectangle-Small")){
            for (int i = 0; i < customRendered.Length; i++) {
                Building building = currentSubnet[i];
                if (customRendered[i] == false) {
                    string buildingType = building.type;
                    if (buildingType == "Windows Server"){
                    // Debug.Log(building.rendered);
                        // Debug.Log(string.Format("rendering small building for {0}", building.ipAddress));
                        customRendered[i] = true;
                        buildingIP = building.ipAddress;
                        buildingProfile = building;
                        break;
                    }
                }
            }
        }
        else if (pBuilding.name.Contains("Circle")){
            for (int i = 0; i < customRendered.Length; i++) {
                Building building = currentSubnet[i];
                if (customRendered[i] == false) {
                    string buildingType = building.type;
                    if (buildingType == "Database"){
                    // Debug.Log(building.rendered);
                        // Debug.Log(string.Format("rendering small building for {0}", building.ipAddress));
                        customRendered[i] = true;
                        buildingIP = building.ipAddress;
                        buildingProfile = building;
                        break;
                    }
                }
            }
        }
        else if (pBuilding.name.Contains("Pentagon")){
            for (int i = 0; i < customRendered.Length; i++) {
                Building building = currentSubnet[i];
                if (customRendered[i] == false) {
                    string buildingType = building.type;
                    if (buildingType == "Linux Server"){
                    // Debug.Log(building.rendered);
                        // Debug.Log(string.Format("rendering small building for {0}", building.ipAddress));
                        customRendered[i] = true;
                        buildingIP = building.ipAddress;
                        buildingProfile = building;
                        break;
                    }
                }
            }
        }
        else if (pBuilding.name.Contains("Cross")){
            for (int i = 0; i < customRendered.Length; i++) {
                Building building = currentSubnet[i];
                if (customRendered[i] == false) {
                    string buildingType = building.type;
                    if (buildingType == "Appliance"){
                    // Debug.Log(building.rendered);
                        // Debug.Log(string.Format("rendering small building for {0}", building.ipAddress));
                        customRendered[i] = true;
                        buildingIP = building.ipAddress;
                        buildingProfile = building;
                        break;
                    }
                }
            }
        }
        else if (pBuilding.name.Contains("Rectangle-Fat") || pBuilding.name.Contains("Rectangle-Super")){
            for (int i = 0; i < customRendered.Length; i++) {
                Building building = currentSubnet[i];
                if (customRendered[i] == false) {
                    string buildingType = building.type;
                    if (buildingType == "Network"){
                    // Debug.Log(building.rendered);
                        // Debug.Log(string.Format("rendering small building for {0}", building.ipAddress));
                        customRendered[i] = true;
                        buildingIP = building.ipAddress;
                        buildingProfile = building;
                        break;
                    }
                }
            }
        }
        else {
            buildingIP = "Unassigned";
        }
        return buildingProfile;

    }
    public void CreateColor(GameObject building, Building buildingProfile = null){
        DestroyLasers(building);

        Dictionary<string, int> colorMap =  new Dictionary<string, int>();
        colorMap.Add("Low", 0);
        colorMap.Add("Medium", 1);
        colorMap.Add("High", 2);
        colorMap.Add("Critical", 2);

        int defaultColorIdx = 3;
        int colorIdx = defaultColorIdx;
        int buildingSize = UnityEngine.Random.Range(2,15);
        if (buildingProfile != null) {
            if (buildingProfile.ipAddress != "Unassigned") {
                string riskRating = buildingProfile.riskRating;
                colorIdx = colorMap[riskRating];
                buildingSize = Int32.Parse(buildingProfile.buildingSize);
            }
        }
        //Size Rendering
        building.transform.localScale = new Vector3(1f, buildingSize/2, 1f);
        // Debug.Log(buildingSize);
        //Color Rendering
        MeshRenderer myRend;
        myRend = building.GetComponent<MeshRenderer>();

        if (myRend != null) {
            Material[] materials = myRend.sharedMaterials;
            Material newMat = _Materials[colorIdx];
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
                }

                else {
                    // Debug.Log(name + " : Nope");

                }
            }
            //Rendering Laser
            // Make sure that you put your prefab in the folder Assets/Resources
            if (colorIdx < 3) {
                if (UnityEngine.Random.Range(1, 100) <= 25) {
                    CreateLaser(building, colorIdx);
                }
                CreateInternalLaser(building, colorIdx);
            }
        }
        else {
            Debug.Log("No Mesh Renderer!");
        }
    }
    public void CreateLaser(GameObject building, int idx){
        // Debug.Log(building);
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
        lr.SetPosition(1, new Vector3(0f, 0f, buildingY ));
        // lr.SetPosition(1, new Vector3(0f, 0f, 200f));

        lr.useWorldSpace = false;
        // laser.transform.localPosition = buildingCenter;
        if ((building.name.Contains("QD")) || (building.name.Contains("building"))){
            laser.transform.localPosition = new Vector3(0f, buildingY, -0.5f);
        }
        else if (building.name.Contains("EC")) {
            laser.transform.localPosition = new Vector3(0f, buildingY, -(buildingHeight*0.5f));
        }
        else {
            laser.transform.localPosition = new Vector3(0f, buildingY, 0f);
        }


        laser.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        Material newLaserMat = _laserMaterials[idx];
        laser.GetComponent<LineRenderer>().sharedMaterial = newLaserMat;

        // Debug.Log(laser);
    }
    public void CreateInternalLaser(GameObject building, int idx) {
        float buildingZ= GetHeight(building);
        float buildingX= GetWidth(building);
        float buildingY = GetY(building);
        int laserTotal = UnityEngine.Random.Range(10, 20);
        for (int i = 0; i < laserTotal; i++) {
            //// Internal Laser
            GameObject _go_internal = Resources.Load("Internal_Laser") as GameObject;
            GameObject internalLaser = (GameObject) Instantiate(_go_internal, new Vector3(0, 0, 0), Quaternion.Euler(0, 90, 0), building.transform);
            internalLaser.transform.SetParent(building.transform);
            LineRenderer lr = internalLaser.GetComponent<LineRenderer>();
            lr.SetPosition(0, lr.transform.position);
            Vector3 targetPosition = new Vector3(0f, 0f, buildingY * UnityEngine.Random.Range(0.7f, 0.90f) );
            lr.SetPosition(1, targetPosition);
            float distance = Vector3.Distance(lr.transform.position, targetPosition);
            lr.sharedMaterial.mainTextureScale = new Vector2 (distance/4, 1);
            // lr.startWidth = 8f;
            // lr.endWidth = 8f;

            //Create Random Position within the Building
            float randomX = buildingX * UnityEngine.Random.Range(-0.4f, 0.4f);
            float randomZ = buildingZ * UnityEngine.Random.Range(-0.4f, 0.4f);
            float randomY = buildingY * UnityEngine.Random.Range(0.02f, 0.07f);



            Vector3 rndPosWithin = new Vector3(randomX, randomY, randomZ);
            // rndPosWithin = building.transform.TransformPoint(rndPosWithin * .5f);
            lr.useWorldSpace = false;
            internalLaser.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            internalLaser.transform.localPosition = rndPosWithin;
            Material newInternalLaserMat = _internalLaserMaterials[idx];
            internalLaser.GetComponent<LineRenderer>().sharedMaterial = newInternalLaserMat;
        }

    }
    public void CreateBuildingsInBlocksCustom(int subnetIdx)
    {
        Debug.Log("subnetIdx: " + subnetIdx);
        int numB = 0;

        tempArray = GameObject.FindObjectsOfType(typeof(GameObject))
                                .Select(g => g as GameObject)
                                .Where(g => g.name == ("Blocks")
                                // )
                                 && g.transform.parent.parent.parent.name == string.Format("Subnet_{0}", subnetIdx))
                                .ToArray();
        Debug.Log("Array Length:" + tempArray.Length);
        foreach (GameObject bks in tempArray)
        {
            Debug.Log(bks.transform.parent.parent.name);
        }
        foreach (GameObject bks in tempArray)
        {

            foreach (Transform bk in bks.transform)
            {

                if (UnityEngine.Random.Range(0, 20) > 0)
                {
                    Debug.Log("creating Block");
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

                    GameObject pBuilding = Instantiate(BK[numB], bk.position, bk.rotation, bk);
                    Building assignedBuilding = AssignIP(pBuilding);
                    pBuilding.name = string.Format("{0} - {1}", pBuilding.name, assignedBuilding.ipAddress);   
                    if (assignedBuilding.ipAddress != "Unassigned")
                        allBuildings.Add(assignedBuilding.ipAddress, pBuilding);

                    CreateColor(pBuilding, assignedBuilding);
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
                        CreateBuildingsInCornersCustom(nc);

                    }
                }


            }

        }

    }

    public void CreateBuildingsInSuperBlocksCustom(int subnetIdx)
    {

        int numB = 0;

        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("SuperBlocks") && g.transform.parent.parent.parent.name == string.Format("Subnet_{0}", subnetIdx)).ToArray();

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

                    GameObject pBuilding = Instantiate(SB[numB], bk.position, bk.rotation, bk);

                    //Color Rendering
                    Building assignedBuilding = AssignIP(pBuilding);
                    pBuilding.name = string.Format("{0} - {1}", pBuilding.name, assignedBuilding.ipAddress);   
                    if (assignedBuilding.ipAddress != "Unassigned")
                        allBuildings.Add(assignedBuilding.ipAddress, pBuilding);

                    CreateColor(pBuilding, assignedBuilding);



            }

        }

    }


    private void CreateBuildingsInLineCustom(GameObject line, float angulo)
	{

        int index = -1;
        GameObject[] pBuilding;
		pBuilding = new GameObject[50];
        string buildingIP = "";
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

                    pWidth = GetWidth(BC[numB]);
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

                    pWidth = GetWidth(BR[numB]);
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

                    pWidth = GetWidth(BB[numB]);
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

                    pBuilding[index] = (GameObject)Instantiate(pB, new Vector3(-(pWidth * 0.5f), 0, init + (pWidth * 0.5f)), Quaternion.Euler(0, angulo, 0));
                    nB++;


                    // pBuilding[index].name = pBuilding[index].name;
                    Building assignedBuilding = AssignIP(pBuilding[index]);
                    pBuilding[index].name = string.Format("{0} - {1}", pBuilding[index].name, assignedBuilding.ipAddress);
                    if (assignedBuilding.ipAddress != "Unassigned")
                        allBuildings.Add(assignedBuilding.ipAddress, pBuilding[index]);

                    pBuilding[index].transform.SetParent (line.transform);

					pBuilding [index].transform.localPosition = new Vector3 (-(pWidth * 0.5f), 0, init + (pWidth * 0.5f));
					pBuilding [index].transform.localRotation = Quaternion.Euler (0, angulo, 0);

					init += pWidth;

					if (init > limit - 6) { //72) {

                         AdjustsWidth(pBuilding, index + 1, limit - init, 0);

					}

                    //Color Rendering
                    CreateColor(pBuilding[index], assignedBuilding);


            }



		}



    }
    private void CreateBuildingsInDoubleLineCustom(GameObject line)
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

                pWidth = GetWidth(MB[numB]);
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

                // pBuilding[index].name = "building";
                Building assignedBuilding = AssignIP(pBuilding[index]);
                pBuilding[index].name = string.Format("{0} - {1}", pBuilding[index].name, assignedBuilding.ipAddress);   
                if (assignedBuilding.ipAddress != "Unassigned")
                    allBuildings.Add(assignedBuilding.ipAddress, pBuilding[index]);
                pBuilding[index].transform.SetParent(line.transform);
                pBuilding[index].transform.localPosition = new Vector3(0,0 , (init + (pWidth * 0.5f)));
                pBuilding[index].transform.localRotation = Quaternion.Euler(0, 90, 0);

                init += pWidth;

                if (init > limit - 6)
                {
                    AdjustsWidth(pBuilding, index + 1, (limit - init), 0);
                }

                //Color Rendering
                CreateColor(pBuilding[index], assignedBuilding);
            }


        }

    }


    private void CreateBuildingsInDoubleCustom(int subnetIdx)
    {
        float limit;

        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("Double") && g.transform.parent.parent.parent.name == string.Format("Subnet_{0}", subnetIdx)).ToArray();

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
                    Building assignedBuilding = AssignIP(e);
                    e.name = string.Format("{0} - {1}", e.name, assignedBuilding.ipAddress);
                    if (assignedBuilding.ipAddress != "Unassigned")
                        allBuildings.Add(assignedBuilding.ipAddress, e);
                    CreateColor(e, assignedBuilding);

                    DB = new GameObject("" + ((limit - wl - wl2)));
                    DB.transform.SetParent(line.transform);
                    DB.transform.localPosition = new Vector3(0, 0, -(limit - wl2));
                    DB.transform.localRotation = Quaternion.Euler(0, 0, 0);

                    DB.name = "" + ((limit - wl - wl2));

                    CreateBuildingsInDoubleLineCustom(DB);

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

                        CreateBuildingsInCornersCustom(mc2);

                    }

                    mc2 = new GameObject("" + (limit - 72));
                    mc2.transform.SetParent(mc.transform);
                    mc2.transform.localPosition = new Vector3(-36, 0.001f, -36);
                    mc2.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    CreateBuildingsInLineCustom(mc2, 90f);

                    mc2 = new GameObject("" + (limit - 72));
                    mc2.transform.SetParent(mc.transform);
                    mc2.transform.localPosition = new Vector3(36, 0.001f, -(limit-36));
                    mc2.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    CreateBuildingsInLineCustom(mc2, 90f);

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

            gw = GetWidth(tBuildings[i]);
            if (gw > 0)
            {
                pScale = 1 + (ajuste / gw);
                pWidth = gw + ajuste;

                tBuildings[i].transform.localPosition = new Vector3(tBuildings[i].transform.localPosition.x, tBuildings[i].transform.localPosition.y, zInit + (pWidth * 0.5f));
                // tBuildings[i].transform.localScale = new Vector3(pScale, 1, 1);
                // pBuilding [index].transform. localScale = new Vector3(1,UnityEngine.Random.Range(1,20),1);

                zInit += pWidth;
            }    

		}
        // Debug.Log("Finish");
	}


	private float GetWidth(GameObject building){

        // Debug.Log(building.name);
        if (building.transform.GetComponent<MeshFilter>() != null) {
            // Debug.Log(building.transform.GetComponent<MeshFilter>().sharedMesh);

            return building.transform.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
        }

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
    private void DestroyUnassigned(){
        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name.Contains("Unassigned")).ToArray();
        foreach (GameObject obj in tempArray) {
            Debug.Log(obj.name);
            DestroyImmediate(obj);
            nB--;
            // foreach (Transform child in obj.transform)	{

            // }
        }
    }
    void DestroyLasers(GameObject building){
        // Debug.Log(building.name);
        tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => (g.name == "Laser(Clone)" || g.name == "Internal_Laser(Clone)") && (g.transform.parent.name == building.name)).ToArray();
        // Debug.Log("Length Temp Array: " +  tempArray.Length);
        foreach (GameObject obj in tempArray) {
            // Debug.Log(obj.transform.parent.name);
            DestroyImmediate(obj);
        }

        // foreach (Transform child in building.transform)
        // {
        //     Transform result = FindTransform(child, "Laser(Clone)");
        //     if (result != null)
        //         DestroyImmediate(result);
        // }

    }

    void DestroyTrees() {
        tempArray = GameObject.FindObjectsOfType(typeof(GameObject))
        .Select(g => g as GameObject)
        .Where(g => g.name == "Objects" || g.name == "TreesAndLights" || g.name == "Road-Mark" || g.name == "Road-Mark-Rev" ).ToArray();
        foreach (GameObject obj in tempArray) {
            // Debug.Log(obj.name);
            DestroyImmediate(obj);
            // foreach (Transform child in obj.transform)	{

            // }
        }
    }







}
