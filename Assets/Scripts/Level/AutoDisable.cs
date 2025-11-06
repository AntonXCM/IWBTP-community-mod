using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDisable : MonoBehaviour
{
    void OnEnable()
    {
        Invoke(nameof(Dead), 1f);
    }
    void Dead()
    {
        gameObject.SetActive(false);
    }
}
