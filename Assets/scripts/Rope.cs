using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    // Start is called before the first frame update
    public LineRenderer lineRenderer;
    public Transform target;
    public float z_offset = 0.0f;

    public DistanceJoint2D joint;

    public float tilingAmount = 1.0f;
    public float clipDistance = 0.3f;

    public List<GameObject> anchors;

    public List<GameObject> unclippedAnchors;

    void OnEnable()
    {
        lineRenderer = GetComponent<LineRenderer>();
        joint = GetComponent<DistanceJoint2D>();
        // Find all 'Checkpoint' objects in the scene
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("Checkpoint");
        // Add them to the list of unclipped anchors
        foreach (GameObject checkpoint in checkpointObjects)
        {
            unclippedAnchors.Add(checkpoint);
        }
        //sort the anchors by name
        unclippedAnchors.Sort((x, y) => x.name.CompareTo(y.name));
        transform.position = anchors[anchors.Count - 1].transform.position;

    }

    void Update()
    {
        SetLinePositions();
        CheckNewAnchors();
    }

    void FixedUpdate()
    {
        joint.enabled = target.transform.position.y < transform.position.y;
    }

    private void CheckNewAnchors()
    {
        foreach (GameObject anchor in unclippedAnchors)
        {
            var distance = Vector2.Distance(anchor.transform.position, target.transform.position);
            if (distance < clipDistance)
            {
                anchors.Add(anchor);
                // remove all previous anchors before this one
                unclippedAnchors.RemoveRange(0, unclippedAnchors.IndexOf(anchor) + 1);
                transform.position = anchor.transform.position;
                break;
            }
        }
    }

    public void SetLinePositions()
    {
        lineRenderer.positionCount = anchors.Count + 1;
        float totalDistance = 0;
        for (int i = 0; i < anchors.Count; i++)
        {
            Vector3 position = anchors[i].transform.position;
            // position.z = z_offset;
            lineRenderer.SetPosition(i, position);
            if (i > 0)
            {
                totalDistance += Vector3.Distance(anchors[i].transform.position, anchors[i - 1].transform.position);
            }
        }
        float distance = Vector3.Distance(anchors[anchors.Count - 1].transform.position, target.transform.position);
        totalDistance += distance;
        Vector3 targetPosition = target.transform.position;
        targetPosition.z = z_offset;
        lineRenderer.SetPosition(anchors.Count, targetPosition);

        lineRenderer.material.mainTextureScale = new Vector2(totalDistance * tilingAmount, 1);

    }
}
