import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CompanyAccessDto } from '../../core/models/auth.model';

interface NavItem {
  icon: string;
  label: string;
  route: string;
  roles?: string[];
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './main-layout.component.html'
})
export class MainLayoutComponent {
  sidebarOpen = signal(false);
  companyMenuOpen = signal(false);

  private allNavItems: NavItem[] = [
    { icon: '📊', label: 'Dashboard', route: '/dashboard' },
    { icon: '👤', label: 'Mon profil', route: '/my-profile' },
    { icon: '🏛', label: 'Comité de direction', route: '/committee' },
    { icon: '🌳', label: 'Organigramme', route: '/org-chart' },
    { icon: '👨‍💼', label: 'Employés', route: '/employees', roles: ['SuperAdmin', 'Admin', 'Director', 'HR', 'Manager'] },
    { icon: '📄', label: 'Contrats', route: '/contracts', roles: ['SuperAdmin', 'Admin', 'Director', 'HR'] },
    { icon: '🏖', label: 'Congés', route: '/leaves' },
    { icon: '🏢', label: 'Filiales', route: '/companies', roles: ['SuperAdmin', 'Admin'] }
  ];

navItems = computed(() => {
  const user = this.auth.currentUser();
  const currentCompany = this.auth.currentCompany();
  if (!user) return [];

  // 🎭 Rôle effectif = rôle système si SuperAdmin/Admin,
  //                   sinon rôle dans la filiale active
  const effectiveRole = this.auth.getCurrentRole();

  return this.allNavItems.filter(item => {
    if (!item.roles) return true;
    return item.roles.includes(effectiveRole);
  });
});

  constructor(public auth: AuthService, private router: Router) {}

  toggleSidebar(): void { this.sidebarOpen.update(v => !v); }
  closeSidebar(): void { this.sidebarOpen.set(false); }
  toggleCompanyMenu(): void { this.companyMenuOpen.update(v => !v); }

  switchCompany(company: CompanyAccessDto): void {
    this.auth.switchCompany(company);
    this.companyMenuOpen.set(false);
  }

  logout(): void { this.auth.logout(); }
}