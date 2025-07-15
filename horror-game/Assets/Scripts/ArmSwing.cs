using UnityEngine;

public class ArmSwing : MonoBehaviour
{
    public Transform body;                // The torso or root of your character
    public Vector3 offset;               // Local offset from body
    public float swingAmplitude = 0.2f;  // How far it swings
    public float swingSpeed = 2f;        // How fast it swings
    public bool isLeftArm = true;        // Invert one arm

    void LateUpdate()
    {
        if (body == null) return;

        float time = Time.time * swingSpeed;
        float swing = Mathf.Sin(time) * swingAmplitude;

        if (!isLeftArm) swing *= -1; // opposite for right arm

        Vector3 swingOffset = body.right * swing;
        Vector3 basePosition = body.TransformPoint(offset);

        transform.position = basePosition + swingOffset;
    }
}
