using UnityEngine;
//Code contibuted by AntonXCM!
public class Platform : MonoBehaviour
{
    [SerializeField] private Transform player, relativeTransform;
    private KeyCode pass1, pass2;
    private bool GetPlayerAbove => relativeTransform.worldToLocalMatrix.MultiplyPoint(player.position).x > 0 && !(Input.GetKey(pass1) && Input.GetKey(pass2));
    private bool wasPlayerAbove;
    private void Start()
    {
        wasPlayerAbove = GetPlayerAbove;
        SwitchColliders(wasPlayerAbove);
        pass1 = (KeyCode)PlayerPrefs.GetInt("Key0");
        pass2 = (KeyCode)PlayerPrefs.GetInt("Key1");
    }
    private void FixedUpdate()
    {
        bool isPlayerAbove = GetPlayerAbove;
        if (isPlayerAbove == wasPlayerAbove) return;
        wasPlayerAbove = isPlayerAbove;
        SwitchColliders(isPlayerAbove);
    }
    [SerializeField] private Collider2D[] colliders;
    private void SwitchColliders(bool enable) //TODO: Extract colliders and this func to new parent 
    {
        foreach (var collider in colliders)
            collider.enabled = enable;
    }
}
