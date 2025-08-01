using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackBattleUI : MonoBehaviour
{

    // private static BackBattleUI _instance;
    // public static BackBattleUI Instance => _instance ??= new BackBattleUI();

    public Animator grilleAnim;
    public GameObject grille;
    public GameObject grilleMask;

    Coroutine grilleHit;



    /// <summary>
    /// 外部调用-受击变红
    /// </summary>
    public void GrilleHit()
    {
        // if (grilleHit != null) StopCoroutine(grilleHit);
        // grilleHit = StartCoroutine(CorGrilleHit());
        grilleAnim.Play("GrilleHit");
    }

    /// <summary>
    /// 受击变红
    /// </summary>
    /// <returns></returns>
    IEnumerator CorGrilleHit()
    {
        var elapsedTime = 0f;
        while (elapsedTime <= 0.2f)
        {
            elapsedTime += Time.deltaTime;
            grille.GetComponent<RawImage>().color = Color.Lerp(Color.white, Color.red, 0.2f);
            yield return null;
        }
        elapsedTime = 0;
        while (elapsedTime <= 0.2f)
        {
            elapsedTime += Time.deltaTime;
            grille.GetComponent<RawImage>().color = Color.Lerp(Color.red, Color.white, 0.2f);
            yield return null;
        }
    }



}
