using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    AudioSource audioSource;

    [SerializeField]
    AudioClip[] soundTracks;
    int currentSongIndex;
    

	// Use this for initialization
	void Start ()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayNextSong());
    }

    /// <summary>
    /// Plays a random song or a specified song based on
    /// the value passed in.
    /// </summary>
    private IEnumerator PlayNextSong()
    {
        audioSource.Stop();

        int prevIndex = currentSongIndex;
        
        do
        {
            currentSongIndex = Random.Range(0, soundTracks.Length);
            yield return null;
        }
        while (currentSongIndex == prevIndex);

        audioSource.clip = soundTracks[currentSongIndex];
        audioSource.Play();

        WaitForSong(audioSource.clip.length);
    }

    private IEnumerator WaitForSong(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        PlayNextSong();
    }
	
}
