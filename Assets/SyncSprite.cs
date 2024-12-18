using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncSprite : NetworkBehaviour
{
    public List<Sprite> sprites;

    [Networked]
    [OnChangedRender(nameof(ChangeSprite))]
    public int CurSpriteIdx { get; set; }

    public void ChangeSprite()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[CurSpriteIdx];
    }
}
