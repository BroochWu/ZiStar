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
        if (GameManager.Instance.gameState == GameManager.GameState.BATTLE)
        {
            mat.mainTextureScale = Vector2.Lerp(mat.mainTextureScale, 30 * (Vector2.right + Vector2.up), 0.05f);
        }
        else
        {
            mat.mainTextureScale = Vector2.Lerp(mat.mainTextureScale, 12 * (Vector2.right + Vector2.up), 0.05f);
        }
    }
}
