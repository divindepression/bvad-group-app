import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CompanyService } from '../../../core/services/company.service';
import { AuthService } from '../../../core/services/auth.service';
import { Company } from '../../../core/models/company.model';
import { CompanyCreateComponent } from '../company-create/company-create.component';

@Component({
  selector: 'app-company-list',
  standalone: true,
  imports: [CommonModule, RouterLink, CompanyCreateComponent],
  templateUrl: './company-list.component.html'
})
export class CompanyListComponent implements OnInit {
  private service = inject(CompanyService);
  auth = inject(AuthService);

  companies = signal<Company[]>([]);
  loading = signal(false);
  showCreateForm = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (data) => {
        this.companies.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  logoUrl(c: Company): string {
    return this.service.logoUrl(c.id);
  }

  openCreateForm(): void {
    this.showCreateForm.set(true);
  }

  onFormClosed(reload: boolean): void {
    this.showCreateForm.set(false);
    if (reload) this.load();
  }

  canCreate(): boolean {
    const role = this.auth.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Admin';
  }
}