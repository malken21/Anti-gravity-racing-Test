using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

public class MainSeat : UdonSharpBehaviour
{
    #region Parameter
    [SerializeField]
    // 機体のリジッドボディ
    private Rigidbody Machine;

    [SerializeField]
    // 機体のタイヤ
    private Transform Wheel;

    [SerializeField]
    private float HoverPower = 500f;

    [SerializeField]
    private float HoverRange = 1f;

    [SerializeField]
    private float Speed_Move = 0f;

    [SerializeField]
    private float speed_Slide = 0f;
    #endregion

    #region Status
    // 座っているプレイヤー
    private VRCPlayerApi rider;

    // 操縦席
    private VRCStation station;

    private LayerMask mask;

    private float Control_Vertical;
    private float Control_Horizontal;

    #endregion

    #region Actions

    //========== キー入力受付 ========== start
    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        if (rider == null || !rider.isLocal)
            return;

        if (value)
        {
            rider = null;
            station.ExitStation(Networking.LocalPlayer); // 自分をstationから出す
            Control_Horizontal = 0;
            Control_Vertical = 0;
        }
    }

    public override void InputUse(bool value, UdonInputEventArgs args)
    {
        if (rider == null || !rider.isLocal)
            return;
    }

    public override void InputGrab(bool value, UdonInputEventArgs args)
    {
        if (rider == null || !rider.isLocal)
            return;
    }

    public override void InputDrop(bool value, UdonInputEventArgs args)
    {
        if (rider == null || !rider.isLocal)
            return;
    }

    public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
    {
        if (rider == null || !rider.isLocal)
            return;
        Control_Horizontal = value;
    }

    public override void InputMoveVertical(float value, UdonInputEventArgs args)
    {
        if (rider == null || !rider.isLocal)
            return;
        Control_Vertical = value;
    }

    //========== キー入力受付 ========== end

    public override void Interact()
    {
        station.UseStation(Networking.LocalPlayer);
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        rider = player;

        if (player.isLocal)
            Networking.SetOwner(player, Machine.gameObject);
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        rider = null;
    }
    #endregion

    #region Main
    // コース内にある時に実行される関数
    private void CourseIn(RaycastHit hit)
    {
        Speed_Move = Control_Vertical * 130;

        Machine.useGravity = false;
        Machine.drag = 1f;

        // 地面との距離
        float distance = hit.distance;

        Vector3 up = Machine.transform.up;

        // 傾きの差を求める
        Quaternion rot = Quaternion.FromToRotation(up, hit.normal);

        rot = rot * Quaternion.AngleAxis(Control_Horizontal, up);

        // 自分を回転させる
        Machine.MoveRotation(rot * Machine.rotation);
        Machine.velocity = rot * Machine.velocity;

        Machine.AddRelativeForce(
            new Vector3(
                speed_Slide,
                // 下に押し付ける
                // 二次関数 を使って 地面から離れれば離れるほど 下に押し付ける力が強くなる
                -HoverPower * distance * distance,
                // 前に進む
                Speed_Move
            ),
            ForceMode.Acceleration
        );
    }

    // コース外にある時に実行される関数
    private void CourseOut()
    {
        Machine.useGravity = true;
        Machine.drag = 0f;
        Machine.rotation = Quaternion.Slerp(
            Machine.rotation,
            QuaternionHorizontal(Machine.rotation),
            Time.deltaTime * 3
        );
    }
    #endregion

    void Start()
    {
        station = this.GetComponent<VRCStation>();
        mask = LayerMask.GetMask("Environment");
    }

    void FixedUpdate()
    {
        RaycastHit hit;

        // 真下の地形の法線を調べる
        // 当たったゲームオブジェクトの名前が 設定された名前と同じなら
        if (Physics.Raycast(Wheel.position, -Wheel.up, out hit, HoverRange, mask))
        {
            // コース内の時に実行する関数を実行する
            CourseIn(hit);
        }
        else
        {
            // コース外の時に実行する関数を実行する
            CourseOut();
        }
    }

    #region Utils
    // Quaternionを水平にする関数
    private static Quaternion QuaternionHorizontal(Quaternion origin)
    {
        Quaternion clone = new Quaternion(origin.x, origin.y, origin.z, origin.w);
        // Quaternionのxとzの成分を0にする
        clone.x = 0;
        clone.z = 0;
        // Quaternionを正規化する
        clone.Normalize();
        return clone;
    }
    #endregion
}
