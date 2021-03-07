namespace MMFlexUtil
open BulletSharp
open BulletSharp.Math
open System.Diagnostics

module BulletUtil =
  type Vec3 = Vector3
  type Mtx = Matrix
  type f32 = float32
  type Text3D= {
    Loc:Vec3;
    Text:string;
  }
  let makeText3D loc text=
    {
      Loc = loc;
      Text = text;
    }
  type DebugBox = {
    Color:Vec3;
    Max:Vec3;
    Min:Vec3;
    World : Mtx
  }
  let makeDebugBox color max min mtx=
    {
      Min = min;
      Max = max;
      Color = color;
      World = mtx
    }
  let makeDebugAABB color max min =
    makeDebugBox color max min Mtx.Identity

  type DebugCapsule= {
    Color:Vec3;
    HalfHeight:f32;
    Radius:f32;
    Trans:Mtx;
    UpAxis:int;
  }
  let makeDebugCapsule color halfHeight radius trans upAxis=
    {
      Radius = radius;
      HalfHeight = halfHeight;
      UpAxis = upAxis;
      Trans = trans;
      Color = color;
    }
  type DebugContact= {
    Color:Vec3;
    Distance:f32;
    LifeTime:int;
    NormalOnB:Vec3;
    PointOnB:Vec3;
  }
  let makeDebugContact color distance lifeTime normalOnB pointOnB=
    {
      PointOnB = pointOnB;
      NormalOnB = normalOnB;
      Distance = distance;
      LifeTime = lifeTime;
      Color = color;
    }
  type DebugCylinder= {
    Color:Vec3;
    Halfheight:f32;
    Radius:f32;
    Transform:Mtx;
    Upaxis:int;
  }
  let makeDebugCylinder color halfheight radius transform upaxis=
    {
      Radius = radius;
      Halfheight = halfheight;
      Upaxis = upaxis;
      Transform = transform;
      Color = color;
    }
  type DebugLine= {
    From:Vec3;
    FromColor:Vec3;
    To:Vec3;
    ToColor:Vec3;
  }
  let makeDebugLine from fromColor top toColor=
    {
      From = from;
      To = top;
      FromColor = fromColor;
      ToColor = toColor;
    }
  type DebugPlane= {
    Color:Vector3;
    PlaneConst:f32;
    PlaneNormal:Vec3;
    Transform:Matrix;
  }
  let makeDebugPlane color planeConst planeNormal transform=
    {
      PlaneNormal = planeNormal;
      PlaneConst = planeConst;
      Transform = transform;
      Color = color;
    }
  type DebugSphere= {
    Color:Vector3;
    Radius:f32;
    Transform:Matrix;
  }
  let makeDebugSphere color radius transform=
    {
      Radius = radius;
      Transform = transform;
      Color = color;
    }
  type DebugSpherePatch= {
    Axis:Vec3;
    Center:Vec3;
    Color:Vec3;
    MaxPs:f32;
    MaxTh:f32;
    MinPs:f32;
    MinTh:f32;
    Radius:f32;
    Up:Vec3;
  }
  let makeDebugSpherePatch axis center color maxPs maxTh minPs minTh radius up=
    {
      Center = center;
      Up = up;
      Axis = axis;
      Radius = radius;
      MinTh = minTh;
      MaxTh = maxTh;
      MinPs = minPs;
      MaxPs = maxPs;
      Color = color;
    }
  type DebugSpherePatch2= {
    Axis:Vec3;
    Center:Vec3;
    Color:Vec3;
    MaxPs:f32;
    MaxTh:f32;
    MinPs:f32;
    MinTh:f32;
    Radius:f32;
    StepDegrees:f32;
    Up:Vec3;
  }
  let makeDebugSpherePatch2 axis center color maxPs maxTh minPs minTh radius stepDegrees up=
    {
      Center = center;
      Up = up;
      Axis = axis;
      Radius = radius;
      MinTh = minTh;
      MaxTh = maxTh;
      MinPs = minPs;
      MaxPs = maxPs;
      Color = color;
      StepDegrees = stepDegrees;
    }
  type DebugTransform= {
    OrthoLen:f32;
    Transform:Matrix;
  }
  let makeDebugTransform orthoLen transform=
    {
      Transform = transform;
      OrthoLen = orthoLen;
    }
  type DebugTriangle= {
    Alpha:f32;
    Color:Vec3;
    N0:Vec3;
    N1:Vec3;
    N2:Vec3;
    V0:Vec3;
    V1:Vec3;
    V2:Vec3;
  }
  let makeDebugTriangle alpha color n0 n1 n2 v0 v1 v2=
    {
      V0 = v0;
      V1 = v1;
      V2 = v2;
      N0 = n0;
      N1 = n1;
      N2 = n2;
      Color = color;
      Alpha = alpha;
    }

  type SharpDXBulletDrawer() = 
    inherit DebugDraw()
    let mutable debugMode = DebugDrawModes.DrawWireframe
    let mutable text3dList = []
    let mutable debugBoxlist = []
    let mutable debugCapsulelist = []
    let mutable debugContactlist = []
    let mutable debugCylinderlist = []
    let mutable debugLinelist = []
    let mutable debugPlanelist = []
    let mutable debugSpherelist = []
    let mutable debugTransformlist = []
    let mutable debugTrianglelist = []
    member t.Present() =
      text3dList <- []
      debugBoxlist <- []
      debugCapsulelist <- []
      debugContactlist <- []
      debugCylinderlist <- []
      debugLinelist <- []
      debugPlanelist <- []
      debugSpherelist <- []
      debugTransformlist <- []
      debugTrianglelist <- []
    member t.LineList = debugLinelist
    override t.DebugMode = debugMode
    override t.set_DebugMode v= debugMode <- v
    override t.Draw3dText( loc , str ) =
      text3dList <- makeText3D loc str :: text3dList
    override t.DrawAabb( minP , maxP , color ) =
      debugBoxlist<- makeDebugAABB color maxP minP :: debugBoxlist
    override t.DrawArc( center , normal , axis , radA , radB , minAng , maxAng , color , drawSect) = ()
    override t.DrawArc( center , normal , axis , radiusA , radiusB , minAngle , maxAngle , color , drawSect , stepDegrees ) = ()
    override t.DrawBox( minP , maxP , color ) =
      debugBoxlist<- makeDebugAABB color minP maxP :: debugBoxlist
    override t.DrawBox( minP , maxP , trans , color ) =
      debugBoxlist<- makeDebugBox color minP maxP trans :: debugBoxlist 
    override t.DrawCapsule( radius , halfHeight , upAxis , trans , color )= 
      debugCapsulelist<- makeDebugCapsule color halfHeight radius trans upAxis :: debugCapsulelist
    override t.DrawCone( radius , height , upAxis , transform , color ) = ()
    override t.DrawContactPoint( pointOnB , normalOnB , distance , lifeTime , color ) =
      debugContactlist<- makeDebugContact color distance lifeTime normalOnB pointOnB :: debugContactlist
    override t.DrawCylinder( radius , halfHeight , upAxis , transform , color ) =
      debugCylinderlist<- makeDebugCylinder color halfHeight radius transform upAxis :: debugCylinderlist
    override t.DrawLine ( from , top , fromColor ) =
      debugLinelist<- makeDebugLine from fromColor top fromColor :: debugLinelist
    override t.DrawLine ( from , fromColor , top , toColor) =
      debugLinelist<- makeDebugLine from fromColor top toColor :: debugLinelist
    override t.DrawPlane(planeNormal , planeConst , transform , color ) =
      debugPlanelist<- makeDebugPlane color planeConst planeNormal transform :: debugPlanelist
    override t.DrawSphere( (pos:byref<Vec3>) , (radius:f32) , (color:byref<Vec3>) )=
      let trans = BulletSharp.Math.Matrix.Translation pos
      debugSpherelist<- makeDebugSphere color radius trans :: debugSpherelist
    override t.DrawSphere( (radius:f32) , (transform:byref<Matrix>) , (color:byref<Vec3>) )=
      debugSpherelist<- makeDebugSphere color radius transform :: debugSpherelist
    override t.DrawSpherePatch( center , up , axis , radius , minTh , maxTh , minPs , maxPs , color )=
      ()
    override t.DrawSpherePatch( center , up , axis , radius , minTh , maxTh , minPs , maxPs , color , stepDegrees )=
      ()
    override t.DrawTransform( transform , orthoLen ) =
      debugTransformlist<- makeDebugTransform orthoLen transform :: debugTransformlist
    override t.DrawTriangle( v0 , v1 , v2 , n0 , n1 , n2 , color , alpha )=
      debugTrianglelist<- makeDebugTriangle alpha color n0 n1 n2 v0 v1 v2 :: debugTrianglelist

    override t.DrawTriangle( v0 , v1 , v2 , color , alpha )=
      let norm = Vec3.Zero
      debugTrianglelist<- makeDebugTriangle alpha color norm norm norm v0 v1 v2 :: debugTrianglelist
    override t.ReportErrorWarning( str )=
      Debug.WriteLine(str)
