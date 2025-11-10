BEGIN TRANSACTION;

    -- 1. Quitar detalle primero (evita restricciones por FK)
    DELETE FROM detalleventas;
    DBCC CHECKIDENT ('detalleventas', RESEED, 0);

    -- 2. Ventas (FK a usuarios permanece intacta)
    DELETE FROM ventas;
    DBCC CHECKIDENT ('ventas', RESEED, 0);

    -- 3. Productos
    DELETE FROM productos;
    DBCC CHECKIDENT ('productos', RESEED, 0);

COMMIT TRANSACTION;