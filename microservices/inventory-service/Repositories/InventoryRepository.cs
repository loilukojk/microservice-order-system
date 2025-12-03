using Dapper;
using Npgsql;
using InventoryService.Models;

namespace InventoryService.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly string _connectionString;

    public InventoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("InventoryDb")
            ?? throw new InvalidOperationException("InventoryDb connection string not found");
    }

    public void InitializeDatabase()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS inventory (
                id SERIAL PRIMARY KEY,
                product_id INTEGER UNIQUE NOT NULL,
                stock INTEGER NOT NULL,
                updated_at TIMESTAMP NOT NULL
            )";

        connection.Execute(createTableSql);

        // Seed initial data (matching products from ProductService)
        var seedSql = @"
            INSERT INTO inventory (product_id, stock, updated_at)
            VALUES
                (1, 10, NOW()),
                (2, 50, NOW()),
                (3, 30, NOW())
            ON CONFLICT (product_id) DO NOTHING";

        connection.Execute(seedSql);
    }

    public async Task<Inventory?> GetInventoryByProductIdAsync(int productId)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var sql = @"
            SELECT id as Id, product_id as ProductId, stock as Stock, updated_at as UpdatedAt
            FROM inventory
            WHERE product_id = @ProductId";

        return await connection.QuerySingleOrDefaultAsync<Inventory>(sql, new { ProductId = productId });
    }

    public async Task<IEnumerable<Inventory>> GetAllInventoriesAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var sql = @"
            SELECT id as Id, product_id as ProductId, stock as Stock, updated_at as UpdatedAt
            FROM inventory
            ORDER BY product_id";

        return await connection.QueryAsync<Inventory>(sql);
    }

    public async Task UpdateStockAsync(int productId, int quantity)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var sql = @"
            INSERT INTO inventory (product_id, stock, updated_at)
            VALUES (@ProductId, @Stock, @UpdatedAt)
            ON CONFLICT (product_id)
            DO UPDATE SET
                stock = inventory.stock - @Quantity,
                updated_at = @UpdatedAt
            WHERE inventory.product_id = @ProductId";

        await connection.ExecuteAsync(sql, new
        {
            ProductId = productId,
            Stock = 0,
            Quantity = quantity,
            UpdatedAt = DateTime.UtcNow
        });
    }
}
