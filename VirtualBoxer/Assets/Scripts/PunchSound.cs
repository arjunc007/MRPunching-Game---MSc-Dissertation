using UnityEngine;

public class PunchSound : MonoBehaviour {

    private AudioSource audioSource;

    public AudioClip[] audioClips;

	// Use this for initialization
	void Start () {
        audioSource = GetComponent<AudioSource>();	
	}

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.root != transform.root)
        {
            int index = Random.Range(0, audioClips.Length);
            audioSource.PlayOneShot(audioClips[index]);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
