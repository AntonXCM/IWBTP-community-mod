using UnityEngine;
//Code contibuted by AntonXCM!
public class Hideout : MonoBehaviour, IDamageAble
{
    [SerializeField] GameObject contained;
    [SerializeField] int hits;
    [SerializeField] bool destroyEntireObject, fall;
    [SerializeField] Component[] componentsToDelete;
    public void TakeDamage(int damage = 1)
    {
        if(fall) { StartCoroutine(GetComponent<FallingLamp>().FallDown()); fall = false; }
        hits -= damage;
        if (hits > 0) return;

        if(contained)
        {
            contained.SetActive(true);
            contained.transform.position = transform.position;
        }

        Destroy(destroyEntireObject ? gameObject : this);
        foreach (var component in componentsToDelete)
            Destroy(component);
    }
}
