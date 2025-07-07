using UnityEngine;

public class BackGroundRoller : MonoBehaviour
{
    public float multi = 0.1f;
    public Material mat;
    void Awake()
    {
        mat = GetComponent<SpriteRenderer>().material;
    }
    // Update is called once per frame
    void Update()
    {
        mat.mainTextureOffset += Time.deltaTime * (Vector2.right + Vector2.up) * multi;
    }
}
