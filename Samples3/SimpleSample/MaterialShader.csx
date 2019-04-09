void Initialize()
{
    CreateVertexShader( 0, "cso/VertexShaderForObject.cso" );
    CreateVertexShader( 1, "cso/VertexShaderForEdge.cso" );
    CreateHullShader( 0, "cso/HullShader.cso" );
    CreateDomainShader( 0, "cso/DomainShader.cso" );
    CreateGeometryShader( 0, "cso/GeometryShader.cso" );
    CreatePixelShader( 0, "cso/PixelShaderForObject.cso" );
    CreatePixelShader( 1, "cso/PixelShaderForEdge.cso" );
}

void Run()
{
    switch( MMDPass )
    {
        case MMDPass.Edge:
            SetVertexShader( 1 );
            SetHullShader( 0 );
            SetDomainShader( 0 );
            SetGeometryShader( 0 );
            SetPixelShader( 1 );
            break;

        default:
            SetVertexShader( 0 );
            SetHullShader( 0 );
            SetDomainShader( 0 );
            SetGeometryShader( 0 );
            SetPixelShader( 0 );
            break;
    }

    Draw();
}
