import { CommonModule } from '@angular/common';
import { Component, Input, inject, signal } from '@angular/core';
import { Employee } from '../../../../core/models/employee.model';
import { EmployeeService } from '../../../../core/services/employee.service';

@Component({
  selector: 'app-tab-info',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tab-info.component.html'
})
export class TabInfoComponent {
  @Input({ required: true }) employee!: Employee;

  private empService = inject(EmployeeService);
  downloadingSheet = signal(false);

  downloadSheet(): void {
    this.downloadingSheet.set(true);
    this.empService.downloadSheetPdf(this.employee.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Fiche_${this.employee.employeeNumber || this.employee.id}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.downloadingSheet.set(false);
      },
      error: () => {
        this.downloadingSheet.set(false);
        alert('❌ Erreur génération fiche');
      }
    });
  }

  formatSalary(salary?: number): string {
    if (!salary) return '—';
    return new Intl.NumberFormat('fr-FR').format(salary) + ' FCFA';
  }
}