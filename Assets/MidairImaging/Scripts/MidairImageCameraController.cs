using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UECMediaDesignLab.MidairImagingTools
{
    // 各視点カメラに対し1つ用意するクラス。視点ごとに個別で用意されるオブジェクトの管理。
    [RequireComponent(typeof(MidairImageMmapController))]
    public class MidairImageCameraController : MonoBehaviour
    {
        // 視点
        public Camera mainCamera = null;
        // 光源を撮影するカメラ
        [SerializeField]
        Camera virtualCamera = null;
        // 光源画像を張り付ける面
        [SerializeField]
        Transform image = null;

        private MidairImageMmapController mimc;

        // ユーザーの視点位置の情報
        private UserAngle userAngle;

        public void SetMimc(MidairImageMmapController mimc)
        {
            this.mimc = mimc;
        }

        // Virtual Cameraのセッティング
        public void UpdateVirtualCamera()
        {
            SetVirtualCameraTransform();
            SetVirtualCameraParameter();

            // 像が視点を向くようにする
            image.LookAt(mainCamera.transform.position, this.transform.up);
            // 光源画像を貼るオブジェクトのスケールを設定
            image.localScale = Vector3.one * mimc.magnification;

            // ユーザーの視点位置を計算
            CalculateUserAngle();
        }

        // Virtual Cameraを視点カメラに対応した位置に配置する
        void SetVirtualCameraTransform()
        {
            // 視点カメラからMMAPへのベクトル
            var diff = mimc.mmap.transform.position - mainCamera.transform.position;
            // MMAPの法線ベクトル
            var normal = mimc.mmap.transform.forward;
            // 鏡面からの反射ベクトル
            // var reflection = diff + 2 * (Vector3.Dot(-diff, normal)) * normal;
            var reflection = Vector3.Reflect(diff, normal);

            //空中像の結像位置を計算
            var midairImagePosition = mimc.GetMidairImagePosition();
            
            // Virtual MMAPの位置を、空中像・光源・MMAPの位置関係から計算
            //Virtual MMAPの位置(Displayに対してMMAPと鏡像の位置)を計算
            var vdiff = mimc.display.position - mimc.mmap.position;
            var vnorm = mimc.display.forward;
            // var vreflection = vdiff + 2 * (Vector3.Dot(-vdiff, vnorm)) * vnorm;
            var vreflection = Vector3.Reflect(vdiff, vnorm);
            var vmmapPosition = mimc.display.position - vreflection;

            mimc.vmmapMarker.position = vmmapPosition; 
            
            // Virtual Cameraは常にVirtual MMAPを見る(Virtual Cameraの画は常にMMAPの中心に貼られるため)
            virtualCamera.transform.LookAt(mimc.vmmapMarker, this.transform.forward);

            // 視点カメラからMMAPまでの距離
            var distanceMaincameraMmap = Vector3.Distance(mimc.mmap.transform.position, mainCamera.transform.position);

            // Virtual Cameraの位置をVirtual MMAPの位置をもとに計算
            var dnorm = mimc.display.forward;
            var dreflection = Vector3.Reflect(-reflection, dnorm);
            virtualCamera.transform.position = mimc.vmmapMarker.position + distanceMaincameraMmap * dreflection.normalized;
        }

        // Virtual Vameraの向きを計算する
        void SetVirtualCameraRotation()
        {
            //Virtual MMAPの位置を計算
            var vdiff = mimc.display.position - mimc.mmap.position;
            var vnorm = mimc.display.forward;
            // var vreflection = vdiff + 2 * (Vector3.Dot(-vdiff, vnorm)) * vnorm;
            var vreflection = Vector3.Reflect(vdiff, vnorm);
            var vmmapPosition = mimc.display.position - vreflection;

            mimc.vmmapMarker.position = vmmapPosition; 

            virtualCamera.transform.LookAt(mimc.vmmapMarker, Vector3.forward);
        }

        // Virtual Cameraのパラメータを設定する
        void SetVirtualCameraParameter()
        {
            // Clip plane
            virtualCamera.nearClipPlane = mainCamera.nearClipPlane;

            // Field of view
            var distance = Vector3.Distance(mimc.mmap.transform.position, mainCamera.transform.position);
            virtualCamera.fieldOfView = 2 * Mathf.Atan(mimc.magnification*mimc.mmapScale / (2 * distance)) * Mathf.Rad2Deg;
        }

        public UserAngle GetUserAngle(){
            return userAngle;
        }

        // ユーザーの視点位置を計算する関数
        public void CalculateUserAngle(){
            // カメラの座標をローカル座標に変換
            Vector3 localCameraPosition = transform.InverseTransformPoint(mainCamera.transform.position);

            Vector3 localReferencePosition = transform.InverseTransformPoint(mimc.GetMidairImagePosition());

            Vector3 localDistance = localCameraPosition - localReferencePosition;
            
            // 水平距離を計算
            float horizontalDistance = Mathf.Sqrt(localDistance.x * localDistance.x + localDistance.z * localDistance.z);

            // 仰角を計算
            float elevation = Mathf.Atan2(localDistance.y, horizontalDistance) * Mathf.Rad2Deg;

            // 方位角を計算
            float azimuth = Mathf.Atan2(localDistance.x, -localDistance.z) * Mathf.Rad2Deg;

            userAngle = new UserAngle(azimuth, elevation);
        }
    }

    // ユーザーの視点位置の情報を持つクラス (空中像装置の位置・向きが基準)
    public class UserAngle{
        public float azimuth;
        public float elevation;

        public UserAngle(float azimuth, float elevation){
            this.azimuth = azimuth;
            this.elevation = elevation;
        }
    }
}