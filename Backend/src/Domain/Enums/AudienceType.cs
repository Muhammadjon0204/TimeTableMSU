namespace Domain.Enums;

public enum AudienceType //тип аудитории
{
    ComputerLab,    // Компьютерный кабинет
    ChemistryLab,   // Лаборатория химии
    GeologyLab,     // Лаборатория геологии
    PhysicsLab,     //кабинет Лаборатория физики
    LectureRoom,    // Backward-compatible value for existing database rows
    LectureHall,    // Лекционная аудитория
    StreamHall,     // Потоковая аудитория
    GeneralHall     // Общая аудитория
}
