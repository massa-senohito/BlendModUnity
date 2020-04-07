#define TEST_ONE_DRAGBAR

using UnityEngine;
    using static UnityEngine.Mathf;
using IceMilkTea.Core;
public class DragBar : EntityBase
{
    [SerializeField]
    EntityBase VerticalBar;
    [SerializeField]
    float HoriLength = 5;
    public float BarValue;

    enum StateID
    {
        Idle,
        Drag,
        NoDrag,
    }
    const int IdleInt = (int)StateID.Idle;
    const int DragInt = (int)StateID.Drag;
    const int NoDragInt = (int)StateID.NoDrag;
    

    private class IdleState : ImtStateMachine<DragBar>.State
    {
    }

    private class DragState : ImtStateMachine<DragBar>.State
    {
        protected internal override void Update( )
        {
            var world = Context.MouseInWorld();
            Context.SetPosInWorld( world );
        }
    }
    private class NoDragState : ImtStateMachine<DragBar>.State
    {
    }

    ImtStateMachine<DragBar> State;

    private void Start( )
    {
        State = new ImtStateMachine<DragBar>( this );
        State.AddTransition<IdleState   , NoDragState >( NoDragInt );
        State.AddTransition<NoDragState , IdleState   >( IdleInt );
        State.AddTransition<IdleState   , DragState   >( DragInt );
        State.AddTransition<DragState   , IdleState   >( IdleInt );
        State.SetStartState<IdleState>( );
    }

    public void SetPosInWorld (Vector3 world)
    {
        float value = BarLocalPos( world ).x;
        value = Clamp( value , 0 , HoriLength );
        BarValue = value / HoriLength;
        Vector3 localPos = new Vector3( value , 0 , 0 );
        //barOrientCtrlpos;
        Vector4 worldPos = ToWorld.MultiplyPoint3x4( localPos);

        VerticalBar.SetPos( worldPos );
    }

    private Vector3 BarLocalPos( Vector3 world )
    {
        var barLocal         = ToInverse;
        var barOrientCtrlpos = barLocal.MultiplyPoint3x4 (world);

        var value = barOrientCtrlpos;
        return value;
    }

    public string StateName;

    private void FixedUpdate()
    {
        State.Update( );
#if TEST_ONE_DRAGBAR
        if(Input.GetMouseButton(0))
        {
            // 一番近い,距離がある程度以下のバーをドラッグ判定に
            // Idleでクリックしたから
            // Horibar上ならドラッグへ
            Vector3 world = MouseInWorld( );
            var value = BarLocalPos( world );
            var isInXRange = 0.0f < value.x && value.x < HoriLength;
            var isInYRange = 0.0f < value.y && value.y < 1.0f;

            if ( isInYRange && isInXRange )
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
#endif

#if TEST_ALWAYS_DRAG
        var mouse = Input.mousePosition;
        var zMouse = new Vector3(mouse.x , mouse.y , Distance(Camera.main.gameObject));
        var world = Camera.main.ScreenToWorldPoint(zMouse);

        SetPosInWorld( world );
        //Debug.Log( world );
#endif
    }

}