using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine.Networking;
public class APICall : MonoBehaviour
{
    Coroutine coroutine = null;
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
    int interval = 1; 
    float nextTime = 0;
    private Dictionary<string, List<Building>> subnetGroups;
    private CityInfo cityInfo;
    void Start()
    {
        // cityInfo = GetBuildings();
        StartCoroutine(ProcessRequest("https://dl.dropbox.com/s/4z4bzprj1pud3tq/Assets.json?dl=0"));
        // subnetGroups = GetSubnetGroups(cityInfo);
        
        // foreach (KeyValuePair<string, List<Building>> kvp in subnetGroups)
        // {
        //     //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
        //     Debug.Log(string.Format("Key = {0}", kvp.Key));
        //     foreach(var x in kvp.Value) {
        //         Debug.Log(string.Format("Member = {0} - {1}", x.hostname, x.ipAddress));
        //     }
        // }

        // coroutine = StartCoroutine(onCoroutine());

    }
    private IEnumerator ProcessRequest(string uri)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError)
            {
                Debug.Log(request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                cityInfo = GetBuildings(jsonResponse);
                subnetGroups = GetSubnetGroups(cityInfo);

                foreach (KeyValuePair<string, List<Building>> kvp in subnetGroups)
                {
                    //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                    Debug.Log(string.Format("Key = {0}", kvp.Key));
                    foreach(var x in kvp.Value) {
                        Debug.Log(string.Format("Member = {0} - {1}", x.hostname, x.ipAddress));
                    }
                }
            }
        }
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
    public CityInfo GetBuildings(string jsonResponse)
    {
        //Valid: "https://dl.dropbox.com/s/4z4bzprj1pud3tq/Assets.json?dl=0"
        //Sample: https://dl.dropbox.com/s/fbh6jbyzrf86g0x/Assets-sample.json?dl=0

        CityInfo info = JsonConvert.DeserializeObject<CityInfo>(jsonResponse);
        return info;
    }

    // Update is called once per frame
    void Update()
    {
        // if (Time.time >= nextTime) {
 
        //       //do something here every interval seconds
 
        //     nextTime += interval; 
        //     cityInfo = GetBuildings();
        //     subnetGroups = GetSubnetGroups(cityInfo);
        //     Debug.Log("API Update!!!");
        //     // foreach (KeyValuePair<string, List<Building>> kvp in subnetGroups)
        //     // {
        //     //     //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
        //     //     Debug.Log(string.Format("Key = {0}", kvp.Key));
        //     //     foreach(var x in kvp.Value) {
        //     //         Debug.Log(string.Format("Member = {0} - {1}", x.hostname, x.ipAddress));
        //     //     }
        //     // }
 
        //  }
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
            string jsonResponse = CallAPI();
            // TrackChanges(jsonResponse);
            yield return new WaitForSeconds(5f);
        }
    }
    public void StopTracking(){
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
    }
}
