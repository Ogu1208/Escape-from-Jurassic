using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public ParticleSystem fxHit;
    private bool isCut;
    

    private void GetHit(int amount)
    {
        if(!isCut)
        {
            isCut = true;
            transform.localScale = new Vector3(2f, 2f, 2f);
            fxHit.Emit(10);
        }
    }
}
