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
    }
}