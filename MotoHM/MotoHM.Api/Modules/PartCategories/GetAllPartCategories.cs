using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.PartCategories;

public static class GetAllPartCategories
{
    public record Query : IRequest<List<PartCategoryDto>>;

    public record PartCategoryDto(int Id, string Icon, string Name);

    public class Handler : IRequestHandler<Query, List<PartCategoryDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<PartCategoryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "SELECT Id, Icon, Name FROM PartCategories ORDER BY Id";

            var result = await connection.QueryAsync<PartCategoryDto>(sql);
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/part-categories", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .WithName("GetAllPartCategories")
            .WithTags("PartCategories");
    }
}