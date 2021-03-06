﻿namespace MMDEng

open SharpDX
open SharpDX.Mathematics
open SharpDX.Mathematics.Interop
open SharpDX.Direct3D11
open SharpDX.Windows
open System
module SharpDXUtil =
  let zeroF = 0.0f
  let oneF = 1.0f
  let color3 r g b = new Color3(r,g,b) 
  let colorA1 r g b = new Color4(r,g,b,oneF) 
  let rawColorA1 r g b = new RawColor4 (r,g,b,oneF)

  let rawBool b = new RawBool (b)
  let texDec w h mip array format sample usage bind cpu option =
    let mutable desc = new Texture2DDescription()
    desc.Width <- w
    desc.Height<- h
    desc.MipLevels <- mip
    desc.ArraySize <- array
    desc.Format <- format
    desc.SampleDescription <- sample
    desc.BindFlags <- bind
    desc.CpuAccessFlags <- cpu
    desc.OptionFlags <- option
    desc

  let createDepth w h samp = 
    let dTex = texDec w h 1 1 SharpDX.DXGI.Format.D24_UNorm_S8_UInt samp ResourceUsage.Default
    dTex BindFlags.DepthStencil CpuAccessFlags.None ResourceOptionFlags.None

  let RGBA32Float =
    DXGI.Format.R32G32B32A32_Float
  let RG32Float =
    DXGI.Format.R32G32_Float
  let colorBlack =
    new Color4(zeroF , zeroF ,zeroF , zeroF) 

  let rawColorBlack =
    let mutable c = new RawColor4 ()
    c.R <- zeroF
    c.G <- zeroF
    c.B <- zeroF
    c.A <- zeroF
    c
  let rawColorBlackRGBA =
    let mutable c = new RawColorBGRA ()
    c.R <- 0uy
    c.G <- 0uy
    c.B <- 0uy
    c.A <- 0uy
    c

  let fillViewPort w h =
    new ViewportF( zeroF , zeroF , w , h , zeroF , oneF );

  let alphaBlendStateDesc =
    let mutable desc = new BlendStateDescription()
    desc.AlphaToCoverageEnable <- rawBool false // alpha透過
    desc.IndependentBlendEnable <- rawBool false  // 1~7も同じ設定
    let mutable target = desc.RenderTarget.[0]
    target.IsBlendEnabled <- rawBool true // alphablend
    target.RenderTargetWriteMask <- ColorWriteMaskFlags.All // rgba描画
    // alpha
    target.SourceAlphaBlend <- BlendOption.One
    target.DestinationAlphaBlend <- BlendOption.Zero
    target.AlphaBlendOperation <- BlendOperation.Add
    // color 加算合成
    target.SourceBlend <- BlendOption.SourceAlpha
    target.DestinationBlend <- BlendOption.One
    target.BlendOperation <- BlendOperation.Add
    desc.RenderTarget.[0] <- target
    desc

  let normalRasterStateDesc =
    let mutable desc = new RasterizerStateDescription()
    desc.CullMode <- CullMode.Back
    // ワイヤーフレームなども設定できる
    desc.FillMode <- FillMode.Solid
    desc
  type OptionDisposable<'a when 'a :>IDisposable> =
    |SomeD of 'a
    |NoneD
    member t.Attempt f =
      match t with
      |SomeD d-> f d
      |NoneD  -> ()
    member t.Value =
      match t with
      |SomeD d-> d
      |NoneD  -> assert(false);Unchecked.defaultof<'a>

  type OPDBuilder ()=
    member t.Bind (m,f) =
      match m with
      |SomeD d-> f d
      |NoneD  -> NoneD
    member t.Return m =
      SomeD m
    member t.Zero () =
      NoneD
  let attemtDispose (mayD:OptionDisposable<'a> byref ) =
    match mayD with
    |SomeD d->d.Dispose(); mayD <- NoneD
    |NoneD  -> ()

module DXUtilV =
  let inline f32<'a> i = float32 i
  let vec3 x y z = new Vector3(x,y,z)
  let vec4 x y z = new Vector4(x,y,z,1.0f)
  let vec4i (x:int) y z = vec4 (f32 x) (f32 y) (f32 z)
  let transform (v:Vector4) (m:Matrix) = Vector4.Transform(v,m)
  let bulletV3ToV4 (v:BulletSharp.Math.Vector3) = vec4 v.X v.Y v.Z
