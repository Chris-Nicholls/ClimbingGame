using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFeet : MonoBehaviour
{
    public Rigidbody2D rightFoot;
    public Rigidbody2D leftFoot;

    public Rigidbody2D body;

    public float footForce = 8.0f;

    public float frictionLevel = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {


        FootLogic(leftFoot, KeyCode.D, new Vector2(0.5f, -1.5f));
        FootLogic(rightFoot, KeyCode.A, new Vector2(-0.5f, -1.5f));
    }

    private void FootLogic(Rigidbody2D foot, KeyCode key, Vector2 offset)
    {

        if (!this.enabled)
        {
            return;
        }
        FrictionJoint2D friction = foot.transform.GetComponent<FrictionJoint2D>();

        Vector2 footDirection = body.transform.TransformPoint(offset) - foot.transform.position;
        if (!Input.GetKey(key))
        {
            foot.AddForce(footDirection * footForce);
            friction.enabled = false;
        }
        else
        {

            List<Collider2D> hits = new List<Collider2D>();
            Physics2D.OverlapPoint(foot.transform.position, new ContactFilter2D().NoFilter(), hits);
            Collider2D closest = null;
            float closestDistance = -float.MaxValue;
            // pick object with mesh extents largetst in z direction
            foreach (var hit in hits)
            {
                var mesh = hit.transform.GetComponent<MeshFilter>();
                if (mesh != null)
                {
                    var extents = mesh.mesh.bounds.extents + hit.transform.position;
                    if (extents.z > closestDistance)
                    {
                        closest = hit;
                        closestDistance = extents.z;
                    }
                }
            }

            if (closest != null && closest.sharedMaterial != null && closest.attachedRigidbody != null && closest.attachedRigidbody != friction.connectedBody)
            {
                // get the friciton of the material
                var frictionAmount = closest.sharedMaterial.friction;
                friction.maxForce = frictionAmount * frictionLevel;
                friction.connectedBody = closest.attachedRigidbody;
            }
            if (closest != null)
            {
                friction.enabled = true;

            }
        }
    }

}
