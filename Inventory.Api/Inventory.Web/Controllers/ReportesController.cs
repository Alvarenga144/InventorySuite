using ClosedXML.Excel;
using Inventory.Web.Filters;
using Inventory.Web.Helpers;
using Inventory.Web.Models.ViewModels;
using Inventory.Web.Services;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Inventory.Web.Controllers
{
    [AuthorizeFilter]
    public class ReportesController : Controller
    {
        private readonly IApiService _apiService;

        public ReportesController(IApiService apiService)
        {
            _apiService = apiService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // GET: Reportes/Index
        public async Task<IActionResult> Index(string? vendedorId = null)
        {
            var token = SessionHelper.GetToken(HttpContext.Session);

            // Obtener lista de vendedores para el dropdown
            var vendedoresResponse = await _apiService.GetVendedoresAsync(token);
            if (!vendedoresResponse.Success)
            {
                TempData["ErrorMessage"] = vendedoresResponse.ErrorMessage ?? "Error al cargar vendedores";
                return View(new ReporteVentaCompletoViewModel());
            }

            ViewBag.Vendedores = vendedoresResponse.Data ?? new List<VendedorViewModel>();
            ViewBag.VendedorSeleccionado = vendedorId;

            if (!string.IsNullOrEmpty(vendedorId) || Request.Query.ContainsKey("generar"))
            {
                var reporteResponse = await _apiService.GetReporteVentasAsync(vendedorId, token);
                
                if (!reporteResponse.Success)
                {
                    TempData["ErrorMessage"] = reporteResponse.ErrorMessage ?? "Error al generar el reporte";
                    return View(new ReporteVentaCompletoViewModel());
                }

                return View(reporteResponse.Data ?? new ReporteVentaCompletoViewModel());
            }

            return View(new ReporteVentaCompletoViewModel());
        }

        // GET: Reportes/DescargarPDF
        public async Task<IActionResult> DescargarPDF(string? vendedorId = null)
        {
            var token = SessionHelper.GetToken(HttpContext.Session);

            // Obtener datos del reporte
            var reporteResponse = await _apiService.GetReporteVentasAsync(vendedorId, token);
            
            if (!reporteResponse.Success || reporteResponse.Data == null)
            {
                TempData["ErrorMessage"] = reporteResponse.ErrorMessage ?? "Error al generar el reporte";
                return RedirectToAction(nameof(Index), new { vendedorId });
            }

            var reporte = reporteResponse.Data;

            // Generar el PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);

                    // Header
                    page.Header().Element(ComposeHeader);

                    // Content
                    page.Content().Element(content => ComposeContent(content, reporte, vendedorId));

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            var pdfBytes = document.GeneratePdf();
            var fileName = $"ReporteVentas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // GET: Reportes/DescargarExcel
        public async Task<IActionResult> DescargarExcel(string? vendedorId = null)
        {
            var token = SessionHelper.GetToken(HttpContext.Session);

            var reporteResponse = await _apiService.GetReporteVentasAsync(vendedorId, token);
            
            if (!reporteResponse.Success || reporteResponse.Data == null)
            {
                TempData["ErrorMessage"] = reporteResponse.ErrorMessage ?? "Error al generar el reporte";
                return RedirectToAction(nameof(Index), new { vendedorId });
            }

            var reporte = reporteResponse.Data;

            // Crear el archivo Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte de Ventas");

                // Título
                worksheet.Cell(1, 1).Value = "REPORTE DE VENTAS";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Range(1, 1, 1, 7).Merge();

                // Filtro aplicado
                var filtro = string.IsNullOrEmpty(vendedorId) ? "Todos los vendedores" : 
                            reporte.Ventas.FirstOrDefault()?.NombreVendedor ?? "N/A";
                worksheet.Cell(2, 1).Value = $"Vendedor: {filtro}";
                worksheet.Range(2, 1, 2, 7).Merge();

                // Fecha de generación
                worksheet.Cell(3, 1).Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Range(3, 1, 3, 7).Merge();

                int currentRow = 5;

                // Headers
                var headers = new[] { "ID Venta", "Fecha", "Vendedor", "Producto", "Cantidad", "Precio Unit.", "IVA", "Total" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(currentRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                currentRow++;

                // Datos
                foreach (var venta in reporte.Ventas)
                {
                    bool primeraLinea = true;
                    foreach (var detalle in venta.Detalles)
                    {
                        if (primeraLinea)
                        {
                            worksheet.Cell(currentRow, 1).Value = venta.IdVenta;
                            worksheet.Cell(currentRow, 2).Value = venta.Fecha.ToString("dd/MM/yyyy");
                            worksheet.Cell(currentRow, 3).Value = venta.NombreVendedor;
                        }
                        worksheet.Cell(currentRow, 4).Value = detalle.NombreProducto;
                        worksheet.Cell(currentRow, 5).Value = detalle.Cantidad;
                        worksheet.Cell(currentRow, 6).Value = detalle.PrecioUnitario;
                        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "$#,##0.00";
                        worksheet.Cell(currentRow, 7).Value = detalle.Iva;
                        worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "$#,##0.00";
                        worksheet.Cell(currentRow, 8).Value = detalle.TotalDetalle;
                        worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "$#,##0.00";

                        primeraLinea = false;
                        currentRow++;
                    }

                    // Subtotal de venta
                    worksheet.Cell(currentRow, 7).Value = "Total Venta:";
                    worksheet.Cell(currentRow, 7).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 8).Value = venta.Total;
                    worksheet.Cell(currentRow, 8).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "$#,##0.00";
                    worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.LightYellow;
                    currentRow++;

                    // Línea en blanco entre ventas
                    currentRow++;
                }

                // Resumen General
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = "RESUMEN GENERAL";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
                worksheet.Range(currentRow, 1, currentRow, 8).Merge();
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Total de Ventas:";
                worksheet.Cell(currentRow, 2).Value = reporte.Resumen.TotalVentas;
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Productos Vendidos:";
                worksheet.Cell(currentRow, 2).Value = reporte.Resumen.TotalProductosVendidos;
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "IVA Total:";
                worksheet.Cell(currentRow, 2).Value = reporte.Resumen.IvaTotal;
                worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "$#,##0.00";
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "MONTO TOTAL:";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 2).Value = reporte.Resumen.MontoTotal;
                worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 2).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.LightGreen;

                // Ajustar anchos de columna
                worksheet.Columns().AdjustToContents();

                // Generar el archivo
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var fileName = $"ReporteVentas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        #region Métodos auxiliares para PDF

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("INVENTORY SUITE").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().AlignCenter().Text("Reporte de Ventas").FontSize(14).SemiBold();
                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });
        }

        private void ComposeContent(IContainer container, ReporteVentaCompletoViewModel reporte, string? vendedorId)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(10);

                // Información del filtro
                var vendedorNombre = string.IsNullOrEmpty(vendedorId) ? "Todos los vendedores" : 
                                    reporte.Ventas.FirstOrDefault()?.NombreVendedor ?? "N/A";
                column.Item().Text($"Vendedor: {vendedorNombre}").FontSize(11);

                // Tabla de ventas
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);  // ID
                        columns.ConstantColumn(80);  // Fecha
                        columns.RelativeColumn(2);   // Vendedor
                        columns.RelativeColumn(2);   // Producto
                        columns.ConstantColumn(50);  // Cantidad
                        columns.ConstantColumn(70);  // P. Unit
                        columns.ConstantColumn(60);  // IVA
                        columns.ConstantColumn(70);  // Total
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("ID").FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Fecha").FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Vendedor").FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Producto").FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Cant.").FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("P. Unit.").FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("IVA").FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Total").FontSize(9).Bold();
                    });

                    // Contenido
                    foreach (var venta in reporte.Ventas)
                    {
                        bool primeraLinea = true;
                        foreach (var detalle in venta.Detalles)
                        {
                            table.Cell().Border(0.5f).Padding(3).Text(primeraLinea ? venta.IdVenta.ToString() : "").FontSize(8);
                            table.Cell().Border(0.5f).Padding(3).Text(primeraLinea ? venta.Fecha.ToString("dd/MM/yyyy") : "").FontSize(8);
                            table.Cell().Border(0.5f).Padding(3).Text(primeraLinea ? venta.NombreVendedor : "").FontSize(8);
                            table.Cell().Border(0.5f).Padding(3).Text(detalle.NombreProducto).FontSize(8);
                            table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(detalle.Cantidad.ToString()).FontSize(8);
                            table.Cell().Border(0.5f).Padding(3).AlignRight().Text($"${detalle.PrecioUnitario:N2}").FontSize(8);
                            table.Cell().Border(0.5f).Padding(3).AlignRight().Text($"${detalle.Iva:N2}").FontSize(8);
                            table.Cell().Border(0.5f).Padding(3).AlignRight().Text($"${detalle.TotalDetalle:N2}").FontSize(8);

                            primeraLinea = false;
                        }

                        // Total de la venta
                        table.Cell().ColumnSpan(6).Background(Colors.Yellow.Lighten3).Border(0.5f).Padding(3).AlignRight().Text("Total Venta:").FontSize(8).Bold();
                        table.Cell().ColumnSpan(2).Background(Colors.Yellow.Lighten3).Border(0.5f).Padding(3).AlignRight().Text($"${venta.Total:N2}").FontSize(8).Bold();
                    }
                });

                // Resumen
                column.Item().PaddingTop(20).Column(resumen =>
                {
                    resumen.Item().Text("RESUMEN GENERAL").FontSize(12).Bold();
                    resumen.Item().PaddingTop(5).Text($"Total de Ventas: {reporte.Resumen.TotalVentas}").FontSize(10);
                    resumen.Item().Text($"Productos Vendidos: {reporte.Resumen.TotalProductosVendidos}").FontSize(10);
                    resumen.Item().Text($"IVA Total: ${reporte.Resumen.IvaTotal:N2}").FontSize(10);
                    resumen.Item().Text($"MONTO TOTAL: ${reporte.Resumen.MontoTotal:N2}").FontSize(12).Bold().FontColor(Colors.Green.Darken2);
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Generado el: ").FontSize(8);
                text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).Bold();
                text.Span(" | Página ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
                text.Span(" de ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        }

        #endregion
    }
}

