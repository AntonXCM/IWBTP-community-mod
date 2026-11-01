using UnityEngine;
namespace LevelParable {
/// <summary>
/// Включает музыку при входе в триггер или при столкновении (3D и 2D).
/// Повесьте на объект с Collider / Collider2D.
/// </summary>
public class MusicTrigger : MonoBehaviour
{
    [Header("Музыка")]
    [SerializeField] private MusicSettings music = new MusicSettings();

    [Header("Условия срабатывания")]
    [Tooltip("Тег объекта, который включает музыку. Пустая строка — любой объект.")]
    [SerializeField] private string requiredTag = "Player";

    [Tooltip("Сработать только один раз.")]
    [SerializeField] private bool triggerOnce = false;

    private bool used;

    // ---- 3D ----
    private void OnTriggerEnter(Collider other)
    {
        TryActivate(other.gameObject,
            other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : null);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;
        TryActivate(other.gameObject,
            other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : null);
    }

    // ---- 2D ----
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryActivate(other.gameObject,
            other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : null);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;
        TryActivate(other.gameObject,
            other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : null);
    }

    private void TryActivate(GameObject hit, GameObject hitRoot)
    {
        if (triggerOnce && used) return;

        // Тег проверяем и на самом коллайдере, и на объекте с Rigidbody (если коллайдер — дочерний)
        if (!string.IsNullOrEmpty(requiredTag))
        {
            bool match = hit.CompareTag(requiredTag) ||
                         (hitRoot != null && hitRoot.CompareTag(requiredTag));
            if (!match) return;
        }

        used = true;
        MusicManager.Play(music);
    }
}
}