namespace CloudProject.Business;

public static class DtoConverters
{
    extension(AttendeeModel model)
    {
        public AttendeeDto ToDto(bool includeIds = false)
        {
            return new AttendeeDto()
            {
                Id = includeIds ? model.Id : null,
                StudentID = model.StudentID,
                FullName = model.FullName,
            };
        }
    }

    extension(ClassroomModel model)
    {
        public ClassroomDto ToDto(bool includeIds = false)
        {
            return new ClassroomDto()
            {
                Id = includeIds ? model.Id : null,
                Name = model.Name
            };
        }
    }

    extension(CourseModel model)
    {
        public CourseDto ToDto(bool includeIds = false)
        {
            return new CourseDto()
            {
                Id = includeIds ? model.Id : null,
                Code = model.Code,
                Name = model.Name
            };
        }
    }
    
    extension(DeviceModel model)
    {
        public DeviceDto ToDto(bool includeIds = true) // Intentional, this is naturally available to administrators.
        {
            return new DeviceDto()
            {
                Id = includeIds ? model.Id : null,
                Classroom =  model.AssignedClassroom?.ToDto(includeIds),
                Fingerprint = Convert.ToBase64String(model.Fingerprint)
            };
        }
    }

    extension(SectionModel model)
    {
        public SectionDto ToDto(bool includeIds = false)
        {
            return new SectionDto()
            {
                Id = includeIds ? model.Id : null,
                Course = model.Course?.ToDto(),
                Section = model.SectionType + model.SectionId,
                User = model.User.ToDto()
            };
        }
    }

    extension(TimetableModel model)
    {
        public TimetableDto ToDto(bool includeIds = false)
        {
            return new TimetableDto()
            {
                Id = includeIds ? model.Id : null,
                Classroom = model.Classroom?.ToDto(),
                Section = model.CourseSection?.ToDto(),
                Timeslot = model.Timeslot
            };
        }
    }

    extension(UserModel model)
    {
        public UserDto ToDto()
        {
            return new UserDto
            {
                UserName = model.UserName!,
                FullName = model.FullName,
                Role = model.Role
            };
        }
    }
}
