namespace MMDEng

open System.Text
open SharpDX
open SharpDX.Direct3D11
open System
open MikuMikuFlex3
open Renderer
open ModelRenderModule
open System.Drawing
open System.Diagnostics
open SharpDXUtil
open SharpDX.Windows

module MMDRenderer =
    
  type MMDForm() as t = 
    inherit SharpDX.Windows.RenderForm()
    let mutable renderDevice = NoneD
    let mutable modelRenderer = NoneD
    let mutable addBlendState = null
    let mutable rasterState = null
    let mutable mouseHandles = []
    let mutable backGroundColor = rawColorA1 0.2f 0.4f 0.8f
    let timer = Stopwatch.StartNew()
    let opd = new SharpDXUtil.OPDBuilder()

    do
      Encoding.RegisterProvider( CodePagesEncodingProvider.Instance )
      t.AutoScaleMode <- System.Windows.Forms.AutoScaleMode.Font;
      t.Text <- "Form1"
      t.ClientSize <- new Size( 1280, 720 )
      t.InitDirect3D()

    member t.OnLoad e =
      t.OnLoad e

    member t.InitDirect3D() =
      let dev = new RenderDevice(t)
      renderDevice <- SomeD <| dev
      modelRenderer <- SomeD <| new ModelRenderer(dev , t)
      addBlendState <- new BlendState(dev.Device , SharpDXUtil.alphaBlendStateDesc)
      let om = dev.ImmCxt.OutputMerger
      om.BlendState <- addBlendState
      om.DepthStencilState <- null
      rasterState <- new RasterizerState(dev.Device , SharpDXUtil.normalRasterStateDesc)

    member t.AddChara path =
      opd{
        let! dev = renderDevice
        let model = new PMXModel( dev.Device , path );
        let! modelR = modelRenderer
        return modelR.Add model
      }

    member t.ResetEnv() =
      modelRenderer.Attempt (fun m->m.ResetScene())
      
    member t.ApplyAnim path model=
      VMDAnimationBuilder.AddAnimation(path , model)

    member t.AddCamera cam =
      match modelRenderer with
      |SomeD m-> m.AddCam cam
      |NoneD ->()
    member t.AddMouseCam() =
      let cam = new MouseMotionCamera(45.0f)
      let h1 = t.MouseDown.Subscribe(fun e-> cam.OnMouseDown (t,e))
      let h2 = t.MouseUp.Subscribe(fun e-> cam.OnMouseUp(t,e))
      let h3 = t.MouseMove.Subscribe(fun e-> cam.OnMouseMove(t,e))
      let h4 = t.MouseWheel.Subscribe(fun e-> cam.OnMouseWheel(t,e))
      //t.ResizeEnd modelRenderer
      mouseHandles <- [h1;h2;h3;h4]
      t.AddCamera cam

    member t.AddLight color dir =
      let light = new Light()
      light.Color <- color
      light.IrradiationDirection <- dir
      match modelRenderer with
      |SomeD m-> m.AddLight light
      |NoneD ->()
    member t.AddDefaultLight() =
      let light = new Light()
      match modelRenderer with
      |SomeD m-> m.AddLight light
      |NoneD ->()

    member t.OnUpdate =
      let render() =
        //onRender()
        match renderDevice with
        |SomeD r-> r.Clear( backGroundColor )
        |NoneD ->()
        let sec = timer.Elapsed.TotalSeconds
        modelRenderer.Attempt (fun m->m.Draw sec)
        renderDevice.Attempt(fun r->r.Present();() )
      //let loop = new RenderLoop(t)
      //loop.
      //RenderLoop.Run(t , render)
      render
    //interface IDisposable with
    member t.OnClose e = 
        let r = &modelRenderer
        attemtDispose &r
        rasterState.Dispose()
        let r = &renderDevice
        attemtDispose &r
        for i in mouseHandles do
          i.Dispose()
        mouseHandles <- []
        addBlendState.Dispose()

