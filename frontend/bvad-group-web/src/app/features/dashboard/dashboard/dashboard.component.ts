import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { CompanyAccessDto } from '../../../core/models/auth.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  constructor(public auth: AuthService) {}

  switchCompany(company: CompanyAccessDto): void {
    this.auth.switchCompany(company);
  }

  logout(): void {
    this.auth.logout();
  }
}