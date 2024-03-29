using AutoMapper;
using Notes.Application.Common.Mappings;

namespace Notes.Application.CommandsQueries.Tasks.Queries.GetTaskList;

public class TaskLookupDto: IMapWith<Domain.Tasks>
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime EditDate { get; set; }
    public bool isDeleted { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Domain.Tasks, TaskLookupDto>()
            .ForMember(taskDto => taskDto.Id,
                options =>
                    options.MapFrom(task => task.Id))
            .ForMember(taskDto => taskDto.Title,
                options =>
                    options.MapFrom(task => task.Title))
            .ForMember(taskDto => taskDto.EditDate,
                options =>
                    options.MapFrom(taskDto => taskDto.EditDate))
            .ForMember(taskDto => taskDto.isDeleted,
            options =>
                options.MapFrom(taskDto => taskDto.isDeleted));
    }
}