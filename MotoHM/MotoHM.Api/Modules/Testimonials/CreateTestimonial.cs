using FluentValidation;
using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Modules.Testimonials;

public static class CreateTestimonial
{
    public record CreateTestimonialCommand(string CustomerName, int Rating, string Text) : IRequest<int>;

    public class Handler : IRequestHandler<CreateTestimonialCommand, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CreateTestimonialCommand request, CancellationToken cancellationToken)
        {
            var entity = new TestimonialEntity
            {
                CustomerName = request.CustomerName,
                Rating = request.Rating,
                Text = request.Text
            };

            _db.Testimonials.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/testimonials", async (CreateTestimonialCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/testimonials/{id}", new { id });
        })
        .WithName("CreateTestimonial")
        .WithTags("Testimonials");
    }
    public class Validator : AbstractValidator<CreateTestimonialCommand>
    {
        public Validator()
        {
            RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Rating).InclusiveBetween(1, 5);
            RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        }
    }
}