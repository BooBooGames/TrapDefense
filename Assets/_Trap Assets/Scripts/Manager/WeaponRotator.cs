using UnityEngine;

public class WeaponRotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] [Min(0f)] private float rotationSpeed = 90f;
    [SerializeField] private Space rotationSpace = Space.Self;

    public void SetRotationSpeed(float newRotationSpeed)
    {
        rotationSpeed = Mathf.Max(0f, newRotationSpeed);
    }

    private void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, rotationSpace);
    }
}
