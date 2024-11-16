
namespace HealthSprintAnalyzer.Engine.Services.SprintAnalyze.Constants;

public static class Constants
{
	public static readonly string[] ClosedStatuses = ["closed", "Закрыто", "Снято"];
	
	public static readonly string[] CreatedStatuses = ["created", "Создано"];
	
	public static readonly string[] DoneStatuses = ["Выполнено", "Закрыто", "closed", "done"];
	
	public static readonly string[] RemovedResolutionForTask = ["Отклонено", "Отменено инициатором", "Дубликат"];
	
	public static readonly string[] RemovedResolutionForBug = ["Отклонен исполнителем"];


}