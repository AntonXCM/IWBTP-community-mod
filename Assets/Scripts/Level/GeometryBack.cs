using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryBack : MonoBehaviour
{
    [SerializeField] private float process;
    private SpriteRenderer sprite;
    private Vector2 player;
    [SerializeField] private Transform cameraFollow;
    [SerializeField] private AudioSource music;
    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        if (cameraFollow.position.x != player.x)
        {
            player = new Vector2(cameraFollow.position.x, 0);
            process = player.x;
            if (player.x < 15) sprite.color = new Color(0, 0, 1);
            else if (player.x < 6000f) sprite.color = new Color(player.x / 6000f, 0, 1);
            else if (player.x < 12000f) sprite.color = new Color(1, 0, (6000f - (player.x - 6000f)) / 6000f);
            else sprite.color = new Color((6000f - (player.x - 12000f)) / 6000f, 0, 0);
            if (music)
            {
                if (player.x > 12000)
                {
                    if (sprite.color.r > 0.1f && sprite.color.r < 0.4f) music.pitch = sprite.color.r * 2.5f;
                    else if (sprite.color.r < 0.4f) music.pitch = 0.25f;
                    else music.pitch = 1;
                }
                else music.pitch = 1;
            }
        }
    }
}
