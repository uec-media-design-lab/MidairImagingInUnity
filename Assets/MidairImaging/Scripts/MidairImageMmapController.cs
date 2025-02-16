using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UECMediaDesignLab.MidairImagingTools
{
    // MMAPや各視点カメラに共通するパラメータを保持
    public class MidairImageMmapController : MonoBehaviour
    {
        [HeaderAttribute ("Objects")]
        // MMAP
        public Transform mmap = null;
        // 光源のディスプレイ（2D）
        public Transform display = null;
        // 光源を撮影するカメラの向きを決定するためのマーカー
        public Transform vmmapMarker = null;

        [HeaderAttribute ("Parameter")]
        // MMAPのスケール
        public float mmapScale = 0.488f;
        // 光源画像を貼る面をMMAPよりどれくらい大きくするか（local scale）
        [HideInInspector]
        public float magnification = 1.2f;

        public void UpdateMmap()
        {
            // MMAPのスケールを設定する
            mmap.localScale = new Vector3(-1f, 1f, 1f) * mmapScale;
        }

        // 空中像の結像位置を計算して返す
        public Vector3 GetMidairImagePosition()
        {
            // MMAPの法線ベクトル
            var normal = mmap.transform.forward;
            // MMAPの位置からディスプレイの位置へのベクトル
            var diff = mmap.transform.position - display.transform.position;
            // 鏡面からの反射ベクトル
            var reflection = Vector3.Reflect(diff, normal);
            // 空中像の結像位置
            var midairImagePosition = mmap.transform.position - reflection;

            return midairImagePosition;
        }

        // 空中像の飛び出し距離を返す
        public float GetPopoutDistance()
        {
            // 空中像の飛び出し距離は光源とMMAPの間の距離に等しい
            var popoutDistance = Vector3.Distance(display.transform.position, mmap.transform.position);

            return popoutDistance;
        }
    }
}