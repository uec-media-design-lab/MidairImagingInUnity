using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UECMediaDesignLab.MidairImagingTools
{
    [RequireComponent(typeof(MidairImageMmapController))]
    [RequireComponent(typeof(MidairImageCameraController))]
    [RequireComponent(typeof(LuminanceController))]
    [RequireComponent(typeof(BlurKernelController))]
    public class MidairImageManager : MonoBehaviour
    {
        // 各視点で共通するオブジェクトを管理するクラス
        private MidairImageMmapController mimc;
        // 各視点ごとに用意する個別オブジェクトを管理するクラスの配列
        private MidairImageCameraController[] miccArray;

        // 輝度を再現するかどうかのフラグ
        public bool isSimulatingLuminance = false;
        // 輝度再現のコンポーネント
        private LuminanceController luminanceController;
        // ぼけを再現するかどうかのフラグ
        public bool isSimulatingSharpness = false;
        // ぼけ再現のコンポーネント
        private BlurKernelController blurKernelController;
        
        void Start(){
            mimc = GetComponent<MidairImageMmapController>();
            miccArray = GetComponents<MidairImageCameraController>();
            luminanceController = GetComponent<LuminanceController>();
            blurKernelController = GetComponent<BlurKernelController>();

            foreach(MidairImageCameraController micc in miccArray){
                micc.SetMimc(mimc);
            }
        }

        void Update(){
            mimc.UpdateMmap();

            foreach(MidairImageCameraController micc in miccArray){
                micc.UpdateVirtualCamera();
                UserAngle userAngle = micc.GetUserAngle();
                luminanceController.SimulateLuminance(isSimulatingLuminance, userAngle);
                blurKernelController.SimulateSharpness(isSimulatingSharpness, userAngle);
            }
        }
    }
}
