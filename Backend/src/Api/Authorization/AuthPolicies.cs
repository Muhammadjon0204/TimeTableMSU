namespace Api.Authorization;

public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string TeacherOnly = "TeacherOnly";
    public const string StudentOnly = "StudentOnly";
    public const string AdminOrTeacher = "AdminOrTeacher";
    public const string AdminOrTeacherOrStudent = "AdminOrTeacherOrStudent";
}
