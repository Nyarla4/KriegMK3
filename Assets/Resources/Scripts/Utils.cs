using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PRS
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;

    public PRS(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        this.pos = pos;
        this.rot = rot;
        this.scale = scale;
    }
}

public class Utils
{
    public static Quaternion QI => Quaternion.identity;
    public static Vector3 MousePos
    {
        get
        {
            Vector3 result = Camera.main.ScreenToViewportPoint(Input.mousePosition);//ScreenToWorldPoint(Input.mousePosition);
            result.x = 19.5f * result.x - 9.75f;
            result.y = 11 * result.y - 5.5f;
            result.z = 0;
            return result;
        }
    }

    public static WaitForSeconds delay1 = new WaitForSeconds(1);

    public static WaitForSeconds delay2 = new WaitForSeconds(2);

    public static WaitForSeconds delay05 = new WaitForSeconds(0.5f);

    public static WaitForSeconds delay07 = new WaitForSeconds(0.7f);
}
