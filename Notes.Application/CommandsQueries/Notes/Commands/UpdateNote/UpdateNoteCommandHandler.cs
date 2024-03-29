using MediatR;
using Microsoft.EntityFrameworkCore;
using Notes.Application.Common.Exceptions;
using Notes.Application.Interfaces;
using Notes.Domain;

namespace Notes.Application.CommandsQueries.Notes.Commands.UpdateNote;

public class UpdateNoteCommandHandler: IRequestHandler<UpdateNoteCommand>
{
    private readonly INotesDbContext _dbContext;

    public UpdateNoteCommandHandler(INotesDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<Unit> Handle(UpdateNoteCommand request,
        CancellationToken cancellationToken)
    {
        var entity =  await _dbContext.Notes
            .FirstOrDefaultAsync(note => note.Id == request.Id, cancellationToken);

        if (entity == null || entity.UserId != request.UserId)
            throw new NotFoundException(nameof(Note), request.Id);

        entity.Text = request.Text;
        entity.Title = request.Title;
        entity.isDeleted = request.isDeleted;
        entity.EditDate = DateTime.Now.ToUniversalTime();

        if (request.FolderId == Guid.Empty)
            entity.FolderId = null;
        else
            entity.FolderId = request.FolderId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}