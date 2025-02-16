using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UECMediaDesignLab.MidairImagingTools
{
    [RequireComponent(typeof(MidairImageCameraController))]
    public class LuminanceController : MonoBehaviour
    {
        public void SimulateLuminance(bool isSimulating, UserAngle userAngle){
            if(isSimulating == false){
                // 空中像は常に明るくなる
                Shader.SetGlobalFloat("MidairImageLuminance", 1.0f);
                return;
            }

            float brightness = GetMidairImageLuminance(userAngle);
            Shader.SetGlobalFloat("MidairImageLuminance",Mathf.Clamp01(brightness));
        }

        // 輝度のルックアップテーブルから値を取得する関数
        private float GetMidairImageLuminance(UserAngle userAngle){
            return GetMidairImageLuminance(userAngle.azimuth, userAngle.elevation);
        }

        private float GetMidairImageLuminance(float azimuth, float elevation){
            // 空中像の輝度比のルックアップテーブル
            // 7x7の配列で輝度情報を保持。[仰角][方位角]。
            // それぞれ-30度~30度、10度ごとの値。すなわち[4][4]が仰角方位角ともに0度で正面。
            float[,] midairImageLuminanceArray = new float[7,7] {
                {0.30369374f,0.287662102f,0.332268106f,0.350351802f,0.332268106f,0.287662102f,0.30369374f},
                {0.225194157f,0.276247975f,0.532579892f,0.62984183f,0.532579892f,0.276247975f,0.225194157f},
                {0.191125513f,0.353554877f,0.634190966f,0.870327118f,0.634190966f,0.353554877f,0.191125513f},
                {0.130965863f,0.42174683f,0.646636927f,1f,0.646636927f,0.42174683f,0.130965863f},
                {0.119386424f,0.278713495f,0.593575603f,0.952489567f,0.593575603f,0.278713495f,0.119386424f},
                {0.125504942f,0.145111522f,0.455717646f,0.725659757f,0.455717646f,0.145111522f,0.125504942f},
                {0.10003083f,0.129248465f,0.202301712f,0.443883113f,0.202301712f,0.129248465f,0.10003083f}
            };
            
            // 仰角・方位角の最小値・最大値
            float minAzimuth = -30.0f;
            // float maxAzimuth = 30.0f;
            float minElevation = -30.0f;
            // float maxElevation = 30.0f;

            // 仰角・包囲角のステップ
            float azimuthStep = 10.0f;
            float elevationStep = 10.0f;

            // ルックアップテーブルのどこを参照するか計算
            float azimuthIndex = (azimuth - minAzimuth) / azimuthStep;
            float elevationIndex = (elevation - minElevation) / elevationStep;

            // インデックスを範囲内に収める
            azimuthIndex = Mathf.Clamp(azimuthIndex, 0, midairImageLuminanceArray.GetLength(1) - 1);
            elevationIndex = Mathf.Clamp(elevationIndex, 0, midairImageLuminanceArray.GetLength(0) - 1);

            // ルックアップテーブルからバイリニア補間で値を取得
            int azimuthIndexFloor = Mathf.FloorToInt(azimuthIndex);
            int azimuthIndexCeil = Mathf.CeilToInt(azimuthIndex);
            float azimuthWeight = azimuthIndex - azimuthIndexFloor;
            int elevationIndexFloor = Mathf.FloorToInt(elevationIndex);
            int elevationIndexCeil = Mathf.CeilToInt(elevationIndex);
            float elevationWeight = elevationIndex - elevationIndexFloor;

            float brightness = -1.0f;
            brightness = Mathf.Lerp(
                Mathf.Lerp(midairImageLuminanceArray[elevationIndexFloor, azimuthIndexFloor], 
                           midairImageLuminanceArray[elevationIndexFloor, azimuthIndexCeil], 
                           azimuthWeight),
                Mathf.Lerp(midairImageLuminanceArray[elevationIndexCeil, azimuthIndexFloor], 
                           midairImageLuminanceArray[elevationIndexCeil, azimuthIndexCeil], 
                           azimuthWeight),
                elevationWeight
            );

            return brightness;
        }
    }
}
