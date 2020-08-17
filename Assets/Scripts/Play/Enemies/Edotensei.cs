using UnityEngine;

public class Edotensei : MonoBehaviour
{
    private float lifetime = 0.0f; 

    // Update is called once per frame
    void FixedUpdate()
    {
        lifetime += Time.deltaTime;
        transform.localScale = new Vector3(5.1f * lifetime, 5.1f * lifetime, 1.0f);
        if (transform.localScale.x >= 2.6f) Destroy(gameObject);
    }
}
