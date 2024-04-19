using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class LightMovement : MonoBehaviour
{
    public bool isSpin;
    public bool isMoving;
    public float speed = 50.0f; // Speed of rotation
    public float movementDistance = 1.0f; // Distance the lights will move up and down
    public float movementSpeed = 1.0f; // Speed of movement
    public float upperLimit = 2.0f; // Upper limit of movement
    public float lowerLimit = 0.0f; // Lower limit of movement

    private Vector3 initialPosition; // Initial position of the light
    private bool movingUp = true; // Flag to track the direction of movement

    void Start()
    {
        initialPosition = transform.position; // Sets initial position
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpin)
        {
            // Get the current rotation of the object
            Vector3 currentRotation = transform.rotation.eulerAngles;

            // Set the rotation around the z-axis to continuously increase
            float newRotationZ = currentRotation.z + speed * Time.deltaTime;

            // Apply the new rotation while keeping x and y position constant
            transform.rotation = Quaternion.Euler(0, 0, newRotationZ);
        }

        if (isMoving)
        {
            float yOffset = Mathf.Sin(Time.time * movementSpeed) * movementDistance;

            // Check if the lights have reached upper or lower limit
            if ((transform.position.y >= initialPosition.y + upperLimit && movingUp) ||
                (transform.position.y <= initialPosition.y - lowerLimit && !movingUp))
            {
                // Change the direction of movement
                movingUp = !movingUp;
            }

            // Set the direction of movement based on the flag
            float direction = movingUp ? 1 : -1;

            // Set the new position with movement limits
            Vector3 newPosition = initialPosition + Vector3.up * direction * Mathf.Clamp(yOffset, -movementDistance, movementDistance);

            // Apply the new position while keeping x and z position constant
            transform.position = new Vector3(initialPosition.x, Mathf.Clamp(newPosition.y, initialPosition.y - lowerLimit, initialPosition.y + upperLimit), initialPosition.z);
        }
    }
}



