using Domain.Entities;
using Domain.Enums;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seed;

public static class DevDataSeeder
{
    private const int RandomSeed = 20250628;
    private const string LocalDomain = "msu.local";

    public static async Task SeedAsync(AppDbContext context, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("DevDataSeeder");

        if (await HasApplicationDataAsync(context))
        {
            logger?.LogWarning("Dev seed skipped because the database already contains application data.");
            return;
        }

        var random = new Random(RandomSeed);
        var passwordHasher = serviceProvider.GetRequiredService<PasswordHasher>();

        var faculties = SeedFaculties(context);
        var specialities = SeedSpecialities(context, faculties);
        var groups = SeedGroups(context, specialities);
        await context.SaveChangesAsync();

        var teachers = SeedTeachers(context, random);
        var weeks = SeedWeeks(context);
        var audiences = SeedAudiences(context);
        var subjects = SeedSubjects(context, specialities, random);
        var students = SeedStudents(context, groups, random);
        await context.SaveChangesAsync();

        var disciplines = SeedDisciplines(context, groups, subjects, teachers, random);
        await context.SaveChangesAsync();

        var schedules = SeedSchedules(context, weeks, groups, disciplines, audiences, random);
        await context.SaveChangesAsync();

        var attendances = SeedAttendances(context, schedules, students, random);
        await context.SaveChangesAsync();

        var performances = SeedAcademicPerformances(context, groups, students, disciplines, random);
        await context.SaveChangesAsync();

        var users = SeedUsers(context, students, teachers, passwordHasher);
        await context.SaveChangesAsync();

        logger?.LogInformation(
            "Dev seed completed. Faculties={Faculties}, Specialities={Specialities}, Groups={Groups}, Students={Students}, Teachers={Teachers}, Subjects={Subjects}, Disciplines={Disciplines}, Weeks={Weeks}, Audiences={Audiences}, Schedules={Schedules}, Attendances={Attendances}, AcademicPerformances={AcademicPerformances}, Users={Users}",
            faculties.Count,
            specialities.Count,
            groups.Count,
            students.Count,
            teachers.Count,
            subjects.Count,
            disciplines.Count,
            weeks.Count,
            audiences.Count,
            schedules.Count,
            attendances.Count,
            performances.Count,
            users.Count);
    }

    private static async Task<bool> HasApplicationDataAsync(AppDbContext context)
    {
        return await context.Faculties.AnyAsync()
            || await context.Specialities.AnyAsync()
            || await context.Groups.AnyAsync()
            || await context.Students.AnyAsync()
            || await context.Teachers.AnyAsync()
            || await context.Subjects.AnyAsync()
            || await context.Disciplines.AnyAsync()
            || await context.Schedules.AnyAsync()
            || await context.Users.AnyAsync();
    }

    private static List<Faculty> SeedFaculties(AppDbContext context)
    {
        var faculties = new List<Faculty>
        {
            new() { Name = "Естественнонаучный факультет" },
            new() { Name = "Гуманитарный факультет" }
        };

        context.Faculties.AddRange(faculties);
        return faculties;
    }

    private static List<SpecialityPlan> SeedSpecialities(AppDbContext context, IReadOnlyList<Faculty> faculties)
    {
        var plans = new List<SpecialityPlan>
        {
            new("Прикладная математика и информатика", "ПМиИ", faculties[0], PmiiPlan()),
            new("Химио-физико-механика материалов", "ХФММ", faculties[0], SimplePlan("Общая химия", "Физическая химия", "Органическая химия", "Механика материалов", "Материаловедение", "Методы исследования материалов", "Физика", "Математика", "Английский язык", "Химия твердого тела")),
            new("Геология", "ГЕО", faculties[0], SimplePlan("Общая геология", "Минералогия", "Петрография", "Геофизика", "Геохимия", "Стратиграфия", "Картография", "Английский язык", "Физика", "Математика")),
            new("Международные отношения", "МО", faculties[1], SimplePlan("История международных отношений", "Международное право", "Политология", "Дипломатический протокол", "Мировая экономика", "Регионоведение", "Иностранный язык", "Современные международные организации", "Экономика", "История России")),
            new("Государственно-муниципальное управление", "ГМУ", faculties[1], SimplePlan("Теория государственного управления", "Муниципальное управление", "Административное право", "Экономика государственного сектора", "Менеджмент", "Государственная служба", "Финансовый контроль", "Документационное обеспечение управления", "Экономика", "Основы правоведения")),
            new("Лингвистика", "ЛИН", faculties[1], SimplePlan("Практическая фонетика", "Практическая грамматика", "Теория перевода", "Лексикология", "Стилистика", "Второй иностранный язык", "Лингвострановедение", "История языка", "Иностранный язык", "Русский язык и культура речи"))
        };

        foreach (var plan in plans)
        {
            plan.Speciality = new Speciality
            {
                Name = plan.Name,
                Faculty = plan.Faculty
            };
        }

        context.Specialities.AddRange(plans.Select(x => x.Speciality!));
        return plans;
    }

    private static List<Group> SeedGroups(AppDbContext context, IReadOnlyList<SpecialityPlan> specialities)
    {
        var groups = new List<Group>();

        foreach (var speciality in specialities)
        {
            for (short course = 1; course <= 4; course++)
            {
                groups.Add(new Group
                {
                    Name = $"{speciality.ShortName}-{course}01",
                    Course = course,
                    Speciality = speciality.Speciality!
                });
            }
        }

        context.Groups.AddRange(groups);
        return groups;
    }

    private static List<Teacher> SeedTeachers(AppDbContext context, Random random)
    {
        var required = new[]
        {
            "Одинабеков Джассур Музаффирович",
            "Абдуллоева Мадина Саидовна",
            "Каримова Шахноза Фарруховна",
            "Саидова Нигина Рустамовна",
            "Сергеев Петр Владимирович",
            "Иванов Иван Игоревич",
            "Попова Елена Николаевна",
            "Волков Дмитрий Алексеевич",
            "Кузнецова Ольга Сергеевна",
            "Тихонов Сергей Николаевич",
            "Федорова Анна Викторовна",
            "Рахимов Бахром Сафарович",
            "Сафарова Манижа Абдуллоевна",
            "Юсупов Фирдавс Каримович",
            "Мирзоева Шахноза Давлатовна",
            "Абдуллоев Рустам Нуриддинович"
        };

        var additional = new[]
        {
            "Назаров Фарход Алиевич", "Косимова Дилноза Шарифовна", "Мирзоев Самандар Рахимович",
            "Ахмедова Зарина Хуршедовна", "Петрова Мария Андреевна", "Сидоров Алексей Павлович",
            "Шарипов Джамшед Каримович", "Азизова Мехринисо Сафаровна", "Холматов Даврон Бахтиерович",
            "Бобоева Малика Абдуллоевна", "Ганиев Олим Махмадович", "Комилова Сабрина Юсуфовна",
            "Рустамов Зафар Хайруллоевич", "Матвеева Ирина Сергеевна", "Карпов Николай Викторович",
            "Орлова Светлана Петровна", "Саидов Амир Музаффарович", "Зокирова Фируза Нуровна",
            "Темуров Бехруз Файзуллоевич", "Лебедева Анна Максимовна", "Громов Илья Дмитриевич",
            "Курбонова Мавзуна Исмоиловна", "Исмоилов Фарид Джураевич", "Алимова Мадина Рахимовна"
        };

        var teachers = required.Concat(additional)
            .Take(40)
            .Select((fullName, index) =>
            {
                var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return new Teacher
                {
                    LastName = parts[0],
                    FirstName = parts[1],
                    FatherName = parts.Length > 2 ? parts[2] : null,
                    Email = $"teacher{index + 1:00}@{LocalDomain}",
                    Telephone = Phone(random, index),
                    Address = Address(random, index),
                    TeacherDegree = index % 7 == 0 ? AcademicDegree.Doctor : index % 3 == 0 ? AcademicDegree.Candidate : AcademicDegree.None,
                    TeacherTitle = index % 8 == 0 ? AcademicTitle.Professor : index % 3 == 0 ? AcademicTitle.Docent : AcademicTitle.SeniorLecturer,
                    TeacherPost = index % 11 == 0 ? Post.HeadOfDepartment : index % 5 == 0 ? Post.Docent : index % 2 == 0 ? Post.SeniorLecturer : Post.Assistant
                };
            })
            .ToList();

        context.Teachers.AddRange(teachers);
        return teachers;
    }

    private static List<Weeks> SeedWeeks(AppDbContext context)
    {
        var start = new DateTime(2025, 9, 1);
        var weeks = Enumerable.Range(0, 16)
            .Select(index => new Weeks
            {
                Name = $"{index + 1} учебная неделя",
                StartDate = start.AddDays(index * 7),
                EndDate = start.AddDays(index * 7 + 5)
            })
            .ToList();

        context.Weeks.AddRange(weeks);
        return weeks;
    }

    private static List<Audience> SeedAudiences(AppDbContext context)
    {
        var numbers = new[]
        {
            "101", "102", "103", "104", "105", "201", "202", "203", "204", "205",
            "301", "302", "303", "304", "305", "401", "402", "403", "Л-1", "Л-2",
            "Л-3", "К-1", "К-2", "К-3", "К-4", "Х-1", "Х-2", "Х-3", "Ф-1", "Ф-2",
            "Г-1", "Г-2", "Г-3", "С-1", "С-2", "Актовый зал", "Поточная аудитория 1", "Поточная аудитория 2", "Медиа-класс", "Конференц-зал"
        };

        var audiences = numbers.Select(number => new Audience
            {
                Number = number,
                Type = number.StartsWith("К-", StringComparison.Ordinal) ? AudienceType.ComputerLab :
                    number.StartsWith("Х-", StringComparison.Ordinal) ? AudienceType.ChemistryLab :
                    number.StartsWith("Ф-", StringComparison.Ordinal) ? AudienceType.PhysicsLab :
                    number.StartsWith("Г-", StringComparison.Ordinal) ? AudienceType.GeologyLab :
                    number.StartsWith("Л-", StringComparison.Ordinal) || number.Contains("Поточная", StringComparison.Ordinal) ? AudienceType.LectureHall :
                    number.Contains("Актовый", StringComparison.Ordinal) ? AudienceType.StreamHall :
                    AudienceType.GeneralHall
            })
            .ToList();

        context.Audiences.AddRange(audiences);
        return audiences;
    }

    private static List<SubjectPlan> SeedSubjects(AppDbContext context, IReadOnlyList<SpecialityPlan> specialities, Random random)
    {
        var plans = new List<SubjectPlan>();

        foreach (var speciality in specialities)
        {
            foreach (var plan in speciality.Plan)
            {
                plan.SpecialityShortName = speciality.ShortName;
                plans.Add(plan);
            }
        }

        foreach (var plan in plans)
        {
            plan.Subject = new Subject
            {
                Name = plan.Name,
                Semester = plan.Semester,
                ControlForm = plan.ControlForm,
                HourCount = random.Next(36, 109)
            };
        }

        context.Subjects.AddRange(plans.Select(x => x.Subject!));
        return plans;
    }

    private static List<Student> SeedStudents(AppDbContext context, IReadOnlyList<Group> groups, Random random)
    {
        var firstNames = new[] { "Александр", "Дмитрий", "Сергей", "Иван", "Андрей", "Фарид", "Амир", "Бехруз", "Далер", "Шахром", "Мария", "Елена", "Анна", "Сабрина", "Мадина", "Зарина", "Дилноза", "Малика", "Нигина", "Шахноза" };
        var lastNames = new[] { "Николаев", "Волкова", "Саидов", "Каримова", "Рахимов", "Абдуллоев", "Юсупов", "Мирзоев", "Попова", "Иванов", "Кузнецова", "Тихонов", "Федорова", "Сафарова", "Назаров", "Косимова", "Шарипов", "Азизова", "Холматов", "Бобоева" };
        var fatherNames = new[] { "Алексеевич", "Игоревич", "Сергеевич", "Дмитриевич", "Каримович", "Рахимович", "Абдуллоевич", "Фаррухович", "Саидович", "Нуриддинович", "Алексеевна", "Игоревна", "Сергеевна", "Дмитриевна", "Каримовна", "Рахимовна", "Абдуллоевна", "Фарруховна", "Саидовна", "Нуриддиновна" };
        var groupSizes = new[] { 21, 22, 23, 24, 22, 23, 21, 24, 23, 22, 24, 21, 22, 24, 23, 21, 24, 22, 23, 22, 21, 24, 23, 22 };
        var students = new List<Student>();
        var index = 0;

        foreach (var group in groups)
        {
            for (var i = 0; i < groupSizes[students.Count == 0 ? 0 : groups.ToList().IndexOf(group)]; i++)
            {
                var firstName = firstNames[(index + random.Next(firstNames.Length)) % firstNames.Length];
                var lastName = lastNames[(index * 3 + random.Next(lastNames.Length)) % lastNames.Length];
                var fatherName = fatherNames[(index * 5 + random.Next(fatherNames.Length)) % fatherNames.Length];
                var age = group.Course switch
                {
                    1 => random.Next(17, 20),
                    2 => random.Next(18, 21),
                    3 => random.Next(19, 22),
                    _ => random.Next(20, 24)
                };

                students.Add(new Student
                {
                    FirstName = firstName,
                    LastName = lastName,
                    FatherName = fatherName,
                    Group = group,
                    Email = $"student{index + 1:0000}@{LocalDomain}",
                    Telephone = Phone(random, index),
                    Address = Address(random, index),
                    BirthDate = new DateTime(2026 - age, random.Next(1, 13), random.Next(1, 28))
                });

                index++;
            }
        }

        context.Students.AddRange(students);
        return students;
    }

    private static List<Discipline> SeedDisciplines(AppDbContext context, IReadOnlyList<Group> groups, IReadOnlyList<SubjectPlan> subjects, IReadOnlyList<Teacher> teachers, Random random)
    {
        var disciplines = new List<Discipline>();

        foreach (var group in groups)
        {
            var groupCode = group.Name.Split('-')[0];
            var maxCompletedSemester = group.Course * 2;
            var plans = subjects
                .Where(x => x.SpecialityShortName == groupCode && x.Semester <= maxCompletedSemester)
                .OrderBy(x => x.Semester)
                .ThenBy(x => x.Name)
                .ToList();

            foreach (var plan in plans)
            {
                var teacher = PickTeacher(plan.Name, plan.Semester, teachers, random);
                disciplines.Add(new Discipline
                {
                    Group = group,
                    Subject = plan.Subject!,
                    Teacher = teacher,
                    LectureHourCount = plan.ControlForm == ControlForm.Exam ? random.Next(18, 37) : random.Next(8, 19),
                    PracticeHourCount = random.Next(12, 37),
                    LaboratoryHourCount = NeedsLab(plan.Name) ? random.Next(12, 37) : 0,
                    OtherHourCount = random.Next(0, 13),
                    Control = random.Next(0, 3)
                });
            }
        }

        context.Disciplines.AddRange(disciplines);
        return disciplines;
    }

    private static List<Schedule> SeedSchedules(AppDbContext context, IReadOnlyList<Weeks> weeks, IReadOnlyList<Group> groups, IReadOnlyList<Discipline> disciplines, IReadOnlyList<Audience> audiences, Random random)
    {
        var schedules = new List<Schedule>();

        foreach (var week in weeks)
        {
            var groupSlots = new HashSet<string>();
            var teacherSlots = new HashSet<string>();
            var audienceSlots = new HashSet<string>();

            foreach (var group in groups)
            {
                var currentSemesters = new[] { (short)(group.Course * 2 - 1), (short)(group.Course * 2) };
                var groupDisciplines = disciplines
                    .Where(x => ReferenceEquals(x.Group, group) && currentSemesters.Contains(x.Subject.Semester))
                    .OrderBy(_ => random.Next())
                    .Take(2)
                    .ToList();

                foreach (var discipline in groupDisciplines)
                {
                    for (var attempt = 0; attempt < 100; attempt++)
                    {
                        var day = (short)random.Next(1, 7);
                        var para = (short)random.Next(1, 8);
                        var audience = PickAudience(discipline.Subject.Name, audiences, random);
                        var key = $"{week.Name}:{day}:{para}";

                        if (!groupSlots.Add($"{group.Name}:{key}"))
                        {
                            continue;
                        }

                        if (!teacherSlots.Add($"{discipline.Teacher.Email}:{key}"))
                        {
                            groupSlots.Remove($"{group.Name}:{key}");
                            continue;
                        }

                        if (!audienceSlots.Add($"{audience.Number}:{key}"))
                        {
                            groupSlots.Remove($"{group.Name}:{key}");
                            teacherSlots.Remove($"{discipline.Teacher.Email}:{key}");
                            continue;
                        }

                        schedules.Add(new Schedule
                        {
                            Week = week,
                            Den = day,
                            Para = para,
                            Discipline = discipline,
                            Teacher = discipline.Teacher,
                            Group = group,
                            Audience = audience,
                            LectureType = PickLectureType(discipline.Subject.Name, random)
                        });
                        break;
                    }
                }
            }
        }

        context.Schedules.AddRange(schedules);
        return schedules;
    }

    private static List<Attendance> SeedAttendances(AppDbContext context, IReadOnlyList<Schedule> schedules, IReadOnlyList<Student> students, Random random)
    {
        var studentsByGroup = students.GroupBy(x => x.Group).ToDictionary(x => x.Key!, x => x.ToList());
        var attendances = new List<Attendance>();

        foreach (var schedule in schedules.OrderBy(_ => random.Next()).Take(420))
        {
            foreach (var student in studentsByGroup[schedule.Group])
            {
                attendances.Add(new Attendance
                {
                    Student = student,
                    Week = schedule.Week,
                    Day = schedule.Den,
                    Para = schedule.Para,
                    Mark = Weighted(random, new[]
                    {
                        (AttendanceMark.Present, 84),
                        (AttendanceMark.Absent, 9),
                        (AttendanceMark.ValidReason, 5),
                        (AttendanceMark.Late, 2)
                    })
                });
            }
        }

        context.Attendances.AddRange(attendances);
        return attendances;
    }

    private static List<AcademicPerformance> SeedAcademicPerformances(AppDbContext context, IReadOnlyList<Group> groups, IReadOnlyList<Student> students, IReadOnlyList<Discipline> disciplines, Random random)
    {
        var studentsByGroup = students.GroupBy(x => x.Group).ToDictionary(x => x.Key!, x => x.ToList());
        var disciplinesByGroup = disciplines.GroupBy(x => x.Group).ToDictionary(x => x.Key, x => x.ToList());
        var performances = new List<AcademicPerformance>();
        var createdKeys = new HashSet<string>();

        foreach (var group in groups)
        {
            foreach (var student in studentsByGroup[group])
            {
                foreach (var discipline in disciplinesByGroup[group])
                {
                    if (!createdKeys.Add($"{student.Email}:{discipline.Subject.Name}:{discipline.Subject.Semester}"))
                    {
                        continue;
                    }

                    var controlForm = discipline.Subject.ControlForm;
                    var mark = controlForm == ControlForm.Credit
                        ? Weighted(random, new[] { ((short)3, 82), ((short)2, 10), ((short)1, 5), ((short)0, 3) })
                        : Weighted(random, new[] { ((short)5, 18), ((short)4, 34), ((short)3, 32), ((short)2, 10), ((short)1, 4), ((short)0, 2) });

                    performances.Add(new AcademicPerformance
                    {
                        Student = student,
                        Discipline = discipline,
                        Teacher = discipline.Teacher,
                        ControlForm = controlForm,
                        Mark = mark,
                        Tur = RetakeRoundFor(mark, random)
                    });
                }
            }
        }

        context.AcademicPerformances.AddRange(performances);
        return performances;
    }

    private static List<User> SeedUsers(AppDbContext context, IReadOnlyList<Student> students, IReadOnlyList<Teacher> teachers, PasswordHasher passwordHasher)
    {
        var users = new List<User>
        {
            new()
            {
                Email = $"admin@{LocalDomain}",
                PasswordHash = passwordHasher.HashPassword("Admin123!"),
                FirstName = "Admin",
                LastName = "MSU",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            }
        };

        users.AddRange(teachers.Take(18).Select(teacher => new User
        {
            Email = teacher.Email!,
            PasswordHash = passwordHasher.HashPassword("Teacher123!"),
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            Role = UserRole.Teacher,
            CreatedAt = DateTime.UtcNow
        }));

        users.AddRange(students.Where((_, index) => index % 5 == 0).Take(110).Select(student => new User
        {
            Email = student.Email!,
            PasswordHash = passwordHasher.HashPassword("Student123!"),
            FirstName = student.FirstName,
            LastName = student.LastName,
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow
        }));

        context.Users.AddRange(users);
        return users;
    }

    private static List<SubjectPlan> PmiiPlan()
    {
        return new List<SubjectPlan>
        {
            new("История России", 1, ControlForm.Exam), new("Иностранный язык", 1, ControlForm.Credit), new("Русский язык и культура речи", 1, ControlForm.Credit), new("Таджикский язык", 1, ControlForm.Credit), new("Математический анализ", 1, ControlForm.Exam), new("Алгебра и геометрия", 1, ControlForm.Exam), new("Алгоритмы и алгоритмические языки", 1, ControlForm.Exam), new("Физическая культура и спорт", 1, ControlForm.Credit), new("Логика", 1, ControlForm.Credit), new("Практический курс на ЭВМ", 1, ControlForm.Credit),
            new("Иностранный язык", 2, ControlForm.Credit), new("Русский язык и культура речи", 2, ControlForm.Exam), new("Таджикский язык", 2, ControlForm.Exam), new("Безопасность жизнедеятельности", 2, ControlForm.Credit), new("Экономика", 2, ControlForm.Exam), new("Математический анализ", 2, ControlForm.Exam), new("Алгебра и геометрия", 2, ControlForm.Exam), new("Дискретная математика", 2, ControlForm.Credit), new("Основы информатики", 2, ControlForm.Credit), new("Архитектура компьютеров", 2, ControlForm.Credit), new("Элективные дисциплины по физической культуре и спорту", 2, ControlForm.Credit), new("Практический курс на ЭВМ", 2, ControlForm.Credit),
            new("Всеобщая история", 3, ControlForm.Exam), new("История таджикского народа", 3, ControlForm.Exam), new("Иностранный язык", 3, ControlForm.Credit), new("География Таджикистана и основы демографии", 3, ControlForm.Credit), new("Математический анализ", 3, ControlForm.Exam), new("Дискретная математика", 3, ControlForm.Exam), new("Физика", 3, ControlForm.Credit), new("Дифференциальные уравнения", 3, ControlForm.Credit), new("Теория вероятностей и математическая статистика", 3, ControlForm.Exam), new("Элективные дисциплины по физической культуре и спорту", 3, ControlForm.Credit), new("Педагогика и психология", 3, ControlForm.Credit), new("Практический курс на ЭВМ", 3, ControlForm.Credit),
            new("Философия", 4, ControlForm.Credit), new("Иностранный язык", 4, ControlForm.Exam), new("Комплексный анализ", 4, ControlForm.Exam), new("Функциональный анализ", 4, ControlForm.Exam), new("Физика", 4, ControlForm.Exam), new("Дифференциальные уравнения", 4, ControlForm.Exam), new("Теория вероятностей и математическая статистика", 4, ControlForm.Exam), new("Элективные дисциплины по физической культуре и спорту", 4, ControlForm.Credit), new("Разработка базы данных MS SQL Server", 4, ControlForm.Credit),
            new("Компьютерная графика", 5, ControlForm.Credit), new("Языки и методы программирования", 5, ControlForm.Exam), new("Численные методы", 5, ControlForm.Exam), new("Базы данных и экспертные системы", 5, ControlForm.Exam), new("Уравнения математической физики", 5, ControlForm.Exam), new("Практический курс на ЭВМ", 5, ControlForm.Credit),
            new("Системное и прикладное программное обеспечение", 6, ControlForm.Exam), new("Офисные технологии", 6, ControlForm.Credit), new("Операционные системы", 6, ControlForm.Exam), new("Компьютерные сети", 6, ControlForm.Credit), new("Информационные банковские технологии", 6, ControlForm.Exam), new("Прикладные интернет-технологии", 6, ControlForm.Exam), new("Практический курс на ЭВМ", 6, ControlForm.Credit),
            new("Концепции современного естествознания", 7, ControlForm.Exam), new("Методы оптимизации", 7, ControlForm.Exam), new("Математическое и компьютерное моделирование", 7, ControlForm.Credit), new("Основы правоведения", 7, ControlForm.Credit), new("Администрирование локальных сетей", 7, ControlForm.Credit), new("Java технологии", 7, ControlForm.Exam), new("Практический курс на ЭВМ", 7, ControlForm.Credit),
            new("Теория риска", 8, ControlForm.Credit), new("Теория чисел", 8, ControlForm.Exam), new("Алгоритмы, анализ сложности и комбинаторика", 8, ControlForm.Exam), new("JavaScript технологии", 8, ControlForm.Exam)
        };
    }

    private static List<SubjectPlan> SimplePlan(params string[] names)
    {
        return names.Select((name, index) => new SubjectPlan(name, (short)((index % 8) + 1), index % 3 == 0 ? ControlForm.Exam : ControlForm.Credit)).ToList();
    }

    private static Teacher PickTeacher(string subject, short semester, IReadOnlyList<Teacher> teachers, Random random)
    {
        var fullName = subject switch
        {
            "Математический анализ" => "Одинабеков Джассур Музаффирович",
            "Иностранный язык" when semester == 1 => "Абдуллоева Мадина Саидовна",
            "Иностранный язык" when semester == 2 => "Каримова Шахноза Фарруховна",
            "Иностранный язык" when semester is 3 or 4 => "Саидова Нигина Рустамовна",
            "Алгебра и геометрия" => "Попова Елена Николаевна",
            "Дискретная математика" => "Сергеев Петр Владимирович",
            var x when x.Contains("Алгоритмы", StringComparison.Ordinal) || x.Contains("программ", StringComparison.OrdinalIgnoreCase) => "Иванов Иван Игоревич",
            var x when x.Contains("Баз", StringComparison.Ordinal) || x.Contains("SQL", StringComparison.Ordinal) => "Волков Дмитрий Алексеевич",
            "Дифференциальные уравнения" => "Кузнецова Ольга Сергеевна",
            var x when x.Contains("вероятностей", StringComparison.OrdinalIgnoreCase) => "Тихонов Сергей Николаевич",
            "Операционные системы" => "Абдуллоев Рустам Нуриддинович",
            "Компьютерные сети" => "Рахимов Бахром Сафарович",
            var x when x.Contains("Java", StringComparison.OrdinalIgnoreCase) => "Юсупов Фирдавс Каримович",
            _ => string.Empty
        };

        return teachers.FirstOrDefault(x => $"{x.LastName} {x.FirstName} {x.FatherName}" == fullName)
            ?? teachers[random.Next(teachers.Count)];
    }

    private static Audience PickAudience(string subject, IReadOnlyList<Audience> audiences, Random random)
    {
        var pool = NeedsLab(subject)
            ? audiences.Where(x => x.Type is AudienceType.ComputerLab or AudienceType.ChemistryLab or AudienceType.PhysicsLab or AudienceType.GeologyLab).ToList()
            : audiences.Where(x => x.Type is AudienceType.GeneralHall or AudienceType.LectureHall or AudienceType.StreamHall).ToList();

        return pool[random.Next(pool.Count)];
    }

    private static LectureType PickLectureType(string subject, Random random)
    {
        if (NeedsLab(subject))
        {
            return random.Next(100) < 65 ? LectureType.Laboratory : LectureType.Practice;
        }

        if (subject.Contains("язык", StringComparison.OrdinalIgnoreCase) || subject.Contains("культура", StringComparison.OrdinalIgnoreCase))
        {
            return LectureType.Practice;
        }

        return random.Next(100) < 55 ? LectureType.Lecture : LectureType.Practice;
    }

    private static bool NeedsLab(string subject)
    {
        return subject.Contains("программ", StringComparison.OrdinalIgnoreCase)
            || subject.Contains("Java", StringComparison.OrdinalIgnoreCase)
            || subject.Contains("компьютер", StringComparison.OrdinalIgnoreCase)
            || subject.Contains("SQL", StringComparison.OrdinalIgnoreCase)
            || subject.Contains("Баз", StringComparison.Ordinal)
            || subject.Contains("хим", StringComparison.OrdinalIgnoreCase)
            || subject.Contains("Физика", StringComparison.Ordinal)
            || subject.Contains("материал", StringComparison.OrdinalIgnoreCase)
            || subject.Contains("геолог", StringComparison.OrdinalIgnoreCase);
    }

    private static short RetakeRoundFor(short mark, Random random)
    {
        if (mark >= 3)
        {
            return 0;
        }

        var roll = random.Next(100);
        if (mark == 1)
        {
            return 0;
        }

        return roll switch
        {
            < 45 => 2,
            < 75 => 2,
            < 90 => 3,
            _ => 4
        };
    }

    private static T Weighted<T>(Random random, IReadOnlyList<(T Value, int Weight)> values)
    {
        var total = values.Sum(x => x.Weight);
        var roll = random.Next(total);
        var current = 0;

        foreach (var (value, weight) in values)
        {
            current += weight;
            if (roll < current)
            {
                return value;
            }
        }

        return values[^1].Value;
    }

    private static string Phone(Random random, int index)
    {
        var codes = new[] { "90", "91", "92", "93" };
        return $"+992 {codes[index % codes.Length]} {random.Next(100, 1000)} {random.Next(10, 100)} {random.Next(10, 100)}";
    }

    private static string Address(Random random, int index)
    {
        var districts = new[] { "Сино", "Фирдавси", "Шохмансур", "Исмоили Сомони" };
        return $"Душанбе, район {districts[index % districts.Length]}, дом {random.Next(1, 180)}";
    }

    private sealed record SpecialityPlan(string Name, string ShortName, Faculty Faculty, List<SubjectPlan> Plan)
    {
        public Speciality? Speciality { get; set; }
    }

    private sealed record SubjectPlan(string Name, short Semester, ControlForm ControlForm)
    {
        public string SpecialityShortName { get; set; } = string.Empty;
        public Subject? Subject { get; set; }
    }
}
