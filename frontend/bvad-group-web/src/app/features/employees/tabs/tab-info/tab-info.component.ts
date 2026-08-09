import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Employee } from '../../../../core/models/employee.model';

@Component({
  selector: 'app-tab-info',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tab-info.component.html'
})
export class TabInfoComponent {
  @Input({ required: true }) employee!: Employee;

  formatSalary(salary?: number): string {
    if (!salary) return '—';
    return new Intl.NumberFormat('fr-FR').format(salary) + ' FCFA';
  }
}