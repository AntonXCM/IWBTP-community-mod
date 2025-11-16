using UnityEngine;

public class ParableNarrator : MonoBehaviour
{
    private static bool completedIntro = false;

    private void Start()
    {
        if(!completedIntro)
            return;
    }
}
