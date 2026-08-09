import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { CompanyService } from '../../../core/services/company.service';
import { EmployeeService } from '../../../core/services/employee.service';
import { CompanyAccessDto } from '../../../core/models/auth.model';
import { Company } from '../../../core/models/company.model';
import { RouterLink } from '@angular/router';

interface QuickAction {
  icon: string;
  label: string;
  route: string;
  color: string;
  roles?: string[];
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  auth = inject(AuthService);
  private companyService = inject(CompanyService);
  private employeeService = inject(EmployeeService);

  companiesDetails = signal<Company[]>([]);
  employeeCount = signal(0);
  loading = signal(false);

  allActions: QuickAction[] = [
    { icon: '👨‍💼', label: 'Employés', route: '/employees', color: 'bg-blue-500', roles: ['SuperAdmin', 'Admin', 'Director', 'HR', 'Manager'] },
    { icon: '🏛', label: 'Comité', route: '/committee', color: 'bg-amber-500' },
    { icon: '🌳', label: 'Organigramme', route: '/org-chart', color: 'bg-emerald-500' },
    { icon: '📄', label: 'Contrats', route: '/contracts', color: 'bg-purple-500', roles: ['SuperAdmin', 'Admin', 'Director', 'HR'] },
    { icon: '🏢', label: 'Filiales', route: '/companies', color: 'bg-cyan-500', roles: ['SuperAdmin', 'Admin'] },
    { icon: '👤', label: 'Mon profil', route: '/my-profile', color: 'bg-slate-500' }
  ];

  quickActions = computed(() => {
    const role = this.auth.getCurrentRole();
    return this.allActions.filter(a => !a.roles || a.roles.includes(role));
  });

  ngOnInit(): void {
    this.loadCompaniesDetails();
    this.loadEmployeeCount();
  }

  loadCompaniesDetails(): void {
    this.loading.set(true);
    this.companyService.getAll().subscribe({
      next: (data) => {
        this.companiesDetails.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadEmployeeCount(): void {
    const current = this.auth.currentCompany();
    const filters: any = {};
    if (current && !current.isHolding) filters.companyId = current.id;

    this.employeeService.getAll(filters).subscribe({
      next: (emps) => this.employeeCount.set(emps.length),
      error: () => this.employeeCount.set(0)
    });
  }

  switchCompany(company: CompanyAccessDto): void {
    this.auth.switchCompany(company);
    this.loadEmployeeCount();
  }

  // ═══ Helpers ═══
  logoUrl(companyId: string): string {
    return this.companyService.logoUrl(companyId);
  }

  hasLogo(companyId: string): boolean {
    return !!this.companiesDetails().find(c => c.id === companyId)?.logoUrl;
  }

  currentCompanyDetails(): Company | undefined {
    const current = this.auth.currentCompany();
    if (!current) return undefined;
    return this.companiesDetails().find(c => c.id === current.id);
  }

  greeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Bonjour';
    if (hour < 18) return 'Bon après-midi';
    return 'Bonsoir';
  }

  todayFormatted(): string {
    return new Date().toLocaleDateString('fr-FR', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}