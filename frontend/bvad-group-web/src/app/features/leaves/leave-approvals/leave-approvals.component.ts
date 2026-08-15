import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { LeaveService } from '../../../core/services/leave.service';
import { LeaveRequest, LeaveStatusColors, LeaveStatusLabels } from '../../../core/models/leave.model';

@Component({
  selector: 'app-leave-approvals',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './leave-approvals.component.html'
})
export class LeaveApprovalsComponent {
  auth = inject(AuthService);
  private leaveService = inject(LeaveService);

  requests = signal<LeaveRequest[]>([]);
  loading = signal(false);

  showRejectModal = signal(false);
  rejectingRequest = signal<LeaveRequest | null>(null);
  rejectComment = '';

  constructor() {
    effect(() => {
      const c = this.auth.currentCompany();
      if (c) this.load();
    }, { allowSignalWrites: true });
  }

  load(): void {
    const company = this.auth.currentCompany();
    if (!company) return;

    this.loading.set(true);
    const filters: any = { status: 'Pending' };
    if (!company.isHolding) filters.companyId = company.id;

    this.leaveService.getRequests(filters).subscribe({
      next: (data) => {
        this.requests.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  approve(r: LeaveRequest): void {
    if (!confirm(`Approuver la demande de ${r.employeeName} ?`)) return;
    this.leaveService.approve(r.id).subscribe({
      next: () => this.load()
    });
  }

  openReject(r: LeaveRequest): void {
    this.rejectingRequest.set(r);
    this.rejectComment = '';
    this.showRejectModal.set(true);
  }

  confirmReject(): void {
    const r = this.rejectingRequest();
    if (!r || !this.rejectComment.trim()) return;

    this.leaveService.reject(r.id, this.rejectComment).subscribe({
      next: () => {
        this.showRejectModal.set(false);
        this.rejectingRequest.set(null);
        this.rejectComment = '';
        this.load();
      }
    });
  }

  cancelReject(): void {
    this.showRejectModal.set(false);
    this.rejectingRequest.set(null);
    this.rejectComment = '';
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  initials(name: string): string {
    const parts = name.trim().split(' ');
    if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    return name.substring(0, 2).toUpperCase();
  }
}