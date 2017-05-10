using UnityEngine;


// Based on UnityStandardAssets.Utility.SmoothFollow
public class FollowTarget : MonoBehaviour
{
    // The target we are following
    public Transform target;

    // The distance in the x-z plane to the target
    [SerializeField] private float distance = 10.0f;

    // the height we want the camera to be above the target
    [SerializeField]
    //private float height = 5.0f;
    [Range(-45f, 45f)] private float angle = 40.0f;

    [SerializeField] private float rotationDamping;

    [SerializeField]
    //private float heightDamping;
    private float pitchDamping;

    // Update is called once per frame
    private void LateUpdate()
    {
        // Early out if we don't have a target
        if (!target)
            return;

        // Calculate the current rotation angles
        var wantedYaw = target.eulerAngles.y;
        //var wantedHeight = target.position.y + height;
        var wantedPitch = angle;

        var currentRotationAngle = transform.eulerAngles.y;
        //var currentHeight = transform.position.y;
        var currentPitch = transform.eulerAngles.x;

        // Damp the rotation around the y-axis
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedYaw, rotationDamping * Time.deltaTime);

        // Damp the height
        //currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        // Damp the rotation around the x-axis
        currentPitch = Mathf.LerpAngle(currentPitch, wantedPitch, pitchDamping * Time.deltaTime);

        // Convert the angle into a rotation
        //var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
        var currentRotation = Quaternion.Euler(currentPitch, currentRotationAngle, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        transform.position = target.position;
        transform.position -= currentRotation * Vector3.forward * distance;

        // Set the height of the camera
        //transform.position = new Vector3(transform.position.x ,currentHeight , transform.position.z);

        // Always look at the target
        transform.LookAt(target);
    }
}