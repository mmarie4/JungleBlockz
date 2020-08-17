using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RotatingBlockz : MonoBehaviour
{
    public bool entered;
    public bool rotating;
    public float rotationSpeed;
    public float radius;

    private List<float> childrenAngles;

    void Start()
    {
        entered = false;
        rotating = true;

        childrenAngles = new List<float> { 0, 0.78539816f, 1.57079633f, 2.35619449f, 3.14159265f, 3.92699082f, 4.71238898f, 5.49778715f };

        rotationSpeed = 0.3f;
    }

    void FixedUpdate()
    {
        if (rotating)
        {
            int i = 0;
            foreach (Transform child in transform)
            {
                float posX = transform.position.x + (radius * Mathf.Cos(childrenAngles[i]));
                float posY = transform.position.y + (radius * Mathf.Sin(childrenAngles[i]));
                child.transform.position = new Vector2(posX, posY);
                i++;
            }

            childrenAngles = childrenAngles.Select(a => a >= 6.28318531f ? (a - 6.28318531f) + Time.deltaTime * rotationSpeed : a + Time.deltaTime * rotationSpeed).ToList();
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) entered = true;
    }
}

