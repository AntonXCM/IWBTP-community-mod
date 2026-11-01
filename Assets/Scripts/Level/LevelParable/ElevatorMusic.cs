namespace LevelParable
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ElevatorMusic : MonoBehaviour
    {
        private Coroutine lyricsRoutione;
        private ParableNarrator dialogue;
        [SerializeField] MusicSettings musicSettings;
        public void Awake()
        {
            dialogue = FindAnyObjectByType<ParableNarrator>();
            ElevatorSystem.PlayerEntered += _ => StartAudio();
            ElevatorSystem.PlayerArrived += (_,_) => StopAudio();
        }
        public void StartAudio()
        {
            MusicManager.Play(musicSettings);
            lyricsRoutione = StartCoroutine(Lyrics());
        }
        public void StopAudio()
        {
            StopCoroutine(lyricsRoutione);
        }

        public IEnumerator Lyrics()
        {
            float t = 0;
            IEnumerable<(float, string)> lyrics()
            {
                yield return (4.25f, Localization.GetText("p_lyrics_1"));
                yield return (5.17f, Localization.GetText("p_lyrics_2"));
                yield return (11.35f, Localization.GetText("p_lyrics_3"));
                yield return (12.05f, Localization.GetText("p_lyrics_4"));
                yield return (13.6f, Localization.GetText("p_lyrics_5"));
                yield return (15.1f, Localization.GetText("p_lyrics_6"));
                yield return (16.65f, Localization.GetText("p_lyrics_7"));
                yield return (20.83f, Localization.GetText("p_lyrics_8"));
                yield return (38.27f, Localization.GetText("p_lyrics_9"));
                yield return (47.58f, Localization.GetText("p_lyrics_10"));
                yield return (48.51f, Localization.GetText("p_lyrics_11"));
                yield return (49f, Localization.GetText("p_lyrics_12"));
                yield return (50.54f, Localization.GetText("p_lyrics_13"));
                yield return (52.03f, Localization.GetText("p_lyrics_14"));
                yield return (53.4f, Localization.GetText("p_lyrics_15"));
                yield return (54.3f, Localization.GetText("p_lyrics_16"));
                yield return (54.89f, Localization.GetText("p_lyrics_17"));
                yield return (56.23f, Localization.GetText("p_lyrics_18"));
                yield return (57.66f, Localization.GetText("p_lyrics_19"));
                yield return (59.5f, Localization.GetText("p_lyrics_20"));
                yield return (64.2f, Localization.GetText("p_lyrics_1"));
                yield return (66.7f, Localization.GetText("p_lyrics_21"));
                yield return (60f + 33.60f, Localization.GetText("p_lyrics_22"));
                yield return (60f + 34.15f, Localization.GetText("p_lyrics_23"));
                yield return (60f + 35.15f, Localization.GetText("p_lyrics_24"));
                yield return (60f + 35.80f, Localization.GetText("p_lyrics_25"));
                yield return (60f + 37.80f, Localization.GetText("p_lyrics_26"));
                yield return (60f + 39.70f, Localization.GetText("p_lyrics_27"));
                yield return (60f + 40.00f, Localization.GetText("p_lyrics_28"));
                yield return (60f + 40.70f, Localization.GetText("p_lyrics_29"));
                yield return (60f + 42.67f, Localization.GetText("p_lyrics_30"));
                yield return (60f + 44.2f, Localization.GetText("p_lyrics_31"));
                yield return (60f + 45.6f, Localization.GetText("p_lyrics_32"));
                yield return (60f + 48.55f, Localization.GetText("p_lyrics_33"));
                yield return (60f + 51.53f, Localization.GetText("p_lyrics_34"));
                yield return (60f + 54.47f, Localization.GetText("p_lyrics_35"));
                yield return (60f + 56.07f, Localization.GetText("p_lyrics_36"));
                yield return (60f + 58f, Localization.GetText("p_lyrics_372"));
                yield return (60f + 59f, Localization.GetText("p_lyrics_373"));
                yield return (60f + 60f, Localization.GetText("p_lyrics_374"));
                for(int i = 1; i < 18; i++)
                {
                    string iniy = "Іній";
                    for(int j = i; j > 0; j--) iniy += "-іній";
                    yield return (60f + 57f + i * 0.5f, iniy);
                }
                yield return (120f + 05f, Localization.GetText("p_lyrics_38"));
                yield return (120f + 07f, Localization.GetText("p_lyrics_39"));
                yield return (120f + 09f, Localization.GetText("p_lyrics_40"));
                yield return (120f + 10f, Localization.GetText("p_lyrics_41"));
            }
            while (true)
            {
                foreach ((float time, string lyric) in lyrics())
                {
                    while (t < time)
                    {
                        yield return null;
                        t += Time.unscaledDeltaTime;
                    }
                    SetText(lyric);
                }
                while (t < 136.8f)
                {
                    yield return null;
                    t += Time.unscaledDeltaTime;
                }
                t = 0;
            }

        }
        public void SetText(string key)
        {
            dialogue.Subtitles.text = key;
        }
    }
}