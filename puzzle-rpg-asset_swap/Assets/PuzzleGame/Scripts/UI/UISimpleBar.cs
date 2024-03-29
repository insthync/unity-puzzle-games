using UnityEngine;
using System.Collections;

public class UISimpleBar : UIAbstractBar {
    public GameObject bar;
    protected Vector3 defaultBarScale;
    void Awake()
    {
        defaultBarScale = bar.transform.localScale;
    }

    public override void SetRate(float rate)
    {
		if (rate >= 0)
		{
        	bar.transform.localScale = new Vector3(rate * defaultBarScale.x, defaultBarScale.y, defaultBarScale.z);
		}
    }
}
