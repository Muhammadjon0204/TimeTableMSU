using Application.DTOs.AdminAcademicJournalDTOs;
using Application.Interfaces.Repository;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AdminAcademicJournalRepository : IAdminAcademicJournalRepository
{
    private readonly AppDbContext _context;

    public AdminAcademicJournalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AcademicFacultyCardDto>> GetFacultiesAsync()
    {
        return await _context.Faculties
            .AsNoTracking()
            .Select(faculty => new AcademicFacultyCardDto
            {
                FacultyId = faculty.Id,
                FacultyName = faculty.Name,
                SpecialitiesCount = faculty.Specialities.Count,
                GroupsCount = faculty.Specialities.SelectMany(speciality => speciality.Groups).Count(),
                StudentsCount = faculty.Specialities
                    .SelectMany(speciality => speciality.Groups)
                    .SelectMany(group => group.Students)
                    .Count()
            })
            .OrderBy(faculty => faculty.FacultyName)
            .ToListAsync();
    }

    public async Task<bool> FacultyExistsAsync(int facultyId)
    {
        return await _context.Faculties.AsNoTracking().AnyAsync(faculty => faculty.Id == facultyId);
    }

    public async Task<List<AcademicGroupCardDto>> GetGroupsByFacultyAsync(int facultyId)
    {
        return await _context.Groups
            .AsNoTracking()
            .Where(group => group.Speciality.FacultyId == facultyId)
            .Select(group => new AcademicGroupCardDto
            {
                GroupId = group.Id,
                GroupName = group.Name,
                Course = group.Course,
                SpecialityName = group.Speciality.Name,
                StudentsCount = group.Students.Count,
                MarksCount = group.Students.SelectMany(student => student.AcademicPerformances).Count()
            })
            .OrderBy(group => group.Course)
            .ThenBy(group => group.GroupName)
            .ToListAsync();
    }

    public async Task<bool> GroupExistsAsync(int groupId)
    {
        return await _context.Groups.AsNoTracking().AnyAsync(group => group.Id == groupId);
    }

    public async Task<List<AcademicStudentCardDto>> GetStudentsByGroupAsync(int groupId)
    {
        var students = await _context.Students
            .AsNoTracking()
            .Where(student => student.GroupId == groupId)
            .Include(student => student.AcademicPerformances)
            .OrderBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToListAsync();

        return students
            .Select(student =>
            {
                List<short> marks = student.AcademicPerformances
                    .Where(performance => performance.Mark.HasValue)
                    .Select(performance => performance.Mark.GetValueOrDefault())
                    .ToList();

                return new AcademicStudentCardDto
                {
                    StudentId = student.Id,
                    FullName = BuildFullName(student.LastName, student.FirstName, student.FatherName),
                    Email = student.Email,
                    Telephone = student.Telephone,
                    Address = student.Address,
                    MarksCount = student.AcademicPerformances.Count,
                    AverageMark = marks.Count > 0 ? marks.Average(mark => (double)mark) : null
                };
            })
            .ToList();
    }

    public async Task<StudentGradebookDto?> GetStudentGradebookAsync(int studentId)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Where(currentStudent => currentStudent.Id == studentId)
            .Select(currentStudent => new
            {
                currentStudent.Id,
                currentStudent.FirstName,
                currentStudent.LastName,
                currentStudent.FatherName,
                GroupName = currentStudent.Group != null ? currentStudent.Group.Name : null,
                SpecialityName = currentStudent.Group != null ? currentStudent.Group.Speciality.Name : null,
                FacultyName = currentStudent.Group != null ? currentStudent.Group.Speciality.Faculty.Name : null,
                Performances = currentStudent.AcademicPerformances
                    .Select(performance => new
                    {
                        performance.DisciplineId,
                        SubjectName = performance.Discipline.Subject.Name,
                        Semester = performance.Discipline.Subject.Semester,
                        ControlForm = performance.Discipline.Subject.ControlForm,
                        Mark = performance.Mark,
                        DisciplineTeacherFirstName = performance.Discipline.Teacher.FirstName,
                        DisciplineTeacherLastName = performance.Discipline.Teacher.LastName,
                        DisciplineTeacherFatherName = performance.Discipline.Teacher.FatherName,
                        PerformanceTeacherFirstName = performance.Teacher.FirstName,
                        PerformanceTeacherLastName = performance.Teacher.LastName,
                        PerformanceTeacherFatherName = performance.Teacher.FatherName
                    })
                    .OrderBy(performance => performance.Semester)
                    .ThenBy(performance => performance.SubjectName)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (student == null)
        {
            return null;
        }

        return new StudentGradebookDto
        {
            StudentId = student.Id,
            StudentFullName = BuildFullName(student.LastName, student.FirstName, student.FatherName),
            GroupName = student.GroupName,
            SpecialityName = student.SpecialityName,
            FacultyName = student.FacultyName,
            Semesters = student.Performances
                .GroupBy(performance => performance.Semester)
                .Select(semesterGroup => new StudentSemesterGradebookDto
                {
                    Semester = semesterGroup.Key,
                    Subjects = semesterGroup
                        .Select(performance => new StudentSubjectGradeDto
                        {
                            DisciplineId = performance.DisciplineId,
                            SubjectName = performance.SubjectName,
                            TeacherFullName = BuildFullName(
                                performance.PerformanceTeacherLastName ?? performance.DisciplineTeacherLastName,
                                performance.PerformanceTeacherFirstName ?? performance.DisciplineTeacherFirstName,
                                performance.PerformanceTeacherFatherName ?? performance.DisciplineTeacherFatherName),
                            ControlForm = performance.ControlForm.ToString(),
                            Mark = performance.Mark?.ToString(),
                            Status = GetStatus(performance.Mark),
                            // TODO: Retake history requires separate backend model later.
                            HasRetake = false,
                            RetakeCount = 0
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    public async Task<StudentJournalDto?> GetStudentJournalAsync(int studentId)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Where(currentStudent => currentStudent.Id == studentId)
            .Select(currentStudent => new
            {
                currentStudent.Id,
                currentStudent.FirstName,
                currentStudent.LastName,
                currentStudent.FatherName,
                GroupName = currentStudent.Group != null ? currentStudent.Group.Name : string.Empty,
                SpecialityName = currentStudent.Group != null ? currentStudent.Group.Speciality.Name : string.Empty,
                FacultyName = currentStudent.Group != null ? currentStudent.Group.Speciality.Faculty.Name : string.Empty,
                Performances = currentStudent.AcademicPerformances
                    .Select(performance => new
                    {
                        SubjectName = performance.Discipline.Subject.Name,
                        Semester = performance.Discipline.Subject.Semester,
                        ControlForm = performance.Discipline.Subject.ControlForm,
                        Mark = performance.Mark,
                        Tur = performance.Tur,
                        TeacherFirstName = performance.Teacher.FirstName,
                        TeacherLastName = performance.Teacher.LastName,
                        TeacherFatherName = performance.Teacher.FatherName
                    })
                    .OrderBy(performance => performance.Semester)
                    .ThenBy(performance => performance.SubjectName)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (student == null)
        {
            return null;
        }

        return new StudentJournalDto
        {
            Header = new StudentJournalHeaderDto
            {
                StudentId = student.Id,
                FullName = BuildFullName(student.LastName, student.FirstName, student.FatherName),
                GroupName = student.GroupName,
                SpecialityName = student.SpecialityName,
                FacultyName = student.FacultyName,
                AcademicYear = "2024/2025"
            },
            Semesters = student.Performances
                .GroupBy(performance => performance.Semester)
                .Select(semesterGroup => new StudentSemesterJournalDto
                {
                    Semester = semesterGroup.Key,
                    AcademicYear = "2024/2025",
                    Rows = semesterGroup
                        .Select((performance, index) => MapJournalRow(performance.SubjectName, performance.ControlForm, performance.Mark, performance.Tur, performance.TeacherLastName, performance.TeacherFirstName, performance.TeacherFatherName, index + 1))
                        .ToList()
                })
                .ToList()
        };
    }

    private static string BuildFullName(string lastName, string firstName, string? fatherName)
    {
        return string.IsNullOrWhiteSpace(fatherName)
            ? $"{lastName} {firstName}"
            : $"{lastName} {firstName} {fatherName}";
    }

    private static string GetStatus(short? mark)
    {
        if (!mark.HasValue)
        {
            return "NotSet";
        }

        return mark.Value >= 3 ? "Passed" : "Failed";
    }

    private static StudentJournalRowDto MapJournalRow(
        string subjectName,
        ControlForm controlForm,
        short? mark,
        short? tur,
        string teacherLastName,
        string teacherFirstName,
        string? teacherFatherName,
        int rowNumber)
    {
        CreditResult? creditResult = null;
        ExamResult? examResult = null;
        string finalDisplayValue;
        string statusDisplayValue;
        RetakeStatus retakeStatus;
        RetakeRound retakeRound;

        if (controlForm == ControlForm.Credit)
        {
            creditResult = MapCreditResult(mark);
            finalDisplayValue = creditResult.HasValue ? GetCreditDisplayValue(creditResult.Value) : "—";
            statusDisplayValue = creditResult.HasValue ? GetCreditStatusDisplayValue(creditResult.Value) : "Не выставлена";
            retakeStatus = creditResult.HasValue ? GetCreditRetakeStatus(creditResult.Value) : RetakeStatus.NotRequired;
        }
        else
        {
            examResult = MapExamResult(mark);
            finalDisplayValue = examResult.HasValue ? GetExamDisplayValue(examResult.Value) : "—";
            statusDisplayValue = examResult.HasValue ? GetExamStatusDisplayValue(examResult.Value) : "Не выставлена";
            retakeStatus = examResult.HasValue ? GetExamRetakeStatus(examResult.Value) : RetakeStatus.NotRequired;
        }

        retakeRound = GetRetakeRound(retakeStatus, tur);

        return new StudentJournalRowDto
        {
            RowNumber = rowNumber,
            SubjectName = subjectName,
            ControlForm = controlForm == ControlForm.Credit ? "Credit" : "Exam",
            TeacherFullName = BuildFullName(teacherLastName, teacherFirstName, teacherFatherName),
            // TODO: rename FirstAttestation/SecondAttestation to FirstControlWork/SecondControlWork in a future migration if those fields are added.
            FirstControlWork = null,
            SecondControlWork = null,
            CreditResult = creditResult,
            ExamResult = examResult,
            FinalDisplayValue = finalDisplayValue,
            StatusDisplayValue = statusDisplayValue,
            RetakeStatusDisplayValue = GetRetakeStatusDisplayValue(retakeStatus),
            RetakeRoundDisplayValue = GetRetakeRoundDisplayValue(retakeRound),
            Note = null,
            ResultType = GetResultType(creditResult, examResult),
            RetakeType = GetRetakeType(retakeStatus, retakeRound)
        };
    }

    private static CreditResult? MapCreditResult(short? mark)
    {
        if (!mark.HasValue)
        {
            return null;
        }

        if (mark.Value <= 0)
        {
            return CreditResult.Absent;
        }

        if (mark.Value == 1)
        {
            return CreditResult.NotAllowed;
        }

        if (mark.Value == 2)
        {
            return CreditResult.NotPassed;
        }

        return CreditResult.Passed;
    }

    private static ExamResult? MapExamResult(short? mark)
    {
        if (!mark.HasValue)
        {
            return null;
        }

        return mark.Value switch
        {
            <= 0 => ExamResult.Absent,
            1 => ExamResult.NotAllowed,
            2 => ExamResult.Unsatisfactory,
            3 => ExamResult.Satisfactory,
            4 => ExamResult.Good,
            _ => ExamResult.Excellent
        };
    }

    private static string GetCreditDisplayValue(CreditResult result)
    {
        return result switch
        {
            CreditResult.Absent => "Неявился",
            CreditResult.NotAllowed => "Недопуск",
            CreditResult.NotPassed => "Незачет",
            CreditResult.Passed => "Зачет",
            _ => "—"
        };
    }

    private static string GetCreditStatusDisplayValue(CreditResult result)
    {
        return result switch
        {
            CreditResult.Absent => "Неявка",
            CreditResult.NotAllowed => "Недопуск",
            CreditResult.NotPassed => "Не зачтено",
            CreditResult.Passed => "Зачтено",
            _ => "Не выставлена"
        };
    }

    private static string GetExamDisplayValue(ExamResult result)
    {
        return result switch
        {
            ExamResult.Absent => "Неявился",
            ExamResult.NotAllowed => "Недопуск",
            ExamResult.Unsatisfactory => "Неудовлетворительно",
            ExamResult.Satisfactory => "Удовлетворительно",
            ExamResult.Good => "Хорошо",
            ExamResult.Excellent => "Отлично",
            _ => "—"
        };
    }

    private static string GetExamStatusDisplayValue(ExamResult result)
    {
        return result switch
        {
            ExamResult.Absent => "Неявка",
            ExamResult.NotAllowed => "Недопуск",
            ExamResult.Unsatisfactory => "Не сдано",
            ExamResult.Satisfactory => "Сдано",
            ExamResult.Good => "Сдано",
            ExamResult.Excellent => "Сдано",
            _ => "Не выставлена"
        };
    }

    private static RetakeStatus GetCreditRetakeStatus(CreditResult result)
    {
        return result switch
        {
            CreditResult.Passed => RetakeStatus.NotRequired,
            CreditResult.NotAllowed => RetakeStatus.NotAllowed,
            CreditResult.NotPassed => RetakeStatus.Required,
            CreditResult.Absent => RetakeStatus.Required,
            _ => RetakeStatus.NotRequired
        };
    }

    private static RetakeStatus GetExamRetakeStatus(ExamResult result)
    {
        return result switch
        {
            ExamResult.Excellent => RetakeStatus.NotRequired,
            ExamResult.Good => RetakeStatus.NotRequired,
            ExamResult.Satisfactory => RetakeStatus.NotRequired,
            ExamResult.NotAllowed => RetakeStatus.NotAllowed,
            ExamResult.Unsatisfactory => RetakeStatus.Required,
            ExamResult.Absent => RetakeStatus.Required,
            _ => RetakeStatus.NotRequired
        };
    }

    private static RetakeRound GetRetakeRound(RetakeStatus status, short? tur)
    {
        if (status == RetakeStatus.NotRequired || status == RetakeStatus.NotAllowed)
        {
            return RetakeRound.None;
        }

        return tur switch
        {
            1 => RetakeRound.FirstRound,
            2 => RetakeRound.SecondRound,
            3 => RetakeRound.ThirdRound,
            4 => RetakeRound.Commission,
            _ => RetakeRound.SecondRound
        };
    }

    private static string GetRetakeStatusDisplayValue(RetakeStatus status)
    {
        return status switch
        {
            RetakeStatus.NotRequired => "Нет",
            RetakeStatus.Required => "Требуется",
            RetakeStatus.InProgress => "В процессе",
            RetakeStatus.Passed => "Закрыта",
            RetakeStatus.Failed => "Не закрыта",
            RetakeStatus.NotAllowed => "Недопуск",
            _ => "Нет"
        };
    }

    private static string GetRetakeRoundDisplayValue(RetakeRound round)
    {
        return round switch
        {
            RetakeRound.None => "—",
            RetakeRound.FirstRound => "1 тур",
            RetakeRound.SecondRound => "2 тур",
            RetakeRound.ThirdRound => "3 тур",
            RetakeRound.Commission => "Комиссия",
            _ => "—"
        };
    }

    private static string GetResultType(CreditResult? creditResult, ExamResult? examResult)
    {
        if (creditResult.HasValue)
        {
            return creditResult.Value switch
            {
                CreditResult.Passed => "success",
                CreditResult.NotPassed => "danger",
                CreditResult.NotAllowed => "warning",
                CreditResult.Absent => "muted",
                _ => "muted"
            };
        }

        if (examResult.HasValue)
        {
            return examResult.Value switch
            {
                ExamResult.Excellent => "success",
                ExamResult.Good => "primary",
                ExamResult.Satisfactory => "warning",
                ExamResult.Unsatisfactory => "danger",
                ExamResult.NotAllowed => "warning",
                ExamResult.Absent => "muted",
                _ => "muted"
            };
        }

        return "muted";
    }

    private static string GetRetakeType(RetakeStatus status, RetakeRound round)
    {
        if (status == RetakeStatus.NotRequired)
        {
            return "muted";
        }

        if (status == RetakeStatus.Passed)
        {
            return "success";
        }

        if (status == RetakeStatus.NotAllowed)
        {
            return "warning";
        }

        if (round == RetakeRound.Commission || status == RetakeStatus.Failed)
        {
            return "danger";
        }

        return round == RetakeRound.ThirdRound ? "warning" : "primary";
    }
}
