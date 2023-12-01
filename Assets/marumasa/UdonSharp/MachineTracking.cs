using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MachineTracking : UdonSharpBehaviour
{
    [SerializeField]
    // 機体
    private Transform Machine;

    // 機体のボディ (機体が表示されるゲームオブジェクトのトランスフォーム)
    private Transform Body;

    void Start()
    {
        Body = this.transform;
    }

    void Update()
    {
        // sqrMagnitude で 機体との直線距離の2乗 を取得する (直線距離を取得するより負荷がかからない)
        float pos_speed = (Machine.position - Body.position).sqrMagnitude;
        // 機体との角度の差を取得する
        float rot_speed = Quaternion.Angle(Machine.rotation, Body.rotation);

        // 補完速度の調整
        pos_speed += 0.06f;
        rot_speed /= 90;
        rot_speed += 0.02f;

        // ボディを滑らかに追従する
        Body.position = Vector3.Lerp(Body.position, Machine.position, pos_speed);
        Body.rotation = Quaternion.Slerp(Body.rotation, Machine.rotation, rot_speed);
    }
}
