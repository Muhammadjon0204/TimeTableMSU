import type { LucideIcon } from 'lucide-react';

type StatCardProps = {
  title: string;
  value: string | number;
  detail?: string;
  icon: LucideIcon;
  variant: 'students' | 'teachers' | 'groups' | 'subjects' | 'schedules' | 'attendance';
};

export function StatCard({ title, value, detail, icon: Icon, variant }: StatCardProps) {
  return (
    <article className={`stat-card stat-card--${variant}`}>
      <div className="stat-card__accent" />

      <div className="stat-card__top">
        <div className="stat-card__icon">
          <Icon />
        </div>
      </div>

      <div className="stat-card__body">
        <p className="stat-card__label">{title}</p>
        <h3 className="stat-card__value">{value}</h3>
        {detail ? <p className="stat-card__description">{detail}</p> : null}
      </div>
    </article>
  );
}
