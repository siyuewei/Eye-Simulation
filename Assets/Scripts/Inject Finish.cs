using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InjectFinish : MonoBehaviour
{
    public GameObject syringe;
    public GameObject injectionTubing;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            syringe.SetActive(false);
            injectionTubing.SetActive(false);
        }
    }

}
