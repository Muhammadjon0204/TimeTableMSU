import type { CrudPageConfig } from './AdminCrudPage';

const idColumn = { key: 'id', label: 'ID' };

export const crudConfigs: Record<string, CrudPageConfig> = {
  faculties: {
    title: 'Факультеты',
    endpoint: '/faculties',
    description: 'Управление факультетами и базовой академической структурой университета.',
    columns: [idColumn, { key: 'name', label: 'Название' }],
    fields: [{ name: 'name', label: 'Название', placeholder: 'Например: Механико-математический факультет', required: true }],
  },
  specialities: {
    title: 'Специальности',
    endpoint: '/specialities',
    description: 'Связь специальностей с факультетами.',
    columns: [idColumn, { key: 'name', label: 'Название' }, { key: 'facultyName', label: 'Факультет' }, { key: 'facultyId', label: 'ID факультета' }],
    fields: [
      { name: 'name', label: 'Название', placeholder: 'Название специальности', required: true },
      { name: 'facultyId', label: 'ID факультета', type: 'number', placeholder: 'Например: 1', required: true },
    ],
  },
  groups: {
    title: 'Группы',
    endpoint: '/groups',
    description: 'Управление учебными группами, курсом и привязкой к специальности.',
    columns: [idColumn, { key: 'name', label: 'Название' }, { key: 'course', label: 'Курс' }, { key: 'specialityName', label: 'Специальность' }],
    fields: [
      { name: 'name', label: 'Название', placeholder: 'Например: 101-А', required: true },
      { name: 'specialityId', label: 'ID специальности', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'course', label: 'Курс', type: 'number', placeholder: '1', required: true },
    ],
  },
  students: {
    title: 'Студенты',
    endpoint: '/students',
    description: 'Администрирование профилей студентов и их принадлежности к группам.',
    columns: [idColumn, { key: 'fullName', label: 'ФИО' }, { key: 'groupName', label: 'Группа' }, { key: 'email', label: 'Email' }],
    fields: [
      { name: 'firstName', label: 'Имя', placeholder: 'Имя', required: true },
      { name: 'lastName', label: 'Фамилия', placeholder: 'Фамилия', required: true },
      { name: 'fatherName', label: 'Отчество', placeholder: 'Отчество' },
      { name: 'groupId', label: 'ID группы', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'telephone', label: 'Телефон', placeholder: '+998...' },
      { name: 'address', label: 'Адрес', placeholder: 'Адрес проживания' },
      { name: 'email', label: 'Email', type: 'email', placeholder: 'student@example.com' },
      { name: 'birthDate', label: 'Дата рождения', type: 'date', required: true },
    ],
  },
  teachers: {
    title: 'Преподаватели',
    endpoint: '/teachers',
    description: 'Управление преподавателями, должностями и академическими данными.',
    columns: [idColumn, { key: 'fullName', label: 'ФИО' }, { key: 'teacherPost', label: 'Должность' }],
    fields: [
      { name: 'firstName', label: 'Имя', placeholder: 'Имя', required: true },
      { name: 'lastName', label: 'Фамилия', placeholder: 'Фамилия', required: true },
      { name: 'fatherName', label: 'Отчество', placeholder: 'Отчество' },
      { name: 'teacherDegree', label: 'Ученая степень', type: 'select', options: optionList(['None', 'Candidate', 'Doctor']) },
      { name: 'teacherTitle', label: 'Ученое звание', type: 'select', options: optionList(['None', 'Docent', 'Professor', 'Academician']) },
      { name: 'teacherPost', label: 'Должность', type: 'select', options: optionList(['Assistant', 'SeniorTeacher', 'Docent', 'Professor', 'HeadOfDepartment']) },
      { name: 'email', label: 'Email', type: 'email', placeholder: 'teacher@example.com' },
      { name: 'telephone', label: 'Телефон', placeholder: '+998...' },
    ],
  },
  subjects: {
    title: 'Предметы',
    endpoint: '/subjects',
    description: 'Управление учебными предметами, семестром и формой контроля.',
    columns: [idColumn, { key: 'name', label: 'Название' }, { key: 'controlForm', label: 'Форма контроля' }],
    fields: [
      { name: 'name', label: 'Название', placeholder: 'Название предмета', required: true },
      { name: 'semester', label: 'Семестр', type: 'number', placeholder: '1', required: true },
      { name: 'hourCount', label: 'Количество часов', type: 'number', placeholder: '72', required: true },
      { name: 'controlForm', label: 'Форма контроля', type: 'select', required: true, options: optionList(['Credit', 'Exam']) },
    ],
  },
  disciplines: {
    title: 'Дисциплины',
    endpoint: '/disciplines',
    description: 'Назначение предметов группам и преподавателям.',
    columns: [idColumn, { key: 'subjectName', label: 'Предмет' }, { key: 'teacherFullName', label: 'Преподаватель' }, { key: 'groupName', label: 'Группа' }],
    fields: [
      { name: 'subjectId', label: 'ID предмета', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'teacherId', label: 'ID преподавателя', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'groupId', label: 'ID группы', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'lectureHourCount', label: 'Лекционные часы', type: 'number', placeholder: '24' },
      { name: 'practiceHourCount', label: 'Практические часы', type: 'number', placeholder: '24' },
      { name: 'laboratoryHourCount', label: 'Лабораторные часы', type: 'number', placeholder: '12' },
    ],
  },
  audiences: {
    title: 'Аудитории',
    endpoint: '/audiences',
    description: 'Управление аудиториями, кабинетами и лабораториями.',
    columns: [idColumn, { key: 'number', label: 'Номер' }, { key: 'type', label: 'Тип' }],
    fields: [
      { name: 'number', label: 'Номер', placeholder: 'Например: 305', required: true },
      { name: 'type', label: 'Тип аудитории', type: 'select', required: true, options: optionList(['ComputerLab', 'ChemistryLab', 'GeologyLab', 'PhysicsLab', 'LectureRoom', 'LectureHall', 'StreamHall', 'GeneralHall']) },
    ],
  },
  weeks: {
    title: 'Учебные недели',
    endpoint: '/weeks',
    description: 'Настройка учебных недель и диапазонов дат.',
    columns: [idColumn, { key: 'name', label: 'Название' }, { key: 'startDate', label: 'Начало' }, { key: 'endDate', label: 'Окончание' }],
    fields: [
      { name: 'name', label: 'Название', placeholder: 'Например: Неделя 1', required: true },
      { name: 'startDate', label: 'Дата начала', type: 'date', required: true },
      { name: 'endDate', label: 'Дата окончания', type: 'date', required: true },
    ],
  },
  schedules: {
    title: 'Расписание',
    endpoint: '/schedules',
    description: 'Координация групп, преподавателей, аудиторий и учебных недель.',
    columns: [
      idColumn,
      { key: 'subjectName', label: 'Предмет' },
      { key: 'teacherFullName', label: 'Преподаватель' },
      { key: 'groupName', label: 'Группа' },
      { key: 'audienceNumber', label: 'Аудитория' },
      { key: 'den', label: 'День' },
      { key: 'para', label: 'Пара' },
    ],
    fields: [
      { name: 'den', label: 'День', type: 'number', placeholder: '1-6', required: true },
      { name: 'para', label: 'Пара', type: 'number', placeholder: '1-7', required: true },
      { name: 'disciplineId', label: 'ID дисциплины', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'teacherId', label: 'ID преподавателя', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'audienceId', label: 'ID аудитории', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'groupId', label: 'ID группы', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'weekId', label: 'ID недели', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'lectureType', label: 'Тип занятия', type: 'select', required: true, options: optionList(['Lecture', 'Practice', 'Laboratory']) },
    ],
  },
  attendances: {
    title: 'Посещаемость',
    endpoint: '/attendances',
    description: 'Учет посещаемости студентов по неделе, дню и паре.',
    columns: [idColumn, { key: 'studentFullName', label: 'Студент' }, { key: 'weekName', label: 'Неделя' }, { key: 'day', label: 'День' }, { key: 'para', label: 'Пара' }, { key: 'mark', label: 'Отметка' }],
    fields: [
      { name: 'studentId', label: 'ID студента', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'weekId', label: 'ID недели', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'day', label: 'День', type: 'number', placeholder: '1-6', required: true },
      { name: 'para', label: 'Пара', type: 'number', placeholder: '1-7', required: true },
      { name: 'mark', label: 'Отметка', type: 'select', options: optionList(['Present', 'Absent', 'Late', 'Excused']) },
    ],
  },
  academicPerformances: {
    title: 'Успеваемость',
    endpoint: '/academic-performances',
    description: 'Фиксация оценок студентов по дисциплинам и преподавателям.',
    columns: [idColumn, { key: 'studentFullName', label: 'Студент' }, { key: 'subjectName', label: 'Предмет' }, { key: 'mark', label: 'Оценка' }],
    fields: [
      { name: 'studentId', label: 'ID студента', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'disciplineId', label: 'ID дисциплины', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'teacherId', label: 'ID преподавателя', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'mark', label: 'Оценка', type: 'number', placeholder: '2-5' },
    ],
  },
  executions: {
    title: 'Выполнение занятий',
    endpoint: '/executions',
    description: 'Журнал статусов выполнения занятий по расписанию.',
    columns: [idColumn, { key: 'scheduleInfo', label: 'Расписание' }, { key: 'executionDate', label: 'Дата' }, { key: 'status', label: 'Статус' }, { key: 'notes', label: 'Заметки' }],
    fields: [
      { name: 'scheduleId', label: 'ID расписания', type: 'number', placeholder: 'Например: 1', required: true },
      { name: 'executionDate', label: 'Дата выполнения', type: 'date', required: true },
      { name: 'status', label: 'Статус', placeholder: 'Например: Проведено', required: true },
      { name: 'notes', label: 'Заметки', type: 'textarea', placeholder: 'Дополнительная информация' },
    ],
  },
  users: {
    title: 'Пользователи',
    endpoint: '/users',
    description: 'Администрирование пользователей.',
    columns: [idColumn, { key: 'email', label: 'Email' }, { key: 'role', label: 'Роль' }],
    fields: [],
    unavailableReason: 'Backend-эндпоинт для административного управления пользователями пока отсутствует.',
  },
};

function optionList(values: string[]) {
  return values.map((value) => ({ label: value, value }));
}
