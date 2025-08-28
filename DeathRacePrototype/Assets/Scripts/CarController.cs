using UnityEngine;
using System;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    /*source https://youtu.be/jr4eb4F9PSQ?si=7j5tSQQ-YjDYH4E8 */
    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public List<Wheel> wheels;

    float moveInput;
}
