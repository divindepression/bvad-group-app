import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EmployeeService } from '../../../core/services/employee.service';
import { Employee } from '../../../core/models/employee.model';
import { TabInfoComponent } from '../tabs/tab-info/tab-info.component';
import { TabDocumentsComponent } from '../tabs/tab-documents/tab-documents.component';
import { TabBadgeComponent } from '../tabs/tab-badge/tab-badge.component';
import { TabIdentityPhotoComponent } from '../tabs/tab-identity-photo/tab-identity-photo.component';
import { TabSignatureComponent } from '../tabs/tab-signature/tab-signature.component';

type TabId = 'info' | 'documents' | 'badge' | 'photo' | 'signature';

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    TabInfoComponent, TabDocumentsComponent,
    TabBadgeComponent, TabIdentityPhotoComponent,
    TabSignatureComponent  
  ],
  templateUrl: './employee-detail.component.html'
})
export class EmployeeDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private empService = inject(EmployeeService);

  employee = signal<Employee | null>(null);
  loading = signal(false);
  activeTab = signal<TabId>('info');

  tabs = [
    { id: 'info' as TabId, icon: '📇', label: 'Informations' },
    { id: 'documents' as TabId, icon: '📎', label: 'Documents' },
    { id: 'badge' as TabId, icon: '🎫', label: 'Badge' },
    { id: 'photo' as TabId, icon: '📸', label: 'Photo identité' },
    { id: 'signature' as TabId, icon: '🖋', label: 'Signature' }
  ];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/employees']);
      return;
    }
    this.load(id);
  }

  load(id: string): void {
    this.loading.set(true);
    this.empService.getById(id).subscribe({
      next: (emp) => {
        this.employee.set(emp);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/employees']);
      }
    });
  }

  reload(): void {
    const emp = this.employee();
    if (emp) this.load(emp.id);
  }

  setTab(id: TabId): void {
    this.activeTab.set(id);
  }

  initials(emp: Employee): string {
    return (emp.firstName.charAt(0) + emp.lastName.charAt(0)).toUpperCase();
  }
}