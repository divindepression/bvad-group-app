import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { MeService } from '../../../core/services/me.service';
import { LeaveService } from '../../../core/services/leave.service';
import {
  LeaveBalance,
  LeaveRequest,
  LeaveStatusColors,
  LeaveStatusLabels
} from '../../../core/models/leave.model';
import { LeaveFormComponent } from '../leave-form/leave-form.component';

@Component({
  selector: 'app-my-leaves',
  standalone: true,
  imports: [CommonModule, LeaveFormComponent],
  templateUrl: './my-leaves.component.html'
})
export class MyLeavesComponent implements OnInit {
  auth = inject(AuthService);
  meService = inject(MeService);
  private leaveService = inject(LeaveService);

  balances = signal<LeaveBalance[]>([]);
  requests = signal<LeaveRequest[]>([]);
  loading = signal(false);
  showForm = signal(false);

  currentYear = new Date().getFullYear();

  ngOnInit(): void {
    this.meService.load();
    setTimeout(() => this.load(), 300);
  }

  load(): void {
    const emp = this.meService.profile()?.employee;
    if (!emp) return;

    this.loading.set(true);

    this.leaveService.getBalances(emp.id, this.currentYear).subscribe({
      next: (data) => this.balances.set(data)
    });

    this.leaveService.getRequests({ employeeId: emp.id }).subscribe({
      next: (data) => {
        this.requests.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openForm(): void {
    this.showForm.set(true);
  }

  onFormClosed(reload: boolean): void {
    this.showForm.set(false);
    if (reload) this.load();
  }

  cancelRequest(r: LeaveRequest): void {
    if (!confirm(`Annuler cette demande de ${r.leaveTypeName} ?`)) return;
    this.leaveService.cancel(r.id).subscribe({
      next: () => this.load()
    });
  }

  statusColor(status: string): string {
    return LeaveStatusColors[status as keyof typeof LeaveStatusColors] || '';
  }
  statusLabel(status: string): string {
    return LeaveStatusLabels[status as keyof typeof LeaveStatusLabels] || status;
  }

  // Stats
  pendingCount = computed(() => this.requests().filter(r => r.status === 'Pending').length);
  approvedCount = computed(() => this.requests().filter(r => r.status === 'Approved').length);

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  progressPercent(b: LeaveBalance): number {
    const total = b.allocatedDays + b.carriedOverDays + b.adjustment;
    if (total <= 0) return 0;
    return Math.min(100, (b.usedDays / total) * 100);
  }
}