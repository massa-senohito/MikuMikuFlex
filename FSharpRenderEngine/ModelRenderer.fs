namespace MMDEng
open SharpDX.Windows
open System.Windows.Forms
open MikuMikuFlex3
open SharpDX
open System

module ModelRenderModule =
  type ModelRenderer( device:Renderer.RenderDevice , form : RenderForm ) =
    let mutable modelList = []
    let mutable scene = null
    let mutable mainCamera = null
    let mutable mainLight = null
    let mutable viewport = None
    let mutable clickX = None
    let mutable clickY = None
    let click (e:MouseEventArgs) =
      clickX <- Some e.X
      clickY <- Some e.Y

    do
      scene <- new Scene(device.Device , device.DepthStencil , device.RenderTarget)
      viewport <- Some <| SharpDXUtil.fillViewPort scene.ViewportSize.Width scene.ViewportSize.Height
      form.MouseClick.Add click

    member t.Add (model:PMXModel)=
      scene.ToAdd( model );
      modelList <- model :: modelList;
      model

    member t.AddCam (cam:Camera) =
      mainCamera <- cam
      scene.ToAdd cam
    member t.AddLight (light:Light) =
      mainLight <- light
      scene.ToAdd light
    member t.VP =
        let v = mainCamera.ViewTransformationMatrix
        let p = mainCamera.HomographicTransformationMatrix
        v * p
    member t.Draw time =
      scene.Draw(time , device.ImmCxt );
    member t.DrawDebugLine f=
      for i in modelList do
        for line in i.Drawer.LineList do
          let from = DXUtilV.bulletV3ToV4 line.From
          let top = DXUtilV.bulletV3ToV4 line.To
          f from top
      
    member t.ResetScene () =
      scene.Clear() 
      modelList <- []

    interface IDisposable with
      member t.Dispose() =
        scene.Dispose()
