namespace MMDEng
open System
open System.Diagnostics
open System.Windows.Forms
open SharpDX.Windows
open SharpDX

module Main =

  [<EntryPoint; STAThread>]
  let main argv =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)
    let form = new MMDRenderer.MMDForm()
    form.AddMouseCam()
    form.AddDefaultLight()
    let mutable chara = form.AddChara @""
    form.ApplyAnim @"" chara.Value
    let onUpdate () =
      match chara with
      |Some c->
        let pos = c.WorldTransformationMatrix.TranslationVector
        let newPos = pos + (SharpDXUtil.vec3 0.00f 0.0f 0.0f)
        c.WorldTransformationMatrix<- Matrix.Translation newPos
      |None -> ()
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
