# ドキュメント

- [使用前の設定](#使用前の設定)
- [使い方](#使い方)
    - [MMAPのスケールを変えるとき](#mmapのスケールを変えるとき)
    - [空中像装置を回転させるとき](#空中像装置を回転させるとき)
    - [複数の視点に対応させるとき](複数の視点に対応させるとき)


## 使用前の設定
1. **システムの配置**

    Asset/MidairImaging/Prefabs/Midair Imaging Device をシーン上に配置する

    ![](https://github.com/user-attachments/assets/1bc40dd0-bf51-4b82-b20a-0225848a0a3c)


    Midair Image Camera ControllerコンポーネントのMain Cameraに視点カメラをセットする。

    ![](https://github.com/user-attachments/assets/ed1aee71-cd54-4151-99f1-468966502630)


2.  **Culling Maskの設定**

    Add Layer…から新しいレイヤーを作成し、Display2Dのレイヤーに設定する。(写真の例では”Display”）

    ![](https://github.com/user-attachments/assets/d47206ad-43af-4097-a0f5-123a3f609f99)


    VirtualCamera(Midair Imaging Device の子オブジェクト)のCameraコンポーネント内、Culling Maskを”Display”(新しく作成したレイヤー)のみにチェックが入っているようにする。

    ![](https://github.com/user-attachments/assets/1db746b5-828a-4946-9358-bae90b3c9d4e)


    実行すると空中像が浮かんで見える。空中像に対して角度がついていると見切れてしまうので、最初はカメラを空中像の正面に置いておくと良いです。

> [!NOTE]
> 空中像が映らない場合、カメラのClipping Planesの設定を確認してください。
> カメラが空中像の視域内にある場合でも、空中像がカメラのnear clipping planeより近くにある場合は描画されません。

## 使い方
### MMAPのスケールを変えるとき
Midair Image Mmap ControllerのパラメータMmap Scaleを変えてMMAPのスケールを変えてください。

![](https://github.com/user-attachments/assets/1b8ff457-149a-46d8-bce7-5afa90f6500a)


### 空中像装置を回転させるとき
MMAPの傾きを変えるときは、"MMAP"ではなく"Midair Imaging Device"のRotationを変えてください

![](https://github.com/user-attachments/assets/7f19fff0-c053-4623-bb76-8fe6a121f5d2)


### 複数の視点に対応させるとき
デフォルトでは1つの視点にしか対応していませんが、以下の手順に従って、同時に空中像を観察できる視点を複数に増やすことができます。これを応用してHMDの両眼視に対応することも可能です。

1. Midair Imaging DeviceにMidair Image Camera Controllerコンポーネントを追加してください

    Main Cameraには新しく対応させる視点のカメラをアタッチしてください。
    
    Midair Imaging Deviceの子オブジェクトであるVirtual CameraとImageを複製し、それらを追加したMidair Image Camera Controllerにアタッチしてください

    ![](https://github.com/user-attachments/assets/da398337-ff33-4bd2-afec-29c5dde9a393)


2. マテリアル・Render texture・シェーダーを複製してください

    (Assets/MidairImaging/Materials 下にあるMidairImageR.mat、MidairImageR.rendertexture、MidairImageShaderR.shaderはデフォルトで使用していないアセットです。2つ目の視点に対応する場合はこれらを使用し、手順3までスキップして構いません)

    Assets/MidairImaging/Materials 下にある MidairImageL.renderTextureとMidairImageShaderL.shaderと同じ内容のレンダーテクスチャとシェーダーを別名で作成してください。

    ![](https://github.com/user-attachments/assets/35852eae-72c4-488d-aed9-74aa8b9287b3)


    新しくマテリアルを作成し、シェーダーに別名で保存したシェーダーを割り当ててください。マテリアルのテクスチャには別名で保存したレンダーテクスチャをアタッチしてください。

    ![](https://github.com/user-attachments/assets/8ac05cee-3497-4c34-895b-3ccdf51f5cba)


3. 作成したマテリアルを手順1で複製したImageにアタッチしてください。

    ![](https://github.com/user-attachments/assets/d9591602-9965-439d-810c-feec2af1263b)


4. 複製したVirtual CameraのTarget Textureに複製したレンダーテクスチャを設定してください

    ![](https://github.com/user-attachments/assets/03ccfb6f-153d-48a2-bdda-f98811a5b368)


5. CameraのCulling Maskを設定してください。
    
    このままだとシーンにImageが複数あるため空中像が2重に見えます。
    
    適宜レイヤーを作成し、各Imageが別のレイヤーに設定されるようにしてください。その後、空中像のカメラのCulling Maskを設定し、同じMidair Image Camera ControllerにアタッチされているImageだけ映るようにしてください。
    
    画像の例では、右目用画像に設定する”Image 2”のLayerをForRightEyeに設定し、右目用カメラ”Main Camera 2”では左目用画像のレイヤーが映らないように設定しています

    ![](https://github.com/user-attachments/assets/7f812bc0-952e-419d-b64d-6975140c4e1f)


    ![](https://github.com/user-attachments/assets/4dde4f20-7fc5-4926-a44a-09c932fe21f1)


6. 複数の視点から同時に空中像が観察できることを確認してください

7. 輝度やぼけの再現を複数の視点に対応させる場合、C#スクリプトから設定しているシェーダーの値が、各視点に対応するImageごとに別の名前を使うようにしてください。

    輝度を再現するLuminanceController.csでは13,18行目で、空中像のぼけを再現するBlurKernelController.csでは44,68,82,85行目でシェーダーの値を設定しています(`Shader.SetGlobalFloat("hoge")`)。

    シェーダーのグローバルなプロパティ`MidairImageLuminance`,`Blur`,`laplaceCoefC`,`lorentzCoefC`が複数のImageで共有されると，どの視点位置からも同じ輝度・ぼけで観察されます。
    
    視点それぞれの位置から適切な輝度・ぼけで観察できるように，C#からシェーダーに送るのプロパティ名を適宜変更し，各Imageごとに違う値を使うように書き換えてください。