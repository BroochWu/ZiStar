using System.Collections;
using UnityEngine;

public class BackBattleUI : MonoBehaviour
{
    public GameObject grilleMask;
    Vector3 initScale = new Vector3(400, 400, 400);
    void Awake()
    {
        grilleMask.transform.localScale = initScale;
    }
    void Start()
    {
        StartCoroutine(ScaleMask());
    }

    IEnumerator ScaleMask()
    {
        while (grilleMask.transform.localScale.x > 0.5f)
        {
            grilleMask.transform.localScale -= Vector3.one * Time.deltaTime * 300f;
            yield return null;
        }
    }
}
