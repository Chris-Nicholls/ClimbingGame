using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[System.Serializable]
public struct MaterialFriction
{
    public Material material;
    public float friction;
}



public class MoveArms2D : MonoBehaviour
{
    public Rigidbody2D body;
    public Rigidbody2D rightHand;
    public Rigidbody2D leftHand;
    public float force = -8.0f;
    public float frictionLevel = 30f;
    public float gripMultiplier = 0.5f;
    public float burstMultiplier = 1.0f;
    public float burstDuration = 1.0f;
    public float handMultiplier = 1.0f;

    private float leftAirTime = 0.0f;
    private float rightAirTime = 0.0f;

    public List<MaterialFriction> materialFrictions;
    private Dictionary<string, float> materialFrictionDict;

    public float body_z;

    void OnEnable()
    {
        // fix framerate to 60
        Application.targetFrameRate = 60;
        body_z = body.transform.position.z;
        materialFrictionDict = new Dictionary<string, float>();
        foreach (var materialFriction in materialFrictions)
        {
            materialFrictionDict[materialFriction.material.name] = materialFriction.friction;
        }
    }

    void FixedUpdate()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 1.0f));

        Vector2 armDirection = (body.transform.position - screenPos) * 5;
        // if norm > 1 then normalize
        if (armDirection.magnitude > 1)
        {
            armDirection.Normalize();
        }

        Vector2 footDirection = new Vector2(armDirection.x, armDirection.y / 4);
        //when 'a' is pressed fix the left hand position
        bool leftHolding = HandLogic(leftHand, armDirection, force, KeyCode.A, rightAirTime);
        bool rightHolding = HandLogic(rightHand, armDirection, force, KeyCode.D, leftAirTime);

        leftAirTime = leftHolding ? 0.0f : leftAirTime + Time.fixedDeltaTime;
        rightAirTime = rightHolding ? 0.0f : rightAirTime + Time.fixedDeltaTime;

        //lock the z positions of the body
        body.transform.position = new Vector3(body.transform.position.x, body.transform.position.y, body_z);

        // set camera y to the body y
        Camera.main.transform.position = new Vector3(body.transform.position.x, body.transform.position.y, Camera.main.transform.position.z);

    }

    public bool HandLogic(Rigidbody2D hand, Vector2 handDirection, float handForce, KeyCode key, float burstTime)
    {
        FrictionJoint2D friction = hand.transform.GetComponent<FrictionJoint2D>();

        var underHand = GetMaterialUnderHand(hand.transform.position);
        var matName = underHand.Item1 != null ? underHand.Item1.name : "None";
        // remove ' (Instance)' from the name, which is added by unity
        if (matName.IndexOf(" (Instance)") > 0)
        {
            matName = matName.Substring(0, matName.IndexOf(" (Instance)"));
        }

        var frictionAmount = 0.0f;
        if (underHand.Item1 != null)
        {

            if (materialFrictionDict.ContainsKey(matName))
            {
                frictionAmount = materialFrictionDict[matName];
            }
            else
            {
                Debug.LogError("Material: " + matName + " not found in material friction list");
            }
        }

        if (underHand.Item2 != null)
        {
            // get the friciton of the material
            // apply the friction to the hand
            friction.maxForce = frictionAmount * frictionLevel;
        }

        bool holding = Input.GetKey(key);
        float handSpeed = hand.velocity.magnitude;

        if (holding && underHand.Item2 != null)
        {
            friction.enabled = true;
            // fix to world position of hand 
            if (handSpeed > 0.2f)
            {
                friction.connectedAnchor = hand.transform.TransformPoint(hand.transform.position);
            }
        }
        else
        {
            friction.enabled = false;
        }

        if (holding && underHand.Item2 != null && handSpeed < 0.2f)
        {
            // lerp between grip and burst multiplier based on time
            float power = Mathf.Lerp(this.gripMultiplier, this.burstMultiplier, 1 - burstTime / burstDuration);

            body.AddForce(handDirection * handForce * power);
            hand.AddForce(-handDirection * handForce * power);
            return true;
        }
        else
        {
            float power = Mathf.Lerp(this.handMultiplier, this.burstMultiplier, 1 - burstTime / burstDuration);
            hand.AddForce(handDirection * handForce * power);
            body.AddForce(-handDirection * handForce * power);
            return false;
        }
    }

    (Material, Transform) GetMaterialUnderHand(Vector3 location)
    {

        // cast along z axis away from camera
        var direction = location - Camera.main.transform.position;
        Debug.DrawRay(location, direction, Color.red, 1.0f);
        RaycastHit[] hits = Physics.RaycastAll(location, direction);

        Vector3 closestHitpoint = new Vector3(0, 0, -100000);
        RaycastHit closest = new RaycastHit();

        // find the closest collider
        foreach (var hit in hits)
        {
            if (hit.point.z > closestHitpoint.z)
            {
                closestHitpoint = hit.point;
                closest = hit;
            }
        }
        if (closest.collider == null)
            return (null, null);

        var meshCollider = closest.collider as MeshCollider;
        if (meshCollider == null)
            return (null, null);


        // get the material of the closest triangle
        var meshFilterMesh = closest.transform.GetComponent<MeshFilter>().mesh;

        var renderer = closest.transform.GetComponent<Renderer>();
        var i = closest.triangleIndex * 3;
        if (renderer.materials.Length == 1)
        {
            // No need to iterate through submeshes, since there is only one material
            // And Unity combines static meshes with the same material, with read/write off
            // so we can't access the mesh's triangles anyway 
            return (renderer.material, closest.transform);
        }

        for (var m = 0; m < renderer.materials.Length; m++)
        {
            var mts = meshFilterMesh.GetTriangles(m);
            if (i + 2 >= mts.Length)
            {
                i -= mts.Length;
                continue;
            }
            //draw the triangle we hit 
            Transform t = closest.transform;
            Debug.DrawLine(t.TransformPoint(meshFilterMesh.vertices[mts[i]]), t.TransformPoint(meshFilterMesh.vertices[mts[i + 1]]), Color.green);
            Debug.DrawLine(t.TransformPoint(meshFilterMesh.vertices[mts[i + 1]]), t.TransformPoint(meshFilterMesh.vertices[mts[i + 2]]), Color.green);
            Debug.DrawLine(t.TransformPoint(meshFilterMesh.vertices[mts[i + 2]]), t.TransformPoint(meshFilterMesh.vertices[mts[i]]), Color.green);
            var material = closest.transform.GetComponent<MeshRenderer>().materials[m];
            return (material, closest.transform);
        }
        // We should always find a material...
        Debug.LogError("No material found");
        return (null, null);
    }
}
