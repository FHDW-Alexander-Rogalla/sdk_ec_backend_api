using Microsoft.AspNetCore.Mvc;
using Sdk_EC_Backend.Models.Dtos;
using Sdk_EC_Backend.Services;

namespace Sdk_EC_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ISupabaseService _supabase;

    public ProductController(ISupabaseService supabase)
    {
        _supabase = supabase;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Get()
    {
        try
        {
            var products = await _supabase.GetProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            // Let centralized middleware or framework handle logging; return Problem for now.
            return Problem(title: "Failed to fetch products", detail: ex.Message, statusCode: 500);
        }
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductDto>> GetById(long id)
    {
        try
        {
            var product = await _supabase.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to fetch product", detail: ex.Message, statusCode: 500);
        }
    }
}
