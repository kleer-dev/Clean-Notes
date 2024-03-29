using Microsoft.EntityFrameworkCore;
using Notes.Application.Interfaces;
using Notes.Domain;
using Notes.Persistence.EntityTypeConfiguration;

namespace Notes.Persistence;

public class NoteDbContext: DbContext, INotesDbContext
{
    public DbSet<Note> Notes { get; set; }
    public DbSet<Tasks> Tasks { get; set; }
    public DbSet<SubTask> SubTasks { get; set; }
    public DbSet<Folder> Folders { get; set; }

    public NoteDbContext(DbContextOptions<NoteDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new NoteConfiguration());
        builder.ApplyConfiguration(new TasksConfiguration());
        builder.ApplyConfiguration(new FolderConfiguration());
        builder.ApplyConfiguration(new SubTasksConfiguration());
        
        base.OnModelCreating(builder);
    }
}