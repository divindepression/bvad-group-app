import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';

import { AuthService } from '../../../core/services/auth.service';
import { CompanyService } from '../../../core/services/company.service';
import { DashboardService } from '../../../core/services/dashboard.service';
import { Company } from '../../../core/models/company.model';
import {
  AbsentToday,
  Birthday,
  DashboardOverview,
  EmployeesByCompany,
  EmployeesByContract,
  EmployeesByDepartment,
  ExpiringContract,
  HiringTrend,
  LeavesByMonth
} from '../../../core/models/dashboard.model';

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
  imports: [CommonModule, RouterLink, BaseChartDirective],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {

   // 🔥 Rendre Math accessible dans le template
  Math = Math;

  auth = inject(AuthService);
  private companyService = inject(CompanyService);
  private dashboardService = inject(DashboardService);

  // Data signals
  companiesDetails = signal<Company[]>([]);
  overview = signal<DashboardOverview | null>(null);
  employeesByCompany = signal<EmployeesByCompany[]>([]);
  employeesByDept = signal<EmployeesByDepartment[]>([]);
  employeesByContract = signal<EmployeesByContract[]>([]);
  hiringTrend = signal<HiringTrend[]>([]);
  leavesByMonth = signal<LeavesByMonth[]>([]);
  expiringContracts = signal<ExpiringContract[]>([]);
  birthdays = signal<Birthday[]>([]);
  absentToday = signal<AbsentToday[]>([]);
  loading = signal(true);

  // Quick actions filtrées par rôle
  private allActions: QuickAction[] = [
    { icon: '👨‍💼', label: 'Employés', route: '/employees', color: 'from-blue-500 to-blue-700', roles: ['SuperAdmin', 'Admin', 'Director', 'HR', 'Manager'] },
    { icon: '🏛', label: 'Comité', route: '/committee', color: 'from-amber-500 to-amber-700' },
    { icon: '🌳', label: 'Organigramme', route: '/org-chart', color: 'from-emerald-500 to-emerald-700' },
    { icon: '📄', label: 'Contrats', route: '/contracts', color: 'from-purple-500 to-purple-700', roles: ['SuperAdmin', 'Admin', 'Director', 'HR'] },
    { icon: '🏖', label: 'Mes congés', route: '/my-leaves', color: 'from-cyan-500 to-cyan-700' },
    { icon: '🏢', label: 'Filiales', route: '/companies', color: 'from-slate-500 to-slate-700', roles: ['SuperAdmin', 'Admin'] }
  ];

  quickActions = computed(() => {
    const role = this.auth.getCurrentRole();
    return this.allActions.filter(a => !a.roles || a.roles.includes(role));
  });

  constructor() {
    // Recharge quand on change de filiale
    effect(() => {
      const c = this.auth.currentCompany();
      if (c) this.loadAll();
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    this.loadCompaniesDetails();
  }

  loadCompaniesDetails(): void {
    this.companyService.getAll().subscribe({
      next: (data) => this.companiesDetails.set(data)
    });
  }

  loadAll(): void {
    this.loading.set(true);
    const company = this.auth.currentCompany();
    const cid = company && !company.isHolding ? company.id : undefined;

    this.dashboardService.getOverview(cid).subscribe({
      next: (data) => this.overview.set(data)
    });

    this.dashboardService.getEmployeesByCompany().subscribe({
      next: (data) => this.employeesByCompany.set(data)
    });

    this.dashboardService.getEmployeesByDepartment(cid).subscribe({
      next: (data) => this.employeesByDept.set(data)
    });

    this.dashboardService.getEmployeesByContract(cid).subscribe({
      next: (data) => this.employeesByContract.set(data)
    });

    this.dashboardService.getHiringTrend(cid, 12).subscribe({
      next: (data) => this.hiringTrend.set(data)
    });

    this.dashboardService.getLeavesByMonth(cid, 12).subscribe({
      next: (data) => this.leavesByMonth.set(data)
    });

    this.dashboardService.getExpiringContracts(cid, 60).subscribe({
      next: (data) => this.expiringContracts.set(data)
    });

    this.dashboardService.getUpcomingBirthdays(cid, 30).subscribe({
      next: (data) => this.birthdays.set(data)
    });

    this.dashboardService.getAbsentToday(cid).subscribe({
      next: (data) => {
        this.absentToday.set(data);
        this.loading.set(false);
      }
    });
  }

  // ═══════════════════════════════════════
  // 📊 CHARTS
  // ═══════════════════════════════════════

  // Camembert : employés par filiale
  employeesByCompanyChart = computed<ChartData<'doughnut'>>(() => {
    const data = this.employeesByCompany();
    return {
      labels: data.map(d => d.companyName),
      datasets: [{
        data: data.map(d => d.count),
        backgroundColor: data.map(d => d.companyColor),
        borderColor: '#0f172a',
        borderWidth: 3
      }]
    };
  });

  doughnutOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'right',
        labels: { color: '#e2e8f0', padding: 15, font: { size: 12 } }
      }
    }
  };

  // Barres : employés par département
  deptChart = computed<ChartData<'bar'>>(() => {
    const data = this.employeesByDept();
    return {
      labels: data.map(d => d.department),
      datasets: [{
        label: 'Employés',
        data: data.map(d => d.count),
        backgroundColor: '#3b82f6',
        borderRadius: 6
      }]
    };
  });

  barOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: {
      x: { ticks: { color: '#94a3b8' }, grid: { display: false } },
      y: {
        beginAtZero: true,
        ticks: { color: '#94a3b8', stepSize: 1 },
        grid: { color: 'rgba(148,163,184,0.1)' }
      }
    }
  };

  // Ligne : embauches sur 12 mois
  hiringChart = computed<ChartData<'line'>>(() => {
    const data = this.hiringTrend();
    return {
      labels: data.map(d => d.monthLabel),
      datasets: [{
        label: 'Embauches',
        data: data.map(d => d.count),
        borderColor: '#10b981',
        backgroundColor: 'rgba(16,185,129,0.2)',
        tension: 0.4,
        fill: true,
        pointBackgroundColor: '#10b981',
        pointBorderColor: '#fff',
        pointRadius: 5
      }]
    };
  });

  lineOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        labels: { color: '#e2e8f0', font: { size: 12 } }
      }
    },
    scales: {
      x: { ticks: { color: '#94a3b8' }, grid: { color: 'rgba(148,163,184,0.05)' } },
      y: {
        beginAtZero: true,
        ticks: { color: '#94a3b8', stepSize: 1 },
        grid: { color: 'rgba(148,163,184,0.1)' }
      }
    }
  };

  // Aires : congés par mois
  leavesChart = computed<ChartData<'bar'>>(() => {
    const data = this.leavesByMonth();
    return {
      labels: data.map(d => d.monthLabel),
      datasets: [
        {
          label: 'Approuvés',
          data: data.map(d => d.approved),
          backgroundColor: '#22c55e',
          borderRadius: 6
        },
        {
          label: 'En attente',
          data: data.map(d => d.pending),
          backgroundColor: '#f59e0b',
          borderRadius: 6
        },
        {
          label: 'Refusés',
          data: data.map(d => d.rejected),
          backgroundColor: '#ef4444',
          borderRadius: 6
        }
      ]
    };
  });

  stackedBarOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top',
        labels: { color: '#e2e8f0', font: { size: 11 } }
      }
    },
    scales: {
      x: {
        stacked: true,
        ticks: { color: '#94a3b8' },
        grid: { display: false }
      },
      y: {
        stacked: true,
        beginAtZero: true,
        ticks: { color: '#94a3b8', stepSize: 1 },
        grid: { color: 'rgba(148,163,184,0.1)' }
      }
    }
  };

  // Camembert : type de contrat
  contractChart = computed<ChartData<'doughnut'>>(() => {
    const data = this.employeesByContract();
    const colors = ['#3b82f6', '#8b5cf6', '#f59e0b', '#10b981', '#ef4444'];
    return {
      labels: data.map(d => d.contractType),
      datasets: [{
        data: data.map(d => d.count),
        backgroundColor: data.map((_, i) => colors[i % colors.length]),
        borderColor: '#0f172a',
        borderWidth: 3
      }]
    };
  });

  // ═══════════════════════════════════════
  // Helpers
  // ═══════════════════════════════════════
  logoUrl(companyId: string): string {
    return this.companyService.logoUrl(companyId);
  }

  hasLogo(companyId: string): boolean {
    return !!this.companiesDetails().find(c => c.id === companyId)?.logoUrl;
  }

  greeting(): string {
    const h = new Date().getHours();
    if (h < 12) return 'Bonjour';
    if (h < 18) return 'Bon après-midi';
    return 'Bonsoir';
  }

  today(): string {
    return new Date().toLocaleDateString('fr-FR', {
      weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
    });
  }

  formatMoney(amount: number): string {
    return new Intl.NumberFormat('fr-FR').format(amount);
  }

  initials(name: string): string {
    const parts = name.trim().split(' ');
    if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    return name.substring(0, 2).toUpperCase();
  }
}