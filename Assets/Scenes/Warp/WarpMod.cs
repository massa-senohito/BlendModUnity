﻿//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Matrix = UnityEngine.Matrix4x4;

public class WarpMod : EntityBase
{
    float SQUARE( float x )
    {
        return x * x;
    }

    Matrix blend_m4_m4m4( Matrix src , Matrix dst , float srcweight )
    {

        //blend_m4_m4m4(tmat, mat_unit, mat_final, fallOff);
        src.GetTRS( out var dLoc , out var dRot , out var dScale );
        dst.GetTRS( out var sLoc , out var sRot , out var sScale );
        // normされてるのでは
        //mat3_normalized_to_quat

        var fLoc  = lerp(dLoc,sLoc,srcweight);
        var fQuat = slerp(dRot,sRot,srcweight);
        RarePrint( $"fQuat : {fQuat}" );
        var fSca  = lerp(dScale,sScale,srcweight);
        var tmp = Matrix.TRS(fLoc,fQuat , fSca);
        return tmp;
    }
    Matrix mul_m4_m4(Matrix m1 ,Matrix m2)
    {
        return m1 * m2;
    }
    [SerializeField]
    float falloff_radius = 1.0f;

    [SerializeField]
    Transform from;

    [SerializeField]
    Transform to;

    [SerializeField]
    float strength;

    Mesh mesh;
    Vector3[] OrgVertice;

    [SerializeField]
    AnimationCurve fall;
    enum WarpFlag
    {
        None,
        MOD_WARP_VOLUME_PRESERVE,
    }
    [SerializeField]
    WarpFlag flag;

    enum WarpFallOff
    {
        Curve,
        Sharp,
        Smooth,
    }
    [SerializeField]
    WarpFallOff fallOff;
    float CulcFall(float fac)
    {
        var tmp = 1.0f;
        switch ( fallOff )
        {
            case WarpFallOff.Curve:
                tmp = fall.Evaluate( fac );
                break;
            case WarpFallOff.Sharp:
                tmp = fac * fac;
                break;
            case WarpFallOff.Smooth:
                tmp = 3.0f * fac * fac - 2.0f * fac * fac * fac;
                break;
            default:
                break;
        }
        return tmp;
    }

    void RarePrint( string log )
    {
        if ( Random.Range( 0 , 100 ) == 0 )
        {
            print( log );
        }
    }

    List<Vector3> warpModifier_do( )
    {
        //var ob = GameMesh;
        var fallOff_rad_sq = SQUARE(falloff_radius);
        var fac = 1.0f;
        // MOD_get_vgroup:
        var obInv = transform.worldToLocalMatrix;
        // 191付近
        // 頂点をobjローカルの位置へ移動させ、fromのワールドオフセットへ移動
        var mat_from  =  mul_m4_m4( obInv , from.localToWorldMatrix);
        //RarePrint( "mat_from\n" + mat_from );
        // 頂点をobjローカルの位置へ移動させ、toのワールドオフセットへ移動
        var mat_to    =  mul_m4_m4( obInv , to.localToWorldMatrix);
        //RarePrint( "mat_to\n" + mat_to );
        // (AB)^-1 = B^-1 A^-1
        // fromローカルに移動させ、オブジェクトのワールドに持っていく
        var tmat      = mat_from.inverse;
        //RarePrint( "tmat\n" + tmat );
        // fromローカルに移動させ、オブジェクトのワールドに持っていく オブジェクトローカルへ toのワールドへ　toをobjローカル
        var mat_final = mul_m4_m4(tmat , mat_to);
        //RarePrint( "mat_final\n" + mat_final);
        var mat_from_inv = mat_from.inverse;
        //RarePrint( "mat_from_inv\n" + mat_from_inv);
        if ( strength < 0.0f )
        {
            strength = -strength;
            var loc = mat_final.GetPosition();
            mat_final = mat_final.inverse;
            //mat_final.location -= loc;
            mat_final.SetTRS(
                mat_final.GetPosition( ) - loc ,
                mat_final.GetRotation( ) ,
                mat_final.GetScale( ) );
        }
        var weight = strength;
        var numVerts = mesh.vertices.Length;
        var temp = new List<Vector3>(numVerts);

        var verts = OrgVertice;

        for ( int i = 0 ; i < numVerts ; i++ )
        {
            var co = verts[i];
            var oriCo = co;
            // 頂点 - fromの距離
            fac = ( co - mat_from.GetPosition( ) ).sqrMagnitude;
            //print( fac );
            bool isInSQ = fac < fallOff_rad_sq;
            fac = falloff_radius - sqrt( fac );
            fac /= falloff_radius;
            if ( //falloff_type == eWarp_Falloff_None || (
                        isInSQ && fac != 0.0f )
            //)
            {
                // vert_groupがあるなら
                // if( defgrp_index != -1)
                //print( fac );
                var fallOff = CulcFall(fac);
                fallOff *= weight;
                //if(tex_co)
                if ( fallOff != 0.0f )
                {
                    // 280付近
                    // from localに頂点を移動 obj分移動
                    co = mat_from_inv.MultiplyPoint( co );
                    //RarePrint( "193 co : " + co + " : ori :" + oriCo );
                    // fromの空間に持っていく
                    if ( fallOff == 1.0f )
                    //(fallOff >0.0f)
                    {
                        co = mat_final.MultiplyPoint( co );
                    }
                    else
                    {
                        if ( flag.HasFlag( WarpFlag.MOD_WARP_VOLUME_PRESERVE ) )
                        {
                            /* interpolate the matrix for nicer locations */
                            var mat_unit = Matrix.identity;

                            tmat = blend_m4_m4m4( mat_unit , mat_final , fallOff );
                            co = tmat.MultiplyPoint( co );
                        }
                        else
                        {
                            // from localに頂点を移動 obj分移動 from localの座標系に移動 obj分移動 obj中心の座標系に移動 to分移動させる
                            //fromInv * ob * fromInv * ob * obInv * to
                            //fromInv * ob * fromInv * to | from ~ to を求めているように見える
                            var tvec = mat_final.MultiplyPoint( co );
                            var newco = Vector3.Lerp( co , tvec , fallOff );
                            //RarePrint( "217 co : " + co + " : ori :" + oriCo + " tvec : " + tvec );
                            //var writeMat = "mat_final\n" + mat_final.GetPosition();
                            //RarePrint( $"{tvec} co : {co} newco : {newco} fallOff : {fallOff}" );
                            co = newco;
                        }
                    }

                    /* out of the 'from' objects space */
                    co = mat_from.MultiplyPoint( co );
                    //RarePrint( $"oriCo {oriCo} ; co {co}" );
                } // if(fallOff != 0.0f)
            } // if( falloff_type == eWarp_Falloff_None || (isInSQ && fac != 0.0f) )
            temp.Add( co );
        } // for
        return temp;
    }

    // Start is called before the first frame update
    void Start( )
    {
        mesh = TryGetMesh( );

        OrgVertice = new Vector3[ mesh.vertices.Length ];
        System.Array.Copy( mesh.vertices , OrgVertice , mesh.vertices.Length );

        var numVerts = mesh.vertices.Length;
        for ( int i = 0 ; i < numVerts ; i++ )
        {
            //OrgVertice[ i ] = mesh.vertices[ i ];
            //SpawnCube( mesh.vertices[ i ] );
        }
    }
    private void OnDestroy( )
    {
        System.Array.Copy( OrgVertice , mesh.vertices , mesh.vertices.Length );
        mesh.SetVertices( OrgVertice );
        
    }

    List<Vector3> CreateVert( List<Vector3> v )
    {
        var temp = new List<Vector3>(v.Count);
        for ( int i = 0 ; i < v.Count ; i++ )
        {
            temp.Add( v[ i ] + Random.insideUnitSphere );
        }
        return temp;
    }

    void FixedUpdate( )
    {
        mesh.SetVertices(warpModifier_do( ) );
    }
}
