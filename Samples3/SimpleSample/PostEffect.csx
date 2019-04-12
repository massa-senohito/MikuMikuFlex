void Initialize()
{
    CreateComputeShaderFromHLSL( 0, "cso/ComputeShader.hlsl" );
}

void Run()
{
    SetComputeShader( 0 );
    Blit( 1280/8, 720/8, 1 );
}
