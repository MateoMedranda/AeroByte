using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem; // Obligatorio para Keyboard.current

namespace AeroByte.FlightSystem
{
    [RequireComponent(typeof(Rigidbody))]
    public class FlightController : MonoBehaviour
    {
        [Header("Motor y Velocidad")]
        public float speed = 20;
        public float maxSpeed = 100;
        public float minSpeed = 5;

        public float rootSpeed1 = 50;
        public float rootSpeed2 = 50;
        
        void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;

            // Seguridad: Comprobamos si hay un teclado conectado para evitar errores (NullReference)
            if (Keyboard.current == null) return;

            // Reemplazo de Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                transform.Rotate(Vector3.forward * rootSpeed1 * Time.deltaTime);
            }

            // Reemplazo de Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                transform.Rotate(Vector3.back * rootSpeed2 * Time.deltaTime);
            }

            // Reemplazo de Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            {
                transform.Rotate(Vector3.left * rootSpeed1 * Time.deltaTime);
            }

            // Reemplazo de Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            {
                transform.Rotate(Vector3.right * rootSpeed1 * Time.deltaTime);
            }
        }
    }
}