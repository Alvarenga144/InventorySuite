-- =============================================
-- Stored Procedure: sp_ReporteVentas
-- Descripción: Genera reporte de ventas con encabezado y detalles
-- Parámetros: @VendedorId (opcional) - Filtra por vendedor específico
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_ReporteVentas]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_ReporteVentas]
GO

CREATE PROCEDURE [dbo].[sp_ReporteVentas]
    @VendedorId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @VendedorId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM usuarios WHERE Id = @VendedorId)
    BEGIN
        RAISERROR('El vendedor especificado no existe', 16, 1);
        RETURN;
    END

    -- Obtener ventas con detalles
    SELECT 
        v.idventa AS IdVenta,
        v.fecha AS Fecha,
        v.total AS Total,
        v.vendedor_id AS VendedorId,
        CONCAT(u.Nombre, ' ', u.Apellido) AS NombreVendedor,
        u.Email AS EmailVendedor,
        
        -- Detalles de la venta
        dv.idde AS IdDetalle,
        dv.idpro AS IdProducto,
        p.producto AS NombreProducto,
        dv.cantidad AS Cantidad,
        dv.precio AS PrecioUnitario,
        dv.iva AS Iva,
        dv.total AS TotalDetalle
    FROM 
        ventas v
        INNER JOIN usuarios u ON v.vendedor_id = u.Id
        INNER JOIN detalleventas dv ON v.idventa = dv.idventa
        INNER JOIN productos p ON dv.idpro = p.idpro
    WHERE 
        (@VendedorId IS NULL OR v.vendedor_id = @VendedorId)
        AND p.activo = 1 -- Solo productos activos
    ORDER BY 
        v.fecha DESC, 
        v.idventa DESC, 
        dv.idde ASC;

    -- Obtener resumen general
    SELECT 
        COUNT(DISTINCT v.idventa) AS TotalVentas,
        ISNULL(SUM(v.total), 0) AS MontoTotal,
        ISNULL(SUM(dv.iva), 0) AS IvaTotal,
        COUNT(dv.idde) AS TotalProductosVendidos
    FROM 
        ventas v
        LEFT JOIN detalleventas dv ON v.idventa = dv.idventa
    WHERE 
        (@VendedorId IS NULL OR v.vendedor_id = @VendedorId);
END
GO
