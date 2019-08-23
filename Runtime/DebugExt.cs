using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DebugExt : MonoBehaviour
{
    // TODO: Only public because DrawBox needs to return it.
    public struct Box
    {
        public Vector3 origin;
        
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
    }
 
    struct Line
    {
        public Vector3 from;
        public Vector3 to;
        public Color color;
        public DateTime ttl;
    }
    
    struct Icon
    {
        public Vector3 center;
        public Material material;
        public DateTime ttl;
    }
    
    static List<Line> m_Lines;
    static List<Line> m_OneShotLines;
    
    Material m_LineMaterial;
    
    Mesh m_IconMesh;
    static List<Icon> m_Icons;
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
    
    public static void DrawCircle(Vector3 center, float radius, Quaternion orientation, Color color, float maxDegrees = 360f, float duration = 0, int vertices = 12)
    {
        float maxRad = maxDegrees * Mathf.Deg2Rad;
        
        // TODO: Fix. 
        for (int i = 0; i < vertices + 1; i++)
        {
            float fromAngle = i * maxRad / vertices;
            float toAngle = Mathf.Min(maxRad, (i + 1) * maxRad / vertices);
            
            Vector3 from = center + new Vector3(Mathf.Cos(fromAngle), Mathf.Sin(fromAngle), 0) * radius;
            Vector3 to = center + new Vector3(Mathf.Cos(toAngle), Mathf.Sin(toAngle), 0) * radius;
            
            from = from.RotateAroundPivot(center, orientation);
            to = to.RotateAroundPivot(center, orientation);
            
            DrawLine(from, to, color, duration);
        }
    }

    public static void DrawBox(Box box, Color color, float duration = 0)
    {        
        DrawLine(box.frontTopLeft, box.frontTopRight, color, duration);
        DrawLine(box.frontTopRight, box.frontBottomRight, color, duration);
        DrawLine(box.frontBottomRight, box.frontBottomLeft, color, duration);
        DrawLine(box.frontBottomLeft, box.frontTopLeft, color, duration);

        DrawLine(box.backTopLeft, box.backTopRight, color, duration);
        DrawLine(box.backTopRight, box.backBottomRight, color, duration);
        DrawLine(box.backBottomRight, box.backBottomLeft, color, duration);
        DrawLine(box.backBottomLeft, box.backTopLeft, color, duration);

        DrawLine(box.frontTopLeft, box.backTopLeft, color, duration);
        DrawLine(box.frontTopRight, box.backTopRight, color, duration);
        DrawLine(box.frontBottomRight, box.backBottomRight, color, duration);
        DrawLine(box.frontBottomLeft, box.backBottomLeft, color, duration);
    }
    
    public static Box DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color, float duration = 0)
    {
        Box box = new Box()
        {
            origin = origin,
            localFrontTopLeft = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z),
            localFrontTopRight = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z),
            localFrontBottomLeft = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z),
            localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z)
        };
        
        box.Rotate(orientation);
        
        DrawBox(box, color, duration);
        return box;
    }
    
    public static void DrawSphere(
        Vector3 origin, 
        float radius, 
        Color color, 
        float duration = 0
    ) {
        DrawCircle(origin, radius, Quaternion.identity, color, 360f, duration);
        DrawCircle(origin, radius, Quaternion.Euler(0, 90f, 0), color, 360f, duration);
    }

    public static void DrawHemisphere(
        Vector3 origin,
        float radius,
        Quaternion orientation,
        Color color,
        float duration = 0
    ) {
        DrawCircle(origin, radius, orientation * Quaternion.Euler(90f, 0, 0), color, 180f, duration);
        DrawCircle(origin, radius, orientation * Quaternion.Euler(90f, 0, 0) * Quaternion.Euler(0, 90f, 0), color, 180f, duration);
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
            DrawHemisphere(point1, radius, Quaternion.LookRotation(point1 - point2), color, duration);
        }
       
        DrawHemisphere(point2, radius, orientation, color, duration);
     
        for (int degree = 0; degree < 360; degree += 90)
        {
            Vector3 offset = new Vector3(
                Mathf.Cos(degree * Mathf.Deg2Rad), 
                Mathf.Sin(degree * Mathf.Deg2Rad), 
                0
            ) * radius;
            
            offset = offset.RotateAroundPivot(Vector3.zero, orientation);
            
            DrawLine(point1 + offset, point2 + offset, color, duration);
        }   
    }
    
    /// <summary>
    /// Draw out spheres to trace the path of Physics.SphereCast
    /// </summary>
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
    }
    
    public static float RAYCAST_HIT_SCALE = 0.1f;
    
    public static void DrawRaycast(
        Vector3 origin,
        Vector3 direction,
        float maxDistance,
        Color color,
        float duration = 0
    ) {
        DrawArrow(origin, origin + (direction * maxDistance), color, duration);
    }

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
        castColor.a *= 0.33f;
         
        // Starting capsule
        DrawCapsule(point1, point2, radius, color, duration);
        
        // Capsule at the end of the cast
        DrawCapsule(
            point1 + direction * maxDistance, 
            point2 + direction * maxDistance, 
            radius, 
            castColor, 
            duration
        );
        
        // TODO: Midpoint rays somehow. Not sure
        // what placement makes sense here. 
        // For now, it's an arrow.
        Vector3 midpoint = Vector3.Lerp(point1, point2, 0.5f);
        DrawArrow(midpoint, midpoint + direction * maxDistance, color, duration);
    }
    
    /// <summary>
    /// Draw out a box to trace out the path of Physics.BoxCast
    /// Author: http://answers.unity.com/answers/1156088/view.html
    /// </summary>
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
        
        Color castColor = color;
        castColor.a *= 0.33f;
         
        // Draw the start and end boxes
        var from = DrawBox(center, halfExtents, orientation, color, duration);
        var to = DrawBox(center + (direction * maxDistance), halfExtents, orientation, castColor, duration);
        
        // Include edges along the cast direction
        DrawLine(from.backBottomLeft, to.backBottomLeft, castColor, duration);
        DrawLine(from.backBottomRight, to.backBottomRight, castColor, duration);
        DrawLine(from.backTopLeft, to.backTopLeft, castColor, duration);
        DrawLine(from.backTopRight, to.backTopRight, castColor, duration);
        DrawLine(from.frontTopLeft, to.frontTopLeft, castColor, duration);
        DrawLine(from.frontTopRight, to.frontTopRight, castColor, duration);
        DrawLine(from.frontBottomLeft, to.frontBottomLeft, castColor, duration);
        DrawLine(from.frontBottomRight, to.frontBottomRight, castColor, duration);
        
        // Add an arrow along the direction of the cast
        // DrawArrow(from.origin, to.origin, color, duration);
    }
    
    private void OnEnable()
    {
        m_Lines = new List<Line>();
        m_OneShotLines = new List<Line>();
        
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
    
    private void DrawLines()
    {
        var now = DateTime.Now;
        
        m_LineMaterial.SetPass(0);
        
        GL.PushMatrix();
        GL.Begin(GL.LINES);

        // Draw one shots
        foreach (var line in m_OneShotLines)
        {
            GL.Color(line.color);
            GL.Vertex(line.from);
            GL.Vertex(line.to);
        }

        // Clear one shots
        // TODO: If it's allocated with 0 length, does this
        // deallocate - forcing us to reallocate room every frame?
        m_OneShotLines.Clear();
        
        // Draw longer lasting lines, removing items
        // that are no longer alive from the updated list
        var alive = new List<Line>(m_Lines.Count);
        foreach (var line in m_Lines)
        {
            // TODO: Perf for rapid color swapping
            GL.Color(line.color); 
            GL.Vertex(line.from);
            GL.Vertex(line.to);
        
            if (line.ttl > now)
            {
                alive.Add(line);
            }
        }

        m_Lines = alive;

        GL.End();
        GL.PopMatrix();
    }

    private void DrawIcons()
    {
        if (!Camera.current)
        {
            return;
        }
        
        var now = DateTime.Now;
        
        var matrix = Matrix4x4.identity;
        
        var alive = new List<Icon>(m_Icons.Count);
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
            
            if (icon.ttl > now)
            {
                alive.Add(icon);
            }
        }

        m_Icons = alive;
    }

    private void Render()
    {
        Debug.Log(string.Format(
            "{0}, {1}, {2}", 
            m_Lines.Count,
            m_OneShotLines.Count,
            m_Icons.Count
        ));

        DrawLines();
        DrawIcons();
    }
    
    /// <summary>
    /// Draw debug geometry during the gizmos pass while in the editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (isActiveAndEnabled)
        {
            Render();
        }
    }

    /// <summary>
    /// Render on the camera after everything else has been rendered
    /// </summary>
    private void OnPostRender()
    {
        // NOTE: This isn't done in HDRP/LWRP.
        if (isActiveAndEnabled)
        {
            Render();
        }
    }
}
