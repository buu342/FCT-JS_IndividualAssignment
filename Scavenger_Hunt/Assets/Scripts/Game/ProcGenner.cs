/****************************************************************
                       ProcGenner.cs
    
This script handles the level procedural generation. The 
algorithm used here is heavily based on the one proposed by
VAZGRIZ on his blog, and uses the code of the Delaunay 
tetrahedralization mesh generator and A* path finding that he
provided: 
https://vazgriz.com/119/procedurally-generated-dungeons/

TODO:
    * Prevent the generation of double doors
    * Prevent the generation of double stairs
****************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;

public class ProcGenner : MonoBehaviour
{
    public  const float   GridScale     = 4.0f; // The size of each grid mesh (in world units)
    private const int     MapSize_X     = 30;   // Maximum map size on X (in grid units)
    private const int     MapSize_Y     = 6;    // Maximum map size on Y (in grid units)
    private const int     MapSize_Z     = 30;   // Maximum map size on Z (in grid units)
    private const int     MinRoomSize_X = 3;    // Minimum room size on X (in grid units)
    private const int     MinRoomSize_Y = 2;    // Minimum room size on Y (in grid units)
    private const int     MinRoomSize_Z = 3;    // Minimum room size on Z (in grid units)
    private const int     MaxRoomSize_X = 6;    // Maximum room size on X (in grid units)
    private const int     MaxRoomSize_Y = 3;    // Maximum room size on Y (in grid units)
    private const int     MaxRoomSize_Z = 6;    // Maximum room size on Z (in grid units)
    private const int     MaxRooms      = 30;   // Maximum number of rooms to generate
    public  Vector3       Center        = new Vector3(ProcGenner.MapSize_X/2, ProcGenner.MapSize_Y/2, ProcGenner.MapSize_Z/2);
    
    private enum LevelType
    {
        First,
        NotFirst
    };
    
    public enum BlockType
    {
        None,
        Room,
        Corridor,
        Stairs
    };
    
    public struct BlockDef
    {
        public BlockType type;
        public RoomDef roomdef;         // C# doesn't allow for void* for some reason??????
        public CorridorDef corridordef;
    };
    
    public struct RoomDef
    {
        public GameObject parentobject;
        public Vector3Int position;
        public Vector3Int size;
        public Vector3 midpoint;
        public List<GameObject> objects;
        public List<GameObject> doors;
        public bool visible;
    };
    
    public struct CorridorDef
    {
        public GameObject parentobject;
        public Vector3Int position;
        public Vector3Int direction;
        public List<GameObject> objects;
    };
    
    public GameObject m_Camera;
    public GameObject m_FloorPrefab;
    public GameObject m_FloorDustPrefab;
    public GameObject m_CeilingPrefab;
    public GameObject m_StairPrefab;
    public GameObject m_Wall1Prefab;
    public GameObject m_Wall2Prefab;
    public GameObject m_Wall3Prefab;
    public GameObject m_DoorPrefab;
    public GameObject m_DoorWall1Prefab;
    public GameObject m_DoorWall2Prefab;
    public GameObject m_PlayerPrefab;
    public GameObject m_NavMesh;
    public Material m_MaterialRoom;
    public Material m_MaterialCorridor;
    public Material m_MaterialStairs;
    public Material m_MaterialSpawn;
    public GameObject m_Airlock;
    public GameObject m_ExitElevator;
    public VisualOptimizer m_Optimizer;
    
    private Delaunay3D m_Delaunay;
    private HashSet<Prim.Edge> m_SelectedEdges;
    private BlockDef[,,] m_Grid;
    private List<GameObject> m_Entities;
    private List<RoomDef> m_Rooms;
    private List<CorridorDef> m_Corridors;
    private List<(Vector3, GameObject)> m_Doors;
    private List<Graphs.Vertex> m_Vertices;
    private Dictionary<Graphs.Vertex, List<GameObject>> m_RoomVerts;
    
    
    /*==============================
        GenerateScene
        Procedurally generates a level
    ==============================*/
    
    public void GenerateScene()
    {
        #if UNITY_EDITOR
            System.DateTime time = System.DateTime.Now;
            int attempts = 1;
            int roomsculled = 0;
        #endif
        while (true)
        {            
            // Initialize our data structures
            this.m_Grid = new BlockDef[ProcGenner.MapSize_X, ProcGenner.MapSize_Y, ProcGenner.MapSize_Z];
            this.m_Vertices = new List<Graphs.Vertex>();
            this.m_RoomVerts = new Dictionary<Graphs.Vertex, List<GameObject>>();
            
            // Initialize the grid
            for (int i=0; i<ProcGenner.MapSize_X; i++)
                for (int j=0; j<ProcGenner.MapSize_Y; j++)
                    for (int k=0; k<ProcGenner.MapSize_Z; k++)
                        this.m_Grid[i, j, k] = new BlockDef(){type = BlockType.None};
                    
            // If we have non empty lists, then cycle through them and destroy all the objects contained within
            if (this.m_Entities != null)
                foreach (GameObject obj in this.m_Entities)
                    Destroy(obj);
            if (this.m_Rooms != null)
                foreach (RoomDef l in this.m_Rooms)
                    foreach (GameObject obj in l.objects)
                        Destroy(obj);
            if (this.m_Corridors != null)
                foreach (CorridorDef l in this.m_Corridors)
                    foreach (GameObject obj in l.objects)
                        Destroy(obj);
            if (this.m_Doors != null)
                foreach ((Vector3, GameObject) l in this.m_Doors)
                    Destroy(l.Item2);
            this.m_Entities = new List<GameObject>();
            this.m_Rooms = new List<RoomDef>();
            this.m_Corridors = new List<CorridorDef>();
            this.m_Doors = new List<(Vector3, GameObject)>();
            this.m_Optimizer.SetPlayer(null);
                    
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
        
        // And then fill everything with walls
        GenerateWalls();
        
        // Generate a walkable navmesh
        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        NavMeshData navmeshdata = new NavMeshData();
        NavMesh.AddNavMeshData(navmeshdata);
        Bounds navmeshbounds = new Bounds(Vector3.zero, Center*ProcGenner.GridScale);
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
        List<NavMeshModifier> modifiers = new List<NavMeshModifier>();
        modifiers = NavMeshModifier.activeModifiers;
        for (int i=0; i<modifiers.Count; i++)
        {
            markups.Add(new NavMeshBuildMarkup()
            {
                root = modifiers[i].transform,
                overrideArea = modifiers[i].overrideArea,
                area = modifiers[i].area,
                ignoreFromBuild = modifiers[i].ignoreFromBuild
            });
        }
        NavMeshSurface surface = this.m_NavMesh.GetComponent<NavMeshSurface>();
        NavMeshBuilder.CollectSources(navmeshbounds, surface.layerMask, surface.useGeometry, surface.defaultArea, markups, sources);
        sources.RemoveAll(source => source.component != null && source.component.gameObject.GetComponent<NavMeshAgent>() != null);
        NavMeshBuilder.UpdateNavMeshData(navmeshdata, surface.GetBuildSettings(), sources, navmeshbounds);
        
        // Show some statistics if we're in debug mode
        #if UNITY_EDITOR
            Debug.Log("Level generation data:");
            Debug.Log("* Time taken -> "+(System.DateTime.Now-time).TotalMilliseconds+"ms");
            Debug.Log("* Attempts -> "+attempts);
            Debug.Log("* Rooms Culled -> "+roomsculled);
        #endif
        
        // Group the objects to make the scene graph easier to traverse
        #if UNITY_EDITOR
            foreach (RoomDef roomdef in this.m_Rooms)
            {
                foreach (GameObject obj in roomdef.objects)
                    obj.transform.SetParent(roomdef.parentobject.transform);
                foreach (GameObject obj in roomdef.doors)
                    obj.transform.SetParent(roomdef.parentobject.transform);
                roomdef.parentobject.name = "Room";
            }
            foreach (CorridorDef cordef in this.m_Corridors)
            {
                foreach (GameObject obj in cordef.objects)
                    obj.transform.SetParent(cordef.parentobject.transform);
                cordef.parentobject.name = "Corridor";
            }
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
        Vector3Int coord;
        Vector3Int size;
        Vector3 doorpos;
        GameObject instobj;
        
        // Start by placing our spawn somewhere outside the grid
        coord = new Vector3Int((int)Random.Range(ProcGenner.MaxRoomSize_X, ProcGenner.MapSize_X-ProcGenner.MaxRoomSize_X), ProcGenner.MapSize_Y/2, -1);
        instobj = Instantiate(this.m_Airlock, (coord-Center)*ProcGenner.GridScale, this.m_Airlock.transform.rotation);
        this.m_Entities.Add(instobj);
        doorpos = coord + (new Vector3(0, 0, 0.25f)*ProcGenner.GridScale/2);
        this.m_Doors.Add((doorpos, instobj));
        
        // Create the player on the spawn
        instobj = Instantiate(this.m_PlayerPrefab, (coord-Center)*ProcGenner.GridScale, Quaternion.identity);
        this.m_Camera.GetComponent<CameraController>().SetTarget(instobj.transform.Find("CameraTarget").gameObject);
        instobj.GetComponent<PlayerController>().SetCamera(this.m_Camera);
        this.m_Entities.Add(instobj);
        this.m_Optimizer.SetPlayer(instobj);
        
        // Now that we have our spawn generated, place a room at our spawn if we're not playing the first level, otherwise make a corridor
        if (ltype != LevelType.First)
        {
            size = GenerateRoomVector();
            coord += new Vector3Int((int)(-size.x/2), 0, 1);
            PlaceRoom(coord, size);
            roomcount--;
        }
        else
        {
            coord += new Vector3Int(0, 0, 4);
            for (int i=0; i<4; i++)
            {
                Vector3Int finalpos = coord - new Vector3Int(0, 0, i);
                CorridorDef cdef = new CorridorDef(){position = finalpos, direction = new Vector3Int(0, 0, 1), objects = PlaceCorridor(finalpos), parentobject = new GameObject()};
                this.m_Corridors.Add(cdef);
                this.m_Grid[finalpos.x, finalpos.y, finalpos.z].type = BlockType.Corridor;
                this.m_Grid[finalpos.x, finalpos.y, finalpos.z].corridordef = cdef;
            }
            this.m_Vertices.Add(new Graphs.Vertex(coord));
        }
        
        // Then place the exit on the other end
        coord = new Vector3Int((int)Random.Range(ProcGenner.MaxRoomSize_X, ProcGenner.MapSize_X-ProcGenner.MaxRoomSize_X), ProcGenner.MapSize_Y/2, ProcGenner.MapSize_Z);
        instobj = Instantiate(this.m_ExitElevator, (coord-Center)*ProcGenner.GridScale, this.m_ExitElevator.transform.rotation);
        this.m_Entities.Add(instobj);
        doorpos = coord + (new Vector3(0, 0, -0.25f)*ProcGenner.GridScale/2);
        this.m_Doors.Add((doorpos, instobj));
        
        // Now place a room just before the end
        size = GenerateRoomVector();
        coord += new Vector3Int((int)(-size.x/2), 0, -size.z);
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
            coord = new Vector3Int(
                (int)Random.Range(0, ProcGenner.MapSize_X-1-ProcGenner.MaxRoomSize_X), 
                (int)yval, 
                (int)Random.Range(0, ProcGenner.MapSize_X-1-ProcGenner.MaxRoomSize_Z)
            );
        
            // Check that the generated coordinate is not overlapping with any room or corridor
            for (int i=0; i<size.z && !overlap; i++)
            {
                for (int j=0; j<size.y && !overlap; j++)
                {
                    for (int k=0; k<size.x; k++)
                    {
                        int gridx = (int)(coord.x + k);
                        int gridy = (int)(coord.y + j);
                        int gridz = (int)(coord.z + i);
                        if ((this.m_Grid[gridx, gridy, gridz].type != BlockType.None) ||
                            (this.m_Grid[Mathf.Min(gridx+1, ProcGenner.MapSize_X-1), gridy, gridz].type != BlockType.None) ||
                            (this.m_Grid[Mathf.Max(gridx-1, 0), gridy, gridz].type != BlockType.None) ||
                            (this.m_Grid[gridx, Mathf.Min(gridy+1, ProcGenner.MapSize_Y-1), gridz].type != BlockType.None) ||
                            (this.m_Grid[gridx, Mathf.Max(gridy-1, 0), gridz].type != BlockType.None) ||
                            (this.m_Grid[gridx, gridy, Mathf.Min(gridz+1, ProcGenner.MapSize_Z-1)].type != BlockType.None) ||
                            (this.m_Grid[gridx, gridy, Mathf.Max(gridz-1, 0)].type != BlockType.None)
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
    
    Vector3Int GenerateRoomVector()
    {
        return new Vector3Int(
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
    
    void PlaceRoom(Vector3Int pos, Vector3Int size)
    {
        GameObject instobj;
        Graphs.Vertex vert;
        List<GameObject> rm = new List<GameObject>();
        List<Vector3Int> positions = new List<Vector3Int>();
        bool createDust = true;//Random.Range(0,10) < 9;
        for (int i=0; i<size.z; i++)
        {
            for (int j=0; j<size.x; j++)
            {
                // Floor
                Vector3Int finalpos = pos + new Vector3Int(j, 0, i);

                instobj = Instantiate(this.m_FloorPrefab, (finalpos-Center)*ProcGenner.GridScale, this.m_FloorPrefab.transform.rotation);
                
                instobj.GetComponent<Renderer>().material = this.m_MaterialRoom;
                rm.Add(instobj);
                
                // Store a list of the positions we added objects to
                for (int k=0; k<size.y; k++)
                    positions.Add(finalpos + new Vector3Int(0, k, 0));
                
                // Ceiling
                finalpos += new Vector3Int(0, size.y, 0);
                instobj = Instantiate(this.m_CeilingPrefab, (finalpos-Center)*ProcGenner.GridScale, this.m_CeilingPrefab.transform.rotation);
                instobj.GetComponent<Renderer>().material = this.m_MaterialRoom;
                rm.Add(instobj);
            }
        }
        if(createDust) {
            Vector3 finalPos = pos + new Vector3(size.x/2,size.y/2,size.z/2);
            instobj=Instantiate(this.m_FloorDustPrefab, (finalPos-Center)*ProcGenner.GridScale, this.m_FloorPrefab.transform.rotation);
            instobj.transform.localScale = new Vector3Int(size.x,0,size.z);

            //int maxScale = Mathf.Max(size.x, size.z);
            //instobj.transform.localScale = new Vector3Int(maxScale, maxScale, maxScale);
            rm.Add(instobj);
        }
        
        // Store the room definition in the helper structures
        Vector3 mid = pos + new Vector3(size.x*0.5f, 0.0f, size.z*0.5f);
        RoomDef rdef = new RoomDef(){position = pos, size = size, objects = rm, visible = true, doors = new List<GameObject>(), parentobject = new GameObject()};
        rdef.midpoint = -(new Vector3(1, 1, 1)*ProcGenner.GridScale)/2 + (mid-Center)*ProcGenner.GridScale;
        vert = new Graphs.Vertex(mid);
        this.m_Rooms.Add(rdef);
        this.m_Vertices.Add(vert);
        this.m_RoomVerts.Add(vert, rm);
        
        // Mark the entire block as a room in our grid
        foreach (Vector3Int gridpos in positions)
        {
            this.m_Grid[gridpos.x, gridpos.y, gridpos.z].type = BlockType.Room;
            this.m_Grid[gridpos.x, gridpos.y, gridpos.z].roomdef = rdef;
        }
    }
      
    
    /*==============================
        PlaceCorridor
        Places a corridor on the map
        @param The coordinate to place the corridor in
        @return A list of created objects
    ==============================*/  
    
    List<GameObject> PlaceCorridor(Vector3Int pos)
    {
        List<GameObject> corridor = new List<GameObject>();
        GameObject instobj;
        
        // Floor
        instobj = Instantiate(this.m_FloorPrefab, (pos-Center)*ProcGenner.GridScale, this.m_FloorPrefab.transform.rotation);
        instobj.GetComponent<Renderer>().material = this.m_MaterialCorridor;
        corridor.Add(instobj);
        
        // Ceiling
        pos += new Vector3Int(0, 1, 0);
        instobj = Instantiate(this.m_CeilingPrefab, (pos-Center)*ProcGenner.GridScale, this.m_CeilingPrefab.transform.rotation);
        instobj.GetComponent<Renderer>().material = this.m_MaterialCorridor;
        corridor.Add(instobj);
        
        // Return our generated objects
        return corridor;
    }
      
    
    /*==============================
        DoorExists
        Checks if a door exists in a given coordinate
        @param The position on the grid to check
        @param The direction to check
        @return The door GameObject that was found
    ==============================*/  
    
    GameObject DoorExists(Vector3Int pos, Vector3Int dir)
    {
        Vector3 checkdir = new Vector3(pos.x + ((float)dir.x)/2, pos.y, pos.z + ((float)dir.z)/2);
        
        // Check if a door exists on the given coordinate
        foreach ((Vector3, GameObject) pair in this.m_Doors)
            if (pair.Item1 == checkdir)
                return pair.Item2;
            
        return null;
    }
    
    
    /*==============================
        CanPlaceCorridorWall
        Checks if a wall can be placed in a corridor's given coordinate
        @param The position on the grid to place the wall in
        @param The direction to check
        @return Whether the wall can be placed
    ==============================*/  
    
    bool CanPlaceCorridorWall(Vector3Int pos, Vector3Int dir)
    {
        Vector3Int check = new Vector3Int(pos.x+dir.x, pos.y+dir.y, pos.z+dir.z);
        
        // Check if a door exists on the given coordinate
        if (DoorExists(pos, dir) != null)
            return false;
        
        // Check if we're out of bounds
        if (check.x < 0 || check.x >= ProcGenner.MapSize_X || check.y < 0 || check.y >= ProcGenner.MapSize_Y || check.z < 0 || check.z >= ProcGenner.MapSize_Z)
            return true;
        
        // Handle stairs edge case
        BlockType gridtype = this.m_Grid[check.x, check.y, check.z].type;
        if (gridtype == BlockType.Stairs)
        {
            CorridorDef stairdef = this.m_Grid[check.x, check.y, check.z].corridordef;
            
            // Prevent placing walls at the start or end of a staircase
            if (Mathf.Abs(dir.x) == Mathf.Abs(stairdef.direction.x) && Mathf.Abs(dir.z) == Mathf.Abs(stairdef.direction.z))
                if (check == stairdef.position || check == stairdef.position+stairdef.direction+new Vector3Int(0, 1, 0))
                    return false;
        }
        
        // Return if this is a valid place to put a wall
        return (gridtype != BlockType.Corridor);
    }
    
    
    /*==============================
        PlaceStairs
        Places a staircase on the map
        @param The coordinate to place the stair in
        @param The angle of the stairs
        @return A list of created objects
    ==============================*/  
    
    List<GameObject> PlaceStairs(Vector3Int pos, Quaternion angle)
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
                for (int i=0; i<this.m_Rooms.Count; i++)
                {
                    if (this.m_Rooms[i].objects == entry.Value)
                    {
                        this.m_Rooms.RemoveAt(i);
                        break;
                    }
                }
                foreach (GameObject obj in entry.Value)
                    Destroy(obj);
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
        List<List<Vector3Int>> foundpaths = new List<List<Vector3Int>>();
        
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
                    if (this.m_Grid[b.Position.x, b.Position.y, b.Position.z].type == BlockType.Stairs)
                        return pathcost;
                    else if (this.m_Grid[b.Position.x, b.Position.y, b.Position.z].type == BlockType.Room)
                        pathcost.cost += 5;
                    else if (this.m_Grid[b.Position.x, b.Position.y, b.Position.z].type == BlockType.None)
                        pathcost.cost += 1;
                    
                    // We're currently in a valid spot
                    pathcost.traversable = true;
                }
                else
                {
                    Vector3Int apos = a.Position;
                    Vector3Int bpos = b.Position;
                    
                    // Stop if we are not connected by 2 corridor blocks
                    if ((this.m_Grid[apos.x, apos.y, apos.z].type != BlockType.None && this.m_Grid[apos.x, apos.y, apos.z].type != BlockType.Corridor)
                        || (this.m_Grid[bpos.x, bpos.y, bpos.z].type != BlockType.None && this.m_Grid[bpos.x, bpos.y, bpos.z].type != BlockType.Corridor)
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
                    if (this.m_Grid[offset1.x, offset1.y, offset1.z].type != BlockType.None
                        || this.m_Grid[offset2.x, offset2.y, offset2.z].type != BlockType.None
                        || this.m_Grid[offset3.x, offset3.y, offset3.z].type != BlockType.None
                        || this.m_Grid[offset4.x, offset4.y, offset4.z].type != BlockType.None) {
                        return pathcost;
                    }

                    // We're currently in a valid spot
                    pathcost.traversable = true;
                    pathcost.isStairs = true;
                }

                return pathcost;
            });
            
            // If a valid path was found, mark the grid with their types
            if (path != null)
            {
                foundpaths.Add(path);
                
                // Travel the path to place corridors/stairs
                for (int i = 0; i < path.Count; i++)
                {
                    Vector3Int current = path[i];

                    if (this.m_Grid[current.x, current.y, current.z].type == BlockType.None)
                        this.m_Grid[current.x, current.y, current.z].type = BlockType.Corridor;

                    if (i > 0)
                    {
                        Vector3Int prev = path[i - 1];
                        Vector3Int delta = current - prev;
                        BlockType prevblock = this.m_Grid[prev.x, prev.y, prev.z].type;

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
                            
                            // Mark the grid areas as stairs
                            this.m_Grid[offset1.x, offset1.y, offset1.z].type = BlockType.Stairs;
                            this.m_Grid[offset2.x, offset2.y, offset2.z].type = BlockType.Stairs;
                            this.m_Grid[offset3.x, offset3.y, offset3.z].type = BlockType.Stairs;
                            this.m_Grid[offset4.x, offset4.y, offset4.z].type = BlockType.Stairs;
                        }
                        
                        // If we were in a room before, and we're in a corridor now, then place a door between the two points
                        if ((prevblock == BlockType.Corridor && this.m_Grid[current.x, current.y, current.z].type == BlockType.Room) || (prevblock == BlockType.Room && this.m_Grid[current.x, current.y, current.z].type == BlockType.Corridor))
                        {
                            Vector3 doordir = (new Vector3(delta.x, 0.0f, delta.z));
                            if (DoorExists(prev, Vector3Int.FloorToInt(doordir)) == null)
                            {
                                float angle = delta.z != 0 ? 90.0f : 0.0f;
                                Vector3 doorpos = doordir/2 + prev;
                                GameObject instobj = Instantiate(this.m_DoorPrefab, (doorpos - Center)*ProcGenner.GridScale, this.m_DoorPrefab.transform.rotation*Quaternion.Euler(0, angle, 0));
                                this.m_Doors.Add((doorpos, instobj));
                            }
                        }
                    }
                }
            }
        }
    
        // Now iterate through the paths again and actually create the objects
        foreach (List<Vector3Int> path in foundpaths)
        {                
            // Travel the path to place corridors/stairs
            for (int i = 0; i < path.Count; i++)
            {
                Vector3Int pos = path[i];

                if (i > 0)
                {
                    Vector3Int prev = path[i - 1];
                    Vector3Int delta = pos - prev;
                    BlockType prevblock = this.m_Grid[prev.x, prev.y, prev.z].type;

                    // If the delta y is non-zero, then we have stairs to place
                    if (delta.y != 0)
                    {
                        float ang = 0;
                        Vector3Int placepos = Vector3Int.zero;
                        int xDir = Mathf.Clamp(delta.x, -1, 1);
                        int zDir = Mathf.Clamp(delta.z, -1, 1);
                        Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                        Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);
                        
                        // Rotate the stairs based on the delta z
                        if ((zDir > 0 && delta.y > 0) || (zDir < 0 && delta.y < 0))
                        {
                            ang = 0.0f;
                            if (delta.y > 0)
                                placepos = prev + horizontalOffset;
                            else
                                placepos = prev + verticalOffset + horizontalOffset*2;
                        }
                        else if ((zDir > 0 && delta.y < 0) || (zDir < 0 && delta.y > 0))
                        {
                            ang = 180.0f;
                            if (delta.y < 0)
                                placepos = prev + verticalOffset + horizontalOffset*2;
                            else
                                placepos = prev + horizontalOffset;
                        }
                        
                        // Rotate the stairs based on the delta x
                        if ((xDir > 0 && delta.y > 0) || (xDir < 0 && delta.y < 0))
                        {
                            ang = 90.0f;
                            if (delta.y > 0)
                                placepos = prev + horizontalOffset;
                            else
                                placepos = prev + verticalOffset + horizontalOffset*2;
                        }
                        else if ((xDir > 0 && delta.y < 0) || (xDir < 0 && delta.y > 0))
                        {
                            ang = -90.0f;
                            if (delta.y < 0)
                                placepos = prev + verticalOffset + horizontalOffset*2;
                            else
                                placepos = prev + horizontalOffset;
                        }
                        
                        // Correct the direction if necessary
                        Vector3Int placedir = horizontalOffset;
                        Vector3Int dircheck = placepos+placedir;
                        if (this.m_Grid[dircheck.x, dircheck.y, dircheck.z].type != BlockType.Stairs)
                            placedir = -placedir;
                        
                        // Place the stair object
                        CorridorDef cdef = new CorridorDef(){position = placepos, direction = placedir, objects = PlaceStairs(placepos, Quaternion.Euler(0, 0, ang)), parentobject = new GameObject()};
                        this.m_Corridors.Add(cdef);
                            
                        // Calculate the offsets to help us out
                        Vector3Int offset1 = prev + horizontalOffset;
                        Vector3Int offset2 = prev + horizontalOffset*2;
                        Vector3Int offset3 = prev + verticalOffset + horizontalOffset;
                        Vector3Int offset4 = prev + verticalOffset + horizontalOffset*2;
                        
                        // Mark the grid areas as stairs
                        this.m_Grid[offset1.x, offset1.y, offset1.z].corridordef = cdef;
                        this.m_Grid[offset2.x, offset2.y, offset2.z].corridordef = cdef;
                        this.m_Grid[offset3.x, offset3.y, offset3.z].corridordef = cdef;
                        this.m_Grid[offset4.x, offset4.y, offset4.z].corridordef = cdef;
                    }
                    
                    // Place corridors in our path
                    if (this.m_Grid[pos.x, pos.y, pos.z].type == BlockType.Corridor)
                    {
                        CorridorDef cdef = new CorridorDef(){position = pos, direction = delta, objects = PlaceCorridor(pos), parentobject = new GameObject()};
                        this.m_Corridors.Add(cdef);
                        this.m_Grid[pos.x, pos.y, pos.z].corridordef = cdef;
                    }
                }
            }
        }
    }


    /*==============================
        GenerateWalls
        Generates the walls
    ==============================*/

    void GenerateWalls()
    {
        GameObject instobj;
            
        // Rooms
        foreach (RoomDef rdef in this.m_Rooms)
        {
            List<GameObject> rm = new List<GameObject>();
            
            // Front walls
            Vector3Int dir = new Vector3Int(0, 0, -1);
            for (int i=0; i<rdef.size.x; i++)
            {
                Vector3Int finalpos = rdef.position + new Vector3Int(i, 0, 0);
                GameObject founddoor = DoorExists(finalpos, dir);
                GameObject wallprefab = (rdef.size.y == 3) ? this.m_Wall3Prefab : this.m_Wall2Prefab;
                if (founddoor != null)
                {
                    wallprefab = (rdef.size.y == 3) ? this.m_DoorWall2Prefab : this.m_DoorWall1Prefab;
                    rdef.doors.Add(founddoor);
                }
                instobj = Instantiate(wallprefab, (finalpos-Center)*ProcGenner.GridScale + (((Vector3)dir)*ProcGenner.GridScale/2), wallprefab.transform.rotation*Quaternion.Euler(0, 0, 180));
                instobj.GetComponent<Renderer>().material = this.m_MaterialRoom;
                rm.Add(instobj);
            }
            
            // Back walls
            dir = new Vector3Int(0, 0, 1);
            for (int i=0; i<rdef.size.x; i++)
            {
                Vector3Int finalpos = rdef.position + new Vector3Int(i, 0, rdef.size.z-1);
                GameObject founddoor = DoorExists(finalpos, dir);
                GameObject wallprefab = (rdef.size.y == 3) ? this.m_Wall3Prefab : this.m_Wall2Prefab;
                if (founddoor != null)
                {
                    wallprefab = (rdef.size.y == 3) ? this.m_DoorWall2Prefab : this.m_DoorWall1Prefab;
                    rdef.doors.Add(founddoor);
                }
                instobj = Instantiate(wallprefab, (finalpos-Center)*ProcGenner.GridScale + (((Vector3)dir)*ProcGenner.GridScale/2), wallprefab.transform.rotation);
                instobj.GetComponent<Renderer>().material = this.m_MaterialRoom;
                rm.Add(instobj);
            }
            
            // Left walls
            dir = new Vector3Int(-1, 0, 0);
            for (int i=0; i<rdef.size.z; i++)
            {
                Vector3Int finalpos = rdef.position + new Vector3Int(0, 0, i);
                GameObject founddoor = DoorExists(finalpos, dir);
                GameObject wallprefab = (rdef.size.y == 3) ? this.m_Wall3Prefab : this.m_Wall2Prefab;
                if (founddoor != null)
                {
                    wallprefab = (rdef.size.y == 3) ? this.m_DoorWall2Prefab : this.m_DoorWall1Prefab;
                    rdef.doors.Add(founddoor);
                }
                instobj = Instantiate(wallprefab, (finalpos-Center)*ProcGenner.GridScale + (((Vector3)dir)*ProcGenner.GridScale/2), wallprefab.transform.rotation*Quaternion.Euler(0, 0, -90));
                instobj.GetComponent<Renderer>().material = this.m_MaterialRoom;
                rm.Add(instobj);
            }
            
            // Right walls
            dir = new Vector3Int(1, 0, 0);
            for (int i=0; i<rdef.size.z; i++)
            {
                Vector3Int finalpos = rdef.position + new Vector3Int(rdef.size.x-1, 0, i);
                GameObject founddoor = DoorExists(finalpos, dir);
                GameObject wallprefab = (rdef.size.y == 3) ? this.m_Wall3Prefab : this.m_Wall2Prefab;
                if (founddoor != null)
                {
                    wallprefab = (rdef.size.y == 3) ? this.m_DoorWall2Prefab : this.m_DoorWall1Prefab;
                    rdef.doors.Add(founddoor);
                }
                instobj = Instantiate(wallprefab, (finalpos-Center)*ProcGenner.GridScale + (((Vector3)dir)*ProcGenner.GridScale/2), wallprefab.transform.rotation*Quaternion.Euler(0, 0, 90));
                instobj.GetComponent<Renderer>().material = this.m_MaterialRoom;
                rm.Add(instobj);
            }
            
            // Add to the list of walls
            rdef.objects.AddRange(rm);
        }
        
        // Corridors
        foreach (CorridorDef cdef in this.m_Corridors)
        {
            if (this.m_Grid[cdef.position.x, cdef.position.y, cdef.position.z].type == BlockType.Stairs)
                continue;
            
            // Walls
            if (CanPlaceCorridorWall(cdef.position, new Vector3Int(1, 0, 0)))
            {
                instobj = Instantiate(this.m_Wall1Prefab, (cdef.position-Center)*ProcGenner.GridScale + (new Vector3(1, 0, 0)*ProcGenner.GridScale/2), this.m_Wall1Prefab.transform.rotation*Quaternion.Euler(0, 0, 90));
                instobj.GetComponent<Renderer>().material = this.m_MaterialCorridor;
                cdef.objects.Add(instobj);
            }
            if (CanPlaceCorridorWall(cdef.position, new Vector3Int(-1, 0, 0)))
            {
                instobj = Instantiate(this.m_Wall1Prefab, (cdef.position-Center)*ProcGenner.GridScale + (new Vector3(-1, 0, 0)*ProcGenner.GridScale/2), this.m_Wall1Prefab.transform.rotation*Quaternion.Euler(0, 0, -90));
                instobj.GetComponent<Renderer>().material = this.m_MaterialCorridor;
                cdef.objects.Add(instobj);
            }
            if (CanPlaceCorridorWall(cdef.position, new Vector3Int(0, 0, 1)))
            {
                instobj = Instantiate(this.m_Wall1Prefab, (cdef.position-Center)*ProcGenner.GridScale + (new Vector3(0, 0, 1)*ProcGenner.GridScale/2), this.m_Wall1Prefab.transform.rotation);
                instobj.GetComponent<Renderer>().material = this.m_MaterialCorridor;
                cdef.objects.Add(instobj);
            }
            if (CanPlaceCorridorWall(cdef.position, new Vector3Int(0, 0, -1)))
            {
                instobj = Instantiate(this.m_Wall1Prefab, (cdef.position-Center)*ProcGenner.GridScale + (new Vector3(0, 0, -1)*ProcGenner.GridScale/2), this.m_Wall1Prefab.transform.rotation*Quaternion.Euler(0, 0, 180));
                instobj.GetComponent<Renderer>().material = this.m_MaterialCorridor;
                cdef.objects.Add(instobj);
            }
        }
    }
    
    public List<ProcGenner.RoomDef> GetRoomDefs()
    {
        return this.m_Rooms;
    }
    
    public void SetRoomVisible(int room, bool visible)
    {
        RoomDef rdef = this.m_Rooms[room];
        rdef.visible = visible;
        this.m_Rooms[room] = rdef;
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