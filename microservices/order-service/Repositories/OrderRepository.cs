using Dapper;
using Npgsql;
using OrderService.Models;

namespace OrderService.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("OrderDb")
            ?? throw new InvalidOperationException("OrderDb connection string not found");
    }

    public void InitializeDatabase()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS orders (
                id SERIAL PRIMARY KEY,
                product_id INTEGER NOT NULL,
                quantity INTEGER NOT NULL,
                total_price DECIMAL(18,2) NOT NULL,
                status VARCHAR(50) NOT NULL,
                created_at TIMESTAMP NOT NULL
            )";

        connection.Execute(createTableSql);
    }

    public async Task<int> CreateOrderAsync(Order order)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var sql = @"
            INSERT INTO orders (product_id, quantity, total_price, status, created_at)
            VALUES (@ProductId, @Quantity, @TotalPrice, @Status, @CreatedAt)
            RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, order);
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var sql = @"
            SELECT id as Id, product_id as ProductId, quantity as Quantity,
                   total_price as TotalPrice, status as Status, created_at as CreatedAt
            FROM orders
            WHERE id = @Id";

        return await connection.QuerySingleOrDefaultAsync<Order>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var sql = @"
            SELECT id as Id, product_id as ProductId, quantity as Quantity,
                   total_price as TotalPrice, status as Status, created_at as CreatedAt
            FROM orders
            ORDER BY created_at DESC";

        return await connection.QueryAsync<Order>(sql);
    }
}
