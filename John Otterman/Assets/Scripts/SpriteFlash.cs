using System.Collections;
using UnityEngine;

public class SpriteFlash
{
    private SpriteRenderer sprite;
    private Material defaultMat;
    private Material flashMat;

    public float duration = 0.1f;
    public int numFlashes = 4;

    public SpriteFlash(SpriteRenderer renderder)
    {
        sprite = renderder;
        defaultMat = sprite.material;

        flashMat = GameManager.instance.spriteFlashMat;
    }

    public void OnStart()
    {
        if (sprite == null) return;
        sprite.material = defaultMat;
    }

    public IEnumerator Flash()
    {
        bool flash = true;
        int x = numFlashes;

        while (x > 0)
        {
            if (flash) sprite.material = flashMat;
            else sprite.material = defaultMat;
            yield return new WaitForSeconds(duration);
            flash = !flash;
            x--;
            yield return null;
        }

        sprite.material = defaultMat;
    }
}
