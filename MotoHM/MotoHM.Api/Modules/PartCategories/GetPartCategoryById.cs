using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.PartCategories;

public static class GetPartCategoryById
{
    public record Query(int Id) : IRequest<PartCategoryDto?>;

    public record PartCategoryDto(int Id, string Icon, string Name);

    public class Handler : IRequestHandler<Query, PartCategoryDto?>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<PartCategoryDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "SELECT Id, Icon, Name FROM PartCategories WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<PartCategoryDto>(sql, new { request.Id });
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/part-categories/{id:int}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new Query(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetPartCategoryById")
        .WithTags("PartCategories");
    }
}