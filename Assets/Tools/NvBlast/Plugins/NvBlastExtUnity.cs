using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

public class NvLogger
{
    public static void Log(String msg)
    {
        //Debug.Log(msg);//Comment out to remove debug messages
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct NoiseConfiguration
{
    public float amplitude;//0 - disabled
    public float frequency;//:1
    public int octaveNumber;//:1
    public int surfaceResolution;//:1
};

[StructLayout(LayoutKind.Sequential)]
public struct SlicingConfiguration
{
    public Vector3Int slices;
    public float offset_variations;//0-1:0
    public float angle_variations;//0-1:0
    public NoiseConfiguration noise;
};

public class NvMesh : DisposablePtr
{
    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _Mesh_Release(IntPtr mesh);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _Mesh_getVertices(IntPtr mesh, [In,Out] Vector3[] arr);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _Mesh_getNormals(IntPtr mesh, [In,Out] Vector3[] arr);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _Mesh_getIndexes(IntPtr mesh, [In,Out] int[] arr);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _Mesh_getUVs(IntPtr mesh, [In,Out] Vector2[] arr);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern int _Mesh_getVerticesCount(IntPtr mesh);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern int _Mesh_getIndexesCount(IntPtr mesh);

    [DllImport("NvBlastExtAuthoring_x64")]
    private static extern IntPtr NvBlastExtAuthoringCreateMesh(Vector3[] positions, Vector3[] normals, Vector2[] uv, Int32 verticesCount, Int32[] indices, Int32 indicesCount);

    public NvMesh(IntPtr mesh)
    {
        Initialize(mesh);
        NvLogger.Log("NvMesh:" + this.ptr.ToString());
    }

    public NvMesh(Vector3[] positions, Vector3[] normals, Vector2[] uv, Int32 verticesCount, Int32[] indices, Int32 indicesCount)
    {
        Initialize(NvBlastExtAuthoringCreateMesh(positions, normals, uv, verticesCount, indices, indicesCount));
        NvLogger.Log("NvMesh:" + this.ptr.ToString());
    }

    public Vector3[] getVertices()
    {
        Vector3[] v = new Vector3[getVerticesCount()];
        _Mesh_getVertices(this.ptr, v);
        return v;
    }
    public Vector3[] getNormals()
    {
        Vector3[] v = new Vector3[getVerticesCount()];
        _Mesh_getNormals(this.ptr, v);
        return v;
    }
    public Vector2[] getUVs()
    {
        Vector2[] v = new Vector2[getVerticesCount()];
        _Mesh_getUVs(this.ptr, v);
        return v;
    }
    public int[] getIndexes()
    {
        int[] v = new int[getIndexesCount()];
        _Mesh_getIndexes(this.ptr, v);
        return v;
    }

    public int getVerticesCount()
    {
        return _Mesh_getVerticesCount(this.ptr);
    }

    public int getIndexesCount()
    {
        return _Mesh_getIndexesCount(this.ptr);
    }

    protected override void Release()
    {
        NvLogger.Log("_Mesh_Release");
        _Mesh_Release(this.ptr);
    }

    //Unity Helper Functions
    public Mesh toUnityMesh()
    {
        Mesh m = new Mesh();
        m.vertices = getVertices();
        m.normals = getNormals();
        m.uv = getUVs();
        m.SetIndices(getIndexes(), MeshTopology.Triangles, 0, true);
        return m;
    }
}

public class NvMeshCleaner : DisposablePtr
{
    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _Cleaner_Release(IntPtr cleaner);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern IntPtr _Cleaner_cleanMesh(IntPtr cleaner, IntPtr mesh);

    [DllImport("NvBlastExtAuthoring_x64")]
    private static extern IntPtr NvBlastExtAuthoringCreateMeshCleaner();

    public NvMeshCleaner()
    {
        Initialize(NvBlastExtAuthoringCreateMeshCleaner());
        NvLogger.Log("NvMeshCleaner:" + this.ptr.ToString());
    }

    public NvMesh cleanMesh(NvMesh mesh)
    {
        return new NvMesh(_Cleaner_cleanMesh(this.ptr, mesh.ptr));
    }

    protected override void Release()
    {
        NvLogger.Log("_Cleaner_Release");
        _Cleaner_Release(this.ptr);
    }
}

public class NvFractureTool : DisposablePtr
{
    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _FractureTool_Release(IntPtr tool);

    [DllImport("NvBlastExtAuthoring_x64")]
    private static extern IntPtr NvBlastExtAuthoringCreateFractureTool();

    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _FractureTool_setSourceMesh(IntPtr tool, IntPtr mesh);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _FractureTool_setRemoveIslands(IntPtr tool, bool remove);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern bool _FractureTool_voronoiFracturing(IntPtr tool, int chunkId, IntPtr vsg);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern bool _FractureTool_slicing(IntPtr tool, int chunkId, [Out] SlicingConfiguration conf, bool replaceChunk);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _FractureTool_finalizeFracturing(IntPtr tool);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern int _FractureTool_getChunkCount(IntPtr tool);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern IntPtr _FractureTool_getChunkMesh(IntPtr tool, int chunkId, bool inside);

    public NvFractureTool()
    {
        Initialize(NvBlastExtAuthoringCreateFractureTool());
        NvLogger.Log("NvFractureTool:" + this.ptr.ToString());
    }

    public void setSourceMesh(NvMesh mesh)
    {
        _FractureTool_setSourceMesh(this.ptr, mesh.ptr);
    }
    
    public void setRemoveIslands(bool remove)
    {
        _FractureTool_setRemoveIslands(this.ptr, remove);
    }

    public bool voronoiFracturing(int chunkId, NvVoronoiSitesGenerator vsg)
    {
        return _FractureTool_voronoiFracturing(this.ptr, chunkId, vsg.ptr);
    }

    public bool slicing(int chunkId, SlicingConfiguration conf, bool replaceChunk)
    {
        return _FractureTool_slicing(this.ptr, chunkId, conf, replaceChunk);
    }

    public void finalizeFracturing()
    {
        _FractureTool_finalizeFracturing(this.ptr);
    }

    public int getChunkCount()
    {
        return _FractureTool_getChunkCount(this.ptr);
    }

    public NvMesh getChunkMesh(int chunkId, bool inside)
    {
        return new NvMesh(_FractureTool_getChunkMesh(this.ptr, chunkId, inside));
    }

    protected override void Release()
    {
        NvLogger.Log("_FractureTool_Release");
        _FractureTool_Release(this.ptr);
    }
}

public class NvVoronoiSitesGenerator : DisposablePtr
{
    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _VoronoiSitesGenerator_Release(IntPtr site);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern IntPtr _VoronoiSitesGenerator_Create(IntPtr mesh);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern IntPtr _NvVoronoiSitesGenerator_uniformlyGenerateSitesInMesh(IntPtr tool,int count);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern IntPtr _NvVoronoiSitesGenerator_addSite(IntPtr tool, [In] Vector3 site);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern bool _NvVoronoiSitesGenerator_clusteredSitesGeneration(IntPtr tool, int numberOfClusters, int sitesPerCluster, float clusterRadius);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern int _NvVoronoiSitesGenerator_getSitesCount(IntPtr tool);

    [DllImport("NvBlastExtUnity_x64")]
    private static extern void _NvVoronoiSitesGenerator_getSites(IntPtr tool, [In, Out] Vector3[] arr);

    public NvVoronoiSitesGenerator(NvMesh mesh)
    {
        Initialize(_VoronoiSitesGenerator_Create(mesh.ptr));
        NvLogger.Log("NvVoronoiSitesGenerator:" + this.ptr.ToString());
    }

    public void uniformlyGenerateSitesInMesh(int count)
    {
        _NvVoronoiSitesGenerator_uniformlyGenerateSitesInMesh(this.ptr, count);
    }

    public void addSite(Vector3 site)
    {
        _NvVoronoiSitesGenerator_addSite(this.ptr, site);
    }

    public void clusteredSitesGeneration(int numberOfClusters, int sitesPerCluster, float clusterRadius)
    {
        _NvVoronoiSitesGenerator_clusteredSitesGeneration(this.ptr, numberOfClusters, sitesPerCluster, clusterRadius);
    }

    public Vector3[] getSites()
    {
        Vector3[] v = new Vector3[getSitesCount()];
        _NvVoronoiSitesGenerator_getSites(this.ptr, v);
        return v;
    }

    public int getSitesCount()
    {
        return _NvVoronoiSitesGenerator_getSitesCount(this.ptr);
    }

    protected override void Release()
    {
        NvLogger.Log("_VoronoiSitesGenerator_Release");
        _VoronoiSitesGenerator_Release(this.ptr);
    }

    //Unity Specific
    public void boneSiteGeneration(SkinnedMeshRenderer smr)
    {
        if (smr == null)
        {
            Debug.Log("No Skinned Mesh Renderer");
            return;
        }

        Animator anim = smr.transform.root.GetComponent<Animator>();
        if (anim == null)
        {
            Debug.Log("Missing Animator");
            return;
        }

        if (anim.GetBoneTransform(HumanBodyBones.Head)) addSite(anim.GetBoneTransform(HumanBodyBones.Head).position);
        if (anim.GetBoneTransform(HumanBodyBones.Neck)) addSite(anim.GetBoneTransform(HumanBodyBones.Neck).position);

        //if (anim.GetBoneTransform(HumanBodyBones.LeftShoulder)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftShoulder).position);
        //if (anim.GetBoneTransform(HumanBodyBones.RightShoulder)) addSite(anim.GetBoneTransform(HumanBodyBones.RightShoulder).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftUpperArm)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightUpperArm)) addSite(anim.GetBoneTransform(HumanBodyBones.RightUpperArm).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftLowerArm)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightLowerArm)) addSite(anim.GetBoneTransform(HumanBodyBones.RightLowerArm).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftHand)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftHand).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightHand)) addSite(anim.GetBoneTransform(HumanBodyBones.RightHand).position);

        if (anim.GetBoneTransform(HumanBodyBones.Chest)) addSite(anim.GetBoneTransform(HumanBodyBones.Chest).position);
        if (anim.GetBoneTransform(HumanBodyBones.Spine)) addSite(anim.GetBoneTransform(HumanBodyBones.Spine).position);
        if (anim.GetBoneTransform(HumanBodyBones.Hips)) addSite(anim.GetBoneTransform(HumanBodyBones.Hips).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightUpperLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightLowerLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftFoot)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftFoot).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightFoot)) addSite(anim.GetBoneTransform(HumanBodyBones.RightFoot).position);

        //if (anim.GetBoneTransform(HumanBodyBones.LeftEye)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftEye).position);
        //if (anim.GetBoneTransform(HumanBodyBones.RightEye)) addSite(anim.GetBoneTransform(HumanBodyBones.RightEye).position);
    }
}

public class NvBlastExtUnity
{
    [DllImport("NvBlastExtUnity_x64")]
    public static extern void Version();

    [DllImport("NvBlastExtUnity_x64")]
    public static extern void setSeed(int seed);


}
