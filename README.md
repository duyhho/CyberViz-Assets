# Cybersecurity Visualization
![WhatsApp Image 2021-06-29 at 10 29 38 PM](https://github.com/duyhho/CyberViz-Assets/assets/17374092/adebb072-e129-4b01-9ab0-0a9e8f68faa5)
![WhatsApp Image 2021-06-29 at 10 32 14 PM](https://github.com/duyhho/CyberViz-Assets/assets/17374092/c502980b-992f-4e64-8501-cdf1366daaed)
## Description: 

An envisioned solution of a corporate network visualized in an intuitive
fashion as a cityscape with the goal of supporting the cybersecurity analysis process. Network traffic and volume are **dynamically** fetched and visualized in **real-time** through a web application and augmented-reality (AR) platform via Microsoft HoloLens 2.
This application transforms the cybersecurity and incident response analysis into a dynamic and game-like experience within an inventive visual realm, aiming to alleviate the tedium and fatigue commonly experienced by analysts. It enhances the efficacy of their analyses. Furthermore, it grants exceptional flexibility in the execution of analytical tasks by integrating gamification into the analysts' workflow, yet it preserves the option for them to revert to conventional methods on their usual platforms as needed.
## Data Stream Sample:
```
{
  "Params": {
    "TableName": "Labyrinth-events"
  },
  "Payload": {
    "Items": [
      {
        "Category_Subcategory": "My Alerts - SecOps",
        "Priority": "High",
        "DateTime": "4/12/2023 14:45",
        "Alert_name": "SecOpsAlertNewThreatDetected",
        "Status": "New",
        "When": "2 hours ago",
        "Destination_Hostname": "SVR2023HST001",
        "AlertID": "98765432",
        "Destination_IP": "192.168.1.10",
        "Source_IP": "10.0.0.5",
        "Alert_Summary": "Suspicious activity detected from multiple endpoints",
        "Source_Hostname": "Unknown"
      },
      {
        "Category_Subcategory": "My Alerts - SecOps",
        "Priority": "High",
        "DateTime": "4/12/2023 09:30",
        "Alert_name": "SecOpsAlertHighRiskPattern",
        "Status": "New",
        "When": "7 hours ago",
        "Destination_Hostname": "SVR2023HST002",
        "AlertID": "12345678",
        "Destination_IP": "192.168.1.20",
        "Source_IP": "10.0.0.6",
        "Alert_Summary": "High-risk traffic pattern observed",
        "Source_Hostname": "Gateway01"
      }
    ]
  }
}

```
# Video:
https://umkc.box.com/s/slbpxov4mim4nuj20u8lm3vh4on7w1iv

https://umkc.box.com/s/4rexb0yaadlsjotu0e6avbfyquzitad5
## Specifications:

1) Walls/city 
block outline represent network subnets (all blocks will have this wall)
- Blue wall is subnet with no security events and should be shorter
- Red wall is a subnet with at least 1 security event and should be taller (picture for reference height/ratio)
2) Size of building represents system size (cloud virtual machine sizes provided)
3) Shape of building represents type of system (hexagon, circle, square, triangle, etc.) (See Diagram 1.2 below)
4) Light on building represents activity (any known traffic on system will generate light)
- Red light on building indicates malware/compromise events
- No light represents known system (IP address) but no known traffic
- Yellow light represents suspicious activity (outside of the scope of this POC unless ideas arise)
5) Beam of light from building represents traffic to the internet. Strength of beam represents frequency. (busy
systems would have a stronger beam of light, intensity is not critical for POC)
-  Red beam of light represents traffic/security event to/from known bad source (represented by blacklisted
countries)
- Blue beam of light represents traffic to/from known-good systems
(whitelisted/vendor/corporate IPs)
- White beam of light to/from unknown internet hosts (majority of data)
6) Drones represent other users and their scope of view (not within scope of POC)
