using UnityEngine;

public class FollowCharacter : MonoBehaviour
{
    public Transform character; // Assign your character's Transform in the Inspector
    public float yDelta = 2f;
    public float zDelta = -8f;
    void Update()
    {
        if (character != null)
        {
            // Calculate the desired position (offset from the character)
            Vector3 desiredPosition = character.position + new Vector3(0f, yDelta, zDelta);

            // Smoothly move the camera towards the desired position
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 5f);

            // Look at the character
            transform.LookAt(character);
        }
    }
}
