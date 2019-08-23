
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 RotateAroundPivot(this Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 direction = point - pivot;
        return pivot + rotation * direction;
    }
}

/// <summary>
/// Extensions to Unity Physics. Primarily for automatic debug draws for raycasts.
/// </summary>
public static class PhysicsExt
{
    public static Color RAYCAST_COLOR = Color.green;
    public static Color SPHERECAST_COLOR = Color.green;
    
    public static Color CAST_HIT_COLOR = Color.red;
    
    /// How long to show the debug visualization of the cast volume
    public static int CAST_DURATION = 0;
    
    /// How long to show the debug visualization of the hit point
    public static int CAST_HIT_DURATION = 0;
    
    public static float CAST_HIT_SCALE = 0.1f;
    
    /// <summary>
    /// Wrapper for Physics.SphereCast to automatically display the cast volume and hits for debugging
    /// </summary>
    public static bool SphereCast(
        Vector3 origin, 
        float radius, 
        Vector3 direction, 
        out RaycastHit hitInfo, 
        float maxDistance,
        int layerMask = -1,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
    ) {
        bool hit = Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        DebugVR.DrawSphereCast(origin, radius, direction, maxDistance, SPHERECAST_COLOR, CAST_DURATION);
        
        if (hit)
        {
            DebugVR.DrawRaycastHit(hitInfo, CAST_HIT_COLOR, CAST_HIT_DURATION);
        }
        
        return hit;
    }
    
    /// <summary>
    /// Wrapper for Physics.SphereCastNonAlloc to automatically display the cast volume and hits for debugging
    /// </summary>
    public static int SphereCastNonAlloc(
        Vector3 origin, 
        float radius, 
        Vector3 direction, 
        RaycastHit[] results,
        float maxDistance,
        int layerMask = -1,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
    ) {
        int hits = Physics.SphereCastNonAlloc(origin, radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
        DebugVR.DrawSphereCast(origin, radius, direction, maxDistance, SPHERECAST_COLOR, CAST_DURATION);
        DebugVR.DrawRaycastHits(hits, results, CAST_HIT_COLOR, CAST_HIT_DURATION);
        
        return hits;
    }
    
    public static bool Raycast(
        Vector3 origin,
        Vector3 direction, 
        out RaycastHit hitInfo, 
        float maxDistance,
        int layerMask = -1,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
    ) {
        bool hit = Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        
        DebugVR.DrawArrow(origin, origin + (direction * maxDistance), RAYCAST_COLOR, CAST_DURATION);
        
        if (hit)
        {
            DebugVR.DrawRaycastHit(hitInfo, CAST_HIT_COLOR, CAST_HIT_DURATION);
        }
            
        return hit;
    }
    
    public static int RaycastNonAlloc(
        Vector3 origin, 
        Vector3 direction, 
        RaycastHit[] results,
        float maxDistance,
        int layerMask = -1,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
    ) {
        int hits = Physics.RaycastNonAlloc(origin, direction, results, maxDistance, layerMask, queryTriggerInteraction);
        DebugVR.DrawRaycastHits(hits, results, CAST_HIT_COLOR, CAST_HIT_DURATION);
        
        return hits;
    }
}

/// Custom debug draw methods that can be rendered in the game view (particularly for VR)
/// Attach this to the main camera. Call static draw methods. 
[ExecuteAlways]
public class DebugVR : MonoBehaviour
{
    // Edge list for an icosahedron on the unit sphere with one level of subdivison
    // Generated from: http://wiki.unity3d.com/index.php/ProceduralPrimitives#C.23_-_IcoSphere
    private static readonly float[] icosphereEdgeList = {
        -0.5257311f, 0.8506508f, 0f, -0.809017f, 0.5f, 0.309017f,
        -0.809017f, 0.5f, 0.309017f, -0.309017f, 0.809017f, 0.5f,
        -0.309017f, 0.809017f, 0.5f, -0.5257311f, 0.8506508f, 0f,
        -0.8506508f, 0f, 0.5257311f, -0.5f, 0.309017f, 0.809017f,
        -0.5f, 0.309017f, 0.809017f, -0.809017f, 0.5f, 0.309017f,
        -0.809017f, 0.5f, 0.309017f, -0.8506508f, 0f, 0.5257311f,
        0f, 0.5257311f, 0.8506508f, -0.309017f, 0.809017f, 0.5f,
        -0.309017f, 0.809017f, 0.5f, -0.5f, 0.309017f, 0.809017f,
        -0.5f, 0.309017f, 0.809017f, 0f, 0.5257311f, 0.8506508f,
        -0.309017f, 0.809017f, 0.5f, 0f, 1f, 0f,
        0f, 1f, 0f, -0.5257311f, 0.8506508f, 0f,
        0f, 0.5257311f, 0.8506508f, 0.309017f, 0.809017f, 0.5f,
        0.309017f, 0.809017f, 0.5f, -0.309017f, 0.809017f, 0.5f,
        0.5257311f, 0.8506508f, 0f, 0f, 1f, 0f,
        0f, 1f, 0f, 0.309017f, 0.809017f, 0.5f,
        0.309017f, 0.809017f, 0.5f, 0.5257311f, 0.8506508f, 0f,
        0f, 1f, 0f, -0.309017f, 0.809017f, -0.5f,
        -0.309017f, 0.809017f, -0.5f, -0.5257311f, 0.8506508f, 0f,
        0.5257311f, 0.8506508f, 0f, 0.309017f, 0.809017f, -0.5f,
        0.309017f, 0.809017f, -0.5f, 0f, 1f, 0f,
        0f, 0.5257311f, -0.8506508f, -0.309017f, 0.809017f, -0.5f,
        -0.309017f, 0.809017f, -0.5f, 0.309017f, 0.809017f, -0.5f,
        0.309017f, 0.809017f, -0.5f, 0f, 0.5257311f, -0.8506508f,
        -0.309017f, 0.809017f, -0.5f, -0.809017f, 0.5f, -0.309017f,
        -0.809017f, 0.5f, -0.309017f, -0.5257311f, 0.8506508f, 0f,
        0f, 0.5257311f, -0.8506508f, -0.5f, 0.309017f, -0.809017f,
        -0.5f, 0.309017f, -0.809017f, -0.309017f, 0.809017f, -0.5f,
        -0.8506508f, 0f, -0.5257311f, -0.809017f, 0.5f, -0.309017f,
        -0.809017f, 0.5f, -0.309017f, -0.5f, 0.309017f, -0.809017f,
        -0.5f, 0.309017f, -0.809017f, -0.8506508f, 0f, -0.5257311f,
        -0.809017f, 0.5f, -0.309017f, -0.809017f, 0.5f, 0.309017f,
        -0.8506508f, 0f, -0.5257311f, -1f, 0f, 0f,
        -1f, 0f, 0f, -0.809017f, 0.5f, -0.309017f,
        -0.809017f, 0.5f, 0.309017f, -1f, 0f, 0f,
        -1f, 0f, 0f, -0.8506508f, 0f, 0.5257311f,
        0.309017f, 0.809017f, 0.5f, 0.809017f, 0.5f, 0.309017f,
        0.809017f, 0.5f, 0.309017f, 0.5257311f, 0.8506508f, 0f,
        0f, 0.5257311f, 0.8506508f, 0.5f, 0.309017f, 0.809017f,
        0.5f, 0.309017f, 0.809017f, 0.309017f, 0.809017f, 0.5f,
        0.8506508f, 0f, 0.5257311f, 0.809017f, 0.5f, 0.309017f,
        0.809017f, 0.5f, 0.309017f, 0.5f, 0.309017f, 0.809017f,
        0.5f, 0.309017f, 0.809017f, 0.8506508f, 0f, 0.5257311f,
        -0.5f, 0.309017f, 0.809017f, 0f, 0f, 1f,
        0f, 0f, 1f, 0f, 0.5257311f, 0.8506508f,
        -0.8506508f, 0f, 0.5257311f, -0.5f, -0.309017f, 0.809017f,
        -0.5f, -0.309017f, 0.809017f, -0.5f, 0.309017f, 0.809017f,
        0f, -0.5257311f, 0.8506508f, 0f, 0f, 1f,
        0f, 0f, 1f, -0.5f, -0.309017f, 0.809017f,
        -0.5f, -0.309017f, 0.809017f, 0f, -0.5257311f, 0.8506508f,
        -1f, 0f, 0f, -0.809017f, -0.5f, 0.309017f,
        -0.809017f, -0.5f, 0.309017f, -0.8506508f, 0f, 0.5257311f,
        -0.8506508f, 0f, -0.5257311f, -0.809017f, -0.5f, -0.309017f,
        -0.809017f, -0.5f, -0.309017f, -1f, 0f, 0f,
        -0.5257311f, -0.8506508f, 0f, -0.809017f, -0.5f, 0.309017f,
        -0.809017f, -0.5f, 0.309017f, -0.809017f, -0.5f, -0.309017f,
        -0.809017f, -0.5f, -0.309017f, -0.5257311f, -0.8506508f, 0f,
        -0.5f, 0.309017f, -0.809017f, -0.5f, -0.309017f, -0.809017f,
        -0.5f, -0.309017f, -0.809017f, -0.8506508f, 0f, -0.5257311f,
        0f, 0.5257311f, -0.8506508f, 0f, 0f, -1f,
        0f, 0f, -1f, -0.5f, 0.309017f, -0.809017f,
        0f, -0.5257311f, -0.8506508f, -0.5f, -0.309017f, -0.809017f,
        -0.5f, -0.309017f, -0.809017f, 0f, 0f, -1f,
        0f, 0f, -1f, 0f, -0.5257311f, -0.8506508f,
        0.309017f, 0.809017f, -0.5f, 0.5f, 0.309017f, -0.809017f,
        0.5f, 0.309017f, -0.809017f, 0f, 0.5257311f, -0.8506508f,
        0.5257311f, 0.8506508f, 0f, 0.809017f, 0.5f, -0.309017f,
        0.809017f, 0.5f, -0.309017f, 0.309017f, 0.809017f, -0.5f,
        0.8506508f, 0f, -0.5257311f, 0.5f, 0.309017f, -0.809017f,
        0.5f, 0.309017f, -0.809017f, 0.809017f, 0.5f, -0.309017f,
        0.809017f, 0.5f, -0.309017f, 0.8506508f, 0f, -0.5257311f,
        0.5257311f, -0.8506508f, 0f, 0.809017f, -0.5f, 0.309017f,
        0.809017f, -0.5f, 0.309017f, 0.309017f, -0.809017f, 0.5f,
        0.309017f, -0.809017f, 0.5f, 0.5257311f, -0.8506508f, 0f,
        0.8506508f, 0f, 0.5257311f, 0.5f, -0.309017f, 0.809017f,
        0.5f, -0.309017f, 0.809017f, 0.809017f, -0.5f, 0.309017f,
        0.809017f, -0.5f, 0.309017f, 0.8506508f, 0f, 0.5257311f,
        0f, -0.5257311f, 0.8506508f, 0.309017f, -0.809017f, 0.5f,
        0.309017f, -0.809017f, 0.5f, 0.5f, -0.309017f, 0.809017f,
        0.5f, -0.309017f, 0.809017f, 0f, -0.5257311f, 0.8506508f,
        0.309017f, -0.809017f, 0.5f, 0f, -1f, 0f,
        0f, -1f, 0f, 0.5257311f, -0.8506508f, 0f,
        0f, -0.5257311f, 0.8506508f, -0.309017f, -0.809017f, 0.5f,
        -0.309017f, -0.809017f, 0.5f, 0.309017f, -0.809017f, 0.5f,
        -0.5257311f, -0.8506508f, 0f, 0f, -1f, 0f,
        0f, -1f, 0f, -0.309017f, -0.809017f, 0.5f,
        -0.309017f, -0.809017f, 0.5f, -0.5257311f, -0.8506508f, 0f,
        0f, -1f, 0f, 0.309017f, -0.809017f, -0.5f,
        0.309017f, -0.809017f, -0.5f, 0.5257311f, -0.8506508f, 0f,
        -0.5257311f, -0.8506508f, 0f, -0.309017f, -0.809017f, -0.5f,
        -0.309017f, -0.809017f, -0.5f, 0f, -1f, 0f,
        0f, -0.5257311f, -0.8506508f, 0.309017f, -0.809017f, -0.5f,
        0.309017f, -0.809017f, -0.5f, -0.309017f, -0.809017f, -0.5f,
        -0.309017f, -0.809017f, -0.5f, 0f, -0.5257311f, -0.8506508f,
        0.309017f, -0.809017f, -0.5f, 0.809017f, -0.5f, -0.309017f,
        0.809017f, -0.5f, -0.309017f, 0.5257311f, -0.8506508f, 0f,
        0f, -0.5257311f, -0.8506508f, 0.5f, -0.309017f, -0.809017f,
        0.5f, -0.309017f, -0.809017f, 0.309017f, -0.809017f, -0.5f,
        0.8506508f, 0f, -0.5257311f, 0.809017f, -0.5f, -0.309017f,
        0.809017f, -0.5f, -0.309017f, 0.5f, -0.309017f, -0.809017f,
        0.5f, -0.309017f, -0.809017f, 0.8506508f, 0f, -0.5257311f,
        0.809017f, -0.5f, -0.309017f, 0.809017f, -0.5f, 0.309017f,
        0.8506508f, 0f, -0.5257311f, 1f, 0f, 0f,
        1f, 0f, 0f, 0.809017f, -0.5f, -0.309017f,
        0.809017f, -0.5f, 0.309017f, 1f, 0f, 0f,
        1f, 0f, 0f, 0.8506508f, 0f, 0.5257311f,
        0.5f, -0.309017f, 0.809017f, 0f, 0f, 1f,
        0.5f, 0.309017f, 0.809017f, 0.5f, -0.309017f, 0.809017f,
        0f, 0f, 1f, 0.5f, 0.309017f, 0.809017f,
        -0.309017f, -0.809017f, 0.5f, -0.809017f, -0.5f, 0.309017f,
        -0.5f, -0.309017f, 0.809017f, -0.309017f, -0.809017f, 0.5f,
        -0.809017f, -0.5f, 0.309017f, -0.5f, -0.309017f, 0.809017f,
        -0.309017f, -0.809017f, -0.5f, -0.5f, -0.309017f, -0.809017f,
        -0.809017f, -0.5f, -0.309017f, -0.309017f, -0.809017f, -0.5f,
        -0.5f, -0.309017f, -0.809017f, -0.809017f, -0.5f, -0.309017f,
        0.5f, -0.309017f, -0.809017f, 0.5f, 0.309017f, -0.809017f,
        0f, 0f, -1f, 0.5f, -0.309017f, -0.809017f,
        0.5f, 0.309017f, -0.809017f, 0f, 0f, -1f,
        1f, 0f, 0f, 0.809017f, 0.5f, 0.309017f,
        0.809017f, 0.5f, -0.309017f, 1f, 0f, 0f,
        0.809017f, 0.5f, 0.309017f, 0.809017f, 0.5f, -0.309017f
    };

    public struct Sphere
    { 
        public Vector3 origin;
        public float radius;
        public Color color;
        public DateTime ttl;
        public bool hemisphere;
        public Vector3 up;
        
        public void Render()
        {
            // addvertex adds it normalized 
            // getMiddlePoint is just midpoint formula between two vertices
            GL.Begin(GL.LINES);
            GL.Color(color);
            
            Quaternion orientation = Quaternion.LookRotation(up);
            
            float minZ = (hemisphere) ? 0 : -1f;
            
            for (int i = 0; i < icosphereEdgeList.Length; i += 6)
            {
                // TODO: Optimize this check
                if (hemisphere)
                {
                    // If both points lie under the base, ignore edge.
                    if (icosphereEdgeList[i + 5] < -Mathf.Epsilon && icosphereEdgeList[i + 2] < -Mathf.Epsilon)
                        continue;
                        
                    // Keep points at the base to -0.309017f to flatten to 0
                    if (icosphereEdgeList[i + 2] > -Mathf.Epsilon && icosphereEdgeList[i + 5] < -0.4f)
                        continue;
                        
                    if (icosphereEdgeList[i + 5] > -Mathf.Epsilon && icosphereEdgeList[i + 2] < -0.4f)
                        continue;
                }
                
                Vector3 from;
                from.x = icosphereEdgeList[i];
                from.y = icosphereEdgeList[i + 1];
                from.z = Mathf.Clamp(icosphereEdgeList[i + 2], minZ, 1);
                
                Vector3 to;
                to.x = icosphereEdgeList[i + 3];
                to.y = icosphereEdgeList[i + 4];
                to.z = Mathf.Clamp(icosphereEdgeList[i + 5], minZ, 1);
                
                GL.Vertex(origin + orientation * (from * radius));
                GL.Vertex(origin + orientation * (to * radius));
            }
            
            GL.End();
        }
    }

    struct Line 
    {
        public Vector3 from;
        public Vector3 to;
        public Color color;
        public DateTime ttl;
        
        public void Render()
        {
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex(from);
            GL.Vertex(to);
            GL.End();
        }
    }
    
    // TODO: Only public because DrawBox needs to return it.
    public struct Box
    {
        public Vector3 origin;
        public Color color;
        public DateTime ttl;
        
        public Vector3 localFrontTopLeft;
        public Vector3 localFrontTopRight;
        public Vector3 localFrontBottomLeft;
        public Vector3 localFrontBottomRight;
        
        public Vector3 localBackTopLeft      {get {return -localFrontBottomRight;}}
        public Vector3 localBackTopRight     {get {return -localFrontBottomLeft;}}
        public Vector3 localBackBottomLeft   {get {return -localFrontTopRight;}}
        public Vector3 localBackBottomRight  {get {return -localFrontTopLeft;}}
        
        public Vector3 frontTopLeft     {get {return localFrontTopLeft + origin;}}
        public Vector3 frontTopRight    {get {return localFrontTopRight + origin;}}
        public Vector3 frontBottomLeft  {get {return localFrontBottomLeft + origin;}}
        public Vector3 frontBottomRight {get {return localFrontBottomRight + origin;}}
        public Vector3 backTopLeft      {get {return localBackTopLeft + origin;}}
        public Vector3 backTopRight     {get {return localBackTopRight + origin;}}
        public Vector3 backBottomLeft   {get {return localBackBottomLeft + origin;}}
        public Vector3 backBottomRight  {get {return localBackBottomRight + origin;}}

        public void Rotate(Quaternion orientation)
        {
            localFrontTopLeft     = localFrontTopLeft.RotateAroundPivot(Vector3.zero, orientation);
            localFrontTopRight    = localFrontTopRight.RotateAroundPivot(Vector3.zero, orientation);
            localFrontBottomLeft  = localFrontBottomLeft.RotateAroundPivot(Vector3.zero, orientation);
            localFrontBottomRight = localFrontBottomRight.RotateAroundPivot(Vector3.zero, orientation);
        }
        
        /// <summary>
        /// Draw out all 12 vertices of the box
        /// </summary>
        public void Render()
        {
            GL.Begin(GL.LINES);
            GL.Color(color);
            
            GL.Vertex(frontTopLeft); GL.Vertex(frontTopRight);
            GL.Vertex(frontTopRight); GL.Vertex(frontBottomRight);
            GL.Vertex(frontBottomRight); GL.Vertex(frontBottomLeft);
            GL.Vertex(backBottomLeft); GL.Vertex(frontTopLeft);
            
            GL.Vertex(backTopLeft); GL.Vertex(backTopRight);
            GL.Vertex(backTopRight); GL.Vertex(backBottomRight);
            GL.Vertex(backBottomRight); GL.Vertex(backBottomLeft);
            GL.Vertex(backBottomLeft); GL.Vertex(backTopLeft);
            
            GL.Vertex(frontTopLeft); GL.Vertex(backTopLeft);
            GL.Vertex(frontTopRight); GL.Vertex(backTopRight);
            GL.Vertex(frontBottomRight); GL.Vertex(backBottomRight);
            GL.Vertex(frontBottomLeft); GL.Vertex(backBottomLeft);
            
            GL.End();
        }
    }
 
    struct Icon
    {
        public Vector3 center;
        public Material material;
        public DateTime ttl;
    }
    
    static List<Line> m_Lines;
    static List<Sphere> m_Spheres;
    static List<Box> m_Boxes;
    static List<Icon> m_Icons;

    static List<Line> m_OneShotLines;
    static List<Sphere> m_OneShotSpheres;
    static List<Box> m_OneShotBoxes;
    
    Material m_LineMaterial;
    
    Mesh m_IconMesh;
    static Dictionary<string, Material> m_IconMaterialPool;

    public static void DrawLine(Vector3 from, Vector3 to, Color color, float duration = 0)
    {
        Line line = new Line()
        {
            from = from,
            to = to,
            color = color,
            ttl = DateTime.Now.AddMilliseconds(duration)
        };
        
        if (Mathf.Approximately(duration, 0))
        {
            m_OneShotLines.Add(line);
        }
        else
        {
            m_Lines.Add(line);
        }
    }
    
    public static Sphere DrawSphere(
        Vector3 origin, 
        float radius, 
        Color color, 
        float duration = 0
    ) {
        Sphere sphere = new Sphere()
        {
            origin = origin,
            radius = radius,
            color = color,
            up = Vector3.up, 
            ttl = DateTime.Now.AddMilliseconds(duration)
        };
        
        if (Mathf.Approximately(duration, 0))
        {
            m_OneShotSpheres.Add(sphere);
        }
        else
        {
            m_Spheres.Add(sphere);
        }
        
        return sphere;
    }
    
    public static Sphere DrawHemisphere(Vector3 origin, float radius, Vector3 up, Color color, float duration = 0)
    {
        Sphere hemisphere = new Sphere()
        {
            origin = origin,
            radius = radius,
            color = color,
            hemisphere = true,
            up = up,
            ttl = DateTime.Now.AddMilliseconds(duration)
        };
        
        if (Mathf.Approximately(duration, 0))
        {
            m_OneShotSpheres.Add(hemisphere);
        }
        else
        {
            m_Spheres.Add(hemisphere);
        }
        
        return hemisphere;
    }
    
    public static Box DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color, float duration = 0)
    {
        Box box = new Box()
        {
            origin = origin,
            color = color,
            ttl = DateTime.Now.AddMilliseconds(duration),
            localFrontTopLeft = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z),
            localFrontTopRight = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z),
            localFrontBottomLeft = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z),
            localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z)
        };
        
        box.Rotate(orientation);
        
        // Technically we could break this down into multiple line components,
        // but I don't want to dump 12 lines of all the same TTL each time we 
        // try to draw a box. It'll slow down cleanup. 
        if (Mathf.Approximately(duration, 0))
        {
            m_OneShotBoxes.Add(box);
        }
        else
        {
            m_OneShotBoxes.Add(box);
        }
        
        return box;
    }
    
    /// <summary>
    /// Draw out a box to trace out the path of Physics.BoxCast
    /// Author: http://answers.unity.com/answers/1156088/view.html
    /// </summary>
    /// <param name="center">Center.</param>
    /// <param name="halfExtents">Half extents.</param>
    /// <param name="direction">Direction.</param>
    /// <param name="orientation">Orientation.</param>
    /// <param name="maxDistance">Max distance.</param>
    /// <param name="color">Color.</param>
    /// <param name="duration">Duration.</param>
    public static void DrawBoxCast(
        Vector3 center, 
        Vector3 halfExtents, 
        Vector3 direction, 
        Quaternion orientation, 
        float maxDistance,
        Color color,
        float duration = 0
    ) {
        direction.Normalize();
        
        // Draw the start and end boxes
        var from = DrawBox(center, halfExtents, orientation, color, duration);
        var to = DrawBox(center + (direction * maxDistance), halfExtents, orientation, color, duration);
        
        // Include edges along the cast direction
        DrawLine(from.backBottomLeft, to.backBottomLeft, color, duration);
        DrawLine(from.backBottomRight, to.backBottomRight, color, duration);
        DrawLine(from.backTopLeft, to.backTopLeft, color, duration);
        DrawLine(from.backTopRight, to.backTopRight, color, duration);
        DrawLine(from.frontTopLeft, to.frontTopLeft, color, duration);
        DrawLine(from.frontTopRight, to.frontTopRight, color, duration);
        DrawLine(from.frontBottomLeft, to.frontBottomLeft, color, duration);
        DrawLine(from.frontBottomRight, to.frontBottomRight, color, duration);
        
        // Add an arrow along the direction of the cast
        DrawArrow(from.origin, to.origin, color, duration);
        
        // TODO: Better colorization? E.g. maybe lower alpha for the edge lines
    }
    
    /// <summary>
    /// Draw out spheres to trace the path of Physics.SphereCast
    /// </summary>
    /// <param name="origin">Origin.</param>
    /// <param name="radius">Radius.</param>
    /// <param name="direction">Direction.</param>
    /// <param name="maxDistance">Max distance.</param>
    /// <param name="color">Color.</param>
    /// <param name="duration">Duration.</param>
    public static void DrawSphereCast(
        Vector3 origin,
        float radius,
        Vector3 direction,
        float maxDistance,
        Color color,
        float duration = 0
    ) {
        direction.Normalize();
        
        Color castColor = color;
        castColor.a *= 0.33f;
         
        // Starting sphere of the cast. Helps visualize whether something
        // will be ignored by the spherecast because it was already inside
        // the initial sphere (PhysX limitation)
        DrawSphere(origin, radius, color, duration);
        
        // Capsule along the ray of the cast
        DrawCapsule(
            origin, 
            origin + direction * maxDistance, 
            radius, 
            castColor, 
            duration,
            false
        );
        
        /*
        var orientation = Quaternion.LookRotation(direction);
        
        // Draw circles along the cast axis.
        for (int i = 0; i < 6; i++)
        {
            DrawCircle(origin + (direction * maxDistance * i / 6.0f), radius, orientation, color, duration);
        }*/
    }
    
    public static void DrawCapsule(
        Vector3 point1, 
        Vector3 point2,
        float radius,
        Color color,
        float duration = 0,
        bool hemisphereForPoint1 = true
    ) { 
        Quaternion orientation = Quaternion.LookRotation(point2 - point1);
        
        // Hemisphere forward axis needs to be the up axis.
        if (hemisphereForPoint1)
        {
            DrawHemisphere(point1, radius, point1 - point2, color, duration);
        }
       
        DrawHemisphere(point2, radius, point2 - point1, color, duration);
        
        int vertices = 12;
        for (int i = 0; i < vertices; i++)
        {
            float fromAngle = i * Mathf.PI * 2.0f / vertices;
            
            Vector3 offset = new Vector3(Mathf.Cos(fromAngle), Mathf.Sin(fromAngle), 0) * radius;
            offset = offset.RotateAroundPivot(Vector3.zero, orientation);
            
            DrawLine(point1 + offset, point2 + offset, color, duration);
        }
    }
    
    public static void DrawCapsuleCast(
        Vector3 point1,
        Vector3 point2,
        float radius,
        Vector3 direction, 
        float maxDistance,
        Color color,
        float duration = 0
    ) {
        direction.Normalize();
       
        Color castColor = color;
        color.a *= 0.5f;
         
        DrawCapsule(point1, point2, radius, color, duration);
        
        DrawCapsule(
            point1 + direction * maxDistance, 
            point2 + direction * maxDistance, 
            radius, 
            castColor, 
            duration
        );
        
        // TODO: Midpoint rays somehow. 
    }
    
    public static void DrawArrow(Vector3 from, Vector3 to, Color color, float duration = 0, bool arrowOnBothEnds = false)
    {
        float distance = (to - from).magnitude;
        float size = 0.25f * Mathf.Min(1f, distance / 0.55f);

        DrawLine(from, to, color, duration);
        DrawArrowhead(to, to - from, color, duration, size);
        
        if (arrowOnBothEnds)
        {
            DrawArrowhead(from, from - to, color, duration, size);
        }
    }
    
    public static void DrawArrowhead(Vector3 position, Vector3 direction, Color color, float duration = 0, float size = 0.25f)
    {
        float angle = 20.0f;
        
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + angle, 0) * Vector3.forward;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - angle, 0) * Vector3.forward;
       
        DrawLine(position, position + right * size, color, duration);
        DrawLine(position, position + left * size, color, duration);
    }
    
    public static void DrawCircle(Vector3 center, float radius, Quaternion orientation, Color color, float duration = 0, int vertices = 12)
    {
        for (int i = 0; i < vertices; i++)
        {
            float fromAngle = i * Mathf.PI * 2.0f / vertices;
            float toAngle = ((i + 1) % vertices) * Mathf.PI * 2.0f / vertices;
            
            Vector3 from = center + new Vector3(Mathf.Cos(fromAngle), Mathf.Sin(fromAngle), 0) * radius;
            Vector3 to = center + new Vector3(Mathf.Cos(toAngle), Mathf.Sin(toAngle), 0) * radius;
            
            from = from.RotateAroundPivot(center, orientation);
            to = to.RotateAroundPivot(center, orientation);
            
            DrawLine(from, to, color, duration);
        }
    }
    
    public static void DrawIcon(Vector3 center, string iconName, float duration = 0)
    {
        Icon icon = new Icon()
        {
            center = center,
            material = GetIconMaterial(iconName),
            ttl =  DateTime.Now.AddMilliseconds(duration)
        };
        
        m_Icons.Add(icon);
    }
    
    public static float RAYCAST_HIT_SCALE = 0.1f;
    
    public static void DrawRaycastHit(RaycastHit hit, Color color, float duration = 0)
    {
        DrawSphere(hit.point, RAYCAST_HIT_SCALE * 0.1f, color, duration);
        DrawArrow(hit.point, hit.point + hit.normal * RAYCAST_HIT_SCALE, color, duration);
    }
    
    public static void DrawRaycastHits(int total, RaycastHit[] hits, Color color, float duration = 0)
    {
        for (var i = 0; i < total; i++)
            DrawRaycastHit(hits[i], color, duration);
    }
    
    private void OnEnable()
    {
        m_Lines = new List<Line>();
        m_Spheres = new List<Sphere>();
        m_Boxes = new List<Box>();
        m_OneShotLines = new List<Line>();
        m_OneShotSpheres = new List<Sphere>();
        m_OneShotBoxes = new List<Box>();
        
        if (!m_LineMaterial)
        {
            // Set to a builtin shader for simple colors
            // Via: https://docs.unity3d.com/ScriptReference/GL.html
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            m_LineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            m_LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m_LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            m_LineMaterial.SetInt("_ZWrite", 0);
        }
        
        // Dump();
        
        SetupIconRenderer();
    }

    private void SetupIconRenderer()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.hideFlags = HideFlags.HideAndDontSave;
        
        m_IconMesh = go.GetComponent<MeshFilter>().sharedMesh;
        
        if (Application.isEditor)
        {
            DestroyImmediate(go);
        }
        else
        {
            Destroy(go);
        }
        
        m_Icons = new List<Icon>();
        m_IconMaterialPool = new Dictionary<string, Material>();
    }
    
    private void RenderIcons()
    {
        if (!Camera.current)
        {
            return;
        }
        
        var matrix = Matrix4x4.identity;
        
        foreach (var icon in m_Icons)
        {
            // Billboard matrix for the icon
            matrix.SetTRS(
                icon.center, 
                Quaternion.LookRotation(Camera.current.transform.forward, Vector3.up), 
                Vector3.one * HandleUtility.GetHandleSize(icon.center)
            );
            
            icon.material.SetPass(0);
            Graphics.DrawMeshNow(m_IconMesh, matrix);
        }
    }
    
    private static Material GetIconMaterial(string iconName)
    {
        Material mat;
        
        if (m_IconMaterialPool.ContainsKey(iconName)) {
            mat = m_IconMaterialPool[iconName];
        }
        else
        {
            Debug.Log("Loading new mat");
            
            // TODO: Usability outside the editor? Not sure if this works in play mode
            Texture2D texture = AssetDatabase.LoadAssetAtPath("Assets/Gizmos/" + iconName, typeof(Texture2D)) as Texture2D;
        
            Debug.Log(texture);
        
            // TODO: Default if not found
            mat = new Material(Shader.Find("Unlit/Transparent"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            
            mat.mainTexture = texture;
            m_IconMaterialPool[iconName] = mat;
        }
        
        return mat;
    }
    
    private void Cleanup()
    {
        m_OneShotLines.Clear();
        m_OneShotSpheres.Clear();
        m_OneShotBoxes.Clear();
        
        var now = DateTime.Now;
        
        m_Lines.RemoveAll(x => (x.ttl < now));
        m_Spheres.RemoveAll(x => (x.ttl < now));
        m_Boxes.RemoveAll(x => (x.ttl < now));
        m_Icons.RemoveAll(x => (x.ttl < now));
    }

    private void Render()
    {
        int i;
        
        m_LineMaterial.SetPass(0);
        
        GL.PushMatrix();
        
        for (i = 0; i < m_Lines.Count; i++)
            m_Lines[i].Render();
        
        for (i = 0; i < m_OneShotLines.Count; i++)
            m_OneShotLines[i].Render();
        
        for (i = 0; i < m_Spheres.Count; i++)
            m_Spheres[i].Render();
        
        for (i = 0; i < m_OneShotSpheres.Count; i++)
            m_OneShotSpheres[i].Render();
        
        for (i = 0; i < m_Boxes.Count; i++)
            m_Boxes[i].Render();
        
        for (i = 0; i < m_OneShotBoxes.Count; i++)
            m_OneShotBoxes[i].Render();
        
        GL.PopMatrix();
        
        RenderIcons();
        
        Cleanup();
    }

    /// <summary>
    /// Draw debug geometry during the gizmos pass while in the editor
    /// </summary>
    private void OnDrawGizmos()
    {
        Render();
    }

    /// <summary>
    /// Render on the camera after everything else has been rendered
    /// </summary>
    private void OnPostRender()
    {
        // NOTE: This isn't done in HDRP/LWRP.
        Render();
    }
    
#region Generate icosphere edgelist

    private struct TriangleIndices
    {
        public int v1;
        public int v2;
        public int v3;
 
        public TriangleIndices(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }
 
    // return index of point in the middle of p1 and p2
    private static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
    {
        // first check if we have it already
        bool firstIsSmaller = p1 < p2;
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;
 
        int ret;
        if (cache.TryGetValue(key, out ret))
        {
            return ret;
        }
 
        // not in cache, calculate it
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = new Vector3
        (
            (point1.x + point2.x) / 2f, 
            (point1.y + point2.y) / 2f, 
            (point1.z + point2.z) / 2f
        );
 
        // add vertex makes sure point is on unit sphere
        int i = vertices.Count;
        vertices.Add( middle.normalized * radius ); 
 
        // store it, return index
        cache.Add(key, i);
 
        return i;
    }

    struct Edge
    {
        public Vector3 a;
        public Vector3 b;
    }
    
    List<Edge> m_Edges = new List<Edge>();
    
    private void AddEdge(Vector3 a, Vector3 b)
    {
        foreach (var edge in m_Edges)
        {
            if (edge.a == a && edge.b == b) return;
            if (edge.a == b && edge.b == a) return;
        }
        
        m_Edges.Add(new Edge() {  a = a, b= b});
    }
    
    private void Dump()
    {
        List<Vector3> vertList = new List<Vector3>();
        Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
 
        int recursionLevel = 1;
        float radius = 1f;
 
        // create 12 vertices of a icosahedron
        float t = (1f + Mathf.Sqrt(5f)) / 2f;
 
        vertList.Add(new Vector3(-1f,  t,  0f).normalized * radius);
        vertList.Add(new Vector3( 1f,  t,  0f).normalized * radius);
        vertList.Add(new Vector3(-1f, -t,  0f).normalized * radius);
        vertList.Add(new Vector3( 1f, -t,  0f).normalized * radius);
 
        vertList.Add(new Vector3( 0f, -1f,  t).normalized * radius);
        vertList.Add(new Vector3( 0f,  1f,  t).normalized * radius);
        vertList.Add(new Vector3( 0f, -1f, -t).normalized * radius);
        vertList.Add(new Vector3( 0f,  1f, -t).normalized * radius);
 
        vertList.Add(new Vector3( t,  0f, -1f).normalized * radius);
        vertList.Add(new Vector3( t,  0f,  1f).normalized * radius);
        vertList.Add(new Vector3(-t,  0f, -1f).normalized * radius);
        vertList.Add(new Vector3(-t,  0f,  1f).normalized * radius);
 
 
        // create 20 triangles of the icosahedron
        List<TriangleIndices> faces = new List<TriangleIndices>();
 
        // 5 faces around point 0
        faces.Add(new TriangleIndices(0, 11, 5));
        faces.Add(new TriangleIndices(0, 5, 1));
        faces.Add(new TriangleIndices(0, 1, 7));
        faces.Add(new TriangleIndices(0, 7, 10));
        faces.Add(new TriangleIndices(0, 10, 11));
 
        // 5 adjacent faces 
        faces.Add(new TriangleIndices(1, 5, 9));
        faces.Add(new TriangleIndices(5, 11, 4));
        faces.Add(new TriangleIndices(11, 10, 2));
        faces.Add(new TriangleIndices(10, 7, 6));
        faces.Add(new TriangleIndices(7, 1, 8));
 
        // 5 faces around point 3
        faces.Add(new TriangleIndices(3, 9, 4));
        faces.Add(new TriangleIndices(3, 4, 2));
        faces.Add(new TriangleIndices(3, 2, 6));
        faces.Add(new TriangleIndices(3, 6, 8));
        faces.Add(new TriangleIndices(3, 8, 9));
 
        // 5 adjacent faces 
        faces.Add(new TriangleIndices(4, 9, 5));
        faces.Add(new TriangleIndices(2, 4, 11));
        faces.Add(new TriangleIndices(6, 2, 10));
        faces.Add(new TriangleIndices(8, 6, 7));
        faces.Add(new TriangleIndices(9, 8, 1));
 
 
        // refine triangles
        for (int i = 0; i < recursionLevel; i++)
        {
            List<TriangleIndices> faces2 = new List<TriangleIndices>();
            foreach (var tri in faces)
            {
                // replace triangle by 4 triangles
                int a = getMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius);
                int b = getMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius);
                int c = getMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius);
 
                faces2.Add(new TriangleIndices(tri.v1, a, c));
                faces2.Add(new TriangleIndices(tri.v2, b, a));
                faces2.Add(new TriangleIndices(tri.v3, c, b));
                faces2.Add(new TriangleIndices(a, b, c));
            }
            faces = faces2;
        }
 
        List< int > triList = new List<int>();
        for( int i = 0; i < faces.Count; i++ )
        {
            triList.Add( faces[i].v1 );
            triList.Add( faces[i].v2 );
            triList.Add( faces[i].v3 );
        }
        
        var vertices = vertList.ToArray();
        
        List<Tuple<Vector3, Vector3>> edges = new List<Tuple<Vector3, Vector3>>();
        
        m_Edges.Clear();
        for( int i = 0; i < faces.Count; i++ )
        {
            var v1 = vertices[faces[i].v1];
            var v2 = vertices[faces[i].v2];
            var v3 = vertices[faces[i].v3];
            
            AddEdge(v1, v2);
            AddEdge(v2, v3);
            AddEdge(v3, v1);
        }
        
        using (StreamWriter file = new StreamWriter(@"/Users/mcmanning.1/Downloads/generated.txt"))
        {
            file.WriteLine(string.Format(
                "// {0} vertices, {1} faces, {2} edges", 
                vertices.Length, 
                faces.Count, 
                edges.Count
            ));
            
            file.WriteLine("const float edgeVertices = new float[] {");

            foreach (var edge in m_Edges)
            {
                // Each output line is an edge (two Vertex3s)
                file.WriteLine(string.Format(
                    "    {0}f, {1}f, {2}f, {3}f, {4}f, {5}f,", 
                    edge.a.x, edge.a.y, edge.a.z,
                    edge.b.x, edge.b.y, edge.b.z
                ));
            }
            file.WriteLine("};");
        }
    }

#endregion
}
