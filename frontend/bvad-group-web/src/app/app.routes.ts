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
    import('./features/employees/employee-detail/employee-detail.component')
      .then(m => m.EmployeeDetailComponent)
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
