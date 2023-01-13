using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place this script on weapon prefabs to handle muzzle position, sprite flipping, etc
/// </summary>
public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Transform m_muzzle;
    public ParticleSystem muzzleFlash { get; private set; }
    private Animator anim;
    public Transform Muzzle => m_muzzle;

    private Vector3 muzzlePos;
    private Vector3 flippedMuzzlePos;

    private void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        muzzleFlash = GetComponentInChildren<ParticleSystem>();
        anim = GetComponentInChildren<Animator>();

        muzzlePos = m_muzzle.transform.localPosition;
        flippedMuzzlePos = muzzlePos;
        flippedMuzzlePos.y *= -1;
    }

    public virtual void FlipSprite(bool flip)
    {
        sprite.flipY = flip;

        if (flip) m_muzzle.transform.localPosition = flippedMuzzlePos;
        else m_muzzle.transform.localPosition = muzzlePos;
    }

    public void PlayEffects()
    {
        //For guns, muzzle flash
        if (muzzleFlash != null) muzzleFlash.Play();
        else if (anim != null) anim.Play("slash");
    }
}
