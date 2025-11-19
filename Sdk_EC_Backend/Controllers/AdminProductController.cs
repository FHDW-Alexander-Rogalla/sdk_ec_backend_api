using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sdk_EC_Backend.Models;
using Sdk_EC_Backend.Models.Dtos;
using Sdk_EC_Backend.Services;
using System.Security.Claims;

namespace Sdk_EC_Backend.Controllers;

[ApiController]
[Route("api/admin/product")]
[Authorize] // Requires JWT authentication
public class AdminProductController : ControllerBase
{
    private readonly SupabaseService _supabaseService;

    public AdminProductController(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    /// <summary>
    /// Gets the current user's ID from the JWT token
    /// </summary>
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        
        return userId;
    }

    /// <summary>
    /// Checks if the current user is an admin by querying the user_roles table
    /// </summary>
    private async Task<bool> IsAdmin()
    {
        try
        {
            var userId = GetUserId();

            var roleResponse = await _supabaseService.Client
                .From<UserRole>()
                .Filter("user_id", Postgrest.Constants.Operator.Equals, userId.ToString())
                .Get();

            if (roleResponse.Models.Count == 0)
            {
                return false;
            }

            var userRole = roleResponse.Models.First();
            return userRole.Role.Equals("admin", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// POST /api/admin/product - Creates a new product (Admin only)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            // Check if user is admin
            if (!await IsAdmin())
            {
                return Forbid();
            }

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Product name is required" });
            }

            if (request.Price < 0)
            {
                return BadRequest(new { message = "Price must be greater than or equal to 0" });
            }

            // Create new product with current timestamps
            var now = DateTime.UtcNow;
            var newProduct = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ImageUrl = request.ImageUrl,
                CreatedAt = now,
                UpdatedAt = now
            };

            var response = await _supabaseService.Client
                .From<Product>()
                .Insert(newProduct);

            var product = response.Models.First();

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return Created($"/api/product/{dto.Id}", dto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in CreateProduct: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return Problem(title: "Failed to create product", detail: ex.Message, statusCode: 500);
        }
    }

    /// <summary>
    /// PUT /api/admin/product/{id} - Updates an existing product (Admin only)
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(long id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            // Check if user is admin
            if (!await IsAdmin())
            {
                return Forbid();
            }

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Product name is required" });
            }

            if (request.Price < 0)
            {
                return BadRequest(new { message = "Price must be greater than or equal to 0" });
            }

            // Get existing product
            var getResponse = await _supabaseService.Client
                .From<Product>()
                .Filter("id", Postgrest.Constants.Operator.Equals, id.ToString())
                .Get();

            if (getResponse.Models.Count == 0)
            {
                return NotFound(new { message = "Product not found" });
            }

            var product = getResponse.Models.First();

            // Update product fields and timestamp
            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.ImageUrl = request.ImageUrl;
            product.UpdatedAt = DateTime.UtcNow;

            var updateResponse = await _supabaseService.Client
                .From<Product>()
                .Update(product);

            var updatedProduct = updateResponse.Models.First();

            var dto = new ProductDto
            {
                Id = updatedProduct.Id,
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                ImageUrl = updatedProduct.ImageUrl,
                CreatedAt = updatedProduct.CreatedAt,
                UpdatedAt = updatedProduct.UpdatedAt
            };

            return Ok(dto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in UpdateProduct: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return Problem(title: "Failed to update product", detail: ex.Message, statusCode: 500);
        }
    }

    /// <summary>
    /// DELETE /api/admin/product/{id} - Deletes a product (Admin only)
    /// </summary>
    // [HttpDelete("{id:long}")]
    // public async Task<ActionResult> DeleteProduct(long id)
    // {
    //     try
    //     {
    //         // Check if user is admin
    //         if (!await IsAdmin())
    //         {
    //             return Forbid();
    //         }

    //         // Check if product exists
    //         var getResponse = await _supabaseService.Client
    //             .From<Product>()
    //             .Filter("id", Postgrest.Constants.Operator.Equals, id.ToString())
    //             .Get();

    //         if (getResponse.Models.Count == 0)
    //         {
    //             return NotFound(new { message = "Product not found" });
    //         }

    //         // Delete product
    //         await _supabaseService.Client
    //             .From<Product>()
    //             .Where(x => x.Id == id)
    //             .Delete();

    //         return NoContent();
    //     }
    //     catch (UnauthorizedAccessException ex)
    //     {
    //         return Unauthorized(new { message = ex.Message });
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"ERROR in DeleteProduct: {ex.GetType().Name}: {ex.Message}");
    //         Console.WriteLine($"StackTrace: {ex.StackTrace}");
    //         return Problem(title: "Failed to delete product", detail: ex.Message, statusCode: 500);
    //     }
    // }
}

// Request DTOs
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}
