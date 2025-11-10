using UnityEngine;
//Code contibuted by AntonXCM!
public class Hideout : MonoBehaviour, IDamageAble
{
    [SerializeField] GameObject contained;
    [SerializeField] byte hits;
    [SerializeField] bool destroyEntireObject;
    [SerializeField] Component[] componentsToDelete;
    public void TakeDamage()
    {
        if (--hits != 0) return;

        contained.SetActive(true);
        contained.transform.position = transform.position;

        Destroy(destroyEntireObject ? gameObject : this);
        foreach (var component in componentsToDelete)
            Destroy(component);
    }
}
