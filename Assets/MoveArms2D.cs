using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public float body_z;

    void Start()
    {
        body_z = body.transform.position.z;
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

        // figure out what object is behind the hand, if any
        List<Collider2D> hits = new List<Collider2D>();
        Physics2D.OverlapPoint(hand.transform.position, new ContactFilter2D().NoFilter(), hits);
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
            // apply the friction to the hand
            friction.maxForce = frictionAmount * frictionLevel;
            friction.connectedBody = closest.attachedRigidbody;
        }

        bool holding = Input.GetKey(key);
        float handSpeed = hand.velocity.magnitude;


        if (holding && closest != null)
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

        if (holding && closest != null && handSpeed < 0.2f)
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
}
