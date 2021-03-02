namespace MMDEng
open System
open System.Diagnostics
open System.Windows.Forms
open SharpDX.Windows
open SharpDX
open SharpDXUtil
open System.Text
open System.IO

module Main =
  let initScene (form :MMDRenderer.MMDForm) =
    form.AddMouseCam()
    form.AddDefaultLight()
    let exePath = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName
    let sp = Path.DirectorySeparatorChar |> string
    let cp () = 
      let builder = new StringBuilder(Directory.GetParent(exePath).Parent.Parent.Parent.FullName)
      let b2 = builder.Append(sp).Append("Samples")
      b2.Append(sp).Append("ニコニ立体ちゃんサンプル").Append( sp).Append( "サンプルデータ").Append( sp).Append( "Alicia").Append( sp)
    let modelPath = cp().Append( "MMD").Append(sp).Append("Alicia_solid.pmx").ToString()
    let motionPath = cp().Append("MMD Motion").Append(sp).Append("2分ループステップ1.vmd").ToString()
    let mutable chara = form.AddChara modelPath
    form.ApplyAnim motionPath chara.Value
    chara
 
  [<EntryPoint; STAThread>]
  let main argv =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)
    let form = new MMDRenderer.MMDForm()
    let mutable chara = initScene form
    let onUpdate () =
      match chara with
      |SomeD c->
        let pos = c.WorldTransformationMatrix.TranslationVector
        let newPos = pos + (SharpDXUtil.vec3 0.01f 0.0f 0.0f)
        if pos.X > 22.0f then
          form.ResetEnv()
          chara <- initScene form
        c.WorldTransformationMatrix<- Matrix.Translation newPos
      |NoneD -> ()
      form.OnUpdate()
    let run e = 
      RenderLoop.Run(form , onUpdate)
    Application.Idle.Add run
    form.FormClosing.Add form.OnClose
    try 
      Application.Run(form)
      form.Dispose()
    with e-> 
      Debug.WriteLine e
    0 // return an integer exit code
