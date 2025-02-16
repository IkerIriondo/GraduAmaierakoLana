using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentManager : MonoBehaviour
{
    public static EnviromentManager Instance { get; private set; }
    public bool IsInitialized { get; private set; } = false;

    public Zone playerCurrentZone;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void Initialize()
    {
        Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();

        foreach (var waypoint in allWaypoints)
        {
            waypoint.WaypointZoneConnect();
        }

        foreach(var waypoint in allWaypoints)
        {
            waypoint.AutoConnectWaypoints();
        }

        Zone[] allZones = FindObjectsOfType<Zone>();
        foreach(var zone in allZones)
        {
            zone.AutoConnectZones();
        }
        IsInitialized = true;
        Debug.Log("Initialization complete");
    }

    public void SetCurrentZone(Zone zone)
    {
        playerCurrentZone = zone;
        TacticalAI.ActiveInstance.OnPlayerZoneChanged(zone);
    }

}
