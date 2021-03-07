namespace MMDEng
open MikuMikuFlex3
open SharpDX
open System
open System.IO
open SharpDX.D3DCompiler
open System.Diagnostics
open SharpDX.Direct3D11
open SharpDX.Direct3D

module ShaderFacadeModule =
  let tryCompileShader (hlslPath:string) (profile:string) (entryPoint) =
    let mutable compileResult = None
    try
      use fs = new FileStream( hlslPath , FileMode.Open , FileAccess.Read , FileShare.Read )
      let buffer = Array.zeroCreate<byte> (int fs.Length)
      let read = fs.Read(buffer , 0 ,buffer.Length)
      let mutable flags = ShaderFlags.None
#if DEBUG
      flags <- flags ||| ShaderFlags.Debug ||| ShaderFlags.SkipOptimization ||| ShaderFlags.EnableBackwardsCompatibility;
#endif
      let mutable effFlags = EffectFlags.None // ChildEffect AllowShowOptions
      let shaderName = Path.GetFileName hlslPath
      let res = ShaderBytecode.Compile(buffer , entryPoint , profile , flags , effFlags , shaderName)
      compileResult <- Some <| res
      Debug.WriteLine $"コンパイルしました {hlslPath} {res.Message}"
     with e->
       Debug.WriteLine $"ファイルからのシェーダーの作成に失敗しました。[{hlslPath}][{e.Message}]"
    compileResult

  let tryCreateVertexShader hlslPath =
    let res = tryCompileShader hlslPath "vs_5_0" "main"
    //let shader = new VertexShader(device , res.Value.Bytecode.Data)
    //shader
    res

  let tryCreatePixelShader hlslPath =
    let res = tryCompileShader hlslPath "ps_5_0" "main"
    //let shader = new PixelShader(device , res.Value.Bytecode.Data)
    //shader
    res

  let createShaderFromCompiled csoFilePath =
    let mutable shaderByte = null
    try
        use fs = new FileStream( csoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read )
        let buffer = Array.zeroCreate<byte> <| int fs.Length
        fs.Read( buffer, 0, buffer.Length )
        shaderByte <- buffer
    with e->
      Debug.WriteLine $"ファイルからのシェーダーの作成に失敗しました。[{csoFilePath}][{e.Message}]"
    shaderByte

  let setVSSampler (context:DeviceContext) slot state =
    context.VertexShader.SetSampler (slot,state)
  let setVSCBuffer (context:DeviceContext) slot buffer =
    context.VertexShader.SetConstantBuffer (slot,buffer)
  let setVSResource (context:DeviceContext) slot resource =
    context.VertexShader.SetShaderResource (slot,resource)

  let setPSSampler (context:DeviceContext) slot state =
    context.PixelShader.SetSampler (slot,state)
  let setPSCBuffer (context:DeviceContext) slot buffer =
    context.PixelShader.SetConstantBuffer (slot,buffer)
  let setPSResource (context:DeviceContext) slot resource =
    context.PixelShader.SetShaderResource (slot,resource)

  let setCSSampler (context:DeviceContext) slot state =
    context.ComputeShader.SetSampler (slot,state)
  let setCSCBuffer (context:DeviceContext) slot buffer =
    context.ComputeShader.SetConstantBuffer (slot,buffer)
  let setCSResource (context:DeviceContext) slot resource =
    context.ComputeShader.SetShaderResource (slot,resource)
  let setCSUAV (context:DeviceContext) slot uav =
    context.ComputeShader.SetUnorderedAccessView (slot,uav)

  let createIL device shaderByte elemList =
    new InputLayout(device , shaderByte , elemList)

  let createPositionColorIL device shaderByte =
    let elemList = [|
       new InputElement("POSITION", 0,SharpDXUtil.RGBA32Float, 0, 0)
       new InputElement("COLOR", 0, SharpDXUtil.RGBA32Float,16, 0)
    |]
    createIL device shaderByte elemList
  let createPositionUVIL device shaderByte =
    let elemList = [|
       new InputElement("POSITION", 0,SharpDXUtil.RGBA32Float, 0, 0)
       new InputElement("TEXCOORD", 0, SharpDXUtil.RG32Float,16, 0)
    |]
    createIL device shaderByte elemList

  let createWrapSampler device =
    let mutable desc = new SamplerStateDescription()
    desc.Filter <- Filter.MinMagMipLinear
    desc.AddressU <- TextureAddressMode.Wrap
    desc.AddressV <- TextureAddressMode.Wrap
    desc.AddressW <- TextureAddressMode.Wrap
    desc.MipLodBias <- 0.0f
    desc.MaximumAnisotropy <- 1
    desc.ComparisonFunction <- Comparison.Always
    desc.BorderColor <- SharpDXUtil.rawColorBlack // Black Border.
    desc.MinimumLod <- 0.0f
    desc.MaximumLod <- System.Single.MaxValue
    new SamplerState(device,desc)
  let pixBegin color (name:string) =
    PixHelper.BeginEvent (color , name)
  let pixBeginBlack = pixBegin SharpDXUtil.rawColorBlackRGBA
  let pixEnd ()= PixHelper.EndEvent()

  type DebugShader( device:Renderer.RenderDevice ) =
    let mutable vShaderByte = null
    let mutable vShader = null
    let mutable pShaderByte = null
    let mutable pShader = null
    let mutable ia = null
    let mutable lineList = []
    do
      //vShaderByte <- createShaderFromCompiled "DebugShadervs.vso"
      let vShaderRes = tryCreateVertexShader "Minitrivs.fx"
      vShaderByte <- vShaderRes.Value.Bytecode.Data
      vShader <- new VertexShader(device.Device , vShaderByte)
      //pShaderByte <- createShaderFromCompiled "DebugShaderps.pso"
      let pShaderRes = tryCreatePixelShader "Minitrips.fx"
      pShaderByte <- pShaderRes.Value.Bytecode.Data
      pShader <- new PixelShader(device.Device , pShaderByte)
      ia <- createPositionColorIL device.Device vShaderByte
    member t.AddLine startP endP =
      let vList = [startP ; Vector4.One ; endP ; Vector4.One]
      lineList <- lineList @ vList
    member t.DrawLine () =
      if lineList.Length > 0 then
        use buffer = Buffer.Create(device.Device , BindFlags.VertexBuffer , Array.ofList lineList)
        let bbind = new VertexBufferBinding(buffer , Vector4.SizeInBytes * 2 , 0)
        device.SetVertexShader vShader
        device.SetPixelShader pShader
        device.SetHullShader null
        device.SetDomainShader null
        device.SetGeoShader null
        device.SetInputLayout ia
        device.SetPrimitiveTopo PrimitiveTopology.LineList
        device.SetVertexBuffer 0 bbind
        let len =lineList.Length / 4 - 1
        let loop = [0 .. len] 
        for i in loop do
          device.Draw 2 (i * 2)
        lineList <-[]

    interface IDisposable with
      member t.Dispose() =
        vShader.Dispose()
        pShader.Dispose()
        ia.Dispose()
   
