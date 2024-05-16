using UnityEngine;
using UnityEngine.UIElements;

public class MoveToDestination : MonoBehaviour
{
    public Transform destination; // Set this in the Inspector

    public float movementSpeed = 5.0f; // Adjust the speed as needed
    public bool loopback = true;

    Vector3 origin;
    Vector3 target;

    private void Start()
    {
        origin = transform.position;
        target = destination.position;
    }

    private void Update()
    {
        // Calculate the direction from the current position to the destination
        Vector3 direction = target - transform.position;

        // Normalize the direction to ensure consistent speed
        direction.Normalize();

        // Move towards the destination
        transform.position += direction * movementSpeed * Time.deltaTime;

        // Optionally, you can also rotate the object to face the destination
        Quaternion targetRotation = Quaternion.LookRotation(target);
        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);

        // Calculate the distance between the object and the specified position
        if (Vector3.Distance(transform.position, target) < 0.01f && loopback)
        {
            target = (target == destination.position) ? origin : destination.position;
            transform.Rotate(new Vector3(0, 180, 0));
            Debug.Log($"Moving {this.name} to {target}");
        }
    }
}
