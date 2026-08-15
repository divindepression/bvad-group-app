import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(
        (m) => m.LoginComponent,
      ),
  },
  {
    path: '',
    loadComponent: () =>
      import('./layouts/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent,
      ),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent,
          ),
      },
      {
        path: 'my-profile',
        loadComponent: () =>
          import('./features/profile/my-profile/my-profile.component').then(
            (m) => m.MyProfileComponent,
          ),
      },
      {
        path: 'employees',
        loadComponent: () =>
          import('./features/employees/employee-list/employee-list.component').then(
            (m) => m.EmployeeListComponent,
          ),
      },
      {
        path: 'employees/:id',
        loadComponent: () =>
          import('./features/employees/employee-detail/employee-detail.component').then(
            (m) => m.EmployeeDetailComponent,
          ),
      },
      {
        path: 'companies',
        loadComponent: () =>
          import('./features/companies/company-list/company-list.component').then(
            (m) => m.CompanyListComponent,
          ),
      },
      {
        path: 'companies/:id',
        loadComponent: () =>
          import('./features/companies/company-edit/company-edit.component').then(
            (m) => m.CompanyEditComponent,
          ),
      },
      {
        path: 'committee',
        loadComponent: () =>
          import('./features/committee/committee/committee.component').then(
            (m) => m.CommitteeComponent,
          ),
      },
      {
        path: 'contracts',
        loadComponent: () =>
          import('./features/contracts/contract-list/contract-list.component').then(
            (m) => m.ContractListComponent,
          ),
      },
      {
        path: 'my-leaves',
        loadComponent: () =>
          import('./features/leaves/my-leaves/my-leaves.component').then(
            (m) => m.MyLeavesComponent,
          ),
      },
      {
        path: 'leave-approvals',
        loadComponent: () =>
          import('./features/leaves/leave-approvals/leave-approvals.component').then(
            (m) => m.LeaveApprovalsComponent,
          ),
      },
      {
        path: 'leave-calendar',
        loadComponent: () =>
          import('./features/leaves/leave-calendar/leave-calendar.component').then(
            (m) => m.LeaveCalendarComponent,
          ),
      },
      {
        path: 'org-chart',
        loadComponent: () =>
          import('./features/org-chart/org-chart/org-chart.component').then(
            (m) => m.OrgChartComponent,
          ),
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: '' },
];
