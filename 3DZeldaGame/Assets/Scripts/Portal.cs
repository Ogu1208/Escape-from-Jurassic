using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject.Find("GameManager").GetComponent<GameManager>().ClearPopup.SetActive(true);
            Time.timeScale = 0;
        }
    }
}
