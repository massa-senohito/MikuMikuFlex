void Initialize()
{
    CreateComputeShaderFromHLSL( 0, "cso/ComputeShader.hlsl" );
}

void Run()
{
    SetComputeShader( 0 );

    var viewSize = GetViewportSize();
    Blit( viewSize.Width/8, viewSize.Height/8, 1 );
}
