using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugExtDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void DrawDemo()
    {
        // Icon rendering
        DebugExt.DrawIcon(5f * Vector3.right, "locator.png");

        // Red line
        DebugExt.DrawLine(Vector3.zero, Vector3.forward, Color.red);

        // Blue arrow
        DebugExt.DrawArrow(Vector3.left, 2f * Vector3.left, Color.blue);

        // Green bidirectional arrow
        DebugExt.DrawArrow(Vector3.right, 2f * Vector3.right, Color.green, 0, true);
    
        // Red circle normal to +Z axis
        DebugExt.DrawCircle(Vector3.up, 0.25f, Quaternion.identity, Color.red);
    
        // Blue semicircle normal to the +Y axis
        DebugExt.DrawCircle(Vector3.zero, 0.25f, Quaternion.identity, Color.blue, 180f);
    
        // Oriented box
        DebugExt.DrawBox(
            2f * Vector3.up, 
            0.5f * Vector3.one, 
            Quaternion.Euler(45f, 45f, 45f),
            Color.cyan
        );
        
        // Simple sphere (2 circles)
        DebugExt.DrawSphere(Vector3.down, 0.5f, Color.blue);
        
        // Hemisphere pointed down the +X axis
        DebugExt.DrawHemisphere(
            3f * Vector3.left,
            0.5f,
            Quaternion.LookRotation(Vector3.right),
            Color.yellow
        );

        DebugExt.DrawCapsule(
            3f * Vector3.right, 
            3f * Vector3.right + Vector3.up,
            0.5f, 
            Color.magenta
        );

        // Visualize a Physics.SphereCast
        // by a sphere representing the initial position and
        // an oriented capsule in the direction of the cast
        DebugExt.DrawSphereCast(
            2f * Vector3.back,
            0.5f,
            Vector3.back + Vector3.left,
            2f,
            Color.cyan
        );

        // Visualize a Physics.CapsuleCast
        DebugExt.DrawCapsuleCast(
            3f * Vector3.forward,
            3f * Vector3.forward + Vector3.right,
            0.5f,
            Vector3.right,
            3f,
            Color.red
        );

        // Visualize a Physics.BoxCast
        DebugExt.DrawBoxCast(
            2f * (Vector3.back + Vector3.right),
            0.25f * Vector3.one,
            Vector3.back + Vector3.right,
            Quaternion.Euler(0, 45f, 45f),
            2f,
            Color.yellow
        );
    }

    private void OnDrawGizmos()
    {
        DrawDemo();
    }

    // Update is called once per frame
    void Update()
    {
        DrawDemo();
    }
}
