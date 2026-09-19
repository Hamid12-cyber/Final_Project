using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Testimonials;

public static class DeleteTestimonial
{
    public record DeleteTestimonialCommand(int Id) : IRequest<bool>;

    public class Handler : IRequestHandler<DeleteTestimonialCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(DeleteTestimonialCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Testimonials.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (entity is null)
                return false;

            _db.Testimonials.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/testimonials/{id:int}", async (int id, ISender sender) =>
        {
            var success = await sender.Send(new DeleteTestimonialCommand(id));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteTestimonial")
        .WithTags("Testimonials");
    }
}