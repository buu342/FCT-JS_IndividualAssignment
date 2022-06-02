/****************************************************************
                       ProcGenner.cs
    
This script handles the level procedural generation. The 
algorithm used here is heavily based on the one proposed by
VAZGRIZ on his blog, and uses the code of the Delaunay 
tetrahedralization mesh generator and A* path finding that he
provided: 
https://vazgriz.com/119/procedurally-generated-dungeons/
****************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcGenner : MonoBehaviour
{
    private const float GridScale     = 3.0f; // The size of each grid mesh (in world units)
    private const int   MapSize_X     = 30;   // Maximum map size on X (in grid units)
    private const int   MapSize_Y     = 5;    // Maximum map size on Y (in grid units)
    private const int   MapSize_Z     = 30;   // Maximum map size on Z (in grid units)
    private const int   MinRoomSize_X = 3;    // Minimum room size on X (in grid units)
    private const int   MinRoomSize_Y = 2;    // Minimum room size on Y (in grid units)
    private const int   MinRoomSize_Z = 3;    // Minimum room size on Z (in grid units)
    private const int   MaxRoomSize_X = 6;    // Maximum room size on X (in grid units)
    private const int   MaxRoomSize_Y = 3;    // Maximum room size on Y (in grid units)
    private const int   MaxRoomSize_Z = 6;    // Maximum room size on Z (in grid units)
    private const int   MaxRooms      = 30;    // Maximum number of rooms to generate
    private Vector3     Center        = new Vector3(ProcGenner.MapSize_X/2, ProcGenner.MapSize_Y/2, ProcGenner.MapSize_Z/2);
    
    private enum LevelType
    {
        First,
        NotFirst
    };
    
    private enum BlockType
    {
        None,
        Room,
        Corridor,
        Stairs
    };
    
    public GameObject m_Camera;
    public GameObject m_FloorPrefab;
    public GameObject m_CeilingPrefab;
    public GameObject m_StairPrefab;
    public GameObject m_PlayerPrefab;
    public Material m_MaterialRoom;
    public Material m_MaterialCorridor;
    public Material m_MaterialStairs;
    public Material m_MaterialSpawn;
    public Material m_MaterialExit;
    
    private Delaunay3D m_Delaunay;
    private HashSet<Prim.Edge> m_SelectedEdges;
    private BlockType[,,] m_Grid;
    private List<GameObject> m_Entities;
    private List<List<GameObject>> m_Rooms;
    private List<List<GameObject>> m_Corridors;
    private List<Graphs.Vertex> m_Vertices;
    private Dictionary<Graphs.Vertex, List<GameObject>> m_RoomVerts;
    
    
    /*==============================
        Awake
        Called when the controller is created
    ==============================*/
    
    void Awake()
    {
        #if UNITY_EDITOR
            System.DateTime time = System.DateTime.Now;
            int attempts = 1;
            int roomsculled = 0;
        #endif
        while (true)
        {            
            // Initialize our data structures
            this.m_Grid = new BlockType[ProcGenner.MapSize_X, ProcGenner.MapSize_Y, ProcGenner.MapSize_Z];
            this.m_Vertices = new List<Graphs.Vertex>();
            this.m_RoomVerts = new Dictionary<Graphs.Vertex, List<GameObject>>();
            
            // Initialize the grid
            for (int i=0; i<ProcGenner.MapSize_X; i++)
                for (int j=0; j<ProcGenner.MapSize_Y; j++)
                    for (int k=0; k<ProcGenner.MapSize_Z; k++)
                        this.m_Grid[i, j, k] = BlockType.None;
                    
            // If we have non empty lists, then cycle through them and destroy all the objects contained within
            if (this.m_Entities != null)
                foreach (GameObject obj in this.m_Entities)
                    Destroy(obj);
            if (this.m_Rooms != null)
                foreach (List<GameObject> l in this.m_Rooms)
                    foreach (GameObject obj in l)
                        Destroy(obj);
            if (this.m_Corridors != null)
                foreach (List<GameObject> l in this.m_Corridors)
                    foreach (GameObject obj in l)
                        Destroy(obj);
            this.m_Entities = new List<GameObject>();
            this.m_Rooms = new List<List<GameObject>>();
            this.m_Corridors = new List<List<GameObject>>();
                    
            // Generate the rooms
            GenerateRooms(LevelType.First);
            
            // Then generate the Delaunay tetrahedralization mesh for the map
            MakeDelaunay3D();
            
            // Find a minimum spanning tree to generate a path that makes every room reachable
            SelectCorridors();
            
            // Check the start and end room have edges, if not, generate another map
            if (ConfirmBeatable())
                break;
            #if UNITY_EDITOR
                attempts++;
            #endif
        }
        
        // Cull rooms which don't have any edges coming out of them
        #if UNITY_EDITOR
            roomsculled = this.m_Rooms.Count;
        #endif
        CullEmptyRooms();
        #if UNITY_EDITOR
            roomsculled -= this.m_Rooms.Count;
        #endif
        
        // Finally, generate the corridors themselves
        GenerateCorridors();
        
        // Show some statistics if we're in debug mode
        #if UNITY_EDITOR
            Debug.Log("Level generation data:");
            Debug.Log("* Time taken -> "+(System.DateTime.Now-time).TotalMilliseconds+"ms");
            Debug.Log("* Attempts -> "+attempts);
            Debug.Log("* Rooms Culled -> "+roomsculled);
        #endif
    }

    
    /*==============================
        GenerateRooms
        Generates the rooms
        @param The type of level
    ==============================*/
    
    void GenerateRooms(LevelType ltype)
    {
        int roomcount = ProcGenner.MaxRooms;
        Vector3 coord;
        Vector3 size;
        GameObject instobj;
        
        // Start by placing our spawn somewhere outside the grid
        coord = new Vector3((int)Random.Range(ProcGenner.MaxRoomSize_X, ProcGenner.MapSize_X-ProcGenner.MaxRoomSize_X), ProcGenner.MapSize_Y/2, -1);
        instobj = Instantiate(this.m_FloorPrefab, (coord-Center)*ProcGenner.GridScale, this.m_FloorPrefab.transform.rotation);
        instobj.GetComponent<Renderer>().material = this.m_MaterialSpawn;
        this.m_Entities.Add(instobj);
        
        // Create the player on the spawn
        instobj = Instantiate(this.m_PlayerPrefab, (coord-Center)*ProcGenner.GridScale, Quaternion.identity);
        this.m_Camera.GetComponent<CameraController>().SetTarget(instobj.transform.Find("CameraTarget").gameObject);
        this.m_Entities.Add(instobj);
        
        // Now that we have our spawn generated, place a room at our spawn if we're not playing the first level, otherwise make a corridor
        if (ltype != LevelType.First)
        {
            size = GenerateRoomVector();
            coord += new Vector3((int)(-size.x/2), 0, 1);
            PlaceRoom(coord, size);
            roomcount--;
        }
        else
        {
            List<GameObject> corridor = new List<GameObject>();
            coord += new Vector3(0, 0, 4);
            for (int i=0; i<4; i++)
            {
                List<GameObject> res = PlaceCorridor(coord - new Vector3(0, 0, i));
                res.ForEach(item => corridor.Add(item));
            }
            this.m_Corridors.Add(corridor);
            this.m_Vertices.Add(new Graphs.Vertex(coord));
        }
        
        // Then place the exit on the other end
        coord = new Vector3((int)Random.Range(ProcGenner.MaxRoomSize_X, ProcGenner.MapSize_X-ProcGenner.MaxRoomSize_X), ProcGenner.MapSize_Y/2, ProcGenner.MapSize_Z);
        instobj = Instantiate(this.m_FloorPrefab, (coord-Center)*ProcGenner.GridScale, this.m_FloorPrefab.transform.rotation);
        instobj.GetComponent<Renderer>().material = this.m_MaterialExit;
        this.m_Entities.Add(instobj);
        
        // Now place a room just before the end
        size = GenerateRoomVector();
        coord += new Vector3((int)(-size.x/2), 0, -size.z);
        PlaceRoom(coord, size);
        roomcount--;
        
        // Now generate the rest of the rooms
        while (roomcount > 0)
        {
            bool overlap = false;
            int yval = ProcGenner.MapSize_Y/2;
            size = GenerateRoomVector();
            
            // Given a 1/4 chance, generate a room not at Y=0
            if (Random.Range(0, 4) == 0)
                yval = Random.Range(0, ProcGenner.MapSize_Y-1-ProcGenner.MaxRoomSize_Y);
            coord = new Vector3(
                (int)Random.Range(0, ProcGenner.MapSize_X-1-ProcGenner.MaxRoomSize_X), 
                (int)yval, 
                (int)Random.Range(0, ProcGenner.MapSize_X-1-ProcGenner.MaxRoomSize_Z)
            );
        
            // Check that the generated coordinate is not overlapping with any room or hallway
            for (int i=0; i<size.z && !overlap; i++)
            {
                for (int j=0; j<size.y && !overlap; j++)
                {
                    for (int k=0; k<size.x; k++)
                    {
                        int gridx = (int)(coord.x + k);
                        int gridy = (int)(coord.y + j);
                        int gridz = (int)(coord.z + i);
                        if ((this.m_Grid[gridx, gridy, gridz] != BlockType.None) ||
                            (this.m_Grid[Mathf.Min(gridx+1, ProcGenner.MapSize_X-1), gridy, gridz] != BlockType.None) ||
                            (this.m_Grid[Mathf.Max(gridx-1, 0), gridy, gridz] != BlockType.None) ||
                            (this.m_Grid[gridx, Mathf.Min(gridy+1, ProcGenner.MapSize_Y-1), gridz] != BlockType.None) ||
                            (this.m_Grid[gridx, Mathf.Max(gridy-1, 0), gridz] != BlockType.None) ||
                            (this.m_Grid[gridx, gridy, Mathf.Min(gridz+1, ProcGenner.MapSize_Z-1)] != BlockType.None) ||
                            (this.m_Grid[gridx, gridy, Mathf.Max(gridz-1, 0)] != BlockType.None)
                        )
                        {
                            overlap = true;
                            break;
                        }
                    }
                }
            }
            if (overlap)
            {
                roomcount--;
                continue;
            }
            
            // We have a valid room, so place it
            PlaceRoom(coord, size);
            roomcount--;
        }
    }

    
    /*==============================
        GenerateRoomVector
        Generates a room vector based on the min and max sizes
        @return The room vector
    ==============================*/
    
    Vector3 GenerateRoomVector()
    {
        return new Vector3(
            Random.Range(ProcGenner.MinRoomSize_X, ProcGenner.MaxRoomSize_X+1),
            Random.Range(ProcGenner.MinRoomSize_Y, ProcGenner.MaxRoomSize_Y+1),
            Random.Range(ProcGenner.MinRoomSize_Z, ProcGenner.MaxRoomSize_Z+1)
        );
    }
    
    
    /*==============================
        PlaceRoom
        Places a room on the map
        @param The coordinate to place the room in
        @param The size of the room
    ==============================*/
    
    void PlaceRoom(Vector3 pos, Vector3 size)
    {
        GameObject instobj;
        Graphs.Vertex vert;
        List<GameObject> rm = new List<GameObject>();
        for (int i=0; i<size.z; i++)
        {
            for (int j=0; j<size.x; j++)
            {
                // Floor
                Vector3 finalpos = pos + new Vector3(j, 0, i);
                instobj = Instantiate(this.m_FloorPrefab, (finalpos-Center)*ProcGenner.GridScale, this.m_FloorPrefab.transform.rotation);
                instobj.GetComponent<Renderer>().material = this.m_MaterialRoom;
                rm.Add(instobj);
                
                // Mark the entire block as a room in our grid
                for (int k=0; k<size.y; k++)
                    this.m_Grid[(int)finalpos.x, (int)finalpos.y+k, (int)finalpos.z] = BlockType.Room;
                
                // Ceiling
                finalpos += new Vector3(0, size.y, 0);
                instobj = Instantiate(this.m_CeilingPrefab, (finalpos-Center)*ProcGenner.GridScale, this.m_CeilingPrefab.transform.rotation);
                instobj.GetComponent<Renderer>().material = this.m_MaterialRoom;
                rm.Add(instobj);
            }
        }
        vert = new Graphs.Vertex(pos + new Vector3(size.x*0.5f, 0.0f, size.z*0.5f));
        this.m_Rooms.Add(rm);
        this.m_Vertices.Add(vert);
        this.m_RoomVerts.Add(vert, rm);
    }
      
    
    /*==============================
        PlaceCorridor
        Places a corridor on the map
        @param The coordinate to place the corridor in
        @return A list of created objects
    ==============================*/  
    
    List<GameObject> PlaceCorridor(Vector3 pos)
    {
        List<GameObject> corridor = new List<GameObject>();
        GameObject instobj;
        
        // Floor
        instobj = Instantiate(this.m_FloorPrefab, (pos-Center)*ProcGenner.GridScale, this.m_FloorPrefab.transform.rotation);
        instobj.GetComponent<Renderer>().material = this.m_MaterialCorridor;
        this.m_Grid[(int)pos.x, (int)pos.y, (int)pos.z] = BlockType.Corridor;
        corridor.Add(instobj);
        
        // Ceiling
        pos += new Vector3(0, 1, 0);
        instobj = Instantiate(this.m_CeilingPrefab, (pos-Center)*ProcGenner.GridScale, this.m_CeilingPrefab.transform.rotation);
        instobj.GetComponent<Renderer>().material = this.m_MaterialCorridor;
        corridor.Add(instobj);
        
        // Return our generated objects
        return corridor;
    }
    
    
    /*==============================
        PlaceStairs
        Places a staircase on the map
        @param The coordinate to place the stair in
        @param The angle of the stairs
        @return A list of created objects
    ==============================*/  
    
    List<GameObject>  PlaceStairs(Vector3 pos, Quaternion angle)
    {
        List<GameObject> corridor = new List<GameObject>();
        
        // Floor
        GameObject instobj = Instantiate(this.m_StairPrefab, (pos-Center)*ProcGenner.GridScale, this.m_StairPrefab.transform.rotation*angle);
        instobj.GetComponent<Renderer>().material = this.m_MaterialStairs;
        corridor.Add(instobj);
        
        // Return our generated objects
        return corridor;
    }
    
    
    /*==============================
        MakeDelaunay3D
        Generates a Delaunay triangulation mesh for the map
    ==============================*/

    void MakeDelaunay3D()
    {
        this.m_Delaunay = Delaunay3D.Triangulate(this.m_Vertices);
    }
    
    
    /*==============================
        ConfirmBeatable
        Confirms whether our level can be finished or not
        @return Whether the level can be completed
    ==============================*/
    
    bool ConfirmBeatable()
    {
        bool hasedge = false;
        Vector3 startvert = this.m_Vertices[0].Position;
        Vector3 endvert = this.m_Vertices[1].Position;
        
        // Check our start vert is in the list of edges
        foreach (Prim.Edge edge in this.m_SelectedEdges)
        {
           if ((edge.U.Position - startvert).sqrMagnitude < 3 || (edge.V.Position - startvert).sqrMagnitude < 3)
           {
               hasedge = true;
               break;
           }
        }
        if (!hasedge)
            return false;
        
        // Check our end vert is in the list of edges
        hasedge = false;
        foreach (Prim.Edge edge in this.m_SelectedEdges)
        {
           if ((edge.U.Position - endvert).sqrMagnitude < 3 || (edge.V.Position - endvert).sqrMagnitude < 3)
           {
               hasedge = true;
               break;
           }
        }
        if (!hasedge)
            return false;
        
        // Both tests passed, we have a completable level
        return true;
    }
    
    
    /*==============================
        CullEmptyRooms
        Culls any rooms which are not connected to anything
    ==============================*/
    
    void CullEmptyRooms()
    {
        // Check if this vert is connected to something
        foreach (KeyValuePair<Graphs.Vertex, List<GameObject>> entry in this.m_RoomVerts)
        {
            bool connected = false;
            Vector3 vertpos = entry.Key.Position;
            foreach (Prim.Edge edge in this.m_SelectedEdges)
            {
               if ((edge.U.Position - vertpos).sqrMagnitude < 3 || (edge.V.Position - vertpos).sqrMagnitude < 3)
               {
                   connected = true;
                   break;
               }
            }
        
            // If the room isn't connected to anything, remove it
            if (!connected)
            {
                foreach (GameObject obj in entry.Value)
                    Destroy(obj);
                this.m_Rooms.Remove(entry.Value);
            }
        }
    }
    
    
    /*==============================
        SelectCorridors
        Creates a minimum spanning tree to generate a completable map
    ==============================*/
    
    void SelectCorridors()
    {
        Graphs.Vertex start = this.m_Vertices[0];
        List<Prim.Edge> edges = new List<Prim.Edge>();
        List<Prim.Edge> mstree;

        // Convert the Delaunay edges to primitive edges
        foreach (Delaunay3D.Edge edge in this.m_Delaunay.Edges)
            edges.Add(new Prim.Edge(edge.U, edge.V));

        // Solve the MSTree
        mstree = Prim.MinimumSpanningTree(edges, start);
        
        // Get a list of all the remaining edges that aren't in the MSTree
        this.m_SelectedEdges = new HashSet<Prim.Edge>(mstree);
        HashSet<Prim.Edge> remaining = new HashSet<Prim.Edge>(edges);
        remaining.ExceptWith(this.m_SelectedEdges);

        // Randomly pick some to be in our list of selected edges (to create loops)
        foreach (Prim.Edge edge in remaining) 
            if (Random.Range(0, 4) == 0)
                this.m_SelectedEdges.Add(edge);
    }

    
    /*==============================
        GenerateCorridors
        Generates the actual corridors themselves
    ==============================*/

    void GenerateCorridors()
    {
        // Try to generate paths from our edges
        foreach (Graphs.Edge edge in this.m_SelectedEdges)
        {
            Vector3Int gridsize = new Vector3Int(ProcGenner.MapSize_X, ProcGenner.MapSize_Y, ProcGenner.MapSize_Z);
            AStar astr = new AStar(gridsize);
            Vector3Int startpos = new Vector3Int((int)edge.U.Position.x, (int)edge.U.Position.y, (int)edge.U.Position.z);
            Vector3Int endpos = new Vector3Int((int)edge.V.Position.x, (int)edge.V.Position.y, (int)edge.V.Position.z);
            
            // Pathfind, using our custom cost function
            List<Vector3Int> path = astr.FindPath(startpos, endpos, (AStar.Node a, AStar.Node b) => {
                
                AStar.PathCost pathcost = new AStar.PathCost();
                Vector3Int delta = b.Position - a.Position;

                // If we have no change in Y, we are making a flat corridor
                if (delta.y == 0)
                {
                    pathcost.cost = Vector3Int.Distance(b.Position, endpos);

                    // Calculate the path heuristic given what we're currently traveling into
                    if (this.m_Grid[b.Position.x, b.Position.y, b.Position.z] == BlockType.Stairs)
                        return pathcost;
                    else if (this.m_Grid[b.Position.x, b.Position.y, b.Position.z] == BlockType.Room)
                        pathcost.cost += 5;
                    else if (this.m_Grid[b.Position.x, b.Position.y, b.Position.z] == BlockType.None)
                        pathcost.cost += 1;
                    
                    // We're currently in a valid spot
                    pathcost.traversable = true;
                }
                else
                {
                    Vector3Int apos = a.Position;
                    Vector3Int bpos = b.Position;
                    
                    // Stop if we are not connected by 2 corridor blocks
                    if ((this.m_Grid[apos.x, apos.y, apos.z] != BlockType.None && this.m_Grid[apos.x, apos.y, apos.z] != BlockType.Corridor)
                        || (this.m_Grid[bpos.x, bpos.y, bpos.z] != BlockType.None && this.m_Grid[bpos.x, bpos.y, bpos.z] != BlockType.Corridor)
                    ) 
                        return pathcost;

                    pathcost.cost = 100 + Vector3Int.Distance(bpos, endpos);

                    // Create some helper variables
                    int xDir = Mathf.Clamp(delta.x, -1, 1);
                    int zDir = Mathf.Clamp(delta.z, -1, 1);
                    Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                    Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                    // Ensure we're not traveling out of bounds
                    if (!(new BoundsInt(Vector3Int.zero, gridsize).Contains(apos + verticalOffset)) ||
                        !(new BoundsInt(Vector3Int.zero, gridsize).Contains(apos + horizontalOffset)) ||
                        !(new BoundsInt(Vector3Int.zero, gridsize).Contains(apos + verticalOffset + horizontalOffset))
                    )
                        return pathcost;

                    // Ensure we're traveling to a grid space that is empty
                    Vector3Int offset1 = apos + horizontalOffset;
                    Vector3Int offset2 = apos + horizontalOffset*2;
                    Vector3Int offset3 = apos + verticalOffset + horizontalOffset;
                    Vector3Int offset4 = apos + verticalOffset + horizontalOffset*2;
                    if (this.m_Grid[offset1.x, offset1.y, offset1.z] != BlockType.None
                        || this.m_Grid[offset2.x, offset2.y, offset2.z] != BlockType.None
                        || this.m_Grid[offset3.x, offset3.y, offset3.z] != BlockType.None
                        || this.m_Grid[offset4.x, offset4.y, offset4.z] != BlockType.None) {
                        return pathcost;
                    }

                    // We're currently in a valid spot
                    pathcost.traversable = true;
                    pathcost.isStairs = true;
                }

                return pathcost;
            });
            
            // If a valid path was found, create the corridor/stairs
            if (path != null)
            {
                List<GameObject> corridor = new List<GameObject>();
                
                // Travel the path to place corridors/stairs
                for (int i = 0; i < path.Count; i++)
                {
                    Vector3Int current = path[i];

                    if (this.m_Grid[current.x, current.y, current.z] == BlockType.None)
                        this.m_Grid[current.x, current.y, current.z] = BlockType.Corridor;

                    if (i > 0)
                    {
                        Vector3Int prev = path[i - 1];
                        Vector3Int delta = current - prev;

                        // If the delta y is non-zero, then we have stairs to place
                        if (delta.y != 0)
                        {
                            int xDir = Mathf.Clamp(delta.x, -1, 1);
                            int zDir = Mathf.Clamp(delta.z, -1, 1);
                            Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);
                            
                            // Calculate the offsets to help us out
                            Vector3Int offset1 = prev + horizontalOffset;
                            Vector3Int offset2 = prev + horizontalOffset*2;
                            Vector3Int offset3 = prev + verticalOffset + horizontalOffset;
                            Vector3Int offset4 = prev + verticalOffset + horizontalOffset*2;
                            
                            // We're going to need these in a sec
                            float ang = 0;
                            Vector3 placepos = offset1;
                            
                            // Mark the grid areas as stairs
                            this.m_Grid[offset1.x, offset1.y, offset1.z] = BlockType.Stairs;
                            this.m_Grid[offset2.x, offset2.y, offset2.z] = BlockType.Stairs;
                            this.m_Grid[offset3.x, offset3.y, offset3.z] = BlockType.Stairs;
                            this.m_Grid[offset4.x, offset4.y, offset4.z] = BlockType.Stairs;

                            // Rotate the stairs based on the delta z
                            if ((zDir > 0 && delta.y > 0) || (zDir < 0 && delta.y < 0))
                            {
                                ang = 0.0f;
                                if (delta.y > 0)
                                    placepos = offset1;
                                else
                                    placepos = offset4;
                            }
                            else if ((zDir > 0 && delta.y < 0) || (zDir < 0 && delta.y > 0))
                            {
                                ang = 180.0f;
                                if (delta.y < 0)
                                    placepos = offset4;
                                else
                                    placepos = offset1;
                            }
                            
                            // Rotate the stairs based on the delta x
                            if ((xDir > 0 && delta.y > 0) || (xDir < 0 && delta.y < 0))
                            {
                                ang = 90.0f;
                                if (delta.y > 0)
                                    placepos = offset1;
                                else
                                    placepos = offset4;
                            }
                            else if ((xDir > 0 && delta.y < 0) || (xDir < 0 && delta.y > 0))
                            {
                                ang = -90.0f;
                                if (delta.y < 0)
                                    placepos = offset4;
                                else
                                    placepos = offset1;
                            }
                            
                            // Place the stair object
                            List<GameObject> res = PlaceStairs(placepos, Quaternion.Euler(0, 0, ang));
                            res.ForEach(item => corridor.Add(item));
                        }
                    }
                }

                // Create hallways
                foreach (Vector3Int pos in path)
                {
                    if (this.m_Grid[pos.x, pos.y, pos.z] == BlockType.Corridor)
                    {
                        List<GameObject> res = PlaceCorridor(pos);
                        res.ForEach(item => corridor.Add(item));
                    }
                }
                
                // Add this to our list of corridors
                this.m_Corridors.Add(corridor);
            }
        }
    }


    #if UNITY_EDITOR
        /*==============================
            OnDrawGizmos
            Draws extra debug stuff in the editor
        ==============================*/
        
        public virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(-(new Vector3(1, -1, 1)*ProcGenner.GridScale)/2, (new Vector3(ProcGenner.MapSize_X, ProcGenner.MapSize_Y, ProcGenner.MapSize_Z)*ProcGenner.GridScale));
            
            if (this.m_Vertices != null && this.m_Vertices.Count > 0)
            {
                Gizmos.color = Color.white;
                for (int i=0; i<this.m_Vertices.Count; i++)
                    Gizmos.DrawWireSphere(-(new Vector3(1, 0, 1)*ProcGenner.GridScale)/2 + (this.m_Vertices[i].Position-Center)*ProcGenner.GridScale, 3.0f);
            }
            
            if (this.m_SelectedEdges != null && this.m_Delaunay.Edges != null && this.m_Delaunay.Edges.Count > 0)
            {
                Gizmos.color = Color.white;
                foreach (var edge in this.m_SelectedEdges)
                    Gizmos.DrawLine((edge.U.Position-Center)*ProcGenner.GridScale, (edge.V.Position-Center)*ProcGenner.GridScale);
            }
        }
    #endif
}