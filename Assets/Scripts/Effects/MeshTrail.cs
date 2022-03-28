using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    private const int VertCount = 12;
    private const int TrailLength = 10;
     
    public GameObject m_Tip = null;
    public GameObject m_Base = null;
    public Material m_MeshMat = null;

    private bool m_TrailEnabled = false;
    private Mesh m_Mesh;
    private Vector3[] m_VertsList;
    private int[] m_TrisList;
    private int m_TrailCount;
    private Vector3 m_OldTipPos;
    private Vector3 m_OldBasePos;
    
    private SkinnedMeshRenderer m_meshrender;

    void Start()
    {
        GameObject meshobj = new GameObject();
        MeshFilter meshfilter = meshobj.AddComponent<MeshFilter>();
        this.m_meshrender = meshobj.AddComponent<SkinnedMeshRenderer>();
        
        // Initialize the mesh that we're going to dynamically generate
        this.m_Mesh = new Mesh();
        meshobj.name = "Trail";
        meshfilter.mesh = this.m_Mesh;
        this.m_meshrender.sharedMesh = this.m_Mesh;
        this.m_meshrender.material = this.m_MeshMat;
        this.m_meshrender.localBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
        
        // Initialize the arrays where we'll keep our mesh data
        this.m_VertsList = new Vector3[MeshTrail.TrailLength*MeshTrail.VertCount];
        this.m_TrisList = new int[this.m_VertsList.Length];

        // Initialize the positions
        this.m_OldTipPos = this.m_Tip.transform.position;
        this.m_OldBasePos = this.m_Base.transform.position;
    }
    
    void LateUpdate()
    {
        if (!this.m_TrailEnabled) 
            return;
        
        // Reset the trail count when we've gone over the limit
        this.m_TrailCount %= (MeshTrail.TrailLength*MeshTrail.VertCount);

        // Create the first triangle's verts
        this.m_VertsList[this.m_TrailCount + 0] = this.m_Base.transform.position;
        this.m_VertsList[this.m_TrailCount + 1] = this.m_Tip.transform.position;
        this.m_VertsList[this.m_TrailCount + 2] = this.m_OldTipPos;
        this.m_VertsList[this.m_TrailCount + 3] = this.m_Base.transform.position;
        this.m_VertsList[this.m_TrailCount + 4] = this.m_OldTipPos;
        this.m_VertsList[this.m_TrailCount + 5] = this.m_Tip.transform.position;

        // Create the second triangle's verts (for the quad)
        this.m_VertsList[this.m_TrailCount + 6] = this.m_OldTipPos;
        this.m_VertsList[this.m_TrailCount + 7] = this.m_Base.transform.position;
        this.m_VertsList[this.m_TrailCount + 8] = this.m_OldBasePos;
        this.m_VertsList[this.m_TrailCount + 9] = this.m_OldTipPos;
        this.m_VertsList[this.m_TrailCount + 10] = this.m_OldBasePos;
        this.m_VertsList[this.m_TrailCount + 11] = this.m_Base.transform.position;

        // Create the triangles from the vert list
        for (int i=0; i<MeshTrail.VertCount; i++)
            this.m_TrisList[this.m_TrailCount+i] = this.m_TrailCount+i;

        // Set the vert and triangle lists
        this.m_Mesh.vertices = this.m_VertsList;
        this.m_Mesh.triangles = this.m_TrisList;

        // Keep track of the old tip and base positions
        this.m_OldTipPos = this.m_Tip.transform.position;
        this.m_OldBasePos = this.m_Base.transform.position;
        this.m_TrailCount += MeshTrail.VertCount;
        
        // Render the mesh if it isn't enabled
        if (!this.m_meshrender.enabled)
            this.m_meshrender.enabled = true;
    }
    
    public bool IsEnabled()
    {
        return this.m_TrailEnabled;
    }
    
    public void EnableTrail(bool enable)
    {
        if (enable && !this.m_TrailEnabled)
        {
            for (int i=0; i<MeshTrail.TrailLength*MeshTrail.VertCount; i++)
                this.m_VertsList[i] = this.m_Base.transform.position;
            this.m_OldTipPos = this.m_Tip.transform.position;
            this.m_OldBasePos = this.m_Base.transform.position;
        }
        else if (!enable)
            this.m_meshrender.enabled = false;
        this.m_TrailEnabled = enable;
    }
}