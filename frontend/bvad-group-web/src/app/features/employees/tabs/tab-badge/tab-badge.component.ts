import { CommonModule } from '@angular/common';
import { Component, Input, inject, signal } from '@angular/core';
import { Employee } from '../../../../core/models/employee.model';
import { EmployeeDocumentService } from '../../../../core/services/employee-document.service';

@Component({
  selector: 'app-tab-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tab-badge.component.html'
})
export class TabBadgeComponent {
  @Input({ required: true }) employee!: Employee;

  private docService = inject(EmployeeDocumentService);
  loading = signal(false);

  get hasIdentityPhoto(): boolean {
    return !!this.employee.identityPhotoUrl;
  }

  get photoUrl(): string {
    return this.docService.identityPhotoUrl(this.employee.id);
  }

  downloadBadge(): void {
    this.loading.set(true);
    this.docService.downloadBadge(this.employee.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Badge_${this.employee.employeeNumber || this.employee.id}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.loading.set(false);
      },
      error: (err) => {
        alert('❌ Erreur : ' + (err.error?.message || 'Impossible'));
        this.loading.set(false);
      }
    });
  }
}