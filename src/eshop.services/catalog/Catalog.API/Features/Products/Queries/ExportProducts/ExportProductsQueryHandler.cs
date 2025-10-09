using BuildingBlocks.CQRS;
using Catalog.API.Models;
using ClosedXML.Excel;
using Marten;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Handles the execution of the <see cref="ExportProductsQuery"/> and retrieves the corresponding
/// </summary>
/// <param name="documentSession">The document session to connect to the DB</param>
public class ExportProductsQueryHandler(IDocumentSession documentSession)
    : IQueryHandler<ExportProductsQuery, ExportProductsQueryResult>
{
    public async Task<ExportProductsQueryResult> Handle(ExportProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await documentSession.Query<Product>()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Products");
        
        // Header
        var headers = new[]
        {
            "Id",
            "Name",
            "Description",
            "Price",
            "Image File",
            "Categories"
        };
        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }
        
        // Content
        var row = 2;
        foreach (var product in products)
        {
            ws.Cell(row, 1).Value = product.Id.ToString();
            ws.Cell(row, 2).Value = product.Name;
            ws.Cell(row, 3).Value = product.Description;
            ws.Cell(row, 4).Value = product.Price;
            ws.Cell(row, 5).Value = product.ImageFile;
            ws.Cell(row, 6).Value = string.Join(", ", product.Categories);
            row++;
        }
        
        // Format
        ws.Column(4).Style.NumberFormat.Format = "#,##0.00";
        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);
        
        // Save
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var content = ms.ToArray();
        const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var fileName = $"products_{DateTime.UtcNow:dd/MM/yyyy}.xlsx";
        
        return new ExportProductsQueryResult(content, contentType, fileName);
    }
}