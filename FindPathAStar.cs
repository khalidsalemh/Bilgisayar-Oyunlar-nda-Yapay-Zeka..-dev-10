using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FindPathAStar : MonoBehaviour
{
    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker goalNode;
    PathMarker startNode;
    PathMarker lastPos;
    bool done = false;

    void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers)
            Destroy(m);
    }

    void BeginSearch()
    {
        done = false;
        RemoveAllMarkers();

        List<MapLocation> locations = new List<MapLocation>();
        for (int z = 1; z < maze.depth - 1; z++)
            for (int x = 1; x < maze.depth - 1; x++)
            {
                if (maze.map[x, z] != 1) //0: empty space, 1: wall
                    locations.Add(new MapLocation(x, z));
            }

        //2
        locations.Shuffle();
        //0 for G, H, and F values. Null for the parent.
        Vector3 startLocation = new Vector3(locations[0].x * maze.scale, 0, locations[0].z * maze.scale);
        startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].z), 0, 0, 0,
        Instantiate(start, startLocation, Quaternion.identity), null);
        //

        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale, 0, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0, 0, 0,
        Instantiate(end, goalLocation, Quaternion.identity), null);


        //3


        //
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0, 0, 0,
        Instantiate(end, goalLocation, Quaternion.identity), null);

        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;

    }



    void GetPath()
    {
        RemoveAllMarkers();
        PathMarker begin = lastPos;
        while (!startNode.Equals(begin) && begin != null)
        {
            Instantiate(pathP, new Vector3(begin.location.x * maze.scale, 0, begin.location.z * maze.scale), Quaternion.identity);
            begin = begin.parent;
        }
        Instantiate(pathP, new Vector3(startNode.location.x * maze.scale, 0, startNode.location.z * maze.scale), Quaternion.identity);
    }
    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach (PathMarker p in open)
        {
            if (p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker)
    {
        foreach (PathMarker p in closed)
        {
            if (p.location.Equals(marker))
                return true;

        }
        return false;
    }
    void Search(PathMarker thisNode)
    {
        //SEARCH 1
        if (thisNode.Equals(goalNode))
        { done = true; return; }
        //The neighbors are horizontal and vertical but we can also add diagonals as well. You can check the Maze script.
        foreach (MapLocation dir in maze.directions)
        {
            MapLocation neighbor = dir + thisNode.location;
            if (maze.map[neighbor.x, neighbor.z] == 1) continue; //skip walls
            if (neighbor.x < 1 || neighbor.x >= maze.width || neighbor.z < 1 || neighbor.z >= maze.depth) continue;
            if (IsClosed(neighbor)) continue;

            //SEARCH 2
            float G = Vector2.Distance(thisNode.location.ToVector(), neighbor.ToVector()) + thisNode.G;
            float H = Vector2.Distance(neighbor.ToVector(), goalNode.location.ToVector());
            float F = G + H;
            GameObject pathBlock = Instantiate(pathP, new Vector3(neighbor.x * maze.scale, 0, neighbor.z * maze.scale), Quaternion.identity);
            TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
            //In the prefab, G is attached first, then H, and then F. You can check the prefab
            values[0].text = "G:" + G.ToString("0.00");
            values[1].text = "H:" + G.ToString("0.00");
            values[2].text = "F:" + G.ToString("0.00");
            if (!UpdateMarker(neighbor, G, H, F, thisNode))
                open.Add(new PathMarker(neighbor, G, H, F, pathBlock, thisNode));

            //SEARCH 3
            //Select the one from the open with the smallest F value
            //If multiple F values, select the one with the smallest H value
            open = open.OrderBy(p => p.F).ThenBy(n => n.H).ToList<PathMarker>();
            PathMarker pm = (PathMarker)open.ElementAt(0);
            closed.Add(pm);
            open.RemoveAt(0);
            pm.marker.GetComponent<Renderer>().material = closedMaterial;
            lastPos = pm;
            //

        }

        // UPDATE MARKER.....



    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) BeginSearch();
        if (Input.GetKeyDown(KeyCode.C) && !done) Search(lastPos);
        if (Input.GetKeyDown(KeyCode.M)) GetPath();
    }
}


