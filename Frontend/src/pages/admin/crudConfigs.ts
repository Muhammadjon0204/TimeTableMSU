import type { CrudPageConfig } from './AdminCrudPage';

const idColumn = { key: 'id', label: 'ID' };

export const crudConfigs: Record<string, CrudPageConfig> = {
  faculties: {
    title: '\u0424\u0430\u043a\u0443\u043b\u044c\u0442\u0435\u0442\u044b',
    endpoint: '/faculties',
    description: '\u0423\u043f\u0440\u0430\u0432\u043b\u0435\u043d\u0438\u0435 \u0444\u0430\u043a\u0443\u043b\u044c\u0442\u0435\u0442\u0430\u043c\u0438 \u0438 \u0431\u0430\u0437\u043e\u0432\u043e\u0439 \u0430\u043a\u0430\u0434\u0435\u043c\u0438\u0447\u0435\u0441\u043a\u043e\u0439 \u0441\u0442\u0440\u0443\u043a\u0442\u0443\u0440\u043e\u0439 \u0443\u043d\u0438\u0432\u0435\u0440\u0441\u0438\u0442\u0435\u0442\u0430.',
    columns: [idColumn, { key: 'name', label: '\u041d\u0430\u0437\u0432\u0430\u043d\u0438\u0435' }],
    fields: [{ name: 'name', label: '\u041d\u0430\u0437\u0432\u0430\u043d\u0438\u0435', placeholder: '\u041d\u0430\u043f\u0440\u0438\u043c\u0435\u0440: \u041c\u0435\u0445\u0430\u043d\u0438\u043a\u043e-\u043c\u0430\u0442\u0435\u043c\u0430\u0442\u0438\u0447\u0435\u0441\u043a\u0438\u0439 \u0444\u0430\u043a\u0443\u043b\u044c\u0442\u0435\u0442', required: true, minLength: 2, maxLength: 150, validation: 'entityName' }],
  },
  specialities: {
    title: '\u0421\u043f\u0435\u0446\u0438\u0430\u043b\u044c\u043d\u043e\u0441\u0442\u0438',
    endpoint: '/specialities',
    description: '\u0421\u0432\u044f\u0437\u044c \u0441\u043f\u0435\u0446\u0438\u0430\u043b\u044c\u043d\u043e\u0441\u0442\u0435\u0439 \u0441 \u0444\u0430\u043a\u0443\u043b\u044c\u0442\u0435\u0442\u0430\u043c\u0438.',
    columns: [idColumn, { key: 'name', label: '\u041d\u0430\u0437\u0432\u0430\u043d\u0438\u0435' }, { key: 'facultyName', label: '\u0424\u0430\u043a\u0443\u043b\u044c\u0442\u0435\u0442' }],
    fields: [
      { name: 'name', label: '\u041d\u0430\u0437\u0432\u0430\u043d\u0438\u0435', placeholder: '\u041d\u0430\u0437\u0432\u0430\u043d\u0438\u0435 \u0441\u043f\u0435\u0446\u0438\u0430\u043b\u044c\u043d\u043e\u0441\u0442\u0438', required: true, minLength: 2, maxLength: 150, validation: 'specialityName' },
      { name: 'facultyId', label: '\u0424\u0430\u043a\u0443\u043b\u044c\u0442\u0435\u0442', type: 'searchable-select', placeholder: '\u0412\u044b\u0431\u0435\u0440\u0438\u0442\u0435 \u0444\u0430\u043a\u0443\u043b\u044c\u0442\u0435\u0442', required: true, lookup: { endpoint: '/faculties', labelKey: 'name' } },
    ],
  },
  groups: {
    title: '\u0413\u0440\u0443\u043f\u043f\u044b',
    endpoint: '/groups',
    description: '\u0423\u043f\u0440\u0430\u0432\u043b\u0435\u043d\u0438\u0435 \u0443\u0447\u0435\u0431\u043d\u044b\u043c\u0438 \u0433\u0440\u0443\u043f\u043f\u0430\u043c\u0438, \u043a\u0443\u0440\u0441\u043e\u043c \u0438 \u043f\u0440\u0438\u0432\u044f\u0437\u043a\u043e\u0439 \u043a \u0441\u043f\u0435\u0446\u0438\u0430\u043b\u044c\u043d\u043e\u0441\u0442\u0438.',
    columns: [idColumn, { key: 'name', label: '\u041d\u0430\u0437\u0432\u0430\u043d\u0438\u0435' }, { key: 'course', label: '\u041a\u0443\u0440\u0441' }, { key: 'specialityName', label: '\u0421\u043f\u0435\u0446\u0438\u0430\u043b\u044c\u043d\u043e\u0441\u0442\u044c' }],
    fields: [
      { name: 'name', label: '\u041d\u0430\u0437\u0432\u0430\u043d\u0438\u0435', placeholder: '\u041d\u0430\u043f\u0440\u0438\u043c\u0435\u0440: \u041f\u041c\u0438\u0418-101', required: true, minLength: 2, maxLength: 30, validation: 'groupName' },
      { name: 'specialityId', label: '\u0421\u043f\u0435\u0446\u0438\u0430\u043b\u044c\u043d\u043e\u0441\u0442\u044c', type: 'searchable-select', placeholder: '\u0412\u044b\u0431\u0435\u0440\u0438\u0442\u0435 \u0441\u043f\u0435\u0446\u0438\u0430\u043b\u044c\u043d\u043e\u0441\u0442\u044c', required: true, lookup: { endpoint: '/specialities', labelKey: 'name', subtitleKeys: ['facultyName'] } },
      { name: 'course', label: '\u041a\u0443\u0440\u0441', type: 'number', placeholder: '1', required: true, min: 1, max: 6 },
    ],
  },
  students: {
    title: '\u0421\u0442\u0443\u0434\u0435\u043d\u0442\u044b',
    endpoint: '/students',
    description: '\u0410\u0434\u043c\u0438\u043d\u0438\u0441\u0442\u0440\u0438\u0440\u043e\u0432\u0430\u043d\u0438\u0435 \u043f\u0440\u043e\u0444\u0438\u043b\u0435\u0439 \u0441\u0442\u0443\u0434\u0435\u043d\u0442\u043e\u0432 \u0438 \u0438\u0445 \u043f\u0440\u0438\u043d\u0430\u0434\u043b\u0435\u0436\u043d\u043e\u0441\u0442\u0438 \u043a \u0433\u0440\u0443\u043f\u043f\u0430\u043c.',
    columns: [idColumn, { key: 'fullName', label: '\u0424\u0418\u041e' }, { key: 'groupName', label: '\u0413\u0440\u0443\u043f\u043f\u0430' }, { key: 'email', label: 'Email' }],
    fields: [
      { name: 'firstName', label: '\u0418\u043c\u044f', placeholder: '\u0418\u043c\u044f', required: true, minLength: 2, maxLength: 50, validation: 'personName' },
      { name: 'lastName', label: '\u0424\u0430\u043c\u0438\u043b\u0438\u044f', placeholder: '\u0424\u0430\u043c\u0438\u043b\u0438\u044f', required: true, minLength: 2, maxLength: 50, validation: 'personName' },
      { name: 'fatherName', label: '\u041e\u0442\u0447\u0435\u0441\u0442\u0432\u043e', placeholder: '\u041e\u0442\u0447\u0435\u0441\u0442\u0432\u043e', minLength: 2, maxLength: 50, validation: 'personName' },
      { name: 'groupId', label: '\u0413\u0440\u0443\u043f\u043f\u0430', type: 'searchable-select', placeholder: '\u0412\u044b\u0431\u0435\u0440\u0438\u0442\u0435 \u0433\u0440\u0443\u043f\u043f\u0443', required: true, lookup: { endpoint: '/groups', labelKey: 'name', subtitleKeys: ['course', 'specialityName', 'facultyName'] } },
      { name: 'telephone', label: '\u0422\u0435\u043b\u0435\u0444\u043e\u043d', placeholder: '+998...', validation: 'phone' },
      { name: 'address', label: '\u0410\u0434\u0440\u0435\u0441', placeholder: '\u0410\u0434\u0440\u0435\u0441 \u043f\u0440\u043e\u0436\u0438\u0432\u0430\u043d\u0438\u044f' },
      { name: 'email', label: 'Email', type: 'email', placeholder: 'student@example.com', validation: 'email' },
      { name: 'birthDate', label: '\u0414\u0430\u0442\u0430 \u0440\u043e\u0436\u0434\u0435\u043d\u0438\u044f', type: 'date', required: true, validation: 'birthDate' },
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
