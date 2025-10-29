using System.Linq;
using System.Collections.Generic;
using Supabase;
using Postgrest;
using Sdk_EC_Backend.Models;
using Sdk_EC_Backend.Models.Dtos;
using Sdk_EC_Backend.Configuration;
using Microsoft.Extensions.Options;
using SupabaseClient = Supabase.Client;

namespace Sdk_EC_Backend.Services;

public interface ISupabaseService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(long id);
}

public class SupabaseService : ISupabaseService
{
    private readonly SupabaseClient _client;

    public SupabaseService(IOptions<SupabaseSettings> settings)
    {
        var url = settings.Value.Url;
        var key = settings.Value.Key;

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Supabase Url and Key must be provided in configuration.");

        var options = new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };

        _client = new SupabaseClient(url, key, options);

        // Initialize client synchronously in ctor (safe for short init). If this blocks too long, consider lazy initialization.
        _client.InitializeAsync().GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        var response = await _client.From<Product>().Get();
        var products = response.Models;

        // Map to DTOs to avoid serializing Postgrest metadata/attributes
        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            ImageUrl = p.ImageUrl,
            Category = p.Category,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });

        return dtos;
    }

    public async Task<ProductDto?> GetProductByIdAsync(long id)
    {
        var response = await _client.From<Product>()
                                   .Filter("id", Constants.Operator.Equals, id.ToString())
                                   .Get();

        var product = response.Models.FirstOrDefault();
        if (product == null)
            return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            Category = product.Category,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}