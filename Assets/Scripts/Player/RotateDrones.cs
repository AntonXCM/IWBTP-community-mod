using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateDrones : MonoBehaviour
{
    private Drones[] drones;
    private void Start()
    {
        Pick();
    }
    public void Pick()
    {
        drones = GetComponentsInChildren<Drones>();
    }
    private void Update()
    {
        if(drones.Length>0)
        {
            for(int i = 0; i < drones.Length; i++)
            {
                drones[i].GetComponent<Transform>().rotation = Quaternion.Euler(0, GetComponentInParent<PlayerController>().transform.eulerAngles.y, 0);
                transform.rotation = Quaternion.Euler(0,0, -Time.time * 360 / drones.Length);
            }
        }
    }
}
