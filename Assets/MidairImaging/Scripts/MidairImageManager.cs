using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UECMediaDesignLab.MidairImagingTools
{
    [RequireComponent(typeof(MidairImageMmapController))]
    [RequireComponent(typeof(MidairImageCameraController))]
    public class MidairImageManager : MonoBehaviour
    {
        // 各視点で共通するオブジェクトを管理するクラス
        private MidairImageMmapController mimc;
        // 各視点ごとに用意する個別オブジェクトを管理するクラスの配列
        private MidairImageCameraController[] miccArray;
        
        void Start(){
            mimc = GetComponent<MidairImageMmapController>();
            miccArray = GetComponents<MidairImageCameraController>();

            foreach(MidairImageCameraController micc in miccArray){
                micc.SetMimc(mimc);
            }
        }

        void Update(){
            mimc.UpdateMmap();

            foreach(MidairImageCameraController micc in miccArray){
                micc.UpdateVirtualCamera();
            }
        }
    }
}
