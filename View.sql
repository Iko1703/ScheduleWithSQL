SELECT Schedule.ID_Schedule
		,Groups.[�_group]
		,Schedule.Day_of_week
		,Schedule.Up_down
		,Auditorium.[�_auditorium]
		,Auditorium.Adress
		,Class.Type_class
		,Class.Class_name
		,Teachers.[Name]
		,Teachers.Patronynic
		,Teachers.Surname
		,ClassTime.[�_class]
		,ClassTime.Class_Start
		,ClassTime.Class_End

FROM [schediule].[dbo].[Schedule]

Join Class on Schedule.ID_Class = Class.ID_Class
join Auditorium on Schedule.ID_Auditorium = Auditorium.ID_Auditorium
Join Groups on Schedule.ID_Groups = Groups.ID_Groups
join Teachers on Schedule.ID_Teachers =Teachers.ID_Teachers
join ClassTime on Schedule.ID_ClassTime = ClassTime.ID_ClassTime