using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmblemRenderer : MonoBehaviour
{
    public List<Image> outlines;
    public List<Image> backing;
    public Image mainIcon;
    public Image backingIcon;

    public bool Draw(EmblemData target)
    {
        if (target.hasCreatedEmblem)
        {
            foreach (Image outline in outlines)
            {
                outline.color = target.highlightColour;
            }

            foreach (Image backing in backing)
            {
                backing.color = target.mainColour;
            }

            mainIcon.sprite = target.mainIcon;
            backingIcon.sprite = target.backingIcon;

            mainIcon.color = target.highlightColour;
            backingIcon.color = target.shadowColour;

            return true;
        }

        return false;
    }
}
