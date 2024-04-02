using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveArms : MonoBehaviour
{

    public Rigidbody leftArm;
    public Rigidbody rightArm;
    public Rigidbody body;

    public Rigidbody leftHand;
    public Rigidbody rightHand;

    private Vector3 leftHandPosition;
    private Vector3 rightHandPosition;


    public float force = -8.0f;
    public float gripMultiplier = 0.5f;

    public float body_z;

    // Start is called before the first frame update
    void Start()
    {
        body_z = body.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        // Apply a force to each arm in the direction the mouse moves
        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10.0f));

        Vector3 leftArmDirection = leftArm.transform.position - screenPos;
        Vector3 rightArmDirection = rightArm.transform.position - screenPos;
        leftArmDirection.Normalize();
        rightArmDirection.Normalize();

        //when 'a' is pressed fix the left hand position
        HandLogic(leftHand, leftArmDirection, force, KeyCode.A);
        HandLogic(rightHand, rightArmDirection, force, KeyCode.D);

        // Lock rotation of the body aprat from the z axis
        body.transform.rotation = Quaternion.Euler(0, 0, body.transform.rotation.eulerAngles.z);
        //lock the z positions of the body
        body.transform.position = new Vector3(body.transform.position.x, body.transform.position.y, body_z);

        // set camera y to the body y
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, body.transform.position.y, Camera.main.transform.position.z);
    }


    public void HandLogic(Rigidbody hand, Vector3 handDirection, float handForce, KeyCode key)
    {


        bool holding = Input.GetKey(key);
        if (holding)
        {
            body.AddForce(handDirection * handForce * gripMultiplier);
            hand.constraints = RigidbodyConstraints.FreezePosition;
        }
        else
        {
            hand.AddForce(handDirection * handForce);
            body.AddForce(-handDirection * handForce);
            hand.constraints = RigidbodyConstraints.None;

        }
    }
}
