using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
enum Mode
{
    penguin, plane, ball, wave, gravity
}
public class GeometryPortal : MonoBehaviour
{
    [SerializeField] private bool altGravity;
    [SerializeField] private Mode mode;
    [SerializeField] private GameObject modePlayer;
    [SerializeField] private CameraFollow cam;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if(player != null)
        {
            if(mode == Mode.gravity && altGravity && player.GetComponent<Rigidbody2D>().gravityScale == 25)
            {
                player.GetComponent<Rigidbody2D>().gravityScale = -25;
                player.GetComponent<Transform>().rotation = Quaternion.Euler(180, player.GetComponent<Transform>().rotation.y, 0);
            }
            if(mode == Mode.gravity && altGravity == false && player.GetComponent<Rigidbody2D>().gravityScale == -25)
            {
                player.GetComponent<Rigidbody2D>().gravityScale = 25;
                player.GetComponent<Transform>().rotation = Quaternion.Euler(0, player.GetComponent<Transform>().rotation.y, 0);
            }
            if(mode == Mode.plane)
            {
                modePlayer.GetComponent<Transform>().position = player.GetComponent<Transform>().position;
                player.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                modePlayer.GetComponent<PlayerShip>().altGravity = altGravity;
                modePlayer.SetActive(true);
            }
            if (mode == Mode.ball)
            {
                modePlayer.GetComponent<Transform>().position = player.GetComponent<Transform>().position;
                player.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                modePlayer.GetComponent<PlayerBall>().altGravity = altGravity;
                modePlayer.SetActive(true);
            }
            if (mode == Mode.wave)
            {
                modePlayer.GetComponent<Transform>().position = player.GetComponent<Transform>().position;
                player.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                modePlayer.GetComponent<PlayerWave>().altGravity = altGravity;
                modePlayer.SetActive(true);
            }
        }
        PlayerShip playerShip = collision.GetComponent<PlayerShip>();
        if (playerShip != null)
        {
            if (mode == Mode.gravity && altGravity && playerShip.altGravity == false)
            {
                playerShip.altGravity = true;
            }
            if (mode == Mode.gravity && altGravity == false && playerShip.altGravity)
            {
                playerShip.altGravity = false;
            }
            if (mode == Mode.penguin)
            {
                modePlayer.GetComponent<Transform>().position = playerShip.GetComponent<Transform>().position;
                playerShip.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                if(altGravity == false)
                {
                    modePlayer.GetComponent<Rigidbody2D>().gravityScale = 25;
                    modePlayer.GetComponent<Transform>().rotation = Quaternion.Euler(0, modePlayer.GetComponent<Transform>().rotation.y, 0);
                }
                else
                {
                    modePlayer.GetComponent<Rigidbody2D>().gravityScale = -25;
                    modePlayer.GetComponent<Transform>().rotation = Quaternion.Euler(180, modePlayer.GetComponent<Transform>().rotation.y, 0);
                }
                modePlayer.SetActive(true);
            }
            if (mode == Mode.ball)
            {
                modePlayer.GetComponent<Transform>().position = playerShip.GetComponent<Transform>().position;
                playerShip.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                modePlayer.GetComponent<PlayerBall>().altGravity = altGravity;
                modePlayer.SetActive(true);
            }
            if (mode == Mode.wave)
            {
                modePlayer.GetComponent<Transform>().position = playerShip.GetComponent<Transform>().position;
                playerShip.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                modePlayer.GetComponent<PlayerWave>().altGravity = altGravity;
                modePlayer.SetActive(true);
            }
        }
        PlayerBall playerBall = collision.GetComponent<PlayerBall>();
        if (playerBall != null)
        {
            if (mode == Mode.gravity && altGravity && playerBall.altGravity == false)
            {
                playerBall.altGravity = true;
            }
            if (mode == Mode.gravity && altGravity == false && playerBall.altGravity)
            {
                playerBall.altGravity = false;
            }
            if (mode == Mode.penguin)
            {
                modePlayer.GetComponent<Transform>().position = playerBall.GetComponent<Transform>().position;
                playerBall.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                if (altGravity == false)
                {
                    modePlayer.GetComponent<Rigidbody2D>().gravityScale = 25;
                    modePlayer.GetComponent<Transform>().rotation = Quaternion.Euler(0, modePlayer.GetComponent<Transform>().rotation.y, 0);
                }
                else
                {
                    modePlayer.GetComponent<Rigidbody2D>().gravityScale = -25;
                    modePlayer.GetComponent<Transform>().rotation = Quaternion.Euler(180, modePlayer.GetComponent<Transform>().rotation.y, 0);
                }
                modePlayer.SetActive(true);
            }
            if (mode == Mode.plane)
            {
                modePlayer.GetComponent<Transform>().position = playerBall.GetComponent<Transform>().position;
                playerBall.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                modePlayer.GetComponent<PlayerShip>().altGravity = altGravity;
                modePlayer.SetActive(true);
            }
            if (mode == Mode.wave)
            {
                modePlayer.GetComponent<Transform>().position = playerBall.GetComponent<Transform>().position;
                playerBall.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                modePlayer.GetComponent<PlayerWave>().altGravity = altGravity;
                modePlayer.SetActive(true);
            }
        }
        PlayerWave playerWave = collision.GetComponent<PlayerWave>();
        if (playerWave != null)
        {
            if (mode == Mode.gravity && altGravity && playerShip.altGravity == false)
            {
                playerWave.altGravity = true;
            }
            if (mode == Mode.gravity && altGravity == false && playerWave.altGravity)
            {
                playerWave.altGravity = false;
            }
            if (mode == Mode.penguin)
            {
                modePlayer.GetComponent<Transform>().position = playerWave.GetComponent<Transform>().position;
                playerWave.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                if (altGravity == false)
                {
                    modePlayer.GetComponent<Rigidbody2D>().gravityScale = 25;
                    modePlayer.GetComponent<Transform>().rotation = Quaternion.Euler(0, modePlayer.GetComponent<Transform>().rotation.y, 0);
                }
                else
                {
                    modePlayer.GetComponent<Rigidbody2D>().gravityScale = -25;
                    modePlayer.GetComponent<Transform>().rotation = Quaternion.Euler(180, modePlayer.GetComponent<Transform>().rotation.y, 0);
                }
                modePlayer.SetActive(true);
            }
            if (mode == Mode.plane)
            {
                modePlayer.GetComponent<Transform>().position = playerWave.GetComponent<Transform>().position;
                playerWave.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                modePlayer.GetComponent<PlayerShip>().altGravity = altGravity;
                modePlayer.SetActive(true);
            }
            if (mode == Mode.ball)
            {
                modePlayer.GetComponent<Transform>().position = playerWave.GetComponent<Transform>().position;
                playerWave.gameObject.SetActive(false);
                cam.player = modePlayer.transform;
                modePlayer.GetComponent<PlayerBall>().altGravity = altGravity;
                modePlayer.SetActive(true);
            }
        }
    }
}
