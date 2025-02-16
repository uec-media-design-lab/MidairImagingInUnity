using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UECMediaDesignLab.MidairImagingTools
{
    // 空中像のぼけを再現する。ぼかしの強さはHMD越しに補正するか、手動で変更する。視点位置によるぼけ方の変化(ぼかし関数の形の変化)は測定値をもとに作成したLUTを参照する。
    [RequireComponent(typeof(MidairImageMmapController))]
    [RequireComponent(typeof(MidairImageCameraController))]
    public class BlurKernelController : MonoBehaviour
    {
        // ブラーフィルタのタイプ
        public enum BlurType{
            Laplace,
            Lorentz
        }

        public BlurType blurType = BlurType.Laplace;

        // ぼけフィルタになるオブジェクトのプレハブ
        public GameObject blurFilterPrefab;

        MidairImageMmapController mimc;

        // ぼけの強さ(補正時に調節)
        [Range(1, 50)]
        public float blurIntensity = 1f;

        // Start is called before the first frame update
        void Start()
        {
            mimc = GetComponent<MidairImageMmapController>();
            
            // MidairImageMmapControllerからmmapを取得
            Transform mmap = mimc.mmap;
            // MMAPの子オブジェクトにぼけフィルタを生成
            if (blurFilterPrefab != null)
            {
                GameObject blurFilter = Instantiate(blurFilterPrefab, mmap);
            }
        }

        public void SimulateSharpness(bool isSimulating, UserAngle userAngle){
            if(isSimulating == false){
                Shader.SetGlobalFloat("Blur",1);
                return;
            }
            
            // 仰角と方位角入れ替え(Unity上での装置の向きと、測定時の向きを合わせる)
            var tmp = userAngle.azimuth;
            userAngle.azimuth = userAngle.elevation;
            userAngle.elevation = tmp;

            // 飛び出し距離[m]
            float popoutDistance = mimc.GetPopoutDistance();

            if (blurType == BlurType.Laplace){
                // LUTから値を取得
                float coef = GetBlurCoef(userAngle, blurType);

                // 飛び出し距離に対するぼけの変化
                // 飛び出し距離による変化の傾き
                float slope = 0.314f;
                // 飛び出し距離の基準(LUTを作成した際の飛び出し距離)
                float popoutDistanceBase = 0.25f;
                // 飛び出し距離によって係数を調整
                coef = coef + slope * (popoutDistance - popoutDistanceBase);
                
                Shader.SetGlobalFloat("laplaceCoefC", coef);
                
            } else if (blurType == BlurType.Lorentz){
                // LUTから値を取得
                float coef = GetBlurCoef(userAngle, blurType);

                // 飛び出し距離に対するぼけの変化
                // 飛び出し距離による変化の傾き
                float slope = 0.26f;
                // 飛び出し距離の基準(LUTを作成した際の飛び出し距離)
                float popoutDistanceBase = 0.25f;
                // 飛び出し距離によって係数を調整
                coef = coef + slope * (popoutDistance - popoutDistanceBase);
                
                Shader.SetGlobalFloat("lorentzCoefC", coef);
            }

            Shader.SetGlobalFloat("Blur",blurIntensity);
        }

        private float GetBlurCoef(UserAngle userAngle, BlurType blurType){
            return GetBlurCoef(userAngle.azimuth, userAngle.elevation, blurType);
        }

        // ブラータイプに合ったLUTから、視点に合った値を取ってくる
        private float GetBlurCoef(float azimuth, float elevation, BlurType blurType){
            // 空中像のぼけを再現するラプラス関数の係数のルックアップテーブル
            // 13x11の配列で輝度情報を保持。[仰角(-30°～30°、5°毎)][方位角(-30°～20°、5°毎)]。
            // [6][5]が仰角方位角ともに0度で正面。
            float[,] laplaceCoefCArray = new float[13,11] {
                {0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f},
                {0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f},
                {0.000000f,0.000000f,0.000000f,0.000000f,27.009756f,2027.603518f,21.824164f,-41.271206f,1.038192f,0.000000f,0.000000f},
                {0.000000f,0.000000f,0.146360f,0.133786f,0.122520f,-28.983365f,17465.812441f,0.198541f,72074.175431f,0.696351f,-2.242649f},
                {0.000000f,6.973764f,0.180215f,0.126480f,26.231365f,0.139234f,0.142004f,-18937.507784f,15.431668f,661849.685466f,2.824383f},
                {0.125768f,0.109893f,0.113535f,0.113096f,16.976444f,36.492095f,0.131624f,0.149600f,-15.505913f,0.287087f,12.580471f},
                {0.000000f,0.103219f,0.102025f,0.110368f,0.120301f,31874.154608f,0.133546f,77429.932768f,60435.884567f,0.223329f,0.396522f},
                {0.125768f,0.109893f,0.113535f,0.113096f,16.976444f,36.492095f,0.131624f,0.149600f,-15.505913f,0.287087f,12.580471f},
                {0.000000f,6.973764f,0.180215f,0.126480f,26.231365f,0.139234f,0.142004f,-18937.507784f,15.431668f,661849.685466f,2.824383f},
                {0.000000f,0.000000f,0.146360f,0.133786f,0.122520f,-28.983365f,17465.812441f,0.198541f,72074.175431f,0.696351f,-2.242649f},
                {0.000000f,0.000000f,0.000000f,0.000000f,27.009756f,2027.603518f,21.824164f,-41.271206f,1.038192f,0.000000f,0.000000f},
                {0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f},
                {0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f}
            };

            // ローレンツ関数の係数のルックアップテーブル
            float[,] lorenzCoefCArray = new float[13,11] {
                {0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f},
                {0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f},
                {0.000000f,0.000000f,0.000000f,0.000000f,0.136228f,0.148047f,0.154447f,0.266362f,0.802020f,0.000000f,0.000000f},
                {0.000000f,0.000000f,0.125310f,0.112678f,0.102758f,0.111797f,0.119091f,0.165282f,0.234247f,0.573115f,1.247233f},
                {0.000000f,0.127803f,0.155805f,0.106711f,0.100150f,0.116922f,0.119296f,0.140938f,0.208982f,0.333004f,0.688303f},
                {0.106638f,0.089572f,0.095754f,0.095824f,0.102801f,0.106147f,0.110383f,0.125482f,0.153219f,0.242028f,0.405233f},
                {0.000000f,0.083076f,0.083704f,0.092348f,0.101057f,0.109930f,0.111224f,0.117970f,0.142795f,0.187844f,0.338900f},
                {0.106638f,0.089572f,0.095754f,0.095824f,0.102801f,0.106147f,0.110383f,0.125482f,0.153219f,0.242028f,0.405233f},
                {0.000000f,0.127803f,0.155805f,0.106711f,0.100150f,0.116922f,0.119296f,0.140938f,0.208982f,0.333004f,0.688303f},
                {0.000000f,0.000000f,0.125310f,0.112678f,0.102758f,0.111797f,0.119091f,0.165282f,0.234247f,0.573115f,1.247233f},
                {0.000000f,0.000000f,0.000000f,0.000000f,0.136228f,0.148047f,0.154447f,0.266362f,0.802020f,0.000000f,0.000000f},
                {0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f},
                {0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f,0.000000f}
            };

            // 仰角・方位角の最小値・最大値
            float minAzimuth = -30.0f;
            // float maxAzimuth = 20.0f;
            float minElevation = -30.0f;
            // float maxElevation = 30.0f;

            // 仰角・包囲角のステップ
            float azimuthStep = 5.0f;
            float elevationStep = 5.0f;

            // ルックアップテーブルのどこを参照するか計算
            float azimuthIndex = (azimuth - minAzimuth) / azimuthStep;
            float elevationIndex = (elevation - minElevation) / elevationStep;

            // インデックスを範囲内に収める
            azimuthIndex = Mathf.Clamp(azimuthIndex, 0, laplaceCoefCArray.GetLength(1) - 1);
            elevationIndex = Mathf.Clamp(elevationIndex, 0, laplaceCoefCArray.GetLength(0) - 1);

            // ルックアップテーブルからバイリニア補間で値を取得
            int azimuthIndexFloor = Mathf.FloorToInt(azimuthIndex);
            int azimuthIndexCeil = Mathf.CeilToInt(azimuthIndex);
            float azimuthWeight = azimuthIndex - azimuthIndexFloor;
            int elevationIndexFloor = Mathf.FloorToInt(elevationIndex);
            int elevationIndexCeil = Mathf.CeilToInt(elevationIndex);
            float elevationWeight = elevationIndex - elevationIndexFloor;

            float blurCoef = -1.0f;
            if (blurType == BlurType.Laplace){
                blurCoef = Mathf.Lerp(
                    Mathf.Lerp(laplaceCoefCArray[elevationIndexFloor, azimuthIndexFloor], 
                            laplaceCoefCArray[elevationIndexFloor, azimuthIndexCeil], 
                            azimuthWeight),
                    Mathf.Lerp(laplaceCoefCArray[elevationIndexCeil, azimuthIndexFloor], 
                            laplaceCoefCArray[elevationIndexCeil, azimuthIndexCeil], 
                            azimuthWeight),
                    elevationWeight
                );
            } else if (blurType == BlurType.Lorentz){
                blurCoef = Mathf.Lerp(
                    Mathf.Lerp(lorenzCoefCArray[elevationIndexFloor, azimuthIndexFloor], 
                            lorenzCoefCArray[elevationIndexFloor, azimuthIndexCeil], 
                            azimuthWeight),
                    Mathf.Lerp(lorenzCoefCArray[elevationIndexCeil, azimuthIndexFloor], 
                            lorenzCoefCArray[elevationIndexCeil, azimuthIndexCeil], 
                            azimuthWeight),
                    elevationWeight
                );
            }

            return blurCoef;
        }
    }
}