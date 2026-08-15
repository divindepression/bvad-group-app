import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { LeaveService } from '../../../core/services/leave.service';
import { CalendarLeave } from '../../../core/models/leave.model';

@Component({
  selector: 'app-leave-calendar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './leave-calendar.component.html'
})
export class LeaveCalendarComponent {
  auth = inject(AuthService);
  private leaveService = inject(LeaveService);

  leaves = signal<CalendarLeave[]>([]);
  loading = signal(false);
  currentMonth = signal(new Date());

  constructor() {
    effect(() => {
      const c = this.auth.currentCompany();
      if (c) this.load();
    }, { allowSignalWrites: true });
  }

  load(): void {
    const c = this.auth.currentCompany();
    if (!c || c.isHolding) {
      this.leaves.set([]);
      return;
    }

    this.loading.set(true);
    const first = new Date(this.currentMonth().getFullYear(), this.currentMonth().getMonth(), 1);
    const last = new Date(this.currentMonth().getFullYear(), this.currentMonth().getMonth() + 1, 0);

    this.leaveService.getCalendar(c.id, first.toISOString(), last.toISOString()).subscribe({
      next: (data) => {
        this.leaves.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  previousMonth(): void {
    const d = new Date(this.currentMonth());
    d.setMonth(d.getMonth() - 1);
    this.currentMonth.set(d);
    this.load();
  }

  nextMonth(): void {
    const d = new Date(this.currentMonth());
    d.setMonth(d.getMonth() + 1);
    this.currentMonth.set(d);
    this.load();
  }

  today(): void {
    this.currentMonth.set(new Date());
    this.load();
  }

  get monthName(): string {
    return this.currentMonth().toLocaleDateString('fr-FR', { month: 'long', year: 'numeric' });
  }

  get days(): Date[] {
    const year = this.currentMonth().getFullYear();
    const month = this.currentMonth().getMonth();
    const first = new Date(year, month, 1);
    const last = new Date(year, month + 1, 0);

    // Aligner sur lundi
    const startOffset = (first.getDay() + 6) % 7;
    const days: Date[] = [];

    for (let i = 0; i < startOffset; i++) {
      const d = new Date(first);
      d.setDate(d.getDate() - (startOffset - i));
      days.push(d);
    }

    for (let d = 1; d <= last.getDate(); d++) {
      days.push(new Date(year, month, d));
    }

    while (days.length % 7 !== 0) {
      const last = days[days.length - 1];
      const d = new Date(last);
      d.setDate(d.getDate() + 1);
      days.push(d);
    }

    return days;
  }

  isCurrentMonth(d: Date): boolean {
    return d.getMonth() === this.currentMonth().getMonth();
  }

  isToday(d: Date): boolean {
    const t = new Date();
    return d.toDateString() === t.toDateString();
  }

  isWeekend(d: Date): boolean {
    return d.getDay() === 0 || d.getDay() === 6;
  }

  leavesOnDay(day: Date): CalendarLeave[] {
    return this.leaves().filter(l => {
      const start = new Date(l.startDate);
      const end = new Date(l.endDate);
      const check = new Date(day);
      check.setHours(12, 0, 0, 0);
      return check >= start && check <= end;
    });
  }
}