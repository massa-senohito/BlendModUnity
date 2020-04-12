using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IceMilkTea.Core;
public class AxisBar : EntityBase
{

    enum StateID
    {
        Idle,
        Drag,
        NoDrag,
    }

    const int IdleInt = (int)StateID.Idle;
    const int DragInt = (int)StateID.Drag;
    const int NoDragInt = (int)StateID.NoDrag;
    

    private class IdleState : ImtStateMachine<AxisBar>.State
    {
    }

    private class DragState : ImtStateMachine<AxisBar>.State
    {
        Vector3 InitLocalPos;
        protected internal override void Enter( )
        {
            var world = Context.MouseInWorld();
            InitLocalPos = Context.LocalPos(world);
        }
        protected internal override void Update( )
        {
            var world = Context.MouseInWorld();
            var currentLocal = Context.LocalPos( world );
            var delta = currentLocal - InitLocalPos;
            delta.x = 0;
            delta.z = 0;
            var parent = Context.transform.parent;
            parent.position += Context.GetRot() * delta ;
        }
    }
    private class NoDragState : ImtStateMachine<AxisBar>.State
    {
    }

    ImtStateMachine<AxisBar> State;

    // Start is called before the first frame update
    void Start()
    {
        State = new ImtStateMachine<AxisBar>( this);
        State.AddTransition<IdleState   , NoDragState >( NoDragInt );
        State.AddTransition<NoDragState , IdleState   >( IdleInt );
        State.AddTransition<IdleState   , DragState   >( DragInt );
        State.AddTransition<DragState   , IdleState   >( IdleInt );
        State.SetStartState<IdleState>( );
    }

    bool IsInRange(Vector3 loc)
    {
        var isInXRange = -1.0f < loc.x && loc.x < 1.0f;
        var isInZRange = -1.0f < loc.z && loc.z < 1.0f;
        var isInYRange =  0.0f < loc.y && loc.y < 7.0f;
        var inSide = isInXRange || isInZRange;
        return inSide && isInYRange;
    }

    public Vector3 LocalPos( Vector3 world )
    {
        var barLocal         = ToInverse;
        var barOrientCtrlpos = barLocal.MultiplyPoint3x4 (world);

        var value = barOrientCtrlpos;
        return value;
    }

    public string StateName;
    // Update is called once per frame
    void FixedUpdate()
    {
        State.Update( );

        if ( Input.GetMouseButton( 0 ) )
        {
            var mouseWorldPos = MouseInWorld( );
            var objlocalMouse = LocalPos( mouseWorldPos );
            if(name == "ZAxis")
            {
                Debug.Log( objlocalMouse );
            }
            var contain = IsInRange( objlocalMouse );
            if ( contain )
            {
                State.SendEvent( DragInt );
            }
            else
            {
                State.SendEvent( NoDragInt );
            }
        }
        else
        {
            State.SendEvent( IdleInt );
        }

        StateName = State.CurrentStateName;
    }
}
