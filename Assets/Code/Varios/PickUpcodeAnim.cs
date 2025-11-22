using UnityEngine;
using UnityEngine.Audio;

public class PickUpcodeAnim : MonoBehaviour
{
    [SerializeField] AudioClip PickUpEffect;
    Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        anim=GetComponent<Animator>();
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        { 
            anim.SetTrigger("PickUp");
            float randomPitch = Random.Range(0.8f, 1.2f);

            AudioManager.Instance.PlaySFX(PickUpEffect, 0.2f, randomPitch);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            anim.SetTrigger("PickUp");

        }
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
