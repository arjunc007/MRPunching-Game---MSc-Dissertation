﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

/// <summary>
/// The SurfaceManager class allows applications to scan the environment for a specified amount of time 
/// and then process the Spatial Mapping Mesh (find planes, remove vertices) after that time has expired.
/// </summary>
public class PlayBoxingManager : Singleton<PlayBoxingManager> {
    [Tooltip("The boxer prefab to generate in the world.")]
    public GameObject boxerPrefab;

    public float spawnDist = 0.85f;

    public AudioClip bell;

    [Tooltip("When checked, the SurfaceObserver will stop running after a specified amount of time.")]
    public bool limitScanningByTime = true;

    [Tooltip("How much time (in seconds) that the SurfaceObserver will run after being started; used when 'Limit Scanning By Time' is checked.")]
    public float scanTime = 30.0f;

    [Tooltip("Material to use when rendering Spatial Mapping meshes while the observer is running.")]
    public Material defaultMaterial;

    [Tooltip("Optional Material to use when rendering Spatial Mapping meshes after the observer has been stopped.")]
    public Material secondaryMaterial;

    [Tooltip("Minimum number of floor planes required in order to exit scanning/processing mode.")]
    public uint minimumFloors = 1;

    [Tooltip("Minimum number of wall planes required in order to exit scanning/processing mode.")]
    public uint minimumWalls = 1;

    /// <summary>
    /// Indicates if processing of the surface meshes is complete.
    /// </summary>
    private bool meshesProcessed = false;

    // Use this for initialization
    void Start () {
        // Update surfaceObserver and storedMeshes to use the same material during scanning.
        SpatialMappingManager.Instance.SetSurfaceMaterial(defaultMaterial);

        // Register for the MakePlanesComplete event.
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += SurfaceMeshesToPlanes_MakePlanesComplete;
    }
	
	// Update is called once per frame
	void Update () {
        // Check to see if the spatial mapping data has been processed
        // and if we are limiting how much time the user can spend scanning.
        if (!meshesProcessed && limitScanningByTime)
        {
            // If we have not processed the spatial mapping data
            // and scanning time is limited...

            // Check to see if enough scanning time has passed
            // since starting the observer.
            if (limitScanningByTime && ((Time.time - SpatialMappingManager.Instance.StartTime) < scanTime))
            {
                // If we have a limited scanning time, then we should wait until
                // enough time has passed before processing the mesh.
            }
            else
            {
                // The user should be done scanning their environment,
                // so start processing the spatial mapping data...

                //Check if IsObserverRunning() is true on the
                // SpatialMappingManager.Instance.
                if (SpatialMappingManager.Instance.IsObserverRunning())
                {
                    // If running, Stop the observer by calling
                    // StopObserver() on the SpatialMappingManager.Instance.
                    SpatialMappingManager.Instance.StopObserver();
                }

                //Stop drawing meshes
                SpatialMappingManager.Instance.DrawVisualMeshes = false;

                //Call CreatePlanes() to generate planes.
                CreatePlanes();

                //Set meshesProcessed to true.
                meshesProcessed = true;

                AudioSource.PlayClipAtPoint(bell, Camera.main.transform.position);
            }
        }
    }

    /// <summary>
    /// Handler for the SurfaceMeshesToPlanes MakePlanesComplete event.
    /// </summary>
    /// <param name="source">Source of the event.</param>
    /// <param name="args">Args for the event.</param>
    private void SurfaceMeshesToPlanes_MakePlanesComplete(object source, System.EventArgs args)
    {
        // Collection of floor and table planes that we can use to set horizontal items on.
        List<GameObject> horizontal = new List<GameObject>();

        // Collection of wall planes that we can use to set vertical items on.
        List<GameObject> vertical = new List<GameObject>();

        // Get all floor and table planes by calling
        // SurfaceMeshesToPlanes.Instance.GetActivePlanes().
        // Assign the result to the 'horizontal' list.
        horizontal = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Floor);

        // Get all wall planes by calling
        // SurfaceMeshesToPlanes.Instance.GetActivePlanes().
        // Assign the result to the 'vertical' list.
        vertical = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);

        // Check to see if we have enough horizontal planes (minimumFloors)
        // and vertical planes (minimumWalls), to set holograms on in the world.
        if (horizontal.Count >= minimumFloors && vertical.Count >= minimumWalls)
        {
            // We have enough floors and walls to place our holograms on...

            // Reduce our triangle count by removing triangles
            // from SpatialMapping meshes that intersect with our active planes.
            // Call RemoveVertices().
            // Pass in all activePlanes found by SurfaceMeshesToPlanes.Instance.
            RemoveVertices(SurfaceMeshesToPlanes.Instance.ActivePlanes);

            // We can indicate to the user that scanning is over by
            // changing the material applied to the Spatial Mapping meshes.
            // Call SpatialMappingManager.Instance.SetSurfaceMaterial().
            // Pass in the secondaryMaterial.
            if(secondaryMaterial)
                SpatialMappingManager.Instance.SetSurfaceMaterial(secondaryMaterial);

            // We are all done processing the mesh, so we can now
            // initialize a collection of Placeable holograms in the world
            // and use horizontal/vertical planes to set their starting positions.
            // Call SpaceCollectionManager.Instance.GenerateItemsInWorld().
            // Pass in the lists of horizontal and vertical planes that we found earlier.
            PlaceBoxerInWorld(horizontal, vertical);
        }
        else
        {
            // We do not have enough floors/walls to place our holograms on...

            // Re-enter scanning mode so the user can find more surfaces by 
            // calling StartObserver() on the SpatialMappingManager.Instance.
            SpatialMappingManager.Instance.StartObserver();

            // Re-process spatial data after scanning completes by
            // re-setting meshesProcessed to false.
            meshesProcessed = false;
        }
    }

    /// <summary>
    /// Creates planes from the spatial mapping surfaces.
    /// </summary>
    private void CreatePlanes()
    {
        // Generate planes based on the spatial map.
        SurfaceMeshesToPlanes surfaceToPlanes = SurfaceMeshesToPlanes.Instance;
        if (surfaceToPlanes != null && surfaceToPlanes.enabled)
        {
            surfaceToPlanes.MakePlanes();
        }
    }

    /// <summary>
    /// Removes triangles from the spatial mapping surfaces.
    /// </summary>
    /// <param name="boundingObjects"></param>
    private void RemoveVertices(IEnumerable<GameObject> boundingObjects)
    {
        RemoveSurfaceVertices removeVerts = RemoveSurfaceVertices.Instance;
        if (removeVerts != null && removeVerts.enabled)
        {
            removeVerts.RemoveSurfaceVerticesWithinBounds(boundingObjects);
        }
    }

    /// <summary>
    /// Called when the GameObject is unloaded.
    /// </summary>
    private new void OnDestroy()
    {
        if (SurfaceMeshesToPlanes.Instance != null)
        {
            SurfaceMeshesToPlanes.Instance.MakePlanesComplete -= SurfaceMeshesToPlanes_MakePlanesComplete;
        }
        base.OnDestroy();
    }

    /// <summary>
    /// Generates a collection of Placeable objects in the world and sets them on planes that match their affinity.
    /// </summary>
    /// <param name="horizontalSurfaces">Horizontal surface planes (floors, tables).</param>
    /// <param name="verticalSurfaces">Vertical surface planes (walls).</param>
    public void PlaceBoxerInWorld(List<GameObject> horizontalSurfaces, List<GameObject> verticalSurfaces)
    {
        // Sort the planes by distance to user.
        horizontalSurfaces.Sort((lhs, rhs) =>
        {
            Vector3 headPosition = Camera.main.transform.position;
            Collider rightCollider = rhs.GetComponent<Collider>();
            Collider leftCollider = lhs.GetComponent<Collider>();

            // This plane is big enough, now we will evaluate how far the plane is from the user's head.  
            // Since planes can be quite large, we should find the closest point on the plane's bounds to the 
            // user's head, rather than just taking the plane's center position.
            Vector3 rightSpot = rightCollider.ClosestPointOnBounds(headPosition);
            Vector3 leftSpot = leftCollider.ClosestPointOnBounds(headPosition);

            return Vector3.Distance(leftSpot, headPosition).CompareTo(Vector3.Distance(rightSpot, headPosition));
        });

        //Find the nearest plane to place the boxer on
        Collider collider = boxerPrefab.GetComponent<Collider>();

        int index = FindNearestPlane(horizontalSurfaces, collider.bounds.size);

        // If we can't find a good plane we will put the object floating in space.
        Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 2.0f;
        Quaternion rotation = Quaternion.identity;

        // If we do find a good plane we can do something smarter.
        if (index >= 0)
        {
            GameObject surface = horizontalSurfaces[index];
            SurfacePlane plane = surface.GetComponent<SurfacePlane>();
            Vector3 spawnXZ = Camera.main.transform.position + Camera.main.transform.forward * spawnDist;
            position = new Vector3(spawnXZ.x, surface.transform.position.y + plane.PlaneThickness, spawnXZ.z);
            //position = surface.transform.position + (plane.PlaneThickness * plane.SurfaceNormal);
            position = AdjustPositionWithSpatialMap(position, plane.SurfaceNormal);
            //rotation = Camera.main.transform.localRotation;
            
            // Horizontal objects should face the user.
            rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
            rotation.x = 0f;
            rotation.z = 0f;
        }

        //Vector3 finalPosition = AdjustPositionWithSpatialMap(position, surfaceType);
        GameObject boxer = Instantiate(boxerPrefab, position, rotation) as GameObject;
        boxer.transform.parent = gameObject.transform;
    }

    /// <summary>
    /// Attempts to find a the closest plane to the user which is large enough to fit the object.
    /// </summary>
    /// <param name="planes">List of planes to consider for object placement.</param>
    /// <param name="minSize">Minimum size that the plane is required to be.</param>
    /// <param name="startIndex">Index in the planes collection that we want to start at (to help avoid double-placement of objects).</param>
    /// <param name="isVertical">True, if we are currently evaluating vertical surfaces.</param>
    /// <returns></returns>
    private int FindNearestPlane(List<GameObject> planes, Vector3 minSize)
    {
        int planeIndex = -1;

        for (int i = 0; i < planes.Count; i++)
        {
            Collider collider = planes[i].GetComponent<Collider>();
            if (collider.bounds.size.x < minSize.x || collider.bounds.size.y < minSize.y)
            {
                // This plane is too small to fit our horizontal object.
                continue;
            }

            return i;
        }

        return planeIndex;
    }

    /// <summary>
    /// Adjusts the initial position of the object if it is being occluded by the spatial map.
    /// </summary>
    /// <param name="position">Position of object to adjust.</param>
    /// <param name="surfaceNormal">Normal of surface that the object is positioned against.</param>
    /// <returns></returns>
    private Vector3 AdjustPositionWithSpatialMap(Vector3 position, Vector3 surfaceNormal)
    {
        Vector3 newPosition = position;
        RaycastHit hitInfo;
        float distance = 0.5f;

        // Check to see if there is a SpatialMapping mesh occluding the object at its current position.
        if (Physics.Raycast(position, surfaceNormal, out hitInfo, distance, SpatialMappingManager.Instance.LayerMask))
        {
            // If the object is occluded, reset its position.
            newPosition = hitInfo.point;
        }

        return newPosition;
    }
}
