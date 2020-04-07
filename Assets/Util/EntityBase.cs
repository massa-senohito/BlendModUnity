//using Optional;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unit =
    //UnityEngine.GameObject;
    EntityBase;
using Gobj = UnityEngine.GameObject;
using V3 = UnityEngine.Vector3;
public class EntityBase : MonoBehaviour
{
    //public abstract void OnJustClick( HandBoneController controller);
    // マウスクリック反応を実装するなら
    public virtual void OnJustClick ( ) { }

    public static string __Function ( )
    {
#if DEBUG
        var stackTrace = new System.Diagnostics.StackTrace ( );
        return stackTrace.GetFrame (1).GetMethod ( ).Name;
#else
        return "";
#endif
    }
    public static Unit SpawnCube (V3 pos)
    {
        var obj = Gobj.CreatePrimitive (PrimitiveType.Cube);
        obj.transform.position = pos;
        var p = obj.transform.position;
        //obj.transform.position = new V3( p.x , p.y , CameraOffset);
        obj.transform.localScale = new V3 (0.1f, 0.1f, 0.1f);
        var unit = obj.AddComponent<Unit> ( );
        return unit;
    }

    //public void SetTransparent()
    //{

    //    var mayMat = GetMaterial( );
    //    var trans = Shader.Find("UI/Lit/Transparent");
    //    mayMat.Match( m => m.shader = trans , () => Debug.Log("shader not found"));
    //}

    //public Option<Material> GetMaterial()
    //{
    //    var renderer = GetComponent<MeshRenderer>( );
    //    if ( renderer )
    //    {

    //        return renderer.material.Some();
    //    }
    //    var skinRend = GetComponent<SkinnedMeshRenderer>();
    //    if(skinRend)
    //    {
    //        return skinRend.material.Some();
    //    }
    //    return Option.None<Material>( );
    //}

    public void SetColor (Color color)
    {
        var renderer = GetComponent<MeshRenderer> ( );
        if (renderer)
        {
            renderer.material.color = color;
            return;
        }
        var skinRend = GetComponent<SkinnedMeshRenderer> ( );
        if (skinRend)
        {
            skinRend.material.color = color;
        }
    }

    public static EntityBase SetEntity (GameObject gobj)
    {
        var ent = gobj.AddComponent<EntityBase> ( );
        return ent;
    }

    public void Show ( )
    {
        SetActive (true);
    }

    public void Hide ( )
    {
        SetActive (false);
    }

    public void SetActive (bool isActive)
    {
        gameObject.SetActive (isActive);
    }

    public void SetParent (EntityBase parent, bool stay)
    {
        transform.SetParent (parent.transform, stay);
    }

    public Vector3 GetPos ( )
    {
        return transform.position;
    }

    public Quaternion GetRot ( )
    {
        return transform.rotation;
    }

    public Vector3 GetScale ( )
    {
        return transform.localScale;
    }

    public float Distance(GameObject obj)
    {
        return V3.Distance( GetPos( ) , obj.transform.position );
    }
    public float Distance(V3 pos)
    {
        return V3.Distance( GetPos( ) , pos );
    }
    public Vector3 MouseInWorld( )
    {
        var mouse = Input.mousePosition;
        float camDist = Distance( Camera.main.gameObject );

        var zMouse =
                new Vector3(mouse.x , mouse.y , camDist);
        var world = Camera.main.ScreenToWorldPoint(zMouse);
        return world;
    }

    public void SetPos (Vector3 pos)
    {
        transform.position = pos;
    }

    public Matrix4x4 ToWorld
    {
        get
        {
            return transform.localToWorldMatrix;
        }
    }
    public Matrix4x4 ToInverse
    {
        get
        {
            return transform.worldToLocalMatrix;
        }
    }

    public void SetOffset (float x, float y = 0, float z = 0)
    {
        var pos = GetPos ( );
        SetPos (new Vector3 (pos.x + x, pos.y + y, pos.z + z));
    }

    public void SetRot (Quaternion rot)
    {
        transform.rotation = rot;
    }

    public void SetScale (Vector3 scale)
    {
        transform.localScale = scale;
    }

    public void SetUnitScale (float x)
    {
        transform.localScale = new Vector3 (x, x, x);
    }

    public static bool IsRange<T> (List<T> self, int index)
    {
        return 0 <= index && index < self.Count;
    }

    public void Log (object log)
    {
        Debug.Log (name + " , " + log);
    }

    public void Warn (object log)
    {
        Debug.LogWarning (name + " , " + log);
    }

}