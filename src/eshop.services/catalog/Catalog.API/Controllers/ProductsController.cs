using System.IO.Compression;
using Catalog.API.Features.Products.Commands.CreateProduct;
using Catalog.API.Features.Products.Commands.DeleteProduct;
using Catalog.API.Features.Products.Commands.ImportProducts;
using Catalog.API.Features.Products.Commands.UpdateProduct;
using Catalog.API.Features.Products.Queries.ExportProducts;
using Catalog.API.Features.Products.Queries.GetProductById;
using Catalog.API.Features.Products.Queries.GetProducts;
using Catalog.API.Features.Products.Queries.GetProductsByCategory;
using Catalog.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

/// <summary>
/// Manages operations related to products within the catalog, including retrieving product data
/// and creating new products.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ProductsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <returns>The product matching the specified identifier, if found; otherwise, a not found response.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProductById(Guid id)
    {
        var result = await sender.Send(new GetProductByIdQuery(id));
        return Ok(result.Product);
    }

    /// <summary>
    /// Retrieves a collection of products within a specified category.
    /// </summary>
    /// <param name="category">The category by which to filter the products.</param>
    /// <returns>A collection of products belonging to the specified category, if found; otherwise, a bad request response.</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> GetProductsByCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return BadRequest("Category is required");

        var result = await sender.Send(new GetProductsByCategoryQuery(category));
        return Ok(result.Products);
    }

    /// <summary>
    /// Retrieves a collection of products from the catalog.
    /// </summary>
    /// <returns>A collection of products wrapped in an action result.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] string? category = null,
        [FromQuery] string? name = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null
    )

    {
        var providedParameters = Request.Query.Keys;

        // Create list of invalid parameters if not in the accepted list
        var invalidParameters = providedParameters.Except(
            [
                "pageNumber", "pageSize", "category", "name", "minPrice", "maxPrice"
            ],
            StringComparer.OrdinalIgnoreCase).ToList();

        if (invalidParameters.Count > 0)
        {
            return BadRequest($"Invalid query parameters: {string.Join(", ", invalidParameters)}");
        }

        // By default, return the first page with 10 items if parameters are not provided or invalid
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var query = new GetProductsQuery(
            PageNumber: pageNumber, 
            PageSize: pageSize, 
            Category: category, 
            Name: name, 
            MinPrice: minPrice, 
            MaxPrice: maxPrice);
        
        var result = await sender.Send(query);

        return Ok(result.Products);
    }

    /// <summary>
    /// Handles the creation of a new product.
    /// </summary>
    /// <param name="request">The command containing the details of the product to be created.</param>
    /// <returns>A result object containing the ID of the newly created product.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateProductCommandResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateProductCommandResult>> CreateProduct(CreateProductCommand request)
    {
        var result = await sender.Send(request);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates a product with the specified ID using the provided update details.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="request">The details to update the specified product.</param>
    /// <returns>A boolean indicating whether the update was successful or an appropriate error response if the product was not found.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> UpdateProduct(Guid id, [FromBody] UpdateProductCommand request)
    {
        var result = await sender.Send(request);
        return Ok(result.IsSuccessful);
    }

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>True if the product was successfully deleted; otherwise, a not found response.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> DeleteProduct(Guid id)
    {
        var result = await sender.Send(new DeleteProductCommand(id));
        return Ok(result.IsSuccessful);
    }

    /// <summary>
    /// Imports products from an uploaded .xlsx file.
    /// </summary>
    /// <param name="file">The file send by the requester</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns></returns>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportProductsCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> ImportProducts([FromForm] IFormFile file, CancellationToken ct)
    {
        // Validate the uploaded file
        if (file is null || file.Length == 0)
        {
            return BadRequest("Fichier .xlsx manquant ou vide.");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Format de fichier non supporté. Veuillez télécharger un fichier .xlsx.");
        }

        // Check MIME type
        var allowedTypes = new[]
        {
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/octet-stream" // Some browsers may use this for .xlsx files
        };
        if (!allowedTypes.Contains(file.ContentType))
        {
            return BadRequest("Type de fichier non supporté. Veuillez télécharger un fichier .xlsx.");
        }

        try
        {
            // Check ZIP signature
            await using var stream = file.OpenReadStream();
            var hdr = new byte[4];
            var bytesRead = await stream.ReadAsync(hdr, ct);
            if (bytesRead < 4)
            {
                return BadRequest("Le fichier téléchargé n'est pas un fichier .xlsx valide.");
            }

            var isZip =
                hdr[0] == 0x50 && hdr[1] == 0x4B &&
                (hdr[2] == 0x03 || hdr[2] == 0x05 || hdr[2] == 0x07) &&
                (hdr[3] == 0x04 || hdr[3] == 0x06 || hdr[3] == 0x08);

            if (!isZip)
            {
                return BadRequest("Le fichier téléchargé n'est pas un fichier .xlsx valide.");
            }

            // Check OOXML structure
            stream.Position = 0;
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
            if (zip.GetEntry("[Content_Types].xml") == null || zip.GetEntry("xl/workbook.xml") == null)
            {
                return BadRequest("Le fichier téléchargé n'est pas un fichier .xlsx valide.");
            }
        }
        catch (InvalidDataException)
        {
            return BadRequest("Archive .xlsx corrompue ou invalide.");
        }

        // Process
        var result = await sender.Send(new ImportProductsCommand(file), ct);
        return Ok(result);
    }

    [HttpGet("export")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportProducts(CancellationToken ct)
    {
        var result = await sender.Send(new ExportProductsQuery(), ct);
        return File(result.Content, result.ContentType, result.FileName);
    }
}