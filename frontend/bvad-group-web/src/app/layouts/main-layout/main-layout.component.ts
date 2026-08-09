import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CompanyAccessDto } from '../../core/models/auth.model';

interface NavItem {
  icon: string;
  label: string;
  route: string;
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  templateUrl: './main-layout.component.html'
})
export class MainLayoutComponent {
  sidebarOpen = signal(false);
  companyMenuOpen = signal(false);

  navItems: NavItem[] = [
    { icon: '📊', label: 'Dashboard', route: '/dashboard' },
    { icon: '👨‍💼', label: 'Employés', route: '/employees' },
    { icon: '📄', label: 'Contrats', route: '/contracts' },
    { icon: '🏖', label: 'Congés', route: '/leaves' },
    { icon: '🏢', label: 'Filiales', route: '/companies' }
  ];

  constructor(public auth: AuthService, private router: Router) {}

  toggleSidebar(): void {
    this.sidebarOpen.update(v => !v);
  }

  closeSidebar(): void {
    this.sidebarOpen.set(false);
  }

  toggleCompanyMenu(): void {
    this.companyMenuOpen.update(v => !v);
  }

  switchCompany(company: CompanyAccessDto): void {
    this.auth.switchCompany(company);
    this.companyMenuOpen.set(false);
  }

  logout(): void {
    this.auth.logout();
  }
}