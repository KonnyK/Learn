using UnityEngine;

public class RandomPitch : MonoBehaviour {

    [SerializeField] private Vector2 Range;
    [SerializeField] private AudioSource Source;
    [SerializeField] private AudioClip[] Clips;

    void Update() {
        if (Source.isPlaying) return;
        System.Random R = new System.Random();
        Source.clip = Clips[R.Next(Clips.Length-1)];
        Source.pitch = UnityEngine.Random.value * (Range.y - Range.x) + Range.x;
        Source.Play(); 
	}
}
