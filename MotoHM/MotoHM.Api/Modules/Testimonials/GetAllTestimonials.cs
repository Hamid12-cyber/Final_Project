using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Testimonials;

public static class GetAllTestimonials
{
    public record Query : IRequest<List<TestimonialDto>>;

    public record TestimonialDto(int Id, string CustomerName, int Rating, string Text, DateTime CreatedAt);

    public class Handler : IRequestHandler<Query, List<TestimonialDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<TestimonialDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, CustomerName, Rating, Text, CreatedAt
                FROM Testimonials
                ORDER BY CreatedAt DESC
                """;

            var result = await connection.QueryAsync<TestimonialDto>(sql);
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/testimonials", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .WithName("GetAllTestimonials")
            .WithTags("Testimonials");
    }
}
