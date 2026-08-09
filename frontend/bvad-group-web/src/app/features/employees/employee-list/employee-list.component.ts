import { CommonModule } from '@angular/common';
import { Component, OnInit, effect, inject, signal, Injector } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { Employee, EmployeeStatus } from '../../../core/models/employee.model';
import { EmployeeService } from '../../../core/services/employee.service';
import { EmployeeFormComponent } from '../employee-form/employee-form.component';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, FormsModule, EmployeeFormComponent, RouterLink],
  templateUrl: './employee-list.component.html'
})
export class EmployeeListComponent implements OnInit {
  employees = signal<Employee[]>([]);
  loading = signal(false);
  error = signal('');

  // Filtres
  search = '';
  statusFilter = signal<EmployeeStatus | ''>('');
  departmentFilter = signal<string>('');

  // Formulaire
  showForm = signal(false);
  editingEmployee = signal<Employee | null>(null);

  // 🔥 Injection propre
  private empService = inject(EmployeeService);
  public auth = inject(AuthService);
  private injector = inject(Injector);

  constructor() {
    // 🔥 Un seul effect ici, dans le contexte d'injection
    effect(() => {
      const company = this.auth.currentCompany();
      if (company) {
        console.log('🔄 Filiale changée →', company.name, '(isHolding:', company.isHolding, ')');
        this.load();
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    // Le premier load est déjà fait par l'effect
    // mais on force au cas où
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');

    const currentCompany = this.auth.currentCompany();
    const filters: any = {
      search: this.search || undefined,
      status: this.statusFilter() || undefined,
      department: this.departmentFilter() || undefined
    };

    // Si ce n'est pas la holding → filtrer par filiale
    if (currentCompany && !currentCompany.isHolding) {
      filters.companyId = currentCompany.id;
    }

    console.log('📡 GET /employees avec filtres :', filters);

    this.empService.getAll(filters).subscribe({
      next: (data) => {
        console.log(`✅ ${data.length} employés reçus`);
        this.employees.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Impossible de charger les employés');
        this.loading.set(false);
        console.error('❌ Erreur GET employees :', err);
      }
    });
  }

  onSearch(): void {
    this.load();
  }

  openAdd(): void {
    this.editingEmployee.set(null);
    this.showForm.set(true);
  }

  openEdit(emp: Employee): void {
    this.editingEmployee.set(emp);
    this.showForm.set(true);
  }

  onFormClosed(reload: boolean): void {
    this.showForm.set(false);
    this.editingEmployee.set(null);
    if (reload) this.load();
  }

  delete(emp: Employee): void {
    if (!confirm(`Supprimer ${emp.fullName} ?`)) return;

    this.empService.delete(emp.id).subscribe({
      next: () => this.load(),
      error: (err) => console.error(err)
    });
  }

  get totalCount(): number {
    return this.employees().length;
  }

  get activeCount(): number {
    return this.employees().filter(e => e.status === 'Active').length;
  }

  get onLeaveCount(): number {
    return this.employees().filter(e => e.status === 'OnLeave').length;
  }

  statusColor(status: string): string {
    const map: Record<string, string> = {
      Active: 'bg-green-500/20 text-green-400 border-green-500/30',
      OnLeave: 'bg-blue-500/20 text-blue-400 border-blue-500/30',
      Suspended: 'bg-orange-500/20 text-orange-400 border-orange-500/30',
      Terminated: 'bg-red-500/20 text-red-400 border-red-500/30',
      Probation: 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30'
    };
    return map[status] || 'bg-slate-500/20 text-slate-400';
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      Active: 'Actif',
      OnLeave: 'En congé',
      Suspended: 'Suspendu',
      Terminated: 'Parti',
      Probation: "Période d'essai"
    };
    return map[status] || status;
  }

  contractLabel(type: string): string {
    const map: Record<string, string> = {
      CDI: 'CDI',
      CDD: 'CDD',
      Internship: 'Stage',
      Freelance: 'Prestation',
      Apprenticeship: 'Apprentissage'
    };
    return map[type] || type;
  }

  formatSalary(salary?: number): string {
    if (!salary) return '—';
    return new Intl.NumberFormat('fr-FR').format(salary) + ' FCFA';
  }

  initials(emp: Employee): string {
    return (emp.firstName.charAt(0) + emp.lastName.charAt(0)).toUpperCase();
  }

  hasValidPhoto(url?: string): boolean {
  if (!url) return false;
  return url.startsWith('http://') ||
         url.startsWith('https://') ||
         url.startsWith('/');
}
}