using System.Data;
using System.Globalization;
using BuildingBlocks.CQRS;
using Catalog.API.Models;
using ExcelDataReader;
using Marten;

namespace Catalog.API.Features.Products.Commands.ImportProducts;

/// <summary>
/// Handler for importing products from a xlsx file
/// </summary>
/// <param name="documentSession">The document session</param>
public class ImportProductsCommandHandler(IDocumentSession documentSession)
    : ICommandHandler<ImportProductsCommand, ImportProductsCommandResult>
{
    /// <summary>
    /// Handle the import products command
    /// </summary>
    /// <param name="request">The request containing the file</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task representing the result, contains the result</returns>
    public async Task<ImportProductsCommandResult> Handle(ImportProductsCommand request,
        CancellationToken cancellationToken)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        int inserted = 0, updated = 0, failed = 0;
        var errorList = new List<string>();

        await using var stream = request.File.OpenReadStream();
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        });

        if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
        {
            return new ImportProductsCommandResult(InsertedCount: inserted, UpdatedCount: updated, FailedCount: failed,
                Errors: errorList);
        }

        var table = dataSet.Tables[0];

        foreach (DataRow row in table.Rows)
        {
            try
            {
                var idText = row.Table.Columns.Contains("Id") ? row["Id"].ToString() : null;
                var name = row.Table.Columns.Contains("Name") ? row["Name"].ToString()?.Trim() ?? "" : "";
                var description = row.Table.Columns.Contains("Description") ? row["Description"].ToString() ?? "" : "";
                var priceText = row.Table.Columns.Contains("Price") ? row["Price"].ToString()?.Replace(',', '.') : null;
                var imageFile = row.Table.Columns.Contains("ImageFile") ? row["ImageFile"].ToString() ?? "" : "";
                var categoriesText =
                    row.Table.Columns.Contains("Categories") ? row["Categories"].ToString() ?? "" : "";

                if (string.IsNullOrWhiteSpace(name))
                {
                    failed++;
                    errorList.Add("Le nom est requis.");
                    continue;
                }
                
                var existingByName = await documentSession.Query<Product>()
                    .FirstOrDefaultAsync(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase), cancellationToken);
                if (existingByName is not null && (idText == null || existingByName.Id.ToString() != idText))
                {
                    failed++;
                    errorList.Add($"Le nom '{name}' est déjà utilisé par un autre produit.");
                    continue;
                }

                // Price
                var okPrice = decimal.TryParse(priceText, NumberStyles.Currency, CultureInfo.InvariantCulture, out var price);
                if (!okPrice || price < 0)
                {
                    failed++;
                    errorList.Add($"Le prix '{priceText}' n'est pas valide.");
                    continue;
                }

                // Categories
                var categories = categoriesText
                    .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();

                Product? existingProduct = null;
                if (Guid.TryParse(idText, out var id))
                {
                    existingProduct = await documentSession.LoadAsync<Product>(id, cancellationToken);
                }

                if (existingProduct is null)
                {
                    var product = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = description,
                        Price = price,
                        ImageFile = imageFile,
                        Categories = categories
                    };

                    documentSession.Store(product);
                    inserted++;
                }
                else
                {
                    // Update existing product
                    existingProduct.Name = name;
                    existingProduct.Description = description;
                    existingProduct.Price = price;
                    existingProduct.ImageFile = imageFile;
                    existingProduct.Categories = categories;
                    
                    documentSession.Update(existingProduct);
                    updated++;
                }
            }
            catch (Exception e)
            {
                failed++;
                errorList.Add($"Erreur lors de l'importation de la ligne : {e.Message}");
            }
        }

        await documentSession.SaveChangesAsync(cancellationToken);

        return new ImportProductsCommandResult(InsertedCount: inserted, UpdatedCount: updated, FailedCount: failed,
            Errors: errorList);
    }
}