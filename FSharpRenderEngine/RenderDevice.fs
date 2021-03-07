namespace MMDEng
open System
open SharpDX.Direct3D11
open SharpDX.Windows
open SharpDXUtil

module Renderer =

  type RenderDevice(form :RenderForm) =
    let mutable d3dDevice:Device ref = ref null
    let mutable immeCxt = null
    
    let mutable swapChain:SharpDX.DXGI.SwapChain ref = ref null
    let mutable renderTarget = null
    let mutable renderTargetView = null
    let mutable depthStencil = null
    let mutable depthStencilView = null
    do
      let swpDec = 
        let mutable dec = new SharpDX.DXGI.SwapChainDescription()
        dec.BufferCount <- 1
        dec.Flags <- SharpDX.DXGI.SwapChainFlags.AllowModeSwitch
        dec.IsWindowed <- new SharpDX.Mathematics.Interop.RawBool true
        let mutable modeDescription = new SharpDX.DXGI.ModeDescription()
        modeDescription.Format <- SharpDX.DXGI.Format.B8G8R8A8_UNorm
        modeDescription.Width <- form.ClientSize.Width
        modeDescription.Height <- form.ClientSize.Height
        modeDescription.Scaling <- SharpDX.DXGI.DisplayModeScaling.Stretched
        dec.ModeDescription <- modeDescription
        dec.OutputHandle <- form.Handle
        dec.SampleDescription <- new SharpDX.DXGI.SampleDescription( 4 ,0 ) // MSAA x4
        dec.SwapEffect <- SharpDX.DXGI.SwapEffect.Discard
        dec.Usage <- SharpDX.DXGI.Usage.RenderTargetOutput
        dec
      Device.CreateWithSwapChain(
        SharpDX.Direct3D.DriverType.Hardware,
        DeviceCreationFlags.BgraSupport,
        [| SharpDX.Direct3D.FeatureLevel.Level_11_1 |],   // 機能レベル 11.1
        swpDec,
        d3dDevice ,
        swapChain )
      let d3dDevice = d3dDevice.Value
      let swapChain = swapChain.Value
      immeCxt <- d3dDevice.ImmediateContext
      renderTarget <- swapChain.GetBackBuffer<Texture2D>(0)
      let depthDesc = SharpDXUtil.createDepth renderTarget.Description.Width renderTarget.Description.Height renderTarget.Description.SampleDescription
      depthStencil <- new Texture2D( d3dDevice , depthDesc )
      renderTargetView <- new RenderTargetView( d3dDevice , renderTarget )
      depthStencilView <- new DepthStencilView( d3dDevice , depthStencil )

    let setTargets (depth:DepthStencilView) (target:RenderTargetView) =
      immeCxt.OutputMerger.SetTargets(depth , target)
    member t.Device =
      d3dDevice.Value
    member t.ImmCxt =
      immeCxt
    member t.Clear color =
      immeCxt.ClearRenderTargetView(renderTargetView , color )
      let depAndSte = DepthStencilClearFlags.Depth ||| DepthStencilClearFlags.Stencil
      immeCxt.ClearDepthStencilView(depthStencilView , depAndSte , oneF , byte 0)
    member t.Present() =
      swapChain.Value.Present(0,  SharpDX.DXGI.PresentFlags.None)
    member t.SetVertexShader p = immeCxt.VertexShader.Set(p)
    member t.SetVertexBuffer (slot:int) (buf:VertexBufferBinding) = immeCxt.InputAssembler.SetVertexBuffers( slot , buf)
    member t.SetPixelShader p = immeCxt.PixelShader.Set(p)

    member t.SetHullShader p = immeCxt.HullShader.Set(p)
    member t.SetDomainShader p = immeCxt.DomainShader.Set(p)
    member t.SetGeoShader p = immeCxt.GeometryShader.Set(p)
    member t.SetComputeShader p = immeCxt.ComputeShader.Set(p)

    member t.SetInputLayout p = immeCxt.InputAssembler.InputLayout <- p
    member t.SetPrimitiveTopo p = immeCxt.InputAssembler.PrimitiveTopology <- p
    member t.Draw count loc = immeCxt.Draw( count, loc)
    member t.BlendState p = immeCxt.OutputMerger.SetBlendState p
    member t.RasterState p = immeCxt.Rasterizer.State <- p
    //member t.Viewport w h = 
    //  immeCxt.Rasterizer.SetViewport( 

    member t.RenderTarget =
      renderTarget
    member t.DepthStencil =
      depthStencil
    interface IDisposable with
      member t.Dispose() =
        depthStencilView.Dispose()
        depthStencil.Dispose()
        renderTargetView.Dispose()
        renderTarget.Dispose()
        swapChain.Value.Dispose()
        d3dDevice.Value.Dispose()


